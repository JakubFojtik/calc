using System;
using System.Collections.Generic;
using System.Linq;
using static calc.OperatorToken;

namespace calc
{
    public class Parser
    {
        public static readonly string ERROR = "Mat chyba";

        //Priority is only to group similar operands, real priority is determined by parser
        public enum Priority { Add, Mult, Pow, Fun }

        public enum Associativity { Left, Right }

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
            }.ToLookup(x => x.Priority, x => x.OperatorType);

        private List<Token> tokens;
        private int curTokIdx;
        //returns null after last token. Does not require fallthrough in recdesc methods, because they just don't capture it (would produce surplus tokens error if there are more)
        private Token curTok => tokens.ElementAtOrDefault(curTokIdx);

        public bool SurplusTokensDetected { get; private set; }

        public AST parser(List<Token> tokens)
        {
            if (tokens.Count < 1) return null;
            this.tokens = tokens;
            curTokIdx = 0;
            AST ret = readAll();

            if (curTokIdx < tokens.Count) SurplusTokensDetected = true;
            return ret;
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
        //Op->NOp Rest
        //Rest-> ? NOp Rest | e
        //RightAssoc
        //Op->NOp Rest
        //Rest-> ? Op | e
        //Op=>NOp Rest=>NOp ? Op=>NOp ? NOp Rest=>NOp ? NOp ? NOp

        //LL parser cannot preserve left associativity when removing left recursion, unless iteration is used.
        private AST readBinaryOperator(Priority priority, Func<AST> nextFun)
        {
            AST ret;
            ret = nextFun();
            var operatorTypes = operatorsByPriority[priority];
            OperatorToken opToken;
            while ((opToken = curTok as OperatorToken) != null)
            {
                var op = opToken.Operator;
                var newOp = op;
                if (priority == Priority.Add) newOp = op == OperatorType.Minus ? OperatorType.BinMinus : OperatorType.BinPlus;
                if (operatorTypes.Contains(op))
                {
                    curTokIdx++;
                    Func<AST, AST, AST> operate = (a, b) => new AST(new OperatorToken(newOp), a, b);
                    if (opToken.Associativity == Associativity.Left)
                    {
                        ret = operate(ret, nextFun());
                    }
                    else
                    {
                        ret = operate(ret, readBinaryOperator(priority, nextFun));
                    }
                }
                else
                {
                    break;
                }
            }
            return ret;
        }

        private AST readAdd() => readBinaryOperator(Priority.Add, readMul);
        private AST readMul() => readBinaryOperator(Priority.Mult, readPow);
        private AST readPow() => readBinaryOperator(Priority.Pow, readFactor);

        private AST readFactor()
        {
            AST ret;
            OperatorToken opToken;
            NumberToken numToken;
            ConstantToken conToken;
            if ((opToken = curTok as OperatorToken) != null)
            {
                var functions = operatorsByPriority[Priority.Fun];

                var op = opToken.Operator;
                if (op == OperatorType.Minus || op == OperatorType.Plus)
                {
                    curTokIdx++;
                    var newOp = op == OperatorType.Minus ? OperatorType.UnMinus : OperatorType.UnPlus;
                    ret = new AST(new OperatorToken(newOp), readPow(), null);
                }
                else if (functions.Contains(op))
                {
                    var fun = opToken;
                    curTokIdx++;
                    if ((opToken = curTok as OperatorToken) != null && opToken.Operator == OperatorType.BraceOpen)
                    {
                        ret = new AST(fun, readFactor(), null);
                    }
                    else
                    {
                        ret = new AST(fun, readPow(), null);
                    }
                }
                else if (op == OperatorType.BraceOpen)
                {
                    ret = readBrace();
                }
                else throw new ArithmeticException(ERROR);
            }
            else if ((numToken = curTok as NumberToken) != null)
            {
                curTokIdx++;
                ret = new AST(numToken);
            }
            else if ((conToken = curTok as ConstantToken) != null)
            {
                curTokIdx++;
                ret = new AST(conToken);
            }
            else throw new ArithmeticException(ERROR);

            return ret;
        }

        private AST readBrace()
        {
            AST ret;
            curTokIdx++;
            ret = readAll();
            if (isCurTok(OperatorType.BraceClose))
            {
                curTokIdx++;
            }
            else throw new ArithmeticException(ERROR);
            return ret;
        }

        private bool isCurTok(OperatorType op)
        {
            return (curTok as OperatorToken)?.Operator == op;
        }
    }
}
