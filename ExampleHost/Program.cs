using System;
using Microsoft.Owin.Hosting;
using Mono.Unix;
using Mono.Unix.Native;

namespace ExampleHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            using (WebApp.Start<Startup>("http://localhost:1234"))
            {
                Console.WriteLine("Running.");
                WaitForTermination();
                Console.WriteLine("Disposing...");
            }
            Console.WriteLine("Disposed.");

            // if there was an execution timeout
            // or the user started some rogue threads,
            // this kills all of them:
            Environment.Exit(0);
        }

        private static void WaitForTermination()
        {
            if (IsRunningOnMono())
            {
                UnixSignal.WaitAny(GetUnixTerminationSignals());
            }
            else
            {
                Console.WriteLine("Press enter to stop the application.");
                Console.ReadLine();
            }
        }

        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private static UnixSignal[] GetUnixTerminationSignals()
        {
            return new[] {
                new UnixSignal(Signum.SIGINT),
                new UnixSignal(Signum.SIGTERM),
                new UnixSignal(Signum.SIGQUIT),
                new UnixSignal(Signum.SIGHUP)
            };
        }
    }
}