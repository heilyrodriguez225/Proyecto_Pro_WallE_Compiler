using System;
using System.Collections.Generic;
using System.Text;
using System;

public class Program
{
    public static void Main()
    {
        string testCode =
        @"Spawn(0, 0)
        Color(Black)
        n <- 5
        k <- 3 + 3 * 10
        n <- k * 2
        actual-x <- GetActualX()
        i <- 0";

        Lexer lexer = new Lexer(testCode);
        //Console.WriteLine(testCode.Length);
        Console.WriteLine("Tokens generados:");
        
        foreach (Token token in lexer.tokens)
        {
            Console.WriteLine(token);
        }
    }
}
