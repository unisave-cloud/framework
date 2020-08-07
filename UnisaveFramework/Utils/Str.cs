using System;
using System.Text.RegularExpressions;

namespace Unisave.Utils
{
    /// <summary>
    /// String utilities
    /// </summary>
    public static class Str
    {
        private static Random random;

        /// <summary>
        /// Generates a random string
        /// (not cryptographically strong though)
        /// </summary>
        /// <param name="length">Length of the generated string</param>
        public static string Random(int length, Random givenRandom = null)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var buffer = new char[length];
            
            if (random == null)
                random = new Random();

            if (givenRandom == null)
                givenRandom = random;

            for (int i = 0; i < length; i++)
                buffer[i] = chars[givenRandom.Next(chars.Length)];

            return new String(buffer);
        }

        /// <summary>
        /// Determine if a given string matches a given pattern.
        /// The only allowed wildcards in the pattern are asterisks '*'.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool Is(string input, string pattern)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            
            if (pattern == input)
                return true;

            var regexPattern = Regex.Escape(pattern).Replace("\\*", ".*");

            return Regex.IsMatch(input, "^" + regexPattern + "$");
        }

        /// <summary>
        /// Finish a string with a single instance of a given value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Finish(string input, string tail)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            if (tail == null)
                throw new ArgumentNullException(nameof(tail));
            
            var escapedTail = Regex.Escape(tail);

            return Regex.Replace(
                input: input,
                pattern: "(?:" + escapedTail + ")+$",
                replacement: ""
            ) + tail;
        }

        /// <summary>
        /// Start a string with a single instance of a given value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="head"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Start(string input, string head)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            if (head == null)
                throw new ArgumentNullException(nameof(head));
            
            var escapedTail = Regex.Escape(head);

            return head + Regex.Replace(
                input: input,
                pattern: "^(?:" + escapedTail + ")+",
                replacement: ""
            );
        }
    }
}
