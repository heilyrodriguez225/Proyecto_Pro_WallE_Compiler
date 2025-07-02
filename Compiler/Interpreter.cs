using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

public class Interpreter
{
    public static List<Exception> Error = new List<Exception>();
    private Scope _scope;
    private Lexer _lexer;
    private Parser _parser;
    private Godot.Color[,] _canvas;
    public Interpreter(int canvasSize)
    {
        _scope = new Scope();
        _scope.SetVariable("CanvasSize", canvasSize);
    
    }
    public Godot.Color[,] Execute(string code)
    {
        _canvas = new Godot.Color[(int)_scope.GetVariable("CanvasSize"), (int)_scope.GetVariable("CanvasSize")];
        _lexer = new Lexer(code);
        _parser = new Parser(_lexer.tokens);
        var result = _parser.ParseProgram();
        foreach (var item in Functions.FunctionMap)
        {
            _scope.SetFunction(item.Key, item.Value);
        }
        _scope.SetVariable("Canvas", _canvas);

        result.Execute(_scope);

        return (Godot.Color[,])_scope.GetVariable("Canvas");
    }
}
public class WallEState
{
	public int X { get; set; }
	public int Y { get; set; }
	public string CurrentColor { get; set; } = "White";
	public int BrushSize { get; set; } = 1;
	public ICanvas PixelCanvas { get; set; }
}
