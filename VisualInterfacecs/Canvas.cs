public interface ICanvas
{
    int Size { get; }
    void SetPixel(int x, int y, string color);
    string GetPixel(int x, int y);
    void DrawLine(int startX, int startY, int endX, int endY, string color, int brushSize);
    void DrawCircle(int centerX, int centerY, int radius, string color, int brushSize);
    void DrawRectangle(int centerX, int centerY, int width, int height, string color, int brushSize);
    int GetColorCount(string color, int x1, int y1, int x2, int y2);
    bool IsColor(int targetX, int targetY, string color);
    void FloodFill(int x, int y, string color);
}
public class CanvasDummy : ICanvas
{
    public int Size { get; private set; } = 256;
    public CanvasDummy(int size)
    {
        Size = size;
    }
    public void SetPixel(int x, int y, string color) { }
    public string GetPixel(int x, int y) => "White";
    public void DrawLine(int startX, int startY, int endX, int endY, string color, int brushSize) 
    { 
        Console.WriteLine($"Dibujando línea: ({startX},{startY}) -> ({endX},{endY})");
    }
    public void DrawCircle(int centerX, int centerY, int radius, string color, int brushSize) 
    {
        Console.WriteLine($"Dibujando círculo: centro ({centerX},{centerY}), radio {radius}");
    }
    public void DrawRectangle(int centerX, int centerY, int width, int height, string color, int brushSize) 
    {
        Console.WriteLine($"Dibujando rectángulo: centro ({centerX},{centerY}), {width}x{height}");
    }
    public void FloodFill(int x, int y, string color) 
    {
        Console.WriteLine($"Relleno en ({x},{y}) con {color}");
    }
    public int GetColorCount(string color, int x1, int y1, int x2, int y2) => 0;
    public bool IsColor(int x, int y, string color) => false;
} 
    