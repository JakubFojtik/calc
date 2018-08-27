using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace calc
{
    public class Strutures
    {
        public enum TokenType { BraceOpen, BraceClose, Number, Operator, Constant, EOF }
        public enum OperatorType { None, Plus, Minus, Star, Slash, Sin, ASin, Caret,
            Sqrt,
            Pi
        }

        public class Token
        {
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

        public enum Priority { None = 0, Add, Mult, Pow, Fun, Brace } //brace mimo?

        public enum Associativity { Left, Right }

        public static Dictionary<string, (TokenType Token, OperatorType Operator)> operators
            = new Dictionary<string, (TokenType, OperatorType)>
        {
            { "(",    (TokenType.BraceOpen,  OperatorType.None) },
            { ")",    (TokenType.BraceClose, OperatorType.None) },
            { "+",    (TokenType.Operator,   OperatorType.Plus) },
            { "-",    (TokenType.Operator,   OperatorType.Minus) },
            { "*",    (TokenType.Operator,   OperatorType.Star) },
            { "/",    (TokenType.Operator,   OperatorType.Slash) },
            { "^",    (TokenType.Operator,   OperatorType.Caret) },
            { "sin",  (TokenType.Operator,   OperatorType.Sin ) },
            { "asin", (TokenType.Operator,   OperatorType.ASin) },
            { "sqrt", (TokenType.Operator,   OperatorType.Sqrt) },
            { "pi",   (TokenType.Constant,   OperatorType.Pi) },//do number.origval
        };
        
          
        public static Dictionary<OperatorType, (Func<decimal, decimal, decimal> getDecimal, Priority Priority, Associativity Associativity)> operatorImpls
            = new Dictionary<OperatorType, (Func<decimal, decimal, decimal>, Priority, Associativity)>
        {
            { OperatorType.Plus,   ((a, b) => a + b                                    ,Priority.Add,  Associativity.Left       )   },
            { OperatorType.Minus,  ((a, b) => a - b                                    ,Priority.Add,  Associativity.Left       )   },
            { OperatorType.Star,   ((a, b) => a * b                                    ,Priority.Mult, Associativity.Left       )   },
            { OperatorType.Slash,  ((a, b) => a / b                                    ,Priority.Mult, Associativity.Left       )   },
            { OperatorType.Caret,  ((a, b) => (decimal)Math.Pow((double)a, (double)b)  ,Priority.Pow,  Associativity.Right      )   },
            { OperatorType.Sin,    ((a, b) => (decimal)Math.Sin((double)a)             ,Priority.Fun,  Associativity.Right      )   },
            { OperatorType.ASin,   ((a, b) => (decimal)Math.Asin((double)a)            ,Priority.Fun,  Associativity.Right      )   },
            { OperatorType.Sqrt,   ((a, b) => (decimal)Math.Sqrt((double)a)            ,Priority.Fun,  Associativity.Right      )   },
            { OperatorType.Pi,     ((a, b) => (decimal)Math.PI                         ,Priority.Fun,  Associativity.Left       )   },
        };

        public static readonly string ERROR = "Mat chyba";

    }
}