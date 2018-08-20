using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace regexp
{
    public class Strutures
    {
        public enum TokenType { BraceOpen, BraceClose, Char, Operator, EOF }
        public enum OperatorType
        {
            None,
            Star,
            Caret,
            Dot,
            Dollar,
            Or,
            CBraceOpen,
            CBraceClose,
            EBraceOpen,
            EBraceClose
        }

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
        /*
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
                var removed = new HashSet<int>();
                foreach (var item in map.OrderBy(x => x.Key))
                {
                    if (removed.Contains(item.Key)) continue;
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
        */

        public static Dictionary<string, (TokenType Token, OperatorType Operator)> operators
            = new Dictionary<string, (TokenType, OperatorType)>
        {
            { "(",    (TokenType.BraceOpen,  OperatorType.CBraceOpen   ) },
            { ")",    (TokenType.BraceClose, OperatorType.CBraceClose   ) },
            { "[",    (TokenType.BraceOpen,  OperatorType.EBraceOpen   ) },
            { "]",    (TokenType.BraceClose, OperatorType.EBraceClose   ) },
            { "*",    (TokenType.Operator,   OperatorType.Star   ) },
            { ".",    (TokenType.Operator,   OperatorType.Dot    ) },
            { "^",    (TokenType.Operator,   OperatorType.Caret  ) },
            { "$",    (TokenType.Operator,   OperatorType.Dollar ) },
            { "|",    (TokenType.Operator,   OperatorType.Or ) },
        };
        /*
        public static Dictionary<OperatorType, (Func<AST, AST, AST> getAST, Func<decimal, decimal, decimal> getDecimal)> operatorImpls
            = new Dictionary<OperatorType, (Func<AST, AST, AST>, Func<decimal, decimal, decimal>)>
        {
            { OperatorType.Star,  ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Star ), a, b), (a, b) => a * b) },
            { OperatorType.Caret, ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Caret), a, b), (a, b) => (decimal)Math.Pow((double)a, (double)b)) },
        };
        */
        public static readonly string ERROR = "Mat chyba";

    }
}