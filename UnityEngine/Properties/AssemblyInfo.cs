using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("UnityEngine")]
[assembly: AssemblyDescription(
    "Fake UnityEngine.dll implementing ony things " +
    "that might beneeded on the server"
)]
[assembly: InternalsVisibleTo("UnisaveFramework")]
[assembly: AssemblyVersion("0.0.0.0")]
