using Godot;
using System;
using System.Collections.Generic;

public class PixelCanvas : Control, ICanvas
{
    [Export] private int _gridSize = 100;   
    [Export] private bool _showGrid = true;      
    [Export] private Color _gridColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);
    private Dictionary<string, Color> _colorMap = new Dictionary<string, Color>()
    {
        {"Red", Colors.Red},
        {"Blue", Colors.Blue},
        {"Green", Colors.Green},
        {"Yellow", Colors.Yellow},
        {"Orange", new Color(1, 0.65f, 0)},
        {"Purple", new Color(0.5f, 0, 0.5f)},
        {"Black", Colors.Black},
        {"White", Colors.White},
        {"Transparent", Colors.Transparent}
    };
    private Color[,] _pixels; 
    private float _cellSize;     
    public int Size => _gridSize;
    public override void _Ready()
    {
        RectMinSize = new Vector2(400, 400);
        
        InitializePixels();
    }
    private void InitializePixels()
    {
        _pixels = new Color[_gridSize, _gridSize];

        for (int y = 0; y < _gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                _pixels[x, y] = Colors.White;
            }
        }
        Update();
    }
    public void SetGridSize(int newSize)
    {
        _gridSize = newSize;
        InitializePixels();
        Update();
    }
    public void SetPixel(int x, int y, string colorName)
    {
        if (IsWithinBounds(x, y) && _colorMap.ContainsKey(colorName))
        {
            _pixels[x, y] = _colorMap[colorName];
            Update();
        }
    }
    public string GetPixel(int x, int y)
    {
        if (!IsWithinBounds(x, y)) return "White";
        foreach (var pair in _colorMap)
        {
            if (_pixels[x, y] == pair.Value)
                return pair.Key;
        }
        return "White";
    }

    public void DrawLine(int startX, int startY, int endX, int endY, string color, int brushSize)
    {
        if (brushSize % 2 == 0) brushSize--;
        if (brushSize < 1) brushSize = 1;
        int radius = (brushSize - 1) / 2;
        
        // Calcular la distancia total entre los puntos
        float distance = (float)Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2));
      
        if (distance == 0)
        {
            DrawBrush(startX, startY, color, radius);
            return;
        }
        float incrementX = (endX - startX) / distance;
        float incrementY = (endY - startY) / distance;
        
        for (float step = 0; step <= distance; step += 0.5f) 
        {
            int currentX = (int)Math.Round(startX + incrementX * step);
            int currentY = (int)Math.Round(startY + incrementY * step);
            DrawBrush(currentX, currentY, color, radius);
        }
    }
    private void DrawBrush(int centerX, int centerY, string color, int radius)
    {
        // Pintar todos los píxeles dentro del área del pincel
        for (int offsetX = -radius; offsetX <= radius; offsetX++)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                int x = centerX + offsetX;
                int y = centerY + offsetY;
                
                if (IsWithinBounds(x, y))
                {
                    grid[x, y] = color;
                }
            }
        }
    }
    public void DrawCircle(int centerX, int centerY, int radius, string color, int brushSize)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = centerX + x;
                    int py = centerY + y;
                    if (IsWithinBounds(px, py))
                    {
                        grid[px, py] = color;
                    }
                }
            }
        }
    }
    public void DrawRectangle(int centerX, int centerY, int width, int height, string color, int brushSize)
    {
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        
        for (int y = -halfHeight; y <= halfHeight; y++)
        {
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                int px = centerX + x;
                int py = centerY + y;
                
                if (IsWithinBounds(px, py))
                {
                    grid[px, py] = color;
                }
            }
        }
    }
    public int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        int count = 0;
        int minX = Math.Min(x1, x2);
        int maxX = Math.Max(x1, x2);
        int minY = Math.Min(y1, y2);
        int maxY = Math.Max(y1, y2);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (IsWithinBounds(x, y) && grid[x, y] == color)
                {
                    count++;
                }
            }
        }
        return count;
    }
    public bool IsColor(int targetX, int targetY, string color)
    {
        return IsWithinBounds(targetX, targetY) && grid[targetX, targetY] == color;
    }
    public void FloodFill(int x, int y, string color)
    {
        if (!IsWithinBounds(x, y)) return;
        
        string targetColor = grid[x, y];
        if (targetColor == color) return;
        
        Stack<(int, int)> stack = new Stack<(int, int)>();
        stack.Push((x, y));
        
        while (stack.Count > 0)
        {
            var (cx, cy) = stack.Pop();
            if (!IsWithinBounds(cx, cy) || grid[cx, cy] != targetColor) continue;
            
            grid[cx, cy] = color;
            
            stack.Push((cx + 1, cy));
            stack.Push((cx - 1, cy));
            stack.Push((cx, cy + 1));
            stack.Push((cx, cy - 1));
        }
    }
    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < Size && y >= 0 && y < Size;
    }
    public override void _Draw()
    {
        _cellSize = Mathf.Min(RectSize.x, RectSize.y) / _gridSize;
        
        for (int y = 0; y < _gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                Vector2 position = new Vector2(x * _cellSize, y * _cellSize);
                DrawRect(new Rect2(position, new Vector2(_cellSize, _cellSize)), _pixels[x, y]);
            }
        }
        if (_showGrid)
        {
            DrawGrid();
        }
    }
    private void DrawGrid()
    {
        for (int x = 0; x <= _gridSize; x++)
        {
            float posX = x * _cellSize;
            DrawLine(
                new Vector2(posX, 0),
                new Vector2(posX, _gridSize * _cellSize),
                _gridColor
            );
        }
        for (int y = 0; y <= _gridSize; y++)
        {
            float posY = y * _cellSize;
            DrawLine(
                new Vector2(0, posY),
                new Vector2(_gridSize * _cellSize, posY),
                _gridColor
            );
        }
    }
}