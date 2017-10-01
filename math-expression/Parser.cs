using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace YuKu.MathExpression
{
    public sealed class Parser
    {
        public Parser()
        {
            _nullDenotations = new Dictionary<TokenType, NullDenotation>();
            _leftDenotations = new Dictionary<TokenType, LeftDenotation>();

            RegisterToken(TokenType.EOF, TokenType.RParen);

            RegisterToken(0, ParseGroup, TokenType.LParen);
            RegisterToken(0, ParseNumber, TokenType.Number);
            RegisterToken(1, ParseAdd, TokenType.Plus);
            RegisterToken(1, ParseSubtract, TokenType.Minus);
            RegisterToken(2, ParseMultiply, TokenType.Multiply);
            RegisterToken(2, ParseImpliedMultiply, TokenType.LParen, TokenType.Number, TokenType.Identifier);
            RegisterToken(2, ParseDivide, TokenType.Divide);
            RegisterToken(2, ParseModulo, TokenType.Modulo);
            RegisterToken(3, ParsePower, TokenType.Power);
            RegisterToken(4, ParseNegate, TokenType.Minus);
            RegisterToken(5, ParseIdentifier, TokenType.Identifier);
        }

        public Expression Parse(IEnumerator<Token> tokens, IEnumerable<ParameterExpression> parameters)
        {
            try
            {
                _tokens = tokens;
                _parameters = parameters.ToDictionary(param => param.Name, StringComparer.OrdinalIgnoreCase);
                Advance();
                return ParseExpression(0);
            }
            finally
            {
                _tokens.Dispose();
                _tokens = null;
                _parameters = null;
                _lookahead = new Token();
            }
        }

        private void RegisterToken(params TokenType[] tokens)
        {
            foreach (TokenType tokenType in tokens)
            {
                _leftDenotations.Add(tokenType, null);
            }
        }

        private void RegisterToken(Int32 leftBindingPower, Func<Int32, Expression> nullDenotation, params TokenType[] tokens)
        {
            var denotation = new NullDenotation(leftBindingPower, nullDenotation);
            foreach (TokenType tokenType in tokens)
            {
                _nullDenotations.Add(tokenType, denotation);
            }
        }

        private void RegisterToken(Int32 leftBindingPower, Func<Int32, Expression, Expression> leftDenotation, params TokenType[] tokens)
        {
            var denotation = new LeftDenotation(leftBindingPower, leftDenotation);
            foreach (TokenType tokenType in tokens)
            {
                _leftDenotations.Add(tokenType, denotation);
            }
        }

        private Expression ParseExpression(Int32 leftBindingPower)
        {
            NullDenotation nullDenotation = _nullDenotations[_lookahead.Type];
            Expression expression = nullDenotation.Invoke();

            LeftDenotation leftDenotation = _leftDenotations[_lookahead.Type];
            while (leftDenotation != null && leftDenotation.LeftBindingPower > leftBindingPower)
            {
                expression = leftDenotation.Invoke(expression);
                leftDenotation = _leftDenotations[_lookahead.Type];
            }

            return expression;
        }

        private Expression ParseGroup(Int32 leftBindingPower)
        {
            Match(TokenType.LParen);
            Expression expression = ParseExpression(leftBindingPower);
            Match(TokenType.RParen);

            return expression;
        }

        private Expression ParseNumber(Int32 leftBindingPower)
        {
            Token token = Match(TokenType.Number);
            Double value = Double.Parse(token.Text, CultureInfo.InvariantCulture);
            return Expression.Constant(value);
        }

        private Expression ParseNegate(Int32 leftBindingPower)
        {
            Match(TokenType.Minus);
            Expression expression = ParseExpression(leftBindingPower);
            return Expression.Negate(expression);
        }

        private Expression ParseAdd(Int32 leftBindingPower, Expression leftExpression)
        {
            Match(TokenType.Plus);
            Expression rightExpression = ParseExpression(leftBindingPower);
            return Expression.Add(leftExpression, rightExpression);
        }

        private Expression ParseSubtract(Int32 leftBindingPower, Expression leftExpression)
        {
            Match(TokenType.Minus);
            Expression rightExpression = ParseExpression(leftBindingPower);
            return Expression.Subtract(leftExpression, rightExpression);
        }

        private Expression ParseMultiply(Int32 leftBindingPower, Expression leftExpression)
        {
            Match(TokenType.Multiply);
            Expression rightExpression = ParseExpression(leftBindingPower);
            return Expression.Multiply(leftExpression, rightExpression);
        }

        private Expression ParseImpliedMultiply(Int32 leftBindingPower, Expression leftExpression)
        {
            Expression rightExpression = ParseExpression(leftBindingPower);
            return Expression.Multiply(leftExpression, rightExpression);
        }

        private Expression ParseDivide(Int32 leftBindingPower, Expression leftExpression)
        {
            Match(TokenType.Divide);
            Expression rightExpression = ParseExpression(leftBindingPower);
            return Expression.Divide(leftExpression, rightExpression);
        }

        private Expression ParseModulo(Int32 leftBindingPower, Expression leftExpression)
        {
            Match(TokenType.Modulo);
            Expression rightExpression = ParseExpression(leftBindingPower);
            return Expression.Modulo(leftExpression, rightExpression);
        }

        private Expression ParsePower(Int32 leftBindingPower, Expression leftExpression)
        {
            Match(TokenType.Power);
            Expression rightExpression = ParseExpression(leftBindingPower - 1);
            return Expression.Power(leftExpression, rightExpression);
        }

        private Expression ParseIdentifier(Int32 leftBindingPower)
        {
            Token identifier = Match(TokenType.Identifier);

            if (_parameters.TryGetValue(identifier.Text, out ParameterExpression parameter))
            {
                return parameter;
            }

            Type typeMath = typeof(Math);
            FieldInfo field = typeMath.GetField(
                identifier.Text,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (field != null)
            {
                return Expression.Field(null, field);
            }

            MethodInfo method = typeMath.GetMethod(
                identifier.Text,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
                null, new[] {typeof(Double)}, null);
            if (method == null)
            {
                throw new InvalidOperationException($"Unknown identifier \"{identifier.Text}\" ({identifier.Location})");
            }
            Expression argExpression = ParseExpression(leftBindingPower);
            return Expression.Call(method, argExpression);
        }

        private Token Match(TokenType tokenType)
        {
            if (_lookahead.Type != tokenType)
            {
                throw new InvalidOperationException("Invalid token.");
            }
            return Consume();
        }

        private Token Consume()
        {
            Token token = _lookahead;
            Advance();
            return token;
        }

        private void Advance()
        {
            if (!_tokens.MoveNext())
            {
                throw new InvalidOperationException("Advance after EOF");
            }
            _lookahead = _tokens.Current;
        }

        private abstract class Denotation
        {
            protected Denotation(Int32 leftBindingPower)
            {
                LeftBindingPower = leftBindingPower;
            }

            public Int32 LeftBindingPower { get; }
        }

        private sealed class NullDenotation : Denotation
        {
            public NullDenotation(Int32 leftBindingPower, Func<Int32, Expression> nullDenotation)
                : base(leftBindingPower)
            {
                _nullDenotation = nullDenotation;
            }

            public Expression Invoke()
            {
                return _nullDenotation(LeftBindingPower);
            }

            private readonly Func<Int32, Expression> _nullDenotation;
        }

        private sealed class LeftDenotation : Denotation
        {
            public LeftDenotation(Int32 leftBindingPower, Func<Int32, Expression, Expression> leftDenotation)
                : base(leftBindingPower)
            {
                _leftDenotation = leftDenotation;
            }

            public Expression Invoke(Expression leftExpression)
            {
                return _leftDenotation(LeftBindingPower, leftExpression);
            }

            private readonly Func<Int32, Expression, Expression> _leftDenotation;
        }

        private IEnumerator<Token> _tokens;
        private Dictionary<String, ParameterExpression> _parameters;
        private Token _lookahead;
        private readonly Dictionary<TokenType, NullDenotation> _nullDenotations;
        private readonly Dictionary<TokenType, LeftDenotation> _leftDenotations;
    }
}
