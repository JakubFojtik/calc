using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace calc
{
    public class Token
    {
        public enum TokenType { BraceOpen, BraceClose, Number, Operator, Constant, EOF }
        public enum OperatorType { None, Plus, Minus, Star, Slash, Sin, ASin, Caret, Sqrt, Pi }

        public TokenType Type { get; set; }
        //Value is OperatorType or decimal
        public object Value { get; set; }

        public Token(TokenType type, object value = null)
        {
            Type = type;
            Value = value;
        }

        //Prints numbers in invariant culture so decimal separator is a dot
        public override string ToString() {
            string stringVal;
            var decVal = Value as decimal?;
            if (decVal.HasValue) stringVal = decVal.Value.ToString(CultureInfo.InvariantCulture);
            else stringVal = Value.ToString();
            return string.Format("{0}({1})", Type, stringVal);
        }
    }
}