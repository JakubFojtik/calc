using System;
using System.Collections.Generic;
using static regexp.Strutures;

namespace regexp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("RegExp: ");
                string input = Console.ReadLine();

                var lexer = new Lexer();
                List<Token> tokens = lexer.lexer(input);
                Console.WriteLine("Lexer: " + string.Join(", ", tokens));

                var parser = new Parser();
                var ast = parser.parser(tokens);
                Console.WriteLine("Parser: ");
                Console.WriteLine(string.Join(", ", ast.printDeriv()));

                Console.WriteLine("Text: ");
                input = Console.ReadLine();

                string rem = null;
                try
                {
                    rem = ast.match(input);
                    if (rem == "") Console.WriteLine("Matches.");
                    else Console.WriteLine("No match.");
                }
                catch
                {
                    Console.WriteLine("Exception.");
                }
            }
        }
    }
}
