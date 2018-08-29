using Microsoft.VisualStudio.TestTools.UnitTesting;
using regexp;

namespace regexpTest
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void Lexer()
        {
            var input = "(9*)*|3*";
            var lexer = new Lexer();
            var res = lexer.lexer(input);
            var str = string.Join(";", res);
            var expected = "Operator(CBraceOpen);Char(9);Operator(Star);Operator(CBraceClose);Operator(Star);Operator(Or);Char(3);Operator(Star)";
            Assert.AreEqual(expected, str);
        }
    }
}
