using Godot;
using System;
using System.Collections.Generic;

public class Interpreter
{
    public static List<Exception> Error = new List<Exception>();
    public WallEState WallEState { get; } = new WallEState();
    private Scope _scope;
    public Interpreter(int canvasSize, Scope scope)
    {
        _scope = scope;
        _scope.SetVariable("CanvasSize", canvasSize);
        
        // 2. Inicializar estado de Wall-E
        WallEState.PixelCanvas = new PixelCanvas();
        WallEState.X = 0;
        WallEState.Y = 0;
        WallEState.CurrentColor = "White";
        WallEState.BrushSize = 1;
        
        _scope.SetVariable("WallEState",WallEState);
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
