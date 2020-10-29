using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Polymorphism
{
    /*
     * The polymorphism is tested on the concept of
     * player moves in a turn-based game.
     */

    public abstract class PlayerMove
    {
        // name of the player that performed the move
        public string player;
    }

    public class DoNothingMove : PlayerMove
    {
        // empty
    }

    public class PlayCardMove : PlayerMove
    {
        // card index within the player's hand
        public int cardIndex;
    }

    [TestFixture]
    public class PolymorphicSerializationTest
    {
        [Test]
        public void EachMoveCanBeSerialized()
        {
            Assert.AreEqual(
                "{'$type':'FrameworkTests.Serialization.DoNothingMove'," +
                "'player':'John'}".Replace('\'', '"'),
                Serializer.ToJsonString(new DoNothingMove {
                    player = "John"
                })
            );
            
            Assert.AreEqual(
                "{'$type':'FrameworkTests.Serialization.PlayCardMove'," +
                "'player':'Peter','cardIndex':2}".Replace('\'', '"'),
                Serializer.ToJsonString(new PlayCardMove {
                    player = "Peter",
                    cardIndex = 2
                })
            );
        }

        [Test]
        public void EachMoveCanBeDeserializedSpecifically()
        {
            // DoNothingMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(DoNothingMove).FullName,
                    ["player"] = "John"
                };
                var value = Serializer.FromJson<DoNothingMove>(json);
                Assert.IsInstanceOf<DoNothingMove>(value);
                Assert.AreEqual("John", value.player);
            }

            // PlayCardMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(PlayCardMove).FullName,
                    ["player"] = "Peter",
                    ["cardIndex"] = 2
                };
                var value = Serializer.FromJson<PlayCardMove>(json);
                Assert.IsInstanceOf<PlayCardMove>(value);
                Assert.AreEqual("Peter", value.player);
                Assert.AreEqual(2, value.cardIndex);
            }
        }
        
        [Test]
        public void EachMoveCanBeDeserializedPolymorphicly()
        {
            // DoNothingMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(DoNothingMove).FullName,
                    ["player"] = "John"
                };
                var value = Serializer.FromJson<PlayerMove>(json);
                Assert.IsInstanceOf<DoNothingMove>(value);
                Assert.AreEqual("John", value.player);
            }

            // PlayCardMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(PlayCardMove).FullName,
                    ["player"] = "Peter",
                    ["cardIndex"] = 2
                };
                var value = Serializer.FromJson<PlayerMove>(json);
                Assert.IsInstanceOf<PlayCardMove>(value);
                Assert.AreEqual("Peter", value.player);
                Assert.AreEqual(2, ((PlayCardMove) value).cardIndex);
            }
        }
        
        [Test]
        public void EachMoveCanBeDeserializedAsObject()
        {
            // DoNothingMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(DoNothingMove).FullName,
                    ["player"] = "John"
                };
                var value = Serializer.FromJson<object>(json);
                Assert.IsInstanceOf<DoNothingMove>(value);
                Assert.AreEqual("John", ((PlayerMove) value).player);
            }

            // PlayCardMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(PlayCardMove).FullName,
                    ["player"] = "Peter",
                    ["cardIndex"] = 2
                };
                var value = Serializer.FromJson<object>(json);
                Assert.IsInstanceOf<PlayCardMove>(value);
                Assert.AreEqual("Peter", ((PlayerMove) value).player);
                Assert.AreEqual(2, ((PlayCardMove) value).cardIndex);
            }
        }
    }
}