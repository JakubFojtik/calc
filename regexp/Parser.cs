using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static regexp.Strutures;

namespace regexp
{
    public class Parser
    {
        private List<Token> tokens;
        private int curTokIdx;
        private Token curTok => tokens.ElementAtOrDefault(curTokIdx) ?? new Token(TokenType.EOF);

        public bool error { get; private set; }

        public AST parser(List<Token> tokens)
        {
            this.tokens = tokens;
            curTokIdx = 0;
            AST ret = readAll();

            if (curTokIdx != tokens.Count) error = true;
            return ret;
        }

        public class Value
        {
            private decimal val;
            public Value(decimal d) => val = d;
            public static implicit operator Value(decimal d) => new Value(d);
            public static implicit operator decimal(Value d) => d.val;
            public void Negate() => val = -val;
            public override string ToString() => val.ToString();
        }



        //(fr[^r])*|qwe
        //REX->PUM\|PUM | PUM
        //PUM->CH DOPUM
        //DOPUM-> * | e
        //CH->a|ANY|ONE|MANY
        //ANY->.
        //ONE->[REX]|[^REX]
        //MANY->(REX)
        /*
        //LL parser cannot preserve left associativity when removing left recursion, unless iteration is used.
        private AST readSimpleBinaryOperator(Priority priority, Func<AST> nextFun)
        {
            AST ret;
            ret = nextFun();
            var operatorTypes = operators.Values.Where(x => x.Priority == priority).Select(x => x.Operator);
            while (curTok.Type == TokenType.Operator && operatorTypes.Contains((OperatorType)curTok.Value))
            {
                var op = (OperatorType)curTok.Value;
                if (operatorTypes.Contains(op))
                {
                    curTokIdx++;
                    var operate = operatorImpls[op].getAST;
                    var operatorData = operators.Values.First(x => x.Operator == op);
                    if (operatorData.Associativity == Associativity.Left)
                    {
                        ret = operate(ret, nextFun());
                    }
                    else
                    {
                        ret = operate(ret, readSimpleBinaryOperator(priority, nextFun));
                    }
                }
                else
                {
                    // zatim nic vyresit zavorky a ostatni (pridat do gramatiky) a tu hazet chybu parseru
                    break;
                }
            }
            return ret;
        }
        */
        private AST readReg() => readSimpleBinaryOperator(readMul);
        private AST readMul() => readSimpleBinaryOperator(readFun);
        private AST readPow() => readSimpleBinaryOperator(readFactor);

        //sin cos 1^5
        //sin (1)^5 todo braces belong to sin
        //todo max(3,5)
        private AST readFun()
        {
            AST ret = null;
            var operatorTypes = operators.Values.Where(x => x.Priority == Priority.Fun).Select(x => x.Operator);

            if (curTok.Type == TokenType.Operator && operatorTypes.Contains((OperatorType)curTok.Value))
            {
                var op = (OperatorType)curTok.Value;
                curTokIdx++;
                var operate = operatorImpls[op].getAST;
                ret = operate(readFun(), null);
            }
            else
            {
                ret = readPow();
            }
            return ret;
        }

        private AST readFactor()
        {
            AST ret;

            if (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Minus)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, OperatorType.Minus), readMul(), null);
                }
                else if (op == OperatorType.Plus)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, OperatorType.Plus), readMul(), null);
                }
                else throw new ArithmeticException(ERROR);
            }
            else if (curTok.Type == TokenType.Number)
            {
                ret = new AST(curTok);
                curTokIdx++;
            }
            else if (curTok.Type == TokenType.BraceOpen)
            {
                ret = readBrace();
            }
            else throw new ArithmeticException(ERROR);

            return ret;
        }

        private AST readCBrace()
        {
            AST ret;
            curTokIdx++;
            ret = readAdd();
            if (isCurTok(OperatorType.CBraceClose))
            {
                curTokIdx++;
            }
            else throw new ArithmeticException(ERROR);
            return ret;
        }

        private AST readEBrace()
        {
            AST ret;
            curTokIdx++;
            ret = readAdd();
            if (isCurTok(OperatorType.EBraceClose))
            {
                curTokIdx++;
            }
            else throw new ArithmeticException(ERROR);
            return ret;
        }

        private bool isCurTok(OperatorType ot)
        {
            return curTok.Type == TokenType.Operator && ((OperatorType)curTok.Value) == ot;
        }
    }
}
