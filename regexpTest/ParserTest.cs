using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using regexp;
using static regexp.Strutures;

namespace regexpTest
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
            return (str, 0/*res.compute()*/);
        }

        //(9*)*|3*
        [TestMethod]
        public void SingleChar()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.CBraceOpen),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Operator, OperatorType.CBraceClose),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Operator, OperatorType.Pipe),
                new Token(TokenType.Char, '3'),
                new Token(TokenType.Operator, OperatorType.Star),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Pipe) Operator(Star) Operator(CBraceOpen) Operator(Star) Char(9) Operator(Star) Char(3) ";
            //var expectedVal = -2.96807m;
            Assert.AreEqual(expectedText, res.Item1);
            //Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        //(9*)*|3*
        [TestMethod]
        public void MultiChar()
        {
            var input = new List<Token> {
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Operator, OperatorType.Star),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Concat) Operator(Concat) Char(9) Char(9) Operator(Star) Char(9) ";
            //var expectedVal = -2.96807m;
            Assert.AreEqual(expectedText, res.Item1);
            //Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        //5|(6|(7|8))
        [TestMethod]
        public void NestedOr()
        {
            var input = new List<Token> {
                new Token(TokenType.Char, '5'),
                new Token(TokenType.Operator, OperatorType.Pipe),
                new Token(TokenType.Operator, OperatorType.CBraceOpen),
                new Token(TokenType.Char, '6'),
                new Token(TokenType.Operator, OperatorType.Pipe),
                new Token(TokenType.Operator, OperatorType.CBraceOpen),
                new Token(TokenType.Char, '7'),
                new Token(TokenType.Operator, OperatorType.Pipe),
                new Token(TokenType.Char, '8'),
                new Token(TokenType.Operator, OperatorType.CBraceClose),
                new Token(TokenType.Operator, OperatorType.CBraceClose),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Pipe) Char(5) Operator(CBraceOpen) Operator(Pipe) Char(6) Operator(CBraceOpen) Operator(Pipe) Char(7) Char(8) ";
            //var expectedVal = -2.96807m;
            Assert.AreEqual(expectedText, res.Item1);
            //Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

        //(999*)*|99*
        [TestMethod]
        public void Harder()
        {
            var input = new List<Token> {
                new Token(TokenType.Operator, OperatorType.CBraceOpen),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Operator, OperatorType.Star),
                new Token(TokenType.Operator, OperatorType.CBraceClose),
                new Token(TokenType.Operator, OperatorType.Pipe),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Char, '9'),
                new Token(TokenType.Operator, OperatorType.Star),
            };
            (string, decimal) res = (null, 0);
            try { res = parse(input); } catch { }
            var expectedText = "Operator(Pipe) Operator(CBraceOpen) Operator(Concat) Operator(Concat) Char(9) Char(9) Operator(Star) Char(9) Operator(Concat) Char(9) Operator(Star) Char(9) ";
            //var expectedVal = -2.96807m;
            Assert.AreEqual(expectedText, res.Item1);
            //Assert.IsTrue(areNumbersEqual(res.Item2, expectedVal));
        }

    }
}
