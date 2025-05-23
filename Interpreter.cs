using System;
using System.Collections.Generic;
using System.Text;
using System;

public class Program
{
    public static void Main()
    {
        string testCode =
        @"Spawn ← 42
         Color ← ""verde#00FF00""
         width ← 1280.5
         DrawCircle (30 * 2) + width, 720 / 2
         GoTo 150, 300
         IsCanvasColor ← 3.14 == (50 % 4) && True";

        Lexer lexer = new Lexer(testCode);
        //Console.WriteLine(testCode.Length);
        Console.WriteLine("Tokens generados:");
        
        foreach (Token token in lexer.tokens)
        {
            Console.WriteLine(token);
        }
    }
}
