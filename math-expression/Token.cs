using System;

namespace YuKu.MathExpression
{
    public struct Token : IEquatable<Token>
    {
        public Token(TokenType type, Int32 location, String text = null)
        {
            Type = type;
            Location = location;
            Text = text;
        }

        public static Boolean operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }

        public TokenType Type { get; }

        public Int32 Location { get; }

        public String Text { get; }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            return obj is Token token && Equals(token);
        }

        public Boolean Equals(Token other)
        {
            return Type == other.Type
                && Location == other.Location
                && String.Equals(Text, other.Text, StringComparison.Ordinal);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                var hashCode = (Int32) Type;
                hashCode = (hashCode * 397) ^ Location;
                hashCode = (hashCode * 397) ^ (Text != null ? StringComparer.Ordinal.GetHashCode(Text) : 0);
                return hashCode;
            }
        }

        public override String ToString()
        {
            return Text == null
                ? $"{Type} ({Location})"
                : $"{Type}: \"{Text}\" ({Location})";
        }
    }
}
