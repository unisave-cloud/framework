using Unisave;
using Unisave.Entities;

namespace FrameworkTests.Stubs
{
    public class SomeEntity : Entity
    {
        [X] public string SomeString { get; set; } = "default value";

        [X] public int SomeInt { get; set; } = 42;
    }
}