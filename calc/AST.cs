using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static calc.OperatorToken;

namespace calc
{
    public class AST
    {
        public static readonly string ERROR = "AST error";

        public static Dictionary<OperatorType, Func<decimal, decimal, decimal>> operatorImpls
            = new Dictionary<OperatorType, Func<decimal, decimal, decimal>>
        {
            { OperatorType.UnPlus,    (a, b) => a                                       },
            { OperatorType.UnMinus,   (a, b) => -a                                      },
            { OperatorType.BinPlus,   (a, b) => a + b                                   },
            { OperatorType.BinMinus,  (a, b) => a - b                                   },
            { OperatorType.Mul,       (a, b) => a * b                                   },
            { OperatorType.Div,       (a, b) => a / b                                   },
            { OperatorType.Pow,       (a, b) => (decimal)Math.Pow((double)a, (double)b) },
            { OperatorType.Sin,       (a, b) => (decimal)Math.Sin((double)a)            },
            { OperatorType.ASin,      (a, b) => (decimal)Math.Asin((double)a)           },
            { OperatorType.Sqr,       (a, b) => a * a                                   },
            { OperatorType.Sqrt,      (a, b) => (decimal)Math.Sqrt((double)a)           },
        };

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

        public override string ToString() => value.ToString();

        public decimal compute()
        {
            var first = left?.compute();
            var second = right?.compute();
            switch (value)
            {
                case NumberToken numToken: //Includes constants
                    return numToken.Value;
                case OperatorToken opToken:
                    var opType = opToken.Operator;
                    decimal defaultBinary(decimal? left, decimal? right) => operatorImpls[opType](first.Value, second.Value);
                    decimal defaultUnary(decimal? left) => operatorImpls[opType](first.Value, 0);

                    //todo remove unary change to impls[x], make it known how many args each op has
                    if (second == null) return defaultUnary(first);
                    else return defaultBinary(first, second);
                default:
                    throw new InvalidOperationException(ERROR);
            }
        }

        public string printDeriv()
        {
            var lines = new Dictionary<int, AST>();
            mapLines(this, lines);
            StringBuilder ret = new StringBuilder();
            string writeItem(AST item)
            {
                if (item != null)
                {
                    var val = item.value;
                    if (val.NumOperands() == 0) return val.ToString();
                    else return lines.First(x => x.Value.value == val).Key.ToString();
                }
                else return "";
            }

            foreach (var item in lines.OrderBy(x => x.Key))
            {
                string first = writeItem(item.Value.left);
                string second = writeItem(item.Value.right);
                if (item.Value.left != null && item.Value.right == null)    //todo item.Value.left != null && item.Value.right == null
                {
                    ret.AppendLine(string.Format("{0}: {1} -> {2}", item.Key, item.Value.value, first));
                }
                if (item.Value.left != null && item.Value.right != null)
                {
                    ret.AppendLine(string.Format("{0}: {1} -> {2}, {3}", item.Key, item.Value.value, first, second));
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
