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
        private static (string, decimal) parse(List<Token> input)
        {
            var parser = new Parser();
            var res = parser.parser(input);
            var str = res.printDFS(res);
            return (str, res.compute());
        }

        private static decimal toNum(double val)
        {
            return Convert.ToDecimal(val);
        }

        private static bool areNumbersEqual(decimal a, decimal b)
        {
            return Math.Abs((double)a) - Math.Abs((double)b) < 0.0001;
        }

        [TestMethod]
        public void Simple()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, toNum(99)),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Number, toNum(3)),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Number, toNum(7)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Minus) Operator(Minus) Operator(Star) Number(99) Number(3) Operator(Minus) Operator(Minus) Operator(Sin) Number(7) ";
            var expectedVal = -297.65698m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Hard()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Number, toNum(99)),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, toNum(3)),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Number, toNum(7)),
                new Token(TokenType.Operator, OperatorType.Caret),
                new Token(TokenType.Number, toNum(7)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Minus) Operator(Minus) Operator(Star) Operator(Sin) Number(99) Operator(Minus) Number(3) Operator(Sin) Operator(Caret) Number(7) Number(7) ";
            var expectedVal = -2.62540m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Harder()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, toNum(8)),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Minus) Operator(Star) Operator(Sin) Operator(Minus) Number(8) Operator(Minus) Number(3) ";
            var expectedVal = -2.96807m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Harderer()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Operator, OperatorType.Sin),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, toNum(8)),
                new Token(TokenType.Operator, OperatorType.Caret),
                new Token(TokenType.Operator, OperatorType.Minus),
                new Token(TokenType.Number, toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Minus) Operator(Sin) Operator(Minus) Operator(Caret) Number(8) Operator(Minus) Number(3) ";
            var expectedVal = 0.00195m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Negative()
        {
            var input = new List<Token> {
                new Token(TokenType.Number, toNum(8)),
                new Token(TokenType.Operator, OperatorType.Caret),
                new Token(TokenType.Operator, OperatorType.Caret),
                new Token(TokenType.Number, toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            string expectedText = null;
            var expectedVal = 0m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }


    }
}
