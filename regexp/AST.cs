using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static regexp.Strutures;

namespace regexp
{
    public class AST
    {
        public Token value;
        public AST left;
        public AST right;

        public AST(Token v, AST l, AST r)
        {
            value = v;
            left = l;
            right = r;
        }
        public AST(Token v) : this(v, null, null) { }

        public override string ToString() => value.Value.ToString();

        public decimal compute()
        {
            var first = left?.compute();
            var second = right?.compute();
            switch (value.Type)
            {
                /*
                case TokenType.Number:
                    return (decimal)value.Value;    //todo convert or generic type
                case TokenType.Operator:
                    var opType = (OperatorType)value.Value;
                    decimal defaultBinary(decimal? left, decimal? right) => operatorImpls[opType].getDecimal(first.Value, second.Value);
                    switch (opType)
                    {
                        default: //binary op
                            return defaultBinary(first, second);
                        case OperatorType.Plus:
                            if (second != null) return defaultBinary(first, second);
                            else return first.Value;
                        case OperatorType.Minus:
                            if (second != null) return defaultBinary(first, second);
                            else return -1 * first.Value;
                        case OperatorType.Sin:
                            return (decimal)Math.Sin((double)first.Value);
                        case OperatorType.ASin:
                            return (decimal)Math.Asin((double)first.Value);
                    }
                    */
                default:
                    throw new ArithmeticException(ERROR);
            }
        }

        public string printDeriv()
        {
            var lines = new Dictionary<int, AST>();
            mapLines(this, lines);
            StringBuilder ret = new StringBuilder();
            Func<AST, string> writeItem = item =>
            {
                if (item != null)
                {
                    var val = item.value;
                    if (item.left == null && item.left == item.right) return val.ToString();
                    else return lines.First(x => x.Value.value == val).Key.ToString();
                }
                else return "";
            };

            foreach (var item in lines.OrderBy(x => x.Key))
            {
                string first = writeItem(item.Value.left);
                string second = writeItem(item.Value.right);
                if (item.Value.left != null && item.Value.right != null)
                {
                    ret.AppendLine(string.Format("{0}: {1} -> {2}, {3}", item.Key, item.Value.value, first, second));
                }
                else if (item.Value.left != null)
                {
                    ret.AppendLine(string.Format("{0}: {1} -> {2}", item.Key, item.Value.value, first));
                }
                else if (item.Value.right != null)
                {
                    ret.AppendLine(string.Format("{0}: {1} -> {2}", item.Key, item.Value.value, second));
                }
            }
            return ret.ToString();
        }

        private void mapLines(AST ast, Dictionary<int, AST> map)
        {
            int num = map.Count;
            map.Add(num, ast);
            if (ast.left != null) mapLines(ast.left, map);
            if (ast.right != null) mapLines(ast.right, map);
        }

        private void fillMap(AST ast, Dictionary<int, AST> map)
        {
            int num = map.Count;
            map.Add(num, ast);
            if (ast.left != null) fillMap(ast.left, map);
            if (ast.right != null) fillMap(ast.right, map);
        }

        public string printDFS(AST ast)
        {
            var str = ast.value + " ";
            if (ast.left != null) str += printDFS(ast.left);
            if (ast.right != null) str += printDFS(ast.right);
            return str;
        }

        public void printBFS(AST ast)
        {
            Queue<AST> q = new Queue<AST>();
            q.Enqueue(ast);
            int numItems = 1;
            while (q.Count > 0)
            {
                AST item = q.Dequeue();
                Console.Write(item.value + " ");
                numItems--;
                AST nextItem = item.left;
                if (nextItem != null)
                {
                    q.Enqueue(nextItem);
                    numItems++;
                }
                nextItem = item.right;
                if (nextItem != null)
                {
                    q.Enqueue(nextItem);
                    numItems++;
                }
            }
        }

    }

}
