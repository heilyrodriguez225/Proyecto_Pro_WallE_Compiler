public class Interpreter
{
    public Dictionary<string, object> Variables { get; } = new Dictionary<string, object>();
    public WallEState WallEState { get; } = new WallEState();
    public Interpreter(int canvasSize)
    {
        WallEState.Canvas = new CanvasDummy(canvasSize);
        Variables["CanvasSize"] = canvasSize;
        Variables["WallEState"] = WallEState;
    }
}
public class WallEState
{
    public int X { get; set; }
    public int Y { get; set; }
    public string CurrentColor { get; set; } = "Transparent";
    public int BrushSize { get; set; } = 1;
    public ICanvas Canvas { get; set; }
}
