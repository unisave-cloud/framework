using System;
using System.IO;
using System.Reflection;

namespace Unisave.Framework
{
    public class MyClass
    {
        // entry point
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting the action...");

            string assemblyName = Path.GetFullPath("./Game.dll");

            MethodInfo method = Assembly.LoadFile(assemblyName).EntryPoint;
            method.Invoke(null, new object[] {
                //new string[] {}
            });
        }

        public static void SayHello()
        {
            Console.WriteLine("Unisave framework says hello!");
        }
    }
}
