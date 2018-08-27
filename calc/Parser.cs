using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static calc.Token;

namespace calc
{
    public class Parser
    {
        public readonly string ERROR = "Mat chyba";

        public enum Priority { None = 0, Add, Mult, Pow, Fun, Brace } //brace mimo?

        public enum Associativity { Left, Right }

        public Associativity operatorAssoc(OperatorType op) => op == OperatorType.Caret ? Associativity.Right : Associativity.Left;

        public ILookup<Priority, OperatorType> operatorsByPriority
            = new (Priority Priority, OperatorType OperatorType)[]
            {
                ( Priority.Add,  OperatorType.Plus  ),
                ( Priority.Add,  OperatorType.Minus ),
                ( Priority.Mult, OperatorType.Star  ),
                ( Priority.Mult, OperatorType.Slash ),
                ( Priority.Pow,  OperatorType.Caret ),
                ( Priority.Fun,  OperatorType.Sin   ),
                ( Priority.Fun,  OperatorType.ASin  ),
                ( Priority.Fun,  OperatorType.Sqrt  ),
                ( Priority.Fun,  OperatorType.Pi    ),
            }.ToLookup(x => x.Priority, x => x.OperatorType);

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
        //NAS -> NAS * FUN | FUN
        //FUN -> Fun FUN | MOC
        //MOC -> FUNB ^ MOC | FUNB
        //FUNB -> Fun(SCIT) | CISLO
        //CISLO -> -NAS | cislo | (SCIT)
        //odstr leve rekurze SCIT->NAS ?SCIT, ?SCIT->+ SCIT ?SCIT|eps posere levou asociativitu => list iteration
        private AST readAll() => readAdd();

        //LeftAssoc
        //???
        //RightAssoc
        //Op->NOp Rest
        //Rest-> ? Op | e

        //LL parser cannot preserve left associativity when removing left recursion, unless iteration is used.
        private AST readSimpleBinaryOperator(Priority priority, Func<AST> nextFun)
        {
            AST ret;
            ret = nextFun();
            var operatorTypes = operatorsByPriority[priority];
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (operatorTypes.Contains(op))
                {
                    curTokIdx++;
                    Func<AST, AST, AST> operate = (a, b) => new AST(new Token(TokenType.Operator, op), a, b);
                    if (operatorAssoc(op) == Associativity.Left)
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
                    break;
                }
            }
            return ret;
        }

        private AST readAdd() => readSimpleBinaryOperator(Priority.Add, readMul);
        private AST readMul() => readSimpleBinaryOperator(Priority.Mult, readPow);
        private AST readPow() => readSimpleBinaryOperator(Priority.Pow, readFactor);

        private AST readFactor()
        {
            AST ret;

            if (curTok.Type == TokenType.Operator)  //cist vsechny unarni opy, i fce
            {
                var functions = operatorsByPriority[Priority.Fun];

                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Minus)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, OperatorType.Minus), readPow(), null);
                }
                else if (op == OperatorType.Plus)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, OperatorType.Plus), readPow(), null);
                }
                else if (functions.Contains(op))
                {
                    curTokIdx++;
                    Func<AST, AST, AST> operate = (a, b) => new AST(new Token(TokenType.Operator, op), a, b);
                    ret = operate(readPow(), null);
                }
                else throw new ArithmeticException(ERROR);
            }
            else if (curTok.Type == TokenType.Number)
            {
                ret = new AST(curTok);
                curTokIdx++;
            }
            else if (curTok.Type == TokenType.Constant)
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
