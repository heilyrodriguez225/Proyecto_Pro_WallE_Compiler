using System;
using System.Collections.Generic;

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
        actual_x <- GetActualX()
        i <- 0

        loop1
        DrawLine(1, 0, 1)
        i <- i + 1
        GoTo [loop_ends_here] (is_brush_color_blue == 1)
        GoTo [loop1] (i < 10)

        Color(""Blue"")
        GoTo [loop1] (1 == 1)

        loop_ends_here";
        // 1. Análisis léxico
        Lexer lexer = new Lexer(testCode);
        Console.WriteLine("Tokens generados:");
        foreach (Token token in lexer.tokens)
        {
            Console.WriteLine(token);
        }
        // 2. Análisis sintáctico
        try
        {
            Parser parser = new Parser(lexer.tokens);
            var program = parser.ParseProgram();
            
            Console.WriteLine("\nÁrbol AST generado:");
            PrintAST(program, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError en el parsing: {ex.Message}");
        }
    }
    // Método recursivo para imprimir el AST
    private static void PrintAST(IASTNode node, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 2);

        if (node is ProgramNode programNode)
        {
            Console.WriteLine($"{indent}Programa ({programNode.Statements.Count} instrucciones)");
            foreach (var stmt in programNode.Statements)
            {
                PrintAST(stmt, indentLevel + 1);
            }
        }
        else if (node is SpawnNode spawnNode)
        {
            Console.WriteLine($"{indent}SpawnNode: X={spawnNode.X}, Y={spawnNode.Y}");
        }
        else if (node is AssignmentNode assignmentNode)
        {
            Console.WriteLine($"{indent}AssignmentNode: {assignmentNode.Variable} ← ...");
            PrintAST(assignmentNode.Expression, indentLevel + 1);
        }
        else if (node is BinaryExpressionNode binaryNode)
        {
            Console.WriteLine($"{indent}BinaryExpressionNode: {binaryNode.Operator.Lexeme}");
            Console.WriteLine($"{indent}  Left:");
            PrintAST(binaryNode.Left, indentLevel + 2);
            Console.WriteLine($"{indent}  Right:");
            PrintAST(binaryNode.Right, indentLevel + 2);
        }
        else if (node is LiteralNode literalNode)
        {
            Console.WriteLine($"{indent}LiteralNode: {literalNode.Value}");
        }
        else if (node is VariableNode variableNode)
        {
            Console.WriteLine($"{indent}VariableNode: {variableNode.Name}");
        }
        else if (node is FunctionCallNode functionNode)
        {
            Console.WriteLine($"{indent}FunctionCallNode: {functionNode.FunctionName}");
            for (int i = 0; i < functionNode.Arguments.Count; i++)
            {
                Console.WriteLine($"{indent}  Argument {i + 1}:");
                PrintAST(functionNode.Arguments[i], indentLevel + 2);
            }
        }
        else if (node is GoToNode goToNode)
        {
            Console.WriteLine($"{indent}GoToNode: [label: {goToNode.Label}]");
            Console.WriteLine($"{indent}  Condition:");
            PrintAST(goToNode.Condition, indentLevel + 2);
        }
        else if (node is LabelNode labelNode)
        {
            Console.WriteLine($"{indent}LabelNode: {labelNode.LabelName}");
        }
        else
        {
            Console.WriteLine($"{indent}UnknownNode: {node.GetType().Name}");
        }
    }
}
