using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unisave.Foundation;

namespace Unisave.Bootstrapping
{
    /// <summary>
    /// This class is responsible for calling all bootstrappers properly.
    /// </summary>
    public class BootstrappingEngine
    {
        /// <summary>
        /// Container used for bootstrapper instantiation
        /// </summary>
        private readonly IContainer container;
        
        /// <summary>
        /// Final list of bootstrapper types that are instantiated
        /// </summary>
        public Type[] BootstrapperTypes { get; }
        
        /// <summary>
        /// Constructs new bootstrapping engine
        /// </summary>
        /// <param name="container">
        /// Container used for bootstrapper singleton registration and resolution
        /// </param>
        /// <param name="backendTypes">
        /// Types to go through to find the bootstrapper classes
        /// </param>
        public BootstrappingEngine(
            IContainer container,
            BackendTypes backendTypes
        )
        {
            this.container = container;
            
            BootstrapperTypes = backendTypes.Where(type => {
                if (type.IsAbstract || type.IsValueType || type.IsInterface)
                    return false;
                
                if (type.IsGenericType || type.IsGenericTypeDefinition)
                    return false;
                
                if (!typeof(IBootstrapper).IsAssignableFrom(type))
                    return false;
                
                return true;
            })
                .Distinct() // remove duplicates
                .ToArray();
        }

        public void Run()
        {
            // register in the container as singletons
            foreach (Type type in BootstrapperTypes)
                container.RegisterSingleton(type);
            
            // instantiate via the container and set the container references
            IBootstrapper[] instances = BootstrapperTypes
                .Select(type => {
                    var instance = (IBootstrapper)container.Resolve(type);
                    instance.Services = container;
                    return instance;
                })
                .ToArray();

            // order by dependencies and stages
            instances = OrderInstances(instances);
            
            // run
            foreach (IBootstrapper instance in instances)
            {
                Console.WriteLine(
                    $"Running bootstrapper: {instance.GetType().FullName} ..."
                );
                
                // synchronous bootstrapper
                instance.Main();
                
                // async bootstrapper is for now also run synchronously
                // (could be made async in the future and make the app
                // initialization also async)
                instance.MainAsync().GetAwaiter().GetResult();
            }
            Console.WriteLine("Bootstrapping done.");
        }

        private IBootstrapper[] OrderInstances(IBootstrapper[] instances)
        {
            // split into stages
            var stages = new Dictionary<int, List<IBootstrapper>>();
            foreach (IBootstrapper instance in instances)
            {
                if (!stages.ContainsKey(instance.StageNumber))
                    stages[instance.StageNumber] = new List<IBootstrapper>();

                stages[instance.StageNumber].Add(instance);
            }

            // concatenate to get the final ordering
            return stages
                .OrderBy(pair => pair.Key) // order stages themselves
                .SelectMany(pair => // concat
                    OrderStage(pair.Value) // order instances within stages
                )
                .ToArray();
        }
        
        private List<IBootstrapper> OrderStage(List<IBootstrapper> instances)
        {
            Node[] graph = ConstructGraph(instances);

            List<Node> cycle = null;
            
            // this is like adding a new node, pointing to all other nodes
            // in the graph and starting the DFS from it:
            foreach (Node node in graph)
            {
                TraverseNode(node, ref cycle);
                
                // no need to check cycle - it can be formed with
                // where we are now and it throws an exception inside, not here
            }
            
            // split into planes
            var planes = new Dictionary<int, List<IBootstrapper>>();
            foreach (Node node in graph)
            {
                if (!planes.ContainsKey(node.planeNumber))
                    planes[node.planeNumber] = new List<IBootstrapper>();

                planes[node.planeNumber].Add(node.instance);
            }
            
            return planes
                .OrderBy(pair => pair.Key) // order planes themselves
                .SelectMany(pair => // concatenate planes
                    // each ordered by their name
                    pair.Value.OrderBy(instance => instance.GetType().FullName)
                )
                .ToList();
        }

        private Node[] ConstructGraph(List<IBootstrapper> instances)
        {
            // graph edges point towards the first bootstrappers to be executed
            // (plane 0 runs first and consists of the lowest children - leaves)

            // wrap instances to nodes
            Dictionary<Type, Node> typeToNodeMap = new Dictionary<Type, Node>();
            Node[] nodes = instances.Select(instance => {
                var node = new Node();
                node.planeNumber = -1; // "uninitialized"
                node.state = NodeState.Unseen;
                node.instance = instance;

                typeToNodeMap[instance.GetType()] = node;
                
                return node;
            }).ToArray();
            
            // create edges
            foreach (Node node in nodes)
            {
                foreach (Type childType in node.instance.RunAfter)
                {
                    if (typeToNodeMap.ContainsKey(childType))
                        node.children.Add(typeToNodeMap[childType]);
                    else
                        throw new BootstrappingException(
                            $"{node.instance.GetType()} was not found " +
                            $"in the same stage as {childType} " +
                            $"(via {nameof(node.instance.RunAfter)})."
                        );
                }

                foreach (Type parentType in node.instance.RunBefore)
                {
                    if (typeToNodeMap.ContainsKey(parentType))
                        typeToNodeMap[parentType].children.Add(node);
                    else
                        throw new BootstrappingException(
                            $"{parentType} was not found " +
                            $"in the same stage as {node.instance.GetType()} " +
                            $"(via {nameof(node.instance.RunBefore)})."
                        );
                }
            }

            return nodes;
        }

        private void TraverseNode(Node node, ref List<Node> cycle)
        {
            // this node has already been visited
            if (node.state == NodeState.Closed)
                return;
            
            // in a DAG, DFS should never hit an open node
            if (node.state == NodeState.Open)
            {
                // there is a cycle
                cycle = new List<Node>();
                cycle.Add(node);
                return;
            }

            node.state = NodeState.Open;
            
            // recursion over children, while tracking the largest plane number
            // and checking for cycle detection
            node.planeNumber = 0;
            foreach (Node child in node.children)
            {
                TraverseNode(child, ref cycle);

                if (cycle != null)
                {
                    // if we are back at the beginning of the cycle,
                    // we have it complete and can throw
                    if (cycle[0] == node)
                        ThrowCycleException(cycle);
                    
                    // else we keep on tracing the cycle
                    cycle.Add(node);
                    return;
                }

                if (child.planeNumber >= node.planeNumber)
                    node.planeNumber = child.planeNumber + 1;
            }

            node.state = NodeState.Closed;
        }

        private void ThrowCycleException(List<Node> cycle)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Bootstrapper dependency cycle detected:");
            sb.AppendLine($" first {cycle[0].instance.GetType()} should run,");
            for (int i = 1; i < cycle.Count; i++)
                sb.AppendLine($"  then {cycle[i].instance.GetType()}");
            sb.AppendLine($"  then {cycle[0].instance.GetType()} again.");

            throw new BootstrappingException(sb.ToString());
        }

        private class Node
        {
            public NodeState state;
            public int planeNumber;
            public IBootstrapper instance;
            public List<Node> children = new List<Node>();
        }

        private enum NodeState
        {
            Unseen = 0,
            Open = 1,
            Closed = 2
        }
    }
}