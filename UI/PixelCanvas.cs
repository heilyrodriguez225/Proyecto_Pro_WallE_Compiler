using Godot;
using System;
using System.Collections.Generic;

public class PixelCanvas : Control, ICanvas
{
    [Export] private int _gridSize = 100;   // Tamaño de la cuadrícula
    [Export] private bool _showGrid = true;  // Mostrar líneas de cuadrícula
    [Export] private Color _gridColor = new Color(0.8f, 0.8f, 0.8f, 0.3f); // Color de la cuadrícula
    
    private Dictionary<string, Color> _colorMap = new Dictionary<string, Color>()
    {
        {"Red", Colors.Red},
        {"Blue", Colors.Blue},
        {"Green", Colors.Green},
        {"Yellow", Colors.Yellow},
        {"Orange", new Color(1, 0.65f, 0)}, // Naranja personalizado
        {"Purple", new Color(0.5f, 0, 0.5f)}, // Púrpura personalizado
        {"Black", Colors.Black},
        {"White", Colors.White},
        {"Transparent", Colors.Transparent}
    };
    private Color[,] _pixels;  // Matriz de colores de píxeles
    private float _cellSize;    // Tamaño de cada celda en píxeles
    public int Size => _gridSize; // Implementación de ICanvas
    public override void _Ready()
    {
        RectMinSize = new Vector2(400, 400); // Tamaño mínimo
        InitializePixels();
    }
    public void InitializePixels()
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
        if (!IsWithinBounds(x, y)) return;
        
        if (!_colorMap.ContainsKey(colorName))
        {
            Interpreter.Error.Add(new Exception ($"Color no válido: {colorName}"));
            return;
        }
        _pixels[x, y] = _colorMap[colorName];
        Update(); // Actualizar visualización
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
        if (!_colorMap.ContainsKey(color))
        {
            Interpreter.Error.Add( new Exception ($"Color no válido: {color}"));
            return;
        }
        Color colorValue = _colorMap[color];
        // Ajustar tamaño impar del pincel
        if (brushSize % 2 == 0) brushSize--;
        if (brushSize < 1) brushSize = 1;
        int radius = (brushSize - 1) / 2;
        // Calcular dirección y distancia
        float dx = endX - startX;
        float dy = endY - startY;
        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        
        // Caso especial: distancia cero
        if (Mathf.IsZeroApprox(distance))
        {
            DrawBrush(startX, startY, colorValue, radius);
            return;
        }
        // Calcular incrementos
        float incrementX = dx / distance;
        float incrementY = dy / distance;
        // Dibujar puntos a lo largo de la línea
        for (float step = 0; step <= distance; step += 0.5f)
        {
            int currentX = (int)Math.Round(startX + incrementX * step);
            int currentY = (int)Math.Round(startY + incrementY * step);
            DrawBrush(currentX, currentY, colorValue, radius);
        }
    }
    private void DrawBrush(int centerX, int centerY, Color color, int radius)
    {
        for (int offsetX = -radius; offsetX <= radius; offsetX++)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                int x = centerX + offsetX;
                int y = centerY + offsetY;
                
                if (IsWithinBounds(x, y))
                {
                    _pixels[x, y] = color;
                }
            }
        }
    }
    public void DrawCircle(int centerX, int centerY, int radius, string color, int brushSize)
    {
        // Validar color
        if (!_colorMap.ContainsKey(color)) return;
        Color colorValue = _colorMap[color];
        
        for (int yOffset = -radius; yOffset <= radius; yOffset++)
        {
            for (int xOffset = -radius; xOffset <= radius; xOffset++)
            {
                // Verificar si está dentro del círculo
                if (xOffset * xOffset + yOffset * yOffset <= radius * radius)
                {
                    int x = centerX + xOffset;
                    int y = centerY + yOffset;
                    
                    if (IsWithinBounds(x, y))
                    {
                        _pixels[x, y] = colorValue;
                    }
                }
            }
        }
        Update();
    }
    public void DrawRectangle(int centerX, int centerY, int width, int height, string color, int brushSize)
    {
        // Validar color
        if (!_colorMap.ContainsKey(color)) return;
        Color colorValue = _colorMap[color];
        
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        
        for (int yOffset = -halfHeight; yOffset <= halfHeight; yOffset++)
        {
            for (int xOffset = -halfWidth; xOffset <= halfWidth; xOffset++)
            {
                int x = centerX + xOffset;
                int y = centerY + yOffset;
                
                if (IsWithinBounds(x, y))
                {
                    _pixels[x, y] = colorValue;
                }
            }
        }
        Update();
    }
    public int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        if (!_colorMap.ContainsKey(color)) return 0;
        Color targetColor = _colorMap[color];
        
        int minX = Math.Min(x1, x2);
        int maxX = Math.Max(x1, x2);
        int minY = Math.Min(y1, y2);
        int maxY = Math.Max(y1, y2);
        
        int count = 0;
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (IsWithinBounds(x, y) && _pixels[x, y] == targetColor)
                {
                    count++;
                }
            }
        }
        return count;
    }
    public bool IsColor(int targetX, int targetY, string color)
    {
        if (!_colorMap.ContainsKey(color)) return false;
        if (!IsWithinBounds(targetX, targetY)) return false;
        
        return _pixels[targetX, targetY] == _colorMap[color];
    }
    public void FloodFill(int x, int y, string color)
    {
        if (!IsWithinBounds(x, y)) return;
        if (!_colorMap.ContainsKey(color)) return;
        
        Color targetColorValue = _pixels[x, y];
        Color newColorValue = _colorMap[color];
        
        if (targetColorValue == newColorValue) return;
        
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(new Vector2(x, y));
        
        while (stack.Count > 0)
        {
            Vector2 pos = stack.Pop();
            int px = (int)pos.x;
            int py = (int)pos.y;
            
            if (!IsWithinBounds(px, py) || _pixels[px, py] != targetColorValue)
                continue;
            
            _pixels[px, py] = newColorValue;
            
            stack.Push(new Vector2(px + 1, py));
            stack.Push(new Vector2(px - 1, py));
            stack.Push(new Vector2(px, py + 1));
            stack.Push(new Vector2(px, py - 1));
        }
        Update();
    }
    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < _gridSize && y >= 0 && y < _gridSize;
    }
    public override void _Draw()
    {
        _cellSize = Mathf.Min(RectSize.x, RectSize.y) / _gridSize;
        
        // Dibujar todos los píxeles
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
            DrawLine(new Vector2(posX, 0), new Vector2(posX, _gridSize * _cellSize), _gridColor);
        }
        for (int y = 0; y <= _gridSize; y++)
        {
            float posY = y * _cellSize;
            DrawLine(new Vector2(0, posY), new Vector2(_gridSize * _cellSize, posY), _gridColor);
        }
    }
}