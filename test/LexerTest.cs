using calc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace test
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void Lexer()
        {
            var input = "-5.5*61-(-sin 3^4)/5";
            var lexer = new Lexer();
            var res = lexer.lexer(input);
            var str = string.Join(";", res);
            var expected = "Operator(Minus);Number(5.5);Operator(Mul);Number(61);Operator(Minus);Operator(BraceOpen);Operator(Minus);Operator(Sin);Number(3);Operator(Pow);Number(4);Operator(BraceClose);Operator(Div);Number(5)";
            Assert.AreEqual(expected, str);
        }
        [TestMethod]
        public void DoubleMulPowPrefixOp()
        {
            var input = "2**3";
            var lexer = new Lexer();
            var res = lexer.lexer(input);
            var str = string.Join(";", res);
            var expected = "Number(2);Operator(Pow);Number(3)";
            Assert.AreEqual(expected, str);
        }
    }
}
