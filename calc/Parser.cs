using System;
using System.Collections.Generic;
using System.Linq;
using static calc.OperatorToken;

namespace calc
{
    public class Parser
    {
        //Grammar - more nested are operations with higher priority. Starts at ADD
        //ADD -> ADD + MUL | ADD - MUL | MUL
        //MUL -> MUL * POW | MUL / POW | POW
        //POW -> FACTOR ^ POW | FACTOR
        //FACTOR -> -POW | +POW | regex([0-9.]+) | (ADD) | FUN(EXP) | FUN POW
        //Left recursive, but keeps associativity

        public static readonly string ERROR = "Math error";

        //Priority here is only to group similar operands, real priority is determined by parser
        public enum Priority { Add, Mult, Pow, Fun }

        public enum Associativity { Left, Right }

        public ILookup<Priority, OperatorType> operatorsByPriority
            = new (Priority Priority, OperatorType OperatorType)[]
            {
                ( Priority.Add,  OperatorType.Plus  ),
                ( Priority.Add,  OperatorType.Minus ),
                ( Priority.Mult, OperatorType.Mul  ),
                ( Priority.Mult, OperatorType.Div ),
                ( Priority.Fun,  OperatorType.Sin   ),
                ( Priority.Fun,  OperatorType.ASin  ),
                ( Priority.Fun,  OperatorType.Sqr  ),
                ( Priority.Fun,  OperatorType.Sqrt  ),
                ( Priority.Pow,  OperatorType.Pow ),
            }.ToLookup(x => x.Priority, x => x.OperatorType);

        private List<Token> tokens;
        private int curTokIdx;
        //returns null after last token. Does not require fallthrough in recdesc methods, because they just don't capture it (would produce surplus tokens error if there are more)
        private Token curTok => tokens.ElementAtOrDefault(curTokIdx);

        public bool SurplusTokensDetected { get; private set; }

        public AST Parse(List<Token> tokens)
        {
            if (tokens.Count < 1) return null;
            this.tokens = tokens;
            curTokIdx = 0;
            AST ret = readAll();

            if (curTokIdx < tokens.Count) SurplusTokensDetected = true;
            return ret;
        }

        //Theoretical starting point for the whole expression, if ever an op with lower pri than ADD is implemented,
        //e.g. = for equations
        private AST readAll() => readAdd();

        //LeftAssoc - not possible to do 1+1+1, not cloning Op ever again unless in parens
        //Op->Op ? NOp changes to:
        //Op->NOp Rest
        //Rest-> ? NOp Rest | e
        //RightAssoc
        //Op->NOp ? Op changes to (but does not need to):
        //Op->NOp Rest
        //Rest-> ? Op | e
        //Op is current operation (e.g. ADD), ? is operator (+), NOp is operation with next higher priority (MUL)

        //Replacing left recursion with right changes associativity to right as well, solved with iteration.
        //see http://www.allisons.org/ll/ProgLang/Grammar/Top-Down/
        private AST readBinaryOperator(Priority priority, Func<AST> nextFun)
        {
            AST ret;
            ret = nextFun();
            var operatorTypes = operatorsByPriority[priority];
            while (curTok is OperatorToken opToken)
            {
                var op = opToken.Operator;
                if (operatorTypes.Contains(op))
                {
                    var binOp = op;
                    if (priority == Priority.Add) binOp = op == OperatorType.Minus ? OperatorType.BinMinus : OperatorType.BinPlus;
                    AST operate(AST a, AST b) => new AST(new OperatorToken(binOp), a, b);

                    curTokIdx++;
                    if (opToken.Associativity == Associativity.Left)
                    {
                        //operate the whole current AST with the following higher-priority function
                        //relying on the iteration cycle to nest it all into next repetition of current op
                        ret = operate(ret, nextFun());
                    }
                    else
                    {
                        //operate the single current higher-priority function with the rest of the expression using recursive self-call
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

        //This is the real priority distribution, the nextFun has it higher
        //ADD -> ADD + MUL | ADD - MUL | MUL
        private AST readAdd() => readBinaryOperator(Priority.Add, readMul);
        //MUL -> MUL * POW | MUL / POW | POW
        private AST readMul() => readBinaryOperator(Priority.Mult, readPow);
        //POW -> FACTOR ^ POW | FACTOR
        private AST readPow() => readBinaryOperator(Priority.Pow, readFactor);

        //FACTOR -> NUM
        //FACTOR -> -POW
        //FACTOR -> (EXP)
        //FACTOR -> FUN(EXP)
        //FACTOR -> FUN POW
        private AST readFactor()
        {
            switch (curTok)
            {
                case NumberToken numToken: //Includes constants
                    curTokIdx++;
                    return new AST(numToken);
                case OperatorToken opToken:
                    var functions = operatorsByPriority[Priority.Fun];

                    var op = opToken.Operator;
                    switch (op)
                    {
                        //Maybe should have lower priority since it has to call Pow instead of Factor or higher pri
                        //-1^2 should be -1 since POW has higher priority, so should MUL but google has it otherwise
                        //per google:
                        //sin 2 * 3 = (sin  2) * 3
                        //sin 2 ^ 3 =  sin (2  ^ 3)
                        //-2^-3^-4 ... unaryMinus should have both lower and higher priority than pow
                        //Lower is proposed for being a simpler op, higher because pow only calls higher ops
                        //ReadUnaryAdd after readAdd? Complicated, it's the only unary, and would hardly work
                        case OperatorType.Minus:
                        case OperatorType.Plus:
                            curTokIdx++;
                            var unaryType = op == OperatorType.Minus ? OperatorType.UnMinus : OperatorType.UnPlus;
                            return new AST(new OperatorToken(unaryType), readPow(), null);
                        case OperatorType.BraceOpen:
                            curTokIdx++;
                            AST ret = readAll();
                            if (isCurTok(OperatorType.BraceClose))
                            {
                                curTokIdx++;
                            }
                            else throw new ArithmeticException(ERROR);
                            return ret;
                        case var _ when functions.Contains(op):
                            curTokIdx++;
                            //Peek 1 token, if brace, then it is owned by the function, per google calc
                            //So Fun (Exp) Op Exp is handled as (Fun (Exp)) Op Exp instead of respecting Op's priority
                            if (curTok is OperatorToken nextTok && nextTok.Operator == OperatorType.BraceOpen)
                            {
                                return new AST(opToken, readFactor(), null);
                            }
                            else
                            {
                                return new AST(opToken, readPow(), null);
                            }
                        default:
                            throw new ArithmeticException(ERROR);
                    }
                default:
                    throw new ArithmeticException(ERROR);
            }
        }

        private bool isCurTok(OperatorType op)
        {
            return (curTok as OperatorToken)?.Operator == op;
        }
    }
}
