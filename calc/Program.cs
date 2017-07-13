using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace calc
{
    class Program
    {
        static char[] decSeps = { ',', '.' };

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Priklad: ");
                string input = Console.ReadLine();

                List<Token> tokens = lexer(input);
                Console.WriteLine(string.Join(", ", tokens));

                Console.WriteLine();
                AST ast = parser(tokens);
                printAST(ast);
            }
        }

        private static void printAST(AST ast)
        {
            Queue<AST> q = new Queue<AST>();
            q.Enqueue(ast);
            int numItems = 1;
            while (q.Count > 0)
            {
                AST item = q.Dequeue();
                Console.Write(item.value + " ");
                numItems--;
                if (numItems == 0) Console.WriteLine();
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

        private static List<Token> tokens;

        private static void recWork(int start)
        {
            int end = start + 1;
            while (!canReduce(start, end))
            {

            }
        }

        private static bool canReduce(int start, int end)
        {
            throw new NotImplementedException();
        }

        private static AST parser(List<Token> tokens)
        {
            Program.tokens = tokens;
            /*
            parse(all)
                while(cantstop) read
                ast token v tokenech
                na zaac zav unarni +- bez priority
                co s sin 5 + 2
                recwork(sezer mi 1 argument jsem sinus)
            */

            return new AST(new Token(TokenType.Operator, OperatorType.Plus),
                new AST(new Token(TokenType.Number, 5)),
                new AST(new Token(TokenType.Number, -1)));
        }

        static bool isNumeric(string buffer)
        {
            return !string.IsNullOrWhiteSpace(buffer) && !buffer.Select(x => char.IsDigit(x) || decSeps.Contains(x)).Contains(false);
            //     && buffer.Count(x => decSeps.Contains(x)) <= 1;   aby ... nebylo 0 0 0 tak je tecka "cislici"-soucasti cisla
        }

        private static bool isOperatorPrefix(string buffer)
        {
            return !string.IsNullOrWhiteSpace(buffer) && operatorPrefixes.Contains(buffer);
        }

        enum Priority { None = 0, Unary, Mult, Add, Brace }

        class OperatorData : Tuple<TokenType, object, Priority>
        {
            public OperatorData(TokenType item1, object item2, Priority item3) : base(item1, item2, item3) { }
        }

        static Dictionary<string, OperatorData> operators = new Dictionary<string, OperatorData>
        {
            { "(",    new OperatorData(TokenType.BraceOpen, null , Priority.Brace)},
            { ")",    new OperatorData(TokenType.BraceClose, null, Priority.Brace)},
            { "+",    new OperatorData(TokenType.Operator, OperatorType.Plus ,Priority.Add)},
            { "-",    new OperatorData(TokenType.Operator, OperatorType.Minus,Priority.Add)},
            { "*",    new OperatorData(TokenType.Operator, OperatorType.Star ,Priority.Mult)},
            { "/",    new OperatorData(TokenType.Operator, OperatorType.Slash,Priority.Mult)},
            { "sin",  new OperatorData(TokenType.Operator, OperatorType.Sin  ,Priority.Unary)},
            { "sinh", new OperatorData(TokenType.Operator, OperatorType.SinH ,Priority.Unary)},
        };
        //Prefix enumeration of all operators.
        static string[] _operatorPrefixes;
        static string[] operatorPrefixes
        {
            get
            {
                return _operatorPrefixes
                    ?? (_operatorPrefixes = operators.Keys.SelectMany(x => Enumerable.Range(0, x.Length + 1).Select(i => x.Substring(0, i))).ToArray());
            }
        }

        public enum State { Empty, Number, Operator }

        private static List<Token> lexer(string input)
        {
            List<Token> tokens = new List<Token>();
            string buffer = "";
            State state = State.Empty;
            Action setEmpty = () => { buffer = ""; state = State.Empty; };
            Action flushBuffer = () =>
            {
                if (string.IsNullOrWhiteSpace(buffer)) return;
                if (isNumeric(buffer))
                {
                    if (buffer.Count(x => decSeps.Contains(x)) > 1) throw new InvalidOperationException("multiple decseps");
                    if (decSeps.Contains(buffer.First())) buffer = 0 + buffer;
                    if (decSeps.Contains(buffer.Last())) buffer = buffer.TrimEnd(decSeps);
                    buffer = string.Join(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, buffer.Split(decSeps));
                    tokens.Add(new Token(TokenType.Number, Convert.ToDouble(buffer)));
                }
                else if (operators.ContainsKey(buffer)) tokens.Add(new Token(operators[buffer].Item1, operators[buffer].Item2));
                else throw new InvalidOperationException("badbuffer");
            };

            //Action<TokenType, object> addToken = (type, val) => tokens.Add(new Token(type, val));
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                switch (state)
                {
                    case State.Empty:
                        buffer += c;
                        state = isNumeric(buffer) ? State.Number : State.Operator;
                        break;
                    case State.Number:
                        if (isNumeric(buffer + c)) buffer += c;
                        else
                        {
                            flushBuffer();
                            i--;
                            setEmpty();
                        }
                        break;
                    case State.Operator:
                        if (isOperatorPrefix(buffer + c)) buffer += c;
                        else
                        {
                            flushBuffer();
                            i--;
                            setEmpty();
                        }
                        break;
                }
            }
            flushBuffer();

            return tokens;
        }

        public enum TokenType { BraceOpen, BraceClose, Number, Operator }
        public enum OperatorType { Plus, Minus, Star, Slash, Sin, SinH }

        public class Token
        {
            public TokenType Type { get; set; }
            public object Value { get; set; }

            public Token(TokenType type, object value = null)
            {
                Type = type;
                Value = value;
            }

            public override string ToString()
            {
                return string.Format("{0}({1})", Type, Value);
            }
        }

        public class ASTToken : Token
        {
            public AST Ast { get; set; }

            public ASTToken(AST ast) : base(TokenType.Number)
            {
                Ast = ast;
            }

            public override string ToString()
            {
                return "AST";
            }
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
        }

    }
}
