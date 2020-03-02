using Unisave;
using Unisave.Entities;

namespace FrameworkTests.Stubs
{
    public class SomeEntity : Entity
    {
        public string SomeString { get; set; } = "default value";

        public int SomeInt { get; set; } = 42;
    }
}