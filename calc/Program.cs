﻿using System;
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
        private static int curTokIdx;
        private static Token curTok
        {
            get
            {
                if (curTokIdx < tokens.Count) return tokens[curTokIdx];
                else return new Token(TokenType.EOF);
            }
        }

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


        private static AST parser(List<Token> tokens)
        {
            Program.tokens = tokens;
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

            curTokIdx = 0;
            Console.WriteLine(readAll());

            return new AST(new Token(TokenType.Operator, OperatorType.Plus),
                new AST(new Token(TokenType.Number, 5)),
                new AST(new Token(TokenType.Number, -1)));
        }

        class Value
        {
            private decimal val;

            public Value(decimal d)
            {
                val = d;
            }

            public static implicit operator Value(decimal d)
            {
                return new Value(d);
            }

            public void Negate()    // Invert?
            {
                val = -val;
            }

            public override string ToString()
            {
                return val.ToString();
            }
        }


        //Gramatika - nejvic zanorene jsou nejvyssi priority, Start: SCIT
        //SCIT -> SCIT + NAS | NAS
        //NAS -> NAS * MOC | MOC
        //MOC -> MOC * CISLO | CISLO
        //CISLO -> CISLO | (SCIT)
        //odstr leve rekurze SCIT->NAS ?SCIT, ?SCIT->+ NAS ?SCIT|eps
        //stromecek
        private static Value readAll()
        {
            Value retExp;
            Value retRem;
            retExp = readRem();
            retRem = readRem();
            return retExp; //todo?
        }

        private static Value readRem() //??? read scit aa read nas...
        {
            Value ret;
            if (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Plus || op == OperatorType.Minus)
                    ret = readFactor();//return
                else throw new ArithmeticException(ERROR);
            }
            else if (curTok.Type == TokenType.Number)
                ret = readFactor();
            else if (curTok.Type == TokenType.BraceOpen)
                ret = readBrace();
            else throw new ArithmeticException(ERROR);
            return ret;
        }

        private static Value readFactor()
        {
            Value ret;
            bool signIsPlus = true;
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Minus) signIsPlus ^= true;
                else if (op == OperatorType.Plus) { }
                else throw new ArithmeticException(ERROR);
                curTokIdx++;
            }

            if (curTok.Type == TokenType.Number)
            {
                ret = Convert.ToDecimal(curTok.Value);
                curTokIdx++;
            }
            else if (curTok.Type == TokenType.BraceOpen)
            {
                ret = readBrace();
            }
            else throw new ArithmeticException(ERROR);

            if (!signIsPlus) ret.Negate();
            return ret;
        }

        private static Value readBrace()
        {
            Value ret;
            curTokIdx++;
            ret = readRem();
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

        private static bool isOperatorPrefix(string buffer)
        {
            return !string.IsNullOrWhiteSpace(buffer) && operatorPrefixes.Contains(buffer);
        }

        enum Priority { None = 0, Add, Mult, Unary, Pow, Brace } //brace mimo?

        enum Associativity { Left = 0, Right }

        class OperatorData : Tuple<TokenType, OperatorType, Priority, Associativity>
        {
            public OperatorData(TokenType item1, OperatorType item2, Priority item3, Associativity item4) : base(item1, item2, item3, item4) { }
        }

        static Dictionary<string, OperatorData> operators = new Dictionary<string, OperatorData>
        {
            { "(",    new OperatorData(TokenType.BraceOpen,  OperatorType.None,  Priority.Brace, Associativity.Right )},
            { ")",    new OperatorData(TokenType.BraceClose, OperatorType.None,  Priority.Brace, Associativity.Left  )},
            { "+",    new OperatorData(TokenType.Operator,   OperatorType.Plus,  Priority.Add,   Associativity.Left  )},
            { "-",    new OperatorData(TokenType.Operator,   OperatorType.Minus, Priority.Add,   Associativity.Left  )}, //dynamic
            { "*",    new OperatorData(TokenType.Operator,   OperatorType.Star,  Priority.Mult,  Associativity.Left  )},
            { "/",    new OperatorData(TokenType.Operator,   OperatorType.Slash, Priority.Mult,  Associativity.Left  )},
            { "^",    new OperatorData(TokenType.Operator,   OperatorType.Caret, Priority.Pow,   Associativity.Right )},
            { "sin",  new OperatorData(TokenType.Operator,   OperatorType.Sin ,  Priority.Unary, Associativity.Left  )},
            { "sinh", new OperatorData(TokenType.Operator,   OperatorType.SinH,  Priority.Unary, Associativity.Left  )},
        };
        //Prefix enumeration of all operators.
        static string[] _operatorPrefixes;
        private static readonly string ERROR = "Mat chyba";

        static string[] operatorPrefixes
        {
            get
            {
                Func<string, IEnumerable<int>> substrLengths = str => Enumerable.Range(1, str.Length);
                Func<string, IEnumerable<int>, IEnumerable<string>> substrings = (str, lengths) => lengths.Select(i => str.Substring(0, i));

                return _operatorPrefixes
                    ?? (_operatorPrefixes = operators.Keys.SelectMany(x => substrings(x, substrLengths(x))).ToArray());
            }
        }

        public enum State { Empty, Number, Operator }

        private static List<Token> lexer(string input)
        {
            List<Token> tokens = new List<Token>();
            string buffer = "";
            State state = State.Empty;
            //Action<TokenType, object> addToken = (type, val) => tokens.Add(new Token(type, val));
            Action finalizeBuffer = () =>
            {
                if (string.IsNullOrWhiteSpace(buffer)) return;
                if (isNumeric(buffer))
                {
                    if (buffer.Count(x => decSeps.Contains(x)) > 1) throw new InvalidOperationException("multiple decseps");
                    if (decSeps.Contains(buffer.First())) buffer = 0 + buffer;
                    if (decSeps.Contains(buffer.Last())) buffer = buffer.TrimEnd(decSeps);
                    // convert all decimal separators to one - does not support thousand separators
                    buffer = string.Join(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, buffer.Split(decSeps));
                    tokens.Add(new Token(TokenType.Number, Convert.ToDouble(buffer)));
                }
                else if (operators.ContainsKey(buffer)) tokens.Add(new Token(operators[buffer].Item1, operators[buffer].Item2));
                else throw new InvalidOperationException("badbuffer");

                Action setEmpty = () => { buffer = ""; state = State.Empty; };
                setEmpty();
            };

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
        public enum OperatorType { None, Plus, Minus, Star, Slash, Sin, SinH, Caret }

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

            public ASTToken(AST ast) : base(TokenType.Number)   //TokenType.Operator?
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
