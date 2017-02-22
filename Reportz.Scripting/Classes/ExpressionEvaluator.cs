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
            _systemKeys["$$true"] = (key) => true;
            _systemKeys["$$false"] = (key) => false;
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


        public IEnumerable<Expr> EnumerateExpressionParts(string expression)
        {
            var rootExpr = new Expr
            {
                Expression = expression,
                Index = 0,
                Root = null,
                Value = null,
                SubExpressions = new Expr[0],
            };

            return EnumerateExpressionParts(expression, rootExpr);
        }

        protected virtual IEnumerable<Expr> EnumerateExpressionParts(string expression, Expr rootExpr)
        {
            var index = expression.IndexOf("$");
            while (index >= 0 && index + 1 < expression.Length)
            {
                var s = expression.Substring(index).TrimStart('$').First();
                var isGroup = s == '{';
                var endChars = new char[] {'.', '[', ']', '{', '}', '$'};
                string exprStr = expression.Substring(index);
                if (isGroup)
                {
                    var d = 0;
                    for (var i = index + 1; i < expression.Length; i++)
                    {
                        var c = expression[i];
                        if (c == '{')
                        {
                            d++;
                        }
                        if (c == '}')
                        {
                            d--;
                        }

                        if (d == 0)
                        {
                            exprStr = expression.Substring(index, index - i);
                            break;
                        }
                    }
                }
                else
                {
                    
                }

                var expr = new Expr
                {
                    Expression = exprStr,
                    Root = rootExpr,
                    Index = index,
                };

                rootExpr.SubExpressions = rootExpr.SubExpressions ?? new Expr[0];
                rootExpr.SubExpressions = rootExpr.SubExpressions.Concat(new[] { expr }).ToArray();

                if (isGroup)
                {
                    expr.SubExpressions = EnumerateExpressionParts(expr.Expression, expr).ToArray();
                }

                yield return expr;

                var nextIndexMargin = exprStr.Substring(index).Length - exprStr.Substring(index).TrimStart('$').Length + 1;
                index = expression.IndexOf("$", index + nextIndexMargin);
            }
        }


        public virtual object EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;
            if (expression.IndexOf('$') < 0)
                return expression;

            var resultParts = new List<object>();
            string str = expression;


            var words = expression.Split(' ').ToArray();
            for (var i = 0; i < words.Length; i++)
            {
                // todo: also split by: ${$now}

                var word = words[i];
                if (word.ElementAtOrDefault(0) == '$')
                {
                    var key = word;
                    if (word.ElementAtOrDefault(1) == '{')
                    {
                        var ei = word.IndexOf('}');
                        key = ei > 0
                            ? word.Substring("${".Length, ei - "${".Length)
                            : word.Substring("${".Length);
                    }

                    object value = EvaluateObject(key);
                    resultParts.Add(value);

                    word = value?.ToString();
                    words[i] = word;
                }
            }
            str = string.Join(" ", words);
            //resultParts = words.Cast<object>().ToList();


            //IEnumerator<Expr> enumerator;
            //do
            //{
            //    enumerator = EnumerateExpressionParts(str).GetEnumerator();
            //    if (enumerator.MoveNext())
            //    {
            //        var expr = enumerator.Current;
            //        var val = EvaluateObject(expr.Expression);

            //        if (expr.SubExpressions != null && expr.SubExpressions.Any())
            //        {

            //        }

            //        resultParts.Add(val);

            //        var a = str.Substring(0, expr.Index);
            //        var b = str.Substring(expr.Index);
            //        str = a + val + b;
            //    }
            //} while (enumerator.Current != null);

            var result = resultParts.Count > 1
                ? (resultParts.All(x => x == null)
                    ? null
                    : str)
                : (resultParts.Count == 1
                    ? resultParts.Single()
                    : null);
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
            public Expr Root { get; set; }
            public int Index { get; set; }
            public object Value { get; set; }
            public string Expression { get; set; }
            public Expr[] SubExpressions { get; set; }
        }

    }
}
