using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static regexp.Strutures;

namespace regexp
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

            }
        }
    }
}
