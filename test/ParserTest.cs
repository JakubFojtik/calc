using System;
using System.Collections.Generic;
using calc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static calc.Strutures;

namespace test
{
    [TestClass]
    public class ParserTest
    {
        private static string parse(List<Token> input)
        {
            var parser = new Parser();
            var res = parser.parser(input);
            var str = res.printDFS(res);
            return str;
        }

        [TestMethod]
        public void Simple()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, 99),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Number, 3),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Number, 7),
            };
            string str = parse(input);
            var expected = "Operator(Minus) Operator(Minus) Operator(Star) Number(99) Number(3) Operator(Sin) Number(7) ";
            Assert.AreEqual(str, expected);
        }

        [TestMethod]
        public void Hard()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Number, 99),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, 3),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Number, 7),
                new Token(TokenType.Operator, OperatorType.Caret),
                new Token(TokenType.Number, 7),
            };
            string str = null;
            try { str = parse(input); } catch { }
            var expected = "";
            Assert.AreEqual(str, expected);
        }
    }
}
