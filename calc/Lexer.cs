﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static calc.ConstantToken;
using static calc.OperatorToken;

namespace calc
{
    public class Lexer
    {
        public static Dictionary<string, OperatorType> operators
            = new Dictionary<string, OperatorType>
        {
            { "(",     OperatorType.BraceOpen },
            { ")",     OperatorType.BraceClose },
            { "+",     OperatorType.Plus },  //we do not know arity yet
            { "-",     OperatorType.Minus }, //we do not know arity yet
            { "*",     OperatorType.Mul },
            { "/",     OperatorType.Div },
            { "^",     OperatorType.Pow },
            { "**",    OperatorType.Pow },
            { "sin",   OperatorType.Sin  },
            { "asin",  OperatorType.ASin },
            { "sqr",   OperatorType.Sqr },
            { "sqrt",  OperatorType.Sqrt },
        };

        public static Dictionary<string, ConstantType> constants
            = new Dictionary<string, ConstantType>
        {
            { "pi",    ConstantType.Pi },
            { "e",     ConstantType.E },
        };

        public char[] decSeps = { ',', '.' };

        public enum State { Empty, Number, OperatorOrConstant }

        public List<Token> Lexerize(string input)
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
                    buffer = unifyDecSeps(buffer);
                    tokens.Add(new NumberToken(Convert.ToDecimal(buffer)));
                }
                else if (operators.ContainsKey(buffer)) tokens.Add(new OperatorToken(operators[buffer]));
                else if (constants.ContainsKey(buffer)) tokens.Add(new ConstantToken(constants[buffer]));
                else throw new InvalidOperationException("badbuffer");

                void setEmpty() { buffer = ""; state = State.Empty; }
                setEmpty();
            }

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsWhiteSpace(c))
                {
                    finalizeBuffer();
                    continue;
                }

                switch (state)
                {
                    case State.Empty:
                        buffer += c;
                        state = isNumeric(buffer) ? State.Number : State.OperatorOrConstant;
                        break;
                    case State.Number:
                        if (isNumeric(buffer + c)) buffer += c;
                        else
                        {
                            finalizeBuffer();
                            i--;
                        }
                        break;
                    case State.OperatorOrConstant:
                        if (isOperatorPrefix(buffer + c) || isConstantPrefix(buffer + c)) buffer += c;
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

        private string unifyDecSeps(string buffer)
        {
            // convert all decimal separators to current culture so converts work => does not support thousand separators
            return string.Join(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, buffer.Split(decSeps));
        }

        bool isNumeric(string buffer)
        {
            return !string.IsNullOrWhiteSpace(buffer) && areAllCharsNumeric(buffer);
            //     && buffer.Count(x => decSeps.Contains(x)) <= 1;   aby ... nebylo 0 0 0 tak je tecka "cislici"-soucasti cisla
        }

        private bool areAllCharsNumeric(string buffer)
        {
            foreach (var c in buffer)
            {
                if (!char.IsDigit(c) && !decSeps.Contains(c)) return false;
            }
            return true;
        }

        bool isOperatorPrefix(string buffer) => operatorPrefixes.Contains(buffer);
        bool isConstantPrefix(string buffer) => constantPrefixes.Contains(buffer);

        //Prefix enumerations of all operators and constants.
        IEnumerable<int> prefixLengths(string str) => Enumerable.Range(1, str.Length);
        IEnumerable<string> substrings(string str) => prefixLengths(str).Select(x => str.Substring(0, x));
        HashSet<string> getPrefixes(IEnumerable<string> keys) => keys.SelectMany(x => substrings(x)).ToHashSet();
        //Lazy getters
        Lazy<HashSet<string>> getLazy(IEnumerable<string> keys) => new Lazy<HashSet<string>>(() => getPrefixes(keys));
        HashSet<string> constantPrefixes => getLazy(constants.Keys).Value;
        HashSet<string> operatorPrefixes => getLazy(operators.Keys).Value;

    }
}
