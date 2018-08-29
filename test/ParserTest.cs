using calc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using static calc.OperatorToken;

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
            if (parser.SurplusTokensDetected) throw new AssertFailedException("Did not parse all.");
            return (str, res.compute());
        }

        private static decimal toNum(double val)
        {
            return Convert.ToDecimal(val);
        }

        private static bool areNumbersEqual(decimal a, decimal b)
        {
            return Math.Abs(Math.Abs((double)a) - Math.Abs((double)b)) < 0.0001;
        }

        [TestMethod]
        public void Simple()
        {
            var input = new List<Token> {
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(3)),
                new OperatorToken(OperatorType.Plus),
                new NumberToken(toNum(3)),
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(99)),
                new OperatorToken(OperatorType.Star),
                new OperatorToken(OperatorType.Minus),
                new ConstantToken(ConstantToken.ConstantType.E),
                new OperatorToken(OperatorType.Minus),
                new OperatorToken(OperatorType.Minus),
                new OperatorToken(OperatorType.Minus),
                new OperatorToken(OperatorType.Sin),
                new NumberToken(toNum(7)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(BinMinus) Operator(BinMinus) Operator(BinPlus) Operator(UnMinus) Number(3) Number(3) Operator(Star) Number(99) Operator(UnMinus) Constant(E) Operator(UnMinus) Operator(UnMinus) Operator(Sin) Number(7) ";
            var expectedVal = 268.45291m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Hard()
        {
            var input = new List<Token> {
                new OperatorToken(OperatorType.Minus),
                new OperatorToken(OperatorType.Sin),
                new NumberToken(toNum(99)),
                new OperatorToken(OperatorType.Star),
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(3)),
                new OperatorToken(OperatorType.Minus),
                new OperatorToken(OperatorType.Sin),
                new NumberToken(toNum(7)),
                new OperatorToken(OperatorType.Caret),
                new NumberToken(toNum(7)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(BinMinus) Operator(Star) Operator(UnMinus) Operator(Sin) Number(99) Operator(UnMinus) Number(3) Operator(Sin) Operator(Caret) Number(7) Number(7) ";
            var expectedVal = -2.62540m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        //-sin-8*-3
        //-((sin-8)*(-3))
        [TestMethod]
        public void Harder()
        {
            var input = new List<Token> {
                new OperatorToken(OperatorType.Minus),
                new OperatorToken(OperatorType.Sin),
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(8)),
                new OperatorToken(OperatorType.Star),
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            //var expectedText = "Operator(Minus) Operator(Star) Operator(Sin) Operator(Minus) Number(8) Operator(Minus) Number(3) ";
            var expectedText = "Operator(Star) Operator(UnMinus) Operator(Sin) Operator(UnMinus) Number(8) Operator(UnMinus) Number(3) ";
            var expectedVal = -2.96807m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Harderer()
        {
            var input = new List<Token> {
                new OperatorToken(OperatorType.Minus),
                new OperatorToken(OperatorType.Sin),
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(8)),
                new OperatorToken(OperatorType.Caret),
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(UnMinus) Operator(Sin) Operator(UnMinus) Operator(Caret) Number(8) Operator(UnMinus) Number(3) ";
            var expectedVal = 0.00195m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Hardererer()
        {
            var input = new List<Token> {
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(1)),
                new OperatorToken(OperatorType.Star),
                new NumberToken(toNum(2)),
                new OperatorToken(OperatorType.Caret),
                new OperatorToken(OperatorType.Sin),
                new OperatorToken(OperatorType.Minus),
                new NumberToken(toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Star) Operator(UnMinus) Number(1) Operator(Caret) Number(2) Operator(Sin) Operator(UnMinus) Number(3) ";
            var expectedVal = -0.90681m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void PrioritySinus()
        {
            var input = new List<Token> {
                new OperatorToken(OperatorType.Sin),
                new OperatorToken(OperatorType.BraceOpen),
                new NumberToken(toNum(3)),
                new OperatorToken(OperatorType.BraceClose),
                new OperatorToken(OperatorType.Caret),
                new NumberToken(toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Caret) Operator(Sin) Number(3) Number(3) ";
            var expectedVal = 0.00281m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }
        [TestMethod]
        public void NonPrioritySinus()
        {
            var input = new List<Token> {
                new OperatorToken(OperatorType.Sin),
                new NumberToken(toNum(3)),
                new OperatorToken(OperatorType.Caret),
                new NumberToken(toNum(3)),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Sin) Operator(Caret) Number(3) Number(3) ";
            var expectedVal = 0.95637m;
            Assert.AreEqual(expectedText, res.Item1);
            Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        [TestMethod]
        public void Negative()
        {
            var input = new List<Token> {
                new NumberToken(toNum(8)),
                new OperatorToken(OperatorType.Caret),
                new OperatorToken(OperatorType.Caret),
                new NumberToken(toNum(3)),
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
