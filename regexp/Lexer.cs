using System.Collections.Generic;
using System.Linq;
using static regexp.Strutures;

namespace regexp
{
    public class Lexer
    {
        public enum State { Empty, Char, Operator }

        public List<Token> lexer(string input)
        {
            List<Token> tokens = new List<Token>();
            string buffer = "";

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                buffer += c;
                if (isOperator(buffer)) tokens.Add(new Token(TokenType.Operator, operators[buffer]));
                else tokens.Add(new Token(TokenType.Char, buffer[0]));
                buffer = "";
            }

            return tokens;
        }

        bool isOperator(string buffer)
        {
            return operators.Keys.Contains(buffer);
        }

        private bool isOperatorPrefix(string buffer) => operatorPrefixes.Contains(buffer);

        //Prefix enumeration of all operators.
        public string[] _operatorPrefixes;

        public string[] operatorPrefixes
        {
            get
            {
                IEnumerable<int> prefixLengths(string str) => Enumerable.Range(1, str.Length);
                IEnumerable<string> substrings(string str, IEnumerable<int> lengths) => lengths.Select(i => str.Substring(0, i));

                return _operatorPrefixes
                    ?? (_operatorPrefixes = operators.Keys.SelectMany(x => substrings(x, prefixLengths(x))).ToArray());
            }
        }

    }
}
