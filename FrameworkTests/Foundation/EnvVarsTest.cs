using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Owin.Builder;
using NUnit.Framework;
using Owin;
using Unisave;
using Unisave.Foundation;

namespace FrameworkTests.Foundation
{
    [TestFixture]
    public class EnvVarsTest
    {
        private EnvStore CreateApplicationViaStartup(
            IDictionary<string, string> unisaveEnvVars
        )
        {
            IAppBuilder app = new AppBuilder();

            app.Properties["host.OnAppDisposing"] = CancellationToken.None;
            app.Properties["unisave.GameAssemblies"] = Array.Empty<Assembly>();
            
            if (unisaveEnvVars != null)
                app.Properties["unisave.EnvironmentVariables"] = unisaveEnvVars;
            
            new FrameworkStartup().Configuration(app);

            var backendApplication = (BackendApplication) app
                .Properties["unisave.BackendApplication"];

            return backendApplication.Services.Resolve<EnvStore>();
        }
        
        [Test]
        public void UnisaveEnvVarsAreOptional()
        {
            Assert.DoesNotThrow(() => {
                // null = no env vars provided
                CreateApplicationViaStartup(null);
            });
        }
        
        [Test]
        public void ItParsesStandardEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable(
                "UNISAVE_TEST_FOO",
                "lorem ipsum"
            );

            var env = CreateApplicationViaStartup(
                new Dictionary<string, string>()
            );

            string foo = env.GetString("UNISAVE_TEST_FOO");
            Assert.AreEqual("lorem ipsum", foo);
        }
        
        [Test]
        public void ItParsesUnisaveEnvironmentVariables()
        {
            var env = CreateApplicationViaStartup(
                new Dictionary<string, string>() {
                    ["FOO"] = "bar"
                }
            );

            string foo = env.GetString("FOO");
            Assert.AreEqual("bar", foo);
        }
        
        [Test]
        public void UnisaveEnvVarsOverrideStandardEnvVars()
        {
            Environment.SetEnvironmentVariable(
                "UNISAVE_TEST_FOO",
                "lorem ipsum"
            );

            var env = CreateApplicationViaStartup(
                new Dictionary<string, string>() {
                    ["UNISAVE_TEST_FOO"] = "overriden!"
                }
            );

            string foo = env.GetString("UNISAVE_TEST_FOO");
            Assert.AreEqual("overriden!", foo);
        }
    }
}