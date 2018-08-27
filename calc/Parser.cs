﻿using System;
using System.Collections.Generic;
using System.Linq;
using static calc.OperatorToken;

namespace calc
{
    public class Parser
    {
        public static readonly string ERROR = "Mat chyba";

        public enum Priority { None = 0, Add, Mult, Pow, Fun, Brace } //brace mimo?

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

            if (curTokIdx != tokens.Count) SurplusTokensDetected = true;
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
            OperatorToken opToken;
            while ((opToken = curTok as OperatorToken) != null)
            {
                var op = opToken.Operator;
                if (operatorTypes.Contains(op))
                {
                    curTokIdx++;
                    Func<AST, AST, AST> operate = (a, b) => new AST(new OperatorToken(op), a, b);
                    if (opToken.Associativity == Associativity.Left)
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
                    ret = new AST(opToken, readPow(), null);
                }
                else if (functions.Contains(op))
                {
                    curTokIdx++;
                    Func<AST, AST, AST> operate = (a, b) => new AST(opToken, a, b);
                    ret = operate(readPow(), null);
                }
                else if (op == OperatorType.BraceOpen)
                {
                    ret = readBrace();
                }
                else throw new ArithmeticException(ERROR);
            }
            else if ((numToken = curTok as NumberToken) != null)
            {
                ret = new AST(numToken);
                curTokIdx++;
            }
            else if ((conToken = curTok as ConstantToken) != null)
            {
                ret = new AST(conToken);
                curTokIdx++;
            }
            else throw new ArithmeticException(ERROR);

            return ret;
        }

        private AST readBrace()
        {
            AST ret;
            curTokIdx++;
            ret = readAdd();
            if ((curTok as OperatorToken)?.Operator == OperatorType.BraceClose)
            {
                curTokIdx++;
            }
            else throw new ArithmeticException(ERROR);
            return ret;
        }

    }
}
