using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static calc.Strutures;

namespace calc
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Priklad: ");
                string input = Console.ReadLine();

                List<Token> tokens = Lexer.lexer(input);
                Console.WriteLine("Lexer: " + string.Join(", ", tokens));

                Console.WriteLine("Parser:");
                var parser = new Parser();
                AST ast = parser.parser(tokens);
                printASTDFS(ast);
                Console.WriteLine();
                Console.WriteLine("Result: " + ast.compute());

                if (parser.error) Console.WriteLine("Error: Did not process all tokens.");
            }
        }

        private static void printASTDFS(AST ast)
        {
            Console.Write(ast.value + " ");
            if (ast.left != null) printASTDFS(ast.left);
            if (ast.right != null) printASTDFS(ast.right);
        }

        private static void printASTBFS(AST ast)
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
