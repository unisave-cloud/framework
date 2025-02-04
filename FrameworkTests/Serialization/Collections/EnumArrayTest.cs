using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Collections
{
    public enum SkillType
    {
        None = 0,
        AoE = 1,
        PushBack = 2,
        JumpSlam = 3,
    }
    
    /// <summary>
    /// Tests the serialization of enum arrays, which crashed the moment
    /// I switched to CoreCLR
    ///
    /// The problem was that the enum serializer returned int instead of
    /// the requested enum (boxed into an object). Fixed it according to:
    /// https://stackoverflow.com/questions/29482/how-do-i-cast-int-to-enum-in-c
    /// </summary>
    [TestFixture]
    public class EnumArrayTest
    {
        [Test]
        public void ItDeserializesEnumArray()
        {
            // this crashed inside the array serializer
            // (this crashes only in CoreCLR, not in Mono)
            SkillType[] a = Serializer.FromJsonString<SkillType[]>("[0,1,2,3]");
            Assert.AreEqual(
                new SkillType[]
                {
                    SkillType.None, SkillType.AoE,
                    SkillType.PushBack, SkillType.JumpSlam
                },
                a
            );
        }
        
        [Test]
        public void ItDeserializesEnum()
        {
            // this is the problem - it returned int instead of the enum instance
            // (this crashes both in Mono and CoreCLR)
            object a = Serializer.FromJson(new JsonValue(1), typeof(SkillType));
            Assert.AreEqual(
                SkillType.AoE,
                a
            );
        }
    }
}