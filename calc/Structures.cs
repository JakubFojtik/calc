using System;
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
                            case OperatorType.Plus:
                                if (second != null) return defaultBinary(first, second);
                                else return first.Value;
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

            public string printDeriv()
            {
                var map = new Dictionary<int, AST>();
                fillMap(this, map);
                StringBuilder ret = new StringBuilder();
                Func<AST, string> writeItem = item =>
                 {
                     if (item != null)
                     {
                         var val = item.value;
                         if (val.Type == TokenType.Number) return val.ToString();
                         else return map.First(x => x.Value.value == val).Key.ToString();
                     }
                     else return "";
                 };
                foreach (var item in map.OrderBy(x => x.Key))
                {
                    string first = writeItem(item.Value.left);
                    string second = writeItem(item.Value.right);
                    if (item.Value.value.Type != TokenType.Number)
                    {
                        ret.AppendLine(string.Format("{0}: {1} -> {2}, {3}", item.Key, item.Value.value, first, second));
                    }
                }
                return ret.ToString();
            }

            private void fillMap(AST ast, Dictionary<int, AST> map)
            {
                int num = map.Count;
                map.Add(num, ast);
                if (ast.left != null) fillMap(ast.left, map);
                if (ast.right != null) fillMap(ast.right, map);
            }

            public string printDFS(AST ast)
            {
                var str = ast.value + " ";
                if (ast.left != null) str += printDFS(ast.left);
                if (ast.right != null) str += printDFS(ast.right);
                return str;
            }

            public void printBFS(AST ast)
            {
                Queue<AST> q = new Queue<AST>();
                q.Enqueue(ast);
                int numItems = 1;
                while (q.Count > 0)
                {
                    AST item = q.Dequeue();
                    Console.Write(item.value + " ");
                    numItems--;
                    AST nextItem = item.left;
                    if (nextItem != null)
                    {
                        q.Enqueue(nextItem);
                        numItems++;
                    }
                    nextItem = item.right;
                    if (nextItem != null)
                    {
                        q.Enqueue(nextItem);
                        numItems++;
                    }
                }
            }

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