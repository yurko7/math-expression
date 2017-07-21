using System;

namespace YuKu.MathExpression
{
    public struct Token
    {
        public Token(TokenType type, Int32 location, String text = null)
        {
            Type = type;
            Location = location;
            Text = text;
        }

        public TokenType Type { get; }

        public Int32 Location { get; }

        public String Text { get; }

        public override String ToString()
        {
            return Text == null
                ? $"{Type} ({Location})"
                : $"{Type}: \"{Text}\" ({Location})";
        }
    }
}
