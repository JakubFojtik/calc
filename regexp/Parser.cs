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

        //Grammar from http://matt.might.net/articles/parsing-regex-with-recursive-descent/
        //<regex> ::= <term> '|' <regex>
        //         |  <term>
        //<term> ::= { <factor> }
        //<factor> ::= <base> { '*' }
        //<base> ::= <char>
        //        |  '\' <char>
        //        |  '(' <regex> ')'
        public AST parser(List<Token> tokens)
        {
            this.tokens = tokens;
            curTokIdx = 0;
            AST ret = readReg();

            if (curTokIdx < tokens.Count) SurplusTokensDetected = true;
            return ret;
        }

        private AST readReg()
        {
            AST ret = readTerm();
            while (curTok.Type == TokenType.Operator)
            {
                var op = (OperatorType)curTok.Value;
                if (op == OperatorType.Pipe)
                {
                    curTokIdx++;
                    ret = new AST(new Token(TokenType.Operator, op), ret, readReg());
                }
                else
                {
                    break;
                }
            }
            return ret;
        }
        //everything up to OR or EOL or closing braces e.g. (gh*(56*))*
        //simply check terminators, otherwise there must be a factor or input error
        private AST readTerm()
        {
            AST ret = new AST(new Token(TokenType.Empty));
            var stoppers = new List<OperatorType> { OperatorType.Pipe, OperatorType.CBraceClose, OperatorType.EBraceClose };
            while (curTok.Type == TokenType.Char
                || (curTok.Type == TokenType.Operator && !stoppers.Contains((OperatorType)curTok.Value)))
            {
                if (ret.value.Type == TokenType.Empty)
                {
                    ret = readFactor();
                }
                else
                {
                    ret = new AST(new Token(TokenType.Operator, OperatorType.Concat), ret, readFactor());
                }
            }
            return ret;
        }

        private AST readFactor()
        {
            AST ret = readBase();
            //microopt - sequential stars can be reduced to one e.g. (ab***)*** should be equal to (ab*)*
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

        private AST readBase()
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
                //if (curTok.Value == ) escaped char
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
            else
                throw new ArithmeticException(ERROR);

            return ret;
        }

        private AST readBrace(OperatorType closer)
        {
            curTokIdx++;
            AST ret = readReg();
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
