using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static calc.Strutures;

namespace calc
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
                case TokenType.Number:
                    return (decimal)value.Value;    //todo convert or generic type
                case TokenType.Constant:
                case TokenType.Operator:
                    var opType = (OperatorType)value.Value;
                    decimal defaultBinary(decimal? left, decimal? right) => operatorImpls[opType].getDecimal(first.Value, second.Value);
                    decimal defaultUnary(decimal? left) => operatorImpls[opType].getDecimal(first.Value, 0);
                    decimal defaultConstant() => operatorImpls[opType].getDecimal(0, 0);
                    switch (opType)
                    {
                        default:
                            if (first == null) return defaultConstant();
                            else if (second == null) return defaultUnary(first);
                            else return defaultBinary(first, second); 
                        case OperatorType.Plus:
                            if (second != null) return defaultBinary(first, second);
                            else return first.Value;
                        case OperatorType.Minus:
                            if (second != null) return defaultBinary(first, second);
                            else return -1 * first.Value;
                    }
                default:
                    throw new ArithmeticException(ERROR);
            }
        }

        public string printDeriv()
        {
            var map = new Dictionary<int, AST>();
            fillMap(this, map);
            StringBuilder ret = new StringBuilder();
            var numberTypes = new List<TokenType> { TokenType.Number, TokenType.Constant };
            Func<AST, string> writeItem = item =>
            {
                if (item != null)
                {
                    var val = item.value;
                    if (numberTypes.Contains(val.Type)) return val.ToString();
                    else return map.First(x => x.Value.value == val).Key.ToString();
                }
                else return "";
            };
            var removed = new HashSet<int>();
            foreach (var item in map.OrderBy(x => x.Key))
            {
                if (removed.Contains(item.Key)) continue;
                string first = writeItem(item.Value.left);
                string second = writeItem(item.Value.right);
                if (!numberTypes.Contains(item.Value.value.Type))
                {
                    ret.AppendLine(string.Format("{0}: {1} -> {2}, {3}", item.Key, item.Value.value, first, second));
                }
            }
            return ret.ToString();
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
