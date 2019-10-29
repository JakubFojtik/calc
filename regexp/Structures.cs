namespace regexp
{
    public class Strutures
    {
        public enum TokenType { Char, Operator, Empty, EOF }
        public enum OperatorType
        {
            Dot,
            Star,
            //Caret,
            //Dollar,
            Pipe,
            CBraceOpen,
            CBraceClose,
            EBraceOpen,
            EBraceClose,
            Concat,
            Escape
        }

        public class Token
        {
            public TokenType Type { get; set; }
            public object Value { get; set; }

            public Token(TokenType type, object value = null)
            {
                Type = type;
                Value = value;
            }

            public override string ToString() => $"{Type}({Value})";
        }

        public static readonly string ERROR = "Math error";

    }
}