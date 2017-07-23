using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace YuKu.MathExpression
{
    public static class Extensions
    {
        public static TDelegate Compile<TDelegate>(this String mathExpression, params String[] parameterNames)
        {
            using (var reader = new StringReader(mathExpression))
            {
                return reader.Compile<TDelegate>(parameterNames);
            }
        }

        public static TDelegate Compile<TDelegate>(this TextReader mathExpression, params String[] parameterNames)
        {
            ParameterExpression[] parameters = parameterNames
                .Select(param => Expression.Parameter(typeof(Double), param))
                .ToArray();
            var lexer = new Lexer(mathExpression);
            var parser = new Parser();
            Expression expression = parser.Parse(lexer, parameters);

            Expression<TDelegate> lambda = Expression.Lambda<TDelegate>(expression, parameters);
            return lambda.Compile();
        }
    }
}
