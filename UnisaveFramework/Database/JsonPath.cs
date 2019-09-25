using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Database
{
    /// <summary>
    /// Path through a JSON object hierarchy
    /// </summary>
    public class JsonPath : IEnumerable<object>
    {
        /// <summary>
        /// Segments of the path, either string or int
        /// </summary>
        private readonly List<object> segments;

        /// <summary>
        /// Returns the segment at a given index
        /// The segment is either a string or an int
        /// </summary>
        public object this[int segmentIndex] => segments[segmentIndex];

        /// <summary>
        /// Number of segments of the path
        /// </summary>
        public int Length => segments.Count;
        
        /// <summary>
        /// Creates new empty JSON path
        /// </summary>
        public JsonPath()
        {
            segments = new List<object>();
        }

        /// <summary>
        /// Creates JSON path with segments
        /// </summary>
        public JsonPath(params object[] segments) : this()
        {
            foreach (var s in segments)
                Add(s);
        }
        
        /// <summary>
        /// Parses the JSON path from a string
        /// </summary>
        public static JsonPath Parse(string path)
        {
            if (path == null)
                path = "";

            using (var reader = new StringReader(path))
            {
                var segments = ReadPath(new TextScanner(reader)).ToArray();
                
                return new JsonPath(segments);
            }
        }

        /// <summary>
        /// Adds a segment to this path and returns this instance for chaining
        /// </summary>
        public JsonPath Add(object segment)
        {
            if (segment == null)
                throw new ArgumentNullException();

            if (segment is string || segment is int)
            {
                segments.Add(segment);
                return this;
            }

            throw new ArgumentException(
                "Segment has to be either string or an int, " +
                $"but {segment.GetType()} provided."
            );
        }

        /// <summary>
        /// Serializes the path to JSON
        /// </summary>
        public JsonArray ToJson()
        {
            var json = new JsonArray();

            foreach (var segment in segments)
            {
                if (segment is int intSegment)
                    json.Add(intSegment);
                else if (segment is string stringSegment)
                    json.Add(stringSegment);
            }
            
            return json;
        }

        /// <summary>
        /// Loads the JSON path from its JSON representation
        /// </summary>
        public static JsonPath FromJson(JsonValue jsonValue)
        {
            JsonArray json = jsonValue.AsJsonArray;
            JsonPath path = new JsonPath();

            foreach (JsonValue item in json)
            {
                if (item.IsInteger)
                    path.Add(item.AsInteger);
                else if (item.IsString)
                    path.Add(item.AsString);
            }

            return path;
        }

        /// <summary>
        /// Extract value at this path from a json value
        /// </summary>
        public JsonValue ExtractValue(JsonValue subject)
        {
            foreach (var segment in segments)
            {
                if (segment is int intSegment)
                {
                    if (
                        subject.IsJsonArray
                        && intSegment >= 0
                        && intSegment < subject.AsJsonArray.Count
                    )
                    {
                        subject = subject.AsJsonArray[intSegment];
                    }
                    else
                    {
                        subject = JsonValue.Null;
                    }
                }
                else if (segment is string stringSegment)
                {
                    if (
                        subject.IsJsonObject
                        && subject.AsJsonObject.ContainsKey(stringSegment)
                    )
                    {
                        subject = subject.AsJsonObject[stringSegment];
                    }
                    else
                    {
                        subject = JsonValue.Null;
                    }
                }
            }

            return subject;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;
            foreach (var segment in segments)
            {
                if (segment is int) // int
                {
                    sb.Append("[");
                    sb.Append(segment);
                    sb.Append("]");
                }
                else if (segment is string str) // string
                {
                    if (str.Contains("\"") || str.Contains("\\"))
                    {
                        sb.Append("[");
                        sb.Append(new JsonValue(str).ToString());
                        sb.Append("]");
                    }
                    else
                    {
                        if (!first)
                            sb.Append('.');
                        
                        sb.Append(segment);
                    }
                }

                first = false;
            }
            
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return segments.GetEnumerator();
        }

        /////////////////////
        // Parsing methods //
        /////////////////////

        private static IEnumerable<object> ReadPath(TextScanner scanner)
        {
            while (scanner.CanRead)
                yield return ReadSegment(scanner);
        }

        private static object ReadSegment(TextScanner scanner)
        {
            switch (scanner.Peek())
            {
                case '[':
                    return ReadBracedSegment(scanner);
                
                case '.':
                    scanner.Read(); // eat up the dot
                    return ReadRegularSegment(scanner);
                
                default:
                    return ReadRegularSegment(scanner);
            }
        }

        private static string ReadRegularSegment(TextScanner scanner)
        {
            StringBuilder sb = new StringBuilder();

            while (
                scanner.CanRead
                && scanner.Peek() != '.'
                && scanner.Peek() != '['
            )
            {
                sb.Append(scanner.Read());
            }

            return sb.ToString();
        }

        private static object ReadBracedSegment(TextScanner scanner)
        {
            object segment;

            scanner.Assert('[');

            if (char.IsDigit(scanner.Peek()))
                segment = ReadInt(scanner);
            else if (scanner.Peek() == '"')
                segment = ReadString(scanner);
            else
            {
                scanner.Read();
                throw new JsonParseException(
                    JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                    scanner.Position
                );
            }
            
            scanner.Assert(']');

            return segment;
        }

        private static int ReadInt(TextScanner scanner)
        {
            var sb = new StringBuilder();
            
            while (char.IsDigit(scanner.Peek()))
                sb.Append(scanner.Read());

            return int.Parse(sb.ToString());
        }

        private static string ReadString(TextScanner scanner)
        {
            // stripped down version of the "JsonReader.ReadString" method
            
            var builder = new StringBuilder();

            scanner.Assert('"');

            while (true)
            {
                var c = scanner.Read();

                if (c == '\\')
                {
                    c = scanner.Read();

                    switch (char.ToLower(c))
                    {
                        case '"':  // "
                        case '\\': // \
                        case '/':  // /
                            builder.Append(c);
                            break;
                        default:
                            throw new JsonParseException(
                                JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                                scanner.Position
                            );
                    }
                }
                else if (c == '"')
                {
                    break;
                }
                else
                {
                    if (char.IsControl(c))
                    {
                        throw new JsonParseException(
                            JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                            scanner.Position
                        );
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
            }

            return builder.ToString();
        }
    }
}