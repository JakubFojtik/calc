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
                Console.WriteLine("Lexer: " + string.Join(", ", tokens));

                Console.WriteLine("Parser:");
                AST ast = parser(tokens);
                printASTDFS(ast);
                Console.WriteLine();
                Console.WriteLine("Result: " + ast.compute());

                if (curTokIdx != tokens.Count) Console.WriteLine("Error: Did not process all tokens.");
            }
        }

        private static void printASTDFS(AST ast)
        {
            Console.Write(ast.value + " ");
            if (ast.left != null) printASTDFS(ast.left);
            if (ast.right != null) printASTDFS(ast.right);
        }

        private static void printASTBFS(AST ast)
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

        private static List<Token> tokens;
        private static int curTokIdx;
        private static Token curTok => tokens.ElementAtOrDefault(curTokIdx) ?? new Token(TokenType.EOF);

        #region ?
        private static void stackParser(List<Token> tokens)
        {
            Program.tokens = tokens;
            Stack<AST> stack = new Stack<AST>();

            tokens.ForEach(x =>
            {
                while (tryReduce(stack, x)) { }

                stack.Push(new AST(x));
            });

        }

        private static bool tryReduce(Stack<AST> stack, Token x)
        {
            //2 aargs for binops, 1 for unop, braces
            //(23)^5
            if (stack.Peek().value.Type == TokenType.BraceClose
                && (x.Type != TokenType.Operator || (OperatorType)x.Value != OperatorType.Caret))
            {

            }

            return false;
        }

        private static void recWork(int start, Priority currentPriority)
        {
            //5+4-6*9^8*2-4
            //readterm .5 =>5.
            //reaadop + pri=add
            //readexpr(untilAddOrBigger) =>.4-
            //readexpr(untilAddOrBigger) =>4-.6*
            //readexpr(untilAddOrBigger) =>6*.9^
            //readexpr(untilAddOrBigger) =>9^8.-
            //finishexpr => 9^8.-
            //finishexpr => 6*9^8.-
            //cosakra 3^4^5^6 vysl = ^3^4^56

            //int end =
            //while ()
            {
                //split unary aand postfix unary, + -6 => neg int
            }
        }

        /*
        parse(all)
            while(cantstop) read
            ast token v tokenech
            na zac zav unarni +- bez priority
            co s sin 5 + 2 - zrat minimum
            recwork(sezer mi 1 argument jsem sinus)
            -5^4 - co driv? ^ neresim unarni minus
            pada priorita, zavorky jsou samost
            lokalni ast, pamatuju, kam ho prippojit

            priority - podle googlu sin sin 2 ^ 3 ^ 4 = sin(sin(2^(3^4)))
            tj 2+2+2 = 2+(2+2) tj ctu aalespon stejne priority
            ale 1/2/3 = (1/2)/3
            takze dulezitejsi  je poradi vyhodnocovani
        */

        #endregion ?

        private static AST parser(List<Token> tokens)
        {
            Program.tokens = tokens;
            curTokIdx = 0;
            AST ret = readAll();

            return ret;
        }

        class Value
        {
            private decimal val;
            public Value(decimal d) => val = d;
            public static implicit operator Value(decimal d) => new Value(d);
            public static implicit operator decimal(Value d) => d.val;
            public void Negate() => val = -val;
            public override string ToString() => val.ToString();
        }


        //Gramatika - nejvic zanorene jsou nejvyssi priority, vsechny pravidla ukousnou 1 a zbytek je vse dalsi. Start: SCIT
        //SCIT -> SCIT + SCIT | NAS
        //NAS -> NAS * NAS | MOC
        //MOC -> MOC ^ CISLO | CISLO
        //CISLO -> cislo | (SCIT)
        //odstr leve rekurze SCIT->NAS ?SCIT, ?SCIT->+ SCIT ?SCIT|eps
        //stromecek
        private static AST readAll() => readAdd();

        private static AST readSimpleBinaryOperator(Priority priority, Func<AST> nextFun)
        {
            AST ret;
            ret = nextFun();
            var operatorTypes = operators.Values.Where(x => x.Priority == priority).Select(x => x.Operator);
            while (curTok.Type == TokenType.Operator)
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

        private static AST readAdd() => readSimpleBinaryOperator(Priority.Add, readMul);
        private static AST readMul() => readSimpleBinaryOperator(Priority.Mult, readPow);
        private static AST readPow() => readSimpleBinaryOperator(Priority.Pow, readFactor);
        /*
        private static AST readUnary()
        {
            AST ret;
            ret = nextFun();
            var operatorTypes = operators.Values.Where(x => x.Priority == priority).Select(x => x.Operator);
            while (curTok.Type == TokenType.Operator)
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
        */

        private static AST readFactor()
        {
            AST ret;
            bool signIsPlus = true;
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Minus) signIsPlus ^= true;
                else if (op == OperatorType.Plus) { }
                else
                {
                    break;
                }
                curTokIdx++;
            }

            if (curTok.Type == TokenType.Number)
            {
                ret = new AST(curTok);
                curTokIdx++;
            }
            else if (curTok.Type == TokenType.BraceOpen)
            {
                ret = readBrace();
            }
            else if (curTok.Type == TokenType.Operator)
            {
                var funOp = curTok;
                curTokIdx++;
                ret = new AST(funOp, readFactor(), null);
            }
            else throw new ArithmeticException(ERROR);

            if (!signIsPlus) ret = new AST(new Token(TokenType.Operator, OperatorType.Minus), ret, null);

            return ret;
        }

        private static AST readBrace()
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

        static bool isNumeric(string buffer)
        {
            return !string.IsNullOrWhiteSpace(buffer) && areAllCharsNumeric(buffer);
            //     && buffer.Count(x => decSeps.Contains(x)) <= 1;   aby ... nebylo 0 0 0 tak je tecka "cislici"-soucasti cisla
        }

        private static bool areAllCharsNumeric(string buffer)
        {
            foreach (var c in buffer)
            {
                if (!char.IsDigit(c) && !decSeps.Contains(c)) return false;
            }
            return true;
        }

        private static bool isOperatorPrefix(string buffer) => operatorPrefixes.Contains(buffer);

        enum Priority { None = 0, Add, Mult, Unary, Pow, Brace } //brace mimo?

        enum Associativity { Left, Right }

        static Dictionary<string, (TokenType Token, OperatorType Operator, Priority Priority, Associativity Associativity)> operators
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
        static Dictionary<OperatorType, (Func<AST, AST, AST> getAST, Func<decimal, decimal, decimal> getDecimal)> operatorImpls
            = new Dictionary<OperatorType, (Func<AST, AST, AST>, Func<decimal, decimal, decimal>)>
        {
            { OperatorType.Plus,  ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Plus ), a, b), (a, b) => a + b) },
            { OperatorType.Minus, ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Minus), a, b), (a, b) => a - b) },
            { OperatorType.Star,  ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Star ), a, b), (a, b) => a * b) },
            { OperatorType.Slash, ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Slash), a, b), (a, b) => a / b) },
            { OperatorType.Caret, ((a, b) => new AST(new Token(TokenType.Operator, OperatorType.Caret), a, b), (a, b) => (decimal)Math.Pow((double)a, (double)b)) },
        };
        //Prefix enumeration of all operators.
        static string[] _operatorPrefixes;
        private static readonly string ERROR = "Mat chyba";

        static string[] operatorPrefixes
        {
            get
            {
                IEnumerable<int> prefixLengths(string str) => Enumerable.Range(1, str.Length);
                IEnumerable<string> substrings(string str, IEnumerable<int> lengths) => lengths.Select(i => str.Substring(0, i));

                return _operatorPrefixes
                    ?? (_operatorPrefixes = operators.Keys.SelectMany(x => substrings(x, prefixLengths(x))).ToArray());
            }
        }

        public enum State { Empty, Number, Operator }

        private static List<Token> lexer(string input)
        {
            List<Token> tokens = new List<Token>();
            string buffer = "";
            State state = State.Empty;
            //Action<TokenType, object> addToken = (type, val) => tokens.Add(new Token(type, val));
            void finalizeBuffer()
            {
                if (string.IsNullOrWhiteSpace(buffer)) return;
                if (isNumeric(buffer))
                {
                    if (buffer.Count(x => decSeps.Contains(x)) > 1) throw new InvalidOperationException("multiple decseps");
                    if (decSeps.Contains(buffer.First())) buffer = 0 + buffer;
                    if (decSeps.Contains(buffer.Last())) buffer = buffer.TrimEnd(decSeps);
                    // convert all decimal separators to one - does not support thousand separators
                    buffer = string.Join(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, buffer.Split(decSeps));
                    tokens.Add(new Token(TokenType.Number, Convert.ToDecimal(buffer)));
                }
                else if (operators.ContainsKey(buffer)) tokens.Add(new Token(operators[buffer].Token, operators[buffer].Operator));
                else throw new InvalidOperationException("badbuffer");

                void setEmpty() { buffer = ""; state = State.Empty; }
                setEmpty();
            }

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == ' ')
                {
                    finalizeBuffer();
                    continue;
                }

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
                            finalizeBuffer();
                            i--;
                        }
                        break;
                    case State.Operator:
                        if (isOperatorPrefix(buffer + c)) buffer += c;
                        else
                        {
                            finalizeBuffer();
                            i--;
                        }
                        break;
                }
            }
            finalizeBuffer();

            return tokens;
        }

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

    }
}
