using System;
using System.Collections.Generic;
using System.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ExpressionEvaluator : IExpressionEvaluator
    {
        private static readonly IDictionary<string, Func<string, object>> _systemKeys;

        static ExpressionEvaluator()
        {
            _systemKeys = new SortedDictionary<string, Func<string, object>>();
            _systemKeys["$$null"] = (key) => null;
            _systemKeys["$$empty"] = (key) => string.Empty;
            _systemKeys["$$now"] = (key) => DateTime.Now;
            _systemKeys["$$today"] = (key) => DateTime.Today;
        }


        private readonly IDictionary<string, Func<string, object>> _knownKeys;

        public ExpressionEvaluator()
        {
            _knownKeys = new SortedDictionary<string, Func<string, object>>(_systemKeys);
        }

        public ExpressionEvaluator(IDictionary<string, Func<string, object>> knownKeys)
            : this()
        {
            foreach (var knownKey in knownKeys)
            {
                _knownKeys[knownKey.Key] = knownKey.Value;
            }
        }



        protected virtual bool TryLookupKey(string key, out Func<string, object> invoker)
        {
            var found = _knownKeys.TryGetValue(key, out invoker);
            return found;
        }


        public virtual Expr FetchExpressionParts(string expression)
        {
            throw new NotImplementedException();
        }


        public virtual object EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;
            if (expression.IndexOf('$') < 0)
                return expression;

            var words = expression.Split(' ').ToArray();
            for (var i = 0; i < words.Length; i++)
            {
                // todo: also split by: ${$now}

                var word = words[i];
                if (word.ElementAtOrDefault(0) == '$')
                {
                    if (word.ElementAtOrDefault(1) == '$')
                    {

                    }

                    var key = word;
                    object value = EvaluateObject(key);
                    
                    word = value?.ToString();
                    words[i] = word;
                }
            }

            var result = words.All(x => x == null)
                ? null
                : string.Join(" ", words);
            return result;
        }

        
        protected virtual object EvaluateObject(string key)
        {
            object result;
            Func<string, object> invoker;
            var found = TryLookupKey(key, out invoker);
            if (found)
            {
                result = invoker?.Invoke(key);
            }
            else
            {
                result = null;
            }
            return result;
        }

        protected virtual object EvaluateToken(object obj, string token, string expr)
        {
            // Tokens:
            //  Property - .
            //  Indexor  - [..]
            //  Method   - (..)
            //  Format   - :..


            // obj = object[4] { "A", "B", "C", "D" }
            // expr = .Length
            // result = 4

            // obj = object[4] { "A", "B", "C", "D" }
            // expr = [0]
            // result = "A"

            // obj = object[4] { "A", "B", "C", "D" }
            // expr = [0].Length
            // result = 1   ("A".Length)

            // obj = DateTime
            // expr = :{formatProvider}  :yyyyMMdd
            // result = 20170127

            throw new NotImplementedException();
        }


        public static bool TryEvaluateExpressionAlias(string key, out object value)
        {
            var success = true;
            key = key?.ToLower();
            if (key == "$$null")
            {
                value = null;
            }
            else if (key == "$$now")
            {
                value = DateTime.Now;
            }
            else
            {
                value = null;
                success = false;
            }
            return success;
        }



        public class Expr
        {
            public object Value { get; set; }
            public string Expression { get; set; }
            public Expr[] SubExpressions { get; set; }
        }

    }
}
