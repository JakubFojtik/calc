using System;
using System.Collections.Generic;
using System.Linq;
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
            AST ret = readReg();

            if (curTokIdx != tokens.Count) error = true;
            return ret;
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
        
        private AST readReg() => readSimpleBinaryOperator(readMul);
        private AST readMul() => readSimpleBinaryOperator(readFun);
        private AST readPow() => readSimpleBinaryOperator(readFactor);
        */
        private AST readReg() => readOr();
        //sin cos 1^5
        //sin (1)^5 todo braces belong to sin
        //todo max(3,5)
        private AST readOr()
        {
            AST ret = readStar();
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Or)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, op), ret, readOr());
                }
                else
                {
                    break;
                }
            }
            return ret;
        }
        private AST readStar()
        {
            AST ret = readFactor();
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Star)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, op), ret, null);
                }
                else
                {
                    break;
                }
            }
            return ret;
        }

        private AST readFactor()
        {
            AST ret;

            if (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.CBraceOpen)
                {
                    curTokIdx++;
                    ret = new AST(curTok, readBrace(OperatorType.CBraceClose), null);
                }
                else if (op == OperatorType.EBraceOpen)
                {
                    curTokIdx++;
                    ret = new AST(curTok, readBrace(OperatorType.EBraceClose), null);
                }
                else throw new ArithmeticException(ERROR);
            }
            else if (curTok.Type == TokenType.Char)
            {
                var tmpTok = curTok;
                curTokIdx++;
                ret = new AST(tmpTok, null, readChars());
                /*
                var chars = new List<char>();
                while (curTok.Type == TokenType.Char)
                {
                    chars.Add((char)curTok.Value);
                    curTokIdx++;
                }
                if (chars.Count > 1)
                {
                    //remove last char as it can be target of postfix operators
                    chars.RemoveAt(chars.Count - 1);
                    curTokIdx--;
                }
                ret = new AST(new Token(TokenType.Char, chars));*/
            }
            else ret = null;

            return ret;
        }

        private AST readChars()
        {
            if (curTok.Type != TokenType.Char) return null;
            var tmpTok = curTok;
            curTokIdx++;
            return new AST(tmpTok, null, readChars());
        }

        private AST readBrace(OperatorType closer)
        {
            AST ret;
            curTokIdx++;
            ret = readReg();
            if (isCurTok(closer))
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
