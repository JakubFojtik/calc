using static calc.Parser;

namespace calc
{
    public class OperatorToken : Token
    {
        public enum OperatorType
        {
            BraceOpen, BraceClose, Plus, Minus, Star, Slash, Sin, ASin, Caret, Sqrt
        }

        public OperatorType Operator { get; private set; }

        //int numoperands

        public OperatorToken(OperatorType oper)
        {
            Operator = oper;
        }

        public Associativity Associativity => Operator == OperatorType.Caret ? Associativity.Right : Associativity.Left;

        //public Priority Priority => operatorsByPriority[]

        public override bool HasOperands() => true;

        public override string ToString()
        {
            return string.Format("Operator({0})", Operator);
        }
    }
}
