using static calc.Parser;

namespace calc
{
    public class OperatorToken : Token
    {
        public enum OperatorType
        {
            BraceOpen, BraceClose, Plus, Minus, Mul, Div, Sin, ASin, Pow, Sqr, Sqrt, UnPlus, UnMinus, BinPlus, BinMinus
        }

        public OperatorType Operator { get; private set; }

        //int numoperands

        public OperatorToken(OperatorType oper)
        {
            Operator = oper;
        }

        public Associativity Associativity => Operator == OperatorType.Pow ? Associativity.Right : Associativity.Left;

        //public Priority Priority => operatorsByPriority[]

        public override int NumOperands() => isUnary() ? 1 : 2;

        /// <summary>
        /// do this better, autodetect and merge with AST descendants/children
        /// </summary>
        /// <returns></returns>
        public bool isUnary()
        {
            return Operator == OperatorType.UnMinus || Operator == OperatorType.UnPlus
                || Operator == OperatorType.Sin || Operator == OperatorType.ASin
                || Operator == OperatorType.Sqr || Operator == OperatorType.Sqrt;
        }

        public override string ToString()
        {
            return string.Format("Operator({0})", Operator);
        }
    }
}
