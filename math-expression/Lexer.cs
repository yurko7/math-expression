using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YuKu.MathExpression
{
    public sealed class Lexer : IEnumerator<Token>
    {
        public Lexer(TextReader textReader)
        {
            _textReader = textReader;
            _buffer = new StringBuilder();
            _state = ReadToken;
            Advance();
        }

        public Token Current { get; private set; }

        object IEnumerator.Current => Current;

        public Boolean MoveNext()
        {
            if (_state == null)
            {
                return false;
            }

            Token? token;
            do
            {
                _state = _state(out token);
            }
            while (token == null && _state != null);

            Current = token ?? default;
            return token.HasValue;
        }

        public void Reset()
        {
            throw new InvalidOperationException();
        }

        public void Dispose()
        { }

        private LexerState ReadToken(out Token? token)
        {
            switch (_lookahead)
            {
                case var lookahead when Char.IsWhiteSpace(lookahead):
                    token = null;
                    return SkipWhiteSpace;
                case var lookahead when Char.IsDigit(lookahead):
                    token = null;
                    return ReadNumber;
                case var lookahead when Char.IsLetter(lookahead):
                    token = null;
                    return ReadIdentifier;
                case '(':
                    token = new Token(TokenType.LParen, _location);
                    Advance();
                    return ReadToken;
                case ')':
                    token = new Token(TokenType.RParen, _location);
                    Advance();
                    return ReadToken;
                case '+':
                    token = new Token(TokenType.Plus, _location);
                    Advance();
                    return ReadToken;
                case '-':
                    token = new Token(TokenType.Minus, _location);
                    Advance();
                    return ReadToken;
                case '*':
                    token = new Token(TokenType.Multiply, _location);
                    Advance();
                    return ReadToken;
                case '/':
                    token = new Token(TokenType.Divide, _location);
                    Advance();
                    return ReadToken;
                case '%':
                    token = new Token(TokenType.Modulo, _location);
                    Advance();
                    return ReadToken;
                case '^':
                    token = new Token(TokenType.Power, _location);
                    Advance();
                    return ReadToken;
                case EOF:
                    token = new Token(TokenType.EOF, _location);
                    return null;
            }
            throw UnexpectedCharacter();
        }

        private LexerState SkipWhiteSpace(out Token? token)
        {
            token = null;
            while (Char.IsWhiteSpace(_lookahead))
            {
                Advance();
            }
            return ReadToken;
        }

        private LexerState ReadNumber(out Token? token)
        {
            while (Char.IsDigit(_lookahead))
            {
                Consume();
            }

            if (_lookahead == '.')
            {
                token = null;
                return ReadFloat;
            }

            String text = ConsumeBuffer(out Int32 location);
            token = new Token(TokenType.Number, location, text);
            return ReadToken;
        }

        private LexerState ReadFloat(out Token? token)
        {
            Match(c => c == '.');
            while (Char.IsDigit(_lookahead))
            {
                Consume();
            }

            String text = ConsumeBuffer(out Int32 location);
            token = new Token(TokenType.Number, location, text);
            return ReadToken;
        }

        private LexerState ReadIdentifier(out Token? token)
        {
            Match(Char.IsLetter);
            while (Char.IsLetterOrDigit(_lookahead))
            {
                Consume();
            }

            String text = ConsumeBuffer(out Int32 location);
            token = new Token(TokenType.Identifier, location, text);
            return ReadToken;
        }

        private void Match(Func<Char, Boolean> predicate)
        {
            if (!predicate(_lookahead))
            {
                throw UnexpectedCharacter();
            }
            Consume();
        }

        private void Consume()
        {
            if (_lookahead == EOF)
            {
                throw new InvalidOperationException("Consume after EOF");
            }
            _buffer.Append(_lookahead);
            if (_bufferLocation == -1)
            {
                _bufferLocation = _location;
            }
            Advance();
        }

        private void Advance()
        {
            Int32 c = _textReader.Read();
            if (c != -1)
            {
                ++_location;
                _lookahead = (Char) c;
            }
            else if (_lookahead != EOF)
            {
                ++_location;
                _lookahead = EOF;
            }
            else
            {
                throw new InvalidOperationException("Advance after EOF");
            }
        }

        private String PreviewBuffer()
        {
            return _buffer.ToString();
        }

        private String ConsumeBuffer(out Int32 bufferLocation)
        {
            String result = PreviewBuffer();
            bufferLocation = _bufferLocation;
            _buffer.Clear();
            _bufferLocation = -1;
            return result;
        }

        private Exception UnexpectedCharacter()
        {
            throw new InvalidOperationException($"Unexpected character '{_lookahead}' at {_location}.");
        }

        private delegate LexerState LexerState(out Token? token);

        private const Char EOF = '\0';

        private readonly TextReader _textReader;
        private Int32 _location = -1;
        private Char _lookahead;
        private readonly StringBuilder _buffer;
        private Int32 _bufferLocation;
        private LexerState _state;
    }
}
