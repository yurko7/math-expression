using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace YuKu.MathExpression
{
    public static class Compiler
    {
        public static TDelegate Compile<TDelegate>(String function, params String[] parameterNames)
        {
            ParameterExpression[] parameters = parameterNames
                .Select(param => Expression.Parameter(typeof(Double), param))
                .ToArray();
            Expression expression;
            using (var reader = new StringReader(function))
            {
                var lexer = new Lexer(reader);
                var parser = new Parser();
                expression = parser.Parse(lexer, parameters);
            }
            Expression<TDelegate> lambda = Expression.Lambda<TDelegate>(expression, parameters);
            return lambda.Compile();
        }
    }
}
