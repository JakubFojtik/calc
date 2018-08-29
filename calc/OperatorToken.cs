using static calc.Parser;

namespace calc
{
    public class OperatorToken : Token
    {
        public enum OperatorType
        {
            BraceOpen, BraceClose, Plus, Minus, Star, Slash, Sin, ASin, Caret, Sqrt, UnPlus, UnMinus, BinPlus, BinMinus
        }

        public OperatorType Operator { get; private set; }

        //int numoperands

        public OperatorToken(OperatorType oper)
        {
            Operator = oper;
        }

        public Associativity Associativity => Operator == OperatorType.Caret ? Associativity.Right : Associativity.Left;

        //public Priority Priority => operatorsByPriority[]

        public override int NumOperands() => isUnary() ? 1 : 2;

        public bool isUnary()
        {
            return Operator == OperatorType.UnMinus || Operator == OperatorType.UnPlus;
        }

        public override string ToString()
        {
            return string.Format("Operator({0})", Operator);
        }
    }
}
