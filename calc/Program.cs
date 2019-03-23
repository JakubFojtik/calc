using System;
using System.Collections.Generic;

namespace calc
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Expression: ");
                string input = Console.ReadLine();

                var lexer = new Lexer();
                List<Token> tokens = lexer.Lexerize(input);
                //Console.WriteLine("Lexer: " + string.Join(", ", tokens));

                Console.WriteLine("Parser:");
                var parser = new Parser();
                AST ast = parser.Parse(tokens);
                Console.WriteLine(ast?.printDeriv());
                Console.WriteLine();
                var res = ast?.compute() ?? 0m;
                Console.WriteLine("Result: " + res + " = " + res.ToString("e"));

                if (parser.SurplusTokensDetected) Console.WriteLine("Error: Did not process all tokens.");
            }
        }

    }
}
