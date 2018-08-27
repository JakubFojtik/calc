using static calc.Parser;

namespace calc
{
    public class OperatorToken : Token
    {
        public enum OperatorType
        {
            None, BraceOpen, BraceClose, Plus, Minus, Star, Slash, Sin, ASin, Caret, Sqrt, Pi
        }

        public OperatorType Operator { get; private set; }

        //int numoperands

        public OperatorToken(OperatorType oper)
        {
            Operator = oper;
        }

        public Associativity Associativity => Operator == OperatorType.Caret ? Associativity.Right : Associativity.Left;

        //public Priority Priority => operatorsByPriority[]

        public override bool HasOperands() => Operator != OperatorType.Pi; //todo gener

        public override string ToString()
        {
            return string.Format("Operator({0})", Operator);
        }
    }
}
