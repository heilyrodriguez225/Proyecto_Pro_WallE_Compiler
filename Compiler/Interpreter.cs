using Godot;
using System;
using System.Collections.Generic;

public class Interpreter
{
    public Dictionary<string, object> GlobalScope { get; } = new Dictionary<string, object>();
    public WallEState WallEState { get; } = new WallEState();

    public Interpreter(int canvasSize)
    {
        GlobalScope["CanvasSize"] = canvasSize;
        
        // 2. Inicializar estado de Wall-E
        WallEState.PixelCanvas = new PixelCanvas();
        WallEState.X = 0;
        WallEState.Y = 0;
        WallEState.CurrentColor = "White";
        WallEState.BrushSize = 1;
        
        GlobalScope["WallEState"] = WallEState;
        GlobalScope["CurrentStatementIndex"] = -1;
        GlobalScope["LastError"] = null;
    
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
