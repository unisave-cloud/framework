using System.Collections.ObjectModel;
using System.Linq;
using LightJson;
using Unisave.Arango.Execution;

namespace Unisave.Arango.Expressions
{
    public class AqlMemberAccessExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.MemberAccess;

        public override ReadOnlyCollection<string> Parameters { get; }
        
        /// <summary>
        /// The instance that we are looking at (object / array)
        /// </summary>
        public AqlExpression Subject { get; }
        
        /// <summary>
        /// Field / index we are asking for
        /// </summary>
        public AqlExpression Member { get; }

        public AqlMemberAccessExpression(
            AqlExpression subject,
            AqlExpression member
        )
        {
            Subject = subject;
            Member = member;

            Parameters = new ReadOnlyCollection<string>(
                Subject.Parameters.Union(Member.Parameters).ToList()
            );
        }
        
        public override string ToAql()
        {
            return Subject.ToAql() + MemberToAql();
        }

        private string MemberToAql()
        {
            if (Member is AqlConstantExpression c && c.Value.IsString)
            {
                string s = c.Value.AsString;

                if (s != null && HasStringSafeCharacters(s))
                {
                    return "." + s;
                }
            }
            
            // default, safest option
            return "[" + Member.ToAql() + "]";
        }

        private bool HasStringSafeCharacters(string s)
        {
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string charset = letters + numbers + "_";

            if (s.Length == 0)
                return false;

            if (numbers.Contains(s[0]))
                return false;
            
            return s.All(c => charset.Contains(char.ToLower(c)));
        }
        
        public override JsonValue Evaluate(
            QueryExecutor executor,
            ExecutionFrame frame
        )
        {
            JsonValue m = Member.Evaluate(executor, frame);
            JsonValue s = Subject.Evaluate(executor, frame);

            if (s.IsJsonArray)
            {
                if (!m.IsInteger)
                    return JsonValue.Null;
                
                int i = m.AsInteger;

                if (i < 0)
                    i += s.AsJsonArray.Count;
                
                return s[i];
            }

            if (s.IsJsonObject)
            {
                if (!m.IsString)
                    return JsonValue.Null;

                return s[m.AsString];
            }
            
            return JsonValue.Null;
        }
    }
}