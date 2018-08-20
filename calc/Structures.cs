﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace calc
{
    public class Strutures
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

        public enum Priority { None = 0, Add, Mult, Fun, Pow, Brace } //brace mimo?

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
            { "sin",  (TokenType.Operator,   OperatorType.Sin ,  Priority.Fun, Associativity.Right  ) },
            { "asin", (TokenType.Operator,   OperatorType.ASin,  Priority.Fun, Associativity.Right  ) },
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

        public static readonly string ERROR = "Mat chyba";

    }
}