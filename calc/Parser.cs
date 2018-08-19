using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static calc.Strutures;

namespace calc
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


        //Gramatika - nejvic zanorene jsou nejvyssi priority, vsechny pravidla ukousnou 1 a zbytek je vse dalsi. Start: SCIT
        //SCIT -> SCIT + NAS | NAS
        //NAS -> NAS * MOC | MOC
        //MOC -> CISLO ^ MOC | CISLO
        //CISLO -> cislo | (SCIT)
        //odstr leve rekurze SCIT->NAS ?SCIT, ?SCIT->+ SCIT ?SCIT|eps
        //funkce? FUN -> ONEFUN FUN | MOC, ONEFUN -> sin | asin
        private AST readAll() => readAdd();

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

        private AST readAdd() => readSimpleBinaryOperator(Priority.Add, readMul);
        private AST readMul() => readSimpleBinaryOperator(Priority.Mult, readFun);
        private AST readPow() => readSimpleBinaryOperator(Priority.Pow, readFactor);

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

        //Fac->-+Mul | Cislo | (Add)
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

            /*
            bool signIsPlus = true;
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Minus) signIsPlus ^= true;
                else if (op == OperatorType.Plus) { }
                else break;
                curTokIdx++;
            }

            if (signIsPlus) return readMul();
            else
            {
                return new AST(new Token(TokenType.Operator, OperatorType.Minus), readMul(), null);
            }
            */
        }

        private AST readBrace()
        {
            AST ret;
            curTokIdx++;
            ret = readAdd();
            if (curTok.Type == TokenType.BraceClose)
            {
                curTokIdx++;
            }
            else throw new ArithmeticException(ERROR);
            return ret;
        }

    }
}
