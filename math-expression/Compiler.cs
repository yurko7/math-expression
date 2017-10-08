using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace YuKu.MathExpression
{
    internal sealed class Compiler : ICompiler
    {
        public Compiler(String mathExpression)
        {
            _mathExpression = mathExpression ?? throw new ArgumentNullException(nameof(mathExpression));
        }

        public ICompiler AddParameter(String parameterName)
        {
            _parameterNames.Add(parameterName);
            return this;
        }

        public ICompiler AddModule(Type module)
        {
            _modules.Add(module);
            return this;
        }

        public TDelegate Compile<TDelegate>()
        {
            ParameterExpression[] parameters = _parameterNames
                .Select(param => Expression.Parameter(typeof(Double), param))
                .ToArray();
            Expression expression;
            using (TextReader stringReader = new StringReader(_mathExpression))
            {
                var lexer = new Lexer(stringReader);
                var parser = new Parser();
                expression = parser.Parse(lexer, parameters, _modules);
            }
            Expression<TDelegate> lambda = Expression.Lambda<TDelegate>(expression, parameters);
            return lambda.Compile();
        }

        private readonly String _mathExpression;
        private readonly List<String> _parameterNames = new List<String>();
        private readonly List<Type> _modules = new List<Type>();
    }
}
