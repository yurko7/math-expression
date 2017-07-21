using System;
using System.IO;
using System.Linq.Expressions;

namespace YuKu.MathExpression
{
    public static class Compiler
    {
        public static Func<Double, Double> Compile(String function)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(Double), "t");
            Expression expression;
            using (var reader = new StringReader(function))
            {
                var lexer = new Lexer(reader);
                var parser = new Parser();
                expression = parser.Parse(lexer, parameter);
            }
            Expression<Func<Double, Double>> lambda = Expression.Lambda<Func<Double, Double>>(expression, parameter);
            return lambda.Compile();
        }
    }
}
