using System;
using System.Collections.Generic;
using System.Linq;
using static regexp.Strutures;

namespace regexp
{
    public class Parser
    {
        private List<Token> tokens;
        private int curTokIdx;
        private Token curTok => tokens.ElementAtOrDefault(curTokIdx) ?? new Token(TokenType.EOF);

        public bool SurplusTokensDetected { get; private set; }

        public AST parser(List<Token> tokens)
        {
            this.tokens = tokens;
            curTokIdx = 0;
            AST ret = readReg();

            if (curTokIdx < tokens.Count) SurplusTokensDetected = true;
            return ret;
        }

        private AST readReg() => readOr();

        //sin cos 1^5
        //sin (1)^5 todo braces belong to sin
        //todo max(3,5)
        private AST readOr()
        {
            AST ret = readCharSeq();
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Or)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, op), ret, readOr());
                }
                else
                {
                    break;
                }
            }
            return ret;
        }
        //g*(56*)*
        private AST readCharSeq()
        {
            AST ret = readRep();
            var starterOps = new List<OperatorType> { OperatorType.CBraceOpen, OperatorType.EBraceOpen };
            while (curTok.Type == TokenType.Char || (curTok.Type == TokenType.Operator && starterOps.Contains((OperatorType)curTok.Value)))
            {
                ret = new AST(new Token(TokenType.Operator, OperatorType.Concat), ret, readCharSeq());
                //curTokIdx++;
            }
            return ret;
        }

        private AST readRep()
        {
            AST ret = readFactor();
            bool hasStar = false;
            while (isCurTok(OperatorType.Star))
            {
                curTokIdx++;
                hasStar = true;
            }
            if (hasStar)
            {
                ret = new AST(new Token(TokenType.Operator, OperatorType.Star), ret, null);
            }
            return ret;
        }

        private AST readFactor()
        {
            AST ret;

            if (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.CBraceOpen)
                {
                    ret = new AST(curTok, readBrace(OperatorType.CBraceClose), null);
                }
                else if (op == OperatorType.EBraceOpen)
                {
                    ret = new AST(curTok, readBrace(OperatorType.EBraceClose), null);
                }
                else throw new ArithmeticException(ERROR);
            }
            else if (curTok.Type == TokenType.Char)
            {
                ret = new AST(curTok);
                curTokIdx++;
                /*
                var chars = new List<char>();
                while (curTok.Type == TokenType.Char)
                {
                    chars.Add((char)curTok.Value);
                    curTokIdx++;
                }
                if (chars.Count > 1)
                {
                    //remove last char as it can be target of postfix operators
                    chars.RemoveAt(chars.Count - 1);
                    curTokIdx--;
                }
                ret = new AST(new Token(TokenType.Char, chars));*/
            }
            else ret = null;

            return ret;
        }

        private AST readChars()
        {
            var tmpTok = curTok;
            curTokIdx++;
            if (curTok.Type != TokenType.Char) return null;
            return new AST(tmpTok, null, readChars());
        }

        private AST readBrace(OperatorType closer)
        {
            AST ret;
            curTokIdx++;
            ret = readReg();
            if (isCurTok(closer))
            {
                curTokIdx++;
            }
            else throw new ArithmeticException(ERROR);
            return ret;
        }

        private bool isCurTok(OperatorType ot)
        {
            return curTok.Type == TokenType.Operator && ((OperatorType)curTok.Value) == ot;
        }
    }
}
