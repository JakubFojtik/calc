using System.Globalization;

namespace calc
{
    public class NumberToken : Token
    {
        public decimal Value { get; private set; }

        public NumberToken(decimal value)
        {
            Value = value;
        }

        public override bool HasOperands() => false;

        //Prints numbers in invariant culture so decimal separator is a dot
        public override string ToString()
        {
            return string.Format("Number({0})", Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
