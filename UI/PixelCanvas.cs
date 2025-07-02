using Godot;
using System;
using System.Collections.Generic;

public class PixelCanvas : TextureRect, ICanvas
{
    [Export] private int _gridSize = 100;   // Tamaño de la cuadrícula
    [Export] private bool _showGrid = true;  // Mostrar líneas de cuadrícula
    [Export] private Color _gridColor = new Color(0.8f, 0.8f, 0.8f, 0.3f); // Color de la cuadrícula
    private Color[,] _pixels; // Matriz de colores para los píxeles

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
    private float _cellSize;    // Tamaño de cada celda en píxeles
    public int Size => _gridSize; // Implementación de ICanvas
    public override void _Ready()
    {
        RectMinSize = new Vector2(400, 400); // Tamaño mínimo
    }
    public void SetGridSize(int newSize, Color[,] canvas)
    {
        _gridSize = newSize;
        canvas = new Color[_gridSize, _gridSize];
        for(int i = 0; i < _gridSize; i++)
        {
            for(int j = 0; j < _gridSize; j++)
            {
                canvas[i, j] = Colors.White; // Inicializar con blanco
            }
        }
        _pixels = canvas;
        Update();
    }
    public void SetPixel(int x, int y, string colorName, Color[,] canvas)
    {
        if (!IsWithinBounds(x, y)) return;
        
        if (!_colorMap.ContainsKey(colorName))
        {
            Interpreter.Error.Add(new Exception ($"Color no válido: {colorName}"));
            return;
        }
        canvas[x, y] = _colorMap[colorName];
        _pixels = canvas;
        Update(); // Actualizar visualización
    }
    public string GetPixel(int x, int y, Color[,] canvas)
    {
        if (!IsWithinBounds(x, y)) return "White";
        foreach (var pair in _colorMap)
        {
            if (canvas[x, y] == pair.Value)
                return pair.Key;
        }
        return "White"; 
    }
    public void DrawLine(int startX, int startY, int endX, int endY, string color, int brushSize, Color[,] canvas)
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
            DrawBrush(startX, startY, colorValue, radius,canvas);
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
            DrawBrush(currentX, currentY, colorValue, radius,canvas);
        }
        _pixels = canvas;
    }
    private void DrawBrush(int centerX, int centerY, Color color, int radius, Color[,] canvas)
    {
        for (int offsetX = -radius; offsetX <= radius; offsetX++)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                int x = centerX + offsetX;
                int y = centerY + offsetY;
                
                if (IsWithinBounds(x, y))
                {
                    canvas[x, y] = color;
                }
            }
        }
        _pixels = canvas;
    }
    public void DrawCircle(int centerX, int centerY, int radius, string color, int brushSize, Color[,] canvas)
    {
        // Validar color
        if (!_colorMap.ContainsKey(color)) return;
        Color colorValue = _colorMap[color];

        int brushRadius = (brushSize - 1) / 2;

        for (double angle = 0; angle < 2 * Math.PI; angle += 0.01 / radius)
        {
            int x = (int)Math.Round(centerX + radius * Math.Cos(angle));
            int y = (int)Math.Round(centerY + radius * Math.Sin(angle));
            
            // Dibujar pincel en cada punto de la circunferencia
            for (int i = -brushRadius; i <= brushRadius; i++)
            {
                for (int j = -brushRadius; j <= brushRadius; j++)
                {
                    int targetX = x + i;
                    int targetY = y + j;
                    
                    if (IsWithinBounds(targetX, targetY))
                    {
                        canvas[targetX, targetY] = colorValue;
                    }
                }
            }
        }
        _pixels = canvas;
        Update();
    }
    public void DrawRectangle(int centerX, int centerY, int width, int height, string color, int brushSize, Color[,] canvas)
    {
        // Validar color
        if (!_colorMap.ContainsKey(color)) return;
        Color colorValue = _colorMap[color];
        
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        int left = centerX - halfWidth;
        int right = centerX + halfWidth;
        int top = centerY - halfHeight;
        int bottom = centerY + halfHeight;

        if (IsWithinBounds(width, height))
        {
            DrawLine(left, top, right, top, color, brushSize, canvas);
            DrawLine(right, top, right, bottom, color, brushSize, canvas);
            DrawLine(right, bottom, left, bottom, color, brushSize, canvas);
            DrawLine(left, bottom, left, top, color, brushSize, canvas);    
        }
        _pixels = canvas;
        Update();
    }
    public int GetColorCount(string color, int x1, int y1, int x2, int y2, Color[,] canvas)
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
                if (IsWithinBounds(x, y) && canvas[x, y] == targetColor)
                {
                    count++;
                }
            }
        }
        _pixels = canvas;
        return count;
    }
    public bool IsColor(int targetX, int targetY, string color, Color[,] canvas)
    {
        if (!_colorMap.ContainsKey(color)) return false;
        if (!IsWithinBounds(targetX, targetY)) return false;
        _pixels = canvas;
        return canvas[targetX, targetY] == _colorMap[color];
    }
    public void FloodFill(int x, int y, string color, Color[,] canvas)
    {
        if (!IsWithinBounds(x, y)) return;
        if (!_colorMap.ContainsKey(color)) return;
        
        Color targetColorValue = canvas[x, y];
        Color newColorValue = _colorMap[color];
        
        if (targetColorValue == newColorValue) return;
        
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(new Vector2(x, y));
        
        while (stack.Count > 0)
        {
            Vector2 pos = stack.Pop();
            int px = (int)pos.x;
            int py = (int)pos.y;
            
            if (!IsWithinBounds(px, py) || canvas[px, py] != targetColorValue)
                continue;
            
            canvas[px, py] = newColorValue;
            
            stack.Push(new Vector2(px + 1, py));
            stack.Push(new Vector2(px - 1, py));
            stack.Push(new Vector2(px, py + 1));
            stack.Push(new Vector2(px, py - 1));
        }
        _pixels = canvas;
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