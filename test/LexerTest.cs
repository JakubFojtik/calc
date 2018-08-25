using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using calc;
using static calc.Strutures;
using System.Collections.Generic;

namespace test
{
    [TestClass]
    public class LexerTest
    {
        /*
        [TestMethod]
        public void Lexer()
        {
            var input = "-55*61-(-sin 3^4)/5";
            var lexer = new Lexer();
            var res = lexer.lexer(input);
            var str = string.Join(";", res);
            var expected = "Operator(Minus);Number(55);Operator(Star);Number(61);Operator(Minus);BraceOpen(None);Operator(Minus);Operator(Sin);Number(3);Operator(Caret);Number(4);BraceClose(None);Operator(Slash);Number(5)";
            Assert.AreEqual(str, expected);
        }
        */
    }
}
