using System;
using FrameworkTests.Facets;
using FrameworkTests.Facets.Stubs;
using NUnit.Framework;
using Unisave;
using Unisave.Runtime;
using LightJson;
using LightJson.Serialization;
using Moq;
using Unisave.Database;
using Unisave.Services;

namespace FrameworkTests
{
    class FakeMigration : Migration
    {
        public static bool flag = false;
        public override int From => 1;
        public override int To => 2;
        public override void Run()
        {
            flag = true;
        }
    }

    class NotFoundMigration : Migration
    {
        public static bool flag = false;
        public override int From => 1;
        public override int To => 444;
        public override void Run() {}
    }

    class AmbiguousMigrationA : Migration
    {
        public static bool flag = false;
        public override int From => 2;
        public override int To => 3;
        public override void Run() {}
    }

    class AmbiguousMigrationB : Migration
    {
        public static bool flag = false;
        public override int From => 2;
        public override int To => 3;
        public override void Run() {}
    }

    class ExceptionalMigration : Migration
    {
        public static bool flag = false;
        public override int From => 3;
        public override int To => 4;
        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class MigrationCallTest
    {
        private JsonObject ExecuteMigration(int from, int to)
        {
            var methodParameters = new JsonObject();
            methodParameters.Add("migrateFrom", from);
            methodParameters.Add("migrateTo", to);

            var executionParameters = new JsonObject();
            executionParameters.Add("executionId", "fake-execution-id");
            executionParameters.Add("databaseProxyIp", JsonValue.Null);
            executionParameters.Add("databaseProxyPort", 0);
            executionParameters.Add("executionMethod", "migration");
            executionParameters.Add("methodParameters", methodParameters);

            ServiceContainer.Default = new ServiceContainer();
            ServiceContainer.Default.Register<IDatabase>(null);
            
            string result = Entrypoint.Start(executionParameters.ToString(), new Type[] {
                typeof(FakeMigration),
                typeof(NotFoundMigration),
                typeof(AmbiguousMigrationA),
                typeof(AmbiguousMigrationB),
                typeof(ExceptionalMigration),

                typeof(WrongFacet),
                typeof(FacetCallTest),
                typeof(System.Collections.Hashtable)
            });

            return JsonReader.Parse(result);
        }

        [TestCase]
        public void ItExecutes()
        {
            FakeMigration.flag = false;

            JsonObject result = ExecuteMigration(1, 2);

            Assert.IsTrue(FakeMigration.flag);
            Assert.AreEqual("ok", result["result"].AsString);
        }

        [TestCase]
        public void MigrationNotFound()
        {
            JsonObject result = ExecuteMigration(2, 45);

            Assert.AreEqual("invalid-method-parameters", result["result"].AsString);
            StringAssert.Contains("Migration wasn't found.", result["message"]);
        }

        [TestCase]
        public void MigrationAmbiguous()
        {
            JsonObject result = ExecuteMigration(2, 3);

            Assert.AreEqual("invalid-method-parameters", result["result"].AsString);
            StringAssert.Contains("Migration wasn't found.", result["message"]);
        }

        [TestCase]
        public void MigrationThrowsException()
        {
            JsonObject result = ExecuteMigration(3, 4);
            Assert.AreEqual("exception", result["result"].AsString);
        }
    }
}
