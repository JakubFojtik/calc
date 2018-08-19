﻿using System;
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

                var lexer = new Lexer();
                List<Token> tokens = lexer.lexer(input);
                Console.WriteLine("Lexer: " + string.Join(", ", tokens));

                Console.WriteLine("Parser:");
                var parser = new Parser();
                AST ast = parser.parser(tokens);
                Console.WriteLine(ast.printDeriv());
                Console.WriteLine();
                Console.WriteLine("Result: " + ast.compute());

                if (parser.error) Console.WriteLine("Error: Did not process all tokens.");
            }
        }

    }
}
