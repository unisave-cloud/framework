using Moq;
using NUnit.Framework;
using Unisave.Contracts;
using Unisave.Runtime;
using Unisave.Sessions;

namespace FrameworkTests.Sessions
{
    [TestFixture]
    public class SandboxApiSessionTest
    {
        private ISession session;
        private Mock<ApiChannel> channelMock;
        
        [SetUp]
        public void SetUp()
        {
            channelMock = new Mock<ApiChannel>("session");
            
            session = new SandboxApiSession(
                channelMock.Object
            );
        }
        
        // TODO: see SandboxDatabaseApiTest for reference
    }
}