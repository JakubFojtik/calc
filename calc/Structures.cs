﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace calc
{
    class Strutures
    {
        public enum TokenType { BraceOpen, BraceClose, Number, Operator, EOF }
        public enum OperatorType { None, Plus, Minus, Star, Slash, Sin, ASin, Caret }

        public class Token
        {
            public TokenType Type { get; set; }
            public object Value { get; set; }

            public Token(TokenType type, object value = null)
            {
                Type = type;
                Value = value;
            }

            public override string ToString() => string.Format("{0}({1})", Type, Value);
        }

        public class ASTToken : Token
        {
            public AST Ast { get; set; }

            public ASTToken(AST ast) : base(TokenType.Number)   //TokenType.Operator?
            {
                Ast = ast;
            }

            public override string ToString() => "AST";
        }

        public class AST
        {
            public Token value;
            public AST left;
            public AST right;

            public AST(Token v, AST l, AST r)
            {
                value = v;
                left = l;
                right = r;
            }
            public AST(Token v) : this(v, null, null) { }

            public override string ToString() => value.Value.ToString();

            public decimal compute()
            {
                var first = left?.compute();
                var second = right?.compute();
                switch (value.Type)
                {
                    case TokenType.Number:
                        return (decimal)value.Value;    //todo convert or generic type
                    case TokenType.Operator:
                        var opType = (OperatorType)value.Value;
                        decimal defaultBinary(decimal? left, decimal? right) => operatorImpls[opType].getDecimal(first.Value, second.Value);
                        switch (opType)
                        {
                            default: //binary op
                                return defaultBinary(first, second);
                            case OperatorType.Minus:
                                if (second != null) return defaultBinary(first, second);
                                else return -1 * first.Value;
                            case OperatorType.Sin:
                                return (decimal)Math.Sin((double)first.Value);
                            case OperatorType.ASin:
                                return (decimal)Math.Asin((double)first.Value);
                        }
                    default:
                        throw new ArithmeticException(ERROR);
                }
            }
        }

        public enum Priority { None = 0, Add, Mult, Unary, Pow, Brace } //brace mimo?

        public enum Associativity { Left, Right }

        public static Dictionary<string, (TokenType Token, OperatorType Operator, Priority Priority, Associativity Associativity)> operators
            = new Dictionary<string, (TokenType, OperatorType, Priority, Associativity)>
        {
            { "(",    (TokenType.BraceOpen,  OperatorType.None,  Priority.Brace, Associativity.Right ) },
            { ")",    (TokenType.BraceClose, OperatorType.None,  Priority.Brace, Associativity.Left  ) },
            { "+",    (TokenType.Operator,   OperatorType.Plus,  Priority.Add,   Associativity.Left  ) },
            { "-",    (TokenType.Operator,   OperatorType.Minus, Priority.Add,   Associativity.Left  ) }, //dynamic
            { "*",    (TokenType.Operator,   OperatorType.Star,  Priority.Mult,  Associativity.Left  ) },
            { "/",    (TokenType.Operator,   OperatorType.Slash, Priority.Mult,  Associativity.Left  ) },
            { "^",    (TokenType.Operator,   OperatorType.Caret, Priority.Pow,   Associativity.Right ) },
            { "sin",  (TokenType.Operator,   OperatorType.Sin ,  Priority.Unary, Associativity.Right  ) },
            { "asin", (TokenType.Operator,   OperatorType.ASin,  Priority.Unary, Associativity.Right  ) },
        };
        public static Dictionary<OperatorType, (Func<AST, AST, AST> getAST, Func<decimal, decimal, decimal> getDecimal)> operatorImpls
            = new Dictionary<OperatorType, (Func<AST, AST, AST>, Func<decimal, decimal, decimal>)>
        {
            { OperatorType.Plus,  ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Plus ), a, b), (a, b) => a + b) },
            { OperatorType.Minus, ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Minus), a, b), (a, b) => a - b) },
            { OperatorType.Star,  ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Star ), a, b), (a, b) => a * b) },
            { OperatorType.Slash, ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Slash), a, b), (a, b) => a / b) },
            { OperatorType.Caret, ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Caret), a, b), (a, b) => (decimal)Math.Pow((double)a, (double)b)) },
            { OperatorType.Sin,   ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Sin), a, b), (a, b) => (decimal)Math.Sin((double)a)) },
        };
        //Prefix enumeration of all operators.
        public static string[] _operatorPrefixes;
        public static readonly string ERROR = "Mat chyba";

        public static string[] operatorPrefixes
        {
            get
            {
                IEnumerable<int> prefixLengths(string str) => Enumerable.Range(1, str.Length);
                IEnumerable<string> substrings(string str, IEnumerable<int> lengths) => lengths.Select(i => str.Substring(0, i));

                return _operatorPrefixes
                    ?? (_operatorPrefixes = operators.Keys.SelectMany(x => substrings(x, prefixLengths(x))).ToArray());
            }
        }

        public static char[] decSeps = { ',', '.' };

    }
}