using System;
using System.Collections.Generic;
using System.Globalization;
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
            RegisterToken(0, ParseParameter, TokenType.Parameter);
            RegisterToken(1, ParseAdd, TokenType.Plus);
            RegisterToken(1, ParseSubtract, TokenType.Minus);
            RegisterToken(2, ParseMultiply, TokenType.Multiply);
            RegisterToken(2, ParseImpliedMultiply, TokenType.LParen, TokenType.Number, TokenType.Parameter, TokenType.Identifier);
            RegisterToken(2, ParseDivide, TokenType.Divide);
            RegisterToken(3, ParsePower, TokenType.Power);
            RegisterToken(4, ParseNegate, TokenType.Minus);
            RegisterToken(5, ParseCall, TokenType.Identifier);
        }

        public Expression Parse(IEnumerable<Token> tokens, Expression parameter)
        {
            try
            {
                _tokens = tokens.GetEnumerator();
                _parameter = parameter;
                Advance();
                return ParseExpression(0);
            }
            finally
            {
                _tokens.Dispose();
                _tokens = null;
                _parameter = null;
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

        private Expression ParseExpression(Int32 rightBindingPower)
        {
            NullDenotation nullDenotation = _nullDenotations[_lookahead.Type];
            Expression expression = nullDenotation.Invoke();

            LeftDenotation leftDenotation = _leftDenotations[_lookahead.Type];
            while (leftDenotation != null && leftDenotation.LeftBindingPower > rightBindingPower)
            {
                expression = leftDenotation.Invoke(expression);
                leftDenotation = _leftDenotations[_lookahead.Type];
            }

            return expression;
        }

        private Expression ParseGroup(Int32 rightBindingPower)
        {
            Match(TokenType.LParen);
            Expression expression = ParseExpression(rightBindingPower);
            Match(TokenType.RParen);

            return expression;
        }

        private Expression ParseNumber(Int32 rightBindingPower)
        {
            Token token = Match(TokenType.Number);
            Double value = Double.Parse(token.Text, CultureInfo.InvariantCulture);
            return Expression.Constant(value);
        }

        private Expression ParseNegate(Int32 rightBindingPower)
        {
            Match(TokenType.Minus);
            Expression expression = ParseExpression(rightBindingPower);
            return Expression.Negate(expression);
        }

        private Expression ParseParameter(Int32 rightBindingPower)
        {
            Match(TokenType.Parameter);
            return _parameter;
        }

        private Expression ParseAdd(Int32 rightBindingPower, Expression leftExpression)
        {
            Match(TokenType.Plus);
            Expression rightExpression = ParseExpression(rightBindingPower);
            return Expression.Add(leftExpression, rightExpression);
        }

        private Expression ParseSubtract(Int32 rightBindingPower, Expression leftExpression)
        {
            Match(TokenType.Minus);
            Expression rightExpression = ParseExpression(rightBindingPower);
            return Expression.Subtract(leftExpression, rightExpression);
        }

        private Expression ParseMultiply(Int32 rightBindingPower, Expression leftExpression)
        {
            Match(TokenType.Multiply);
            Expression rightExpression = ParseExpression(rightBindingPower);
            return Expression.Multiply(leftExpression, rightExpression);
        }

        private Expression ParseImpliedMultiply(Int32 rightBindingPower, Expression leftExpression)
        {
            Expression rightExpression = ParseExpression(rightBindingPower);
            return Expression.Multiply(leftExpression, rightExpression);
        }

        private Expression ParseDivide(Int32 rightBindingPower, Expression leftExpression)
        {
            Match(TokenType.Divide);
            Expression rightExpression = ParseExpression(rightBindingPower);
            return Expression.Divide(leftExpression, rightExpression);
        }

        private Expression ParsePower(Int32 rightBindingPower, Expression leftExpression)
        {
            Match(TokenType.Power);
            Expression rightExpression = ParseExpression(rightBindingPower - 1);
            return Expression.Power(leftExpression, rightExpression);
        }

        private Expression ParseCall(Int32 rightBindingPower)
        {
            Token identifier = Match(TokenType.Identifier);
            if (String.Equals(identifier.Text, "PI", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(identifier.Text, "E", StringComparison.OrdinalIgnoreCase))
            {
                FieldInfo field = typeof(Math).GetField(
                    identifier.Text,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
                return Expression.Field(null, field);
            }

            MethodInfo method = typeof(Math).GetMethod(
                identifier.Text,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
                null, new[] {typeof(Double)}, null);
            Expression argExpression = ParseExpression(rightBindingPower);
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
        private Expression _parameter;
        private Token _lookahead;
        private readonly Dictionary<TokenType, NullDenotation> _nullDenotations;
        private readonly Dictionary<TokenType, LeftDenotation> _leftDenotations;
    }
}
