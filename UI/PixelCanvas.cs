using Godot;
using System;
using System.Collections.Generic;

public class PixelCanvas : Control, ICanvas
{
    [Export] public int CanvasSize { get; set; } = 100;
    [Export] public Color BackgroundColor { get; set; } = Colors.White;
    [Export] public bool ShowGrid { get; set; } = true;
    [Export] public Color GridColor { get; set; } = new Color(0.8f, 0.8f, 0.8f, 0.3f);
    
    public int Size => CanvasSize;  
    
    private Color[,] _pixels;  // Matriz de colores para cada píxel
    private float _pixelSize;  // Tamaño visual de cada píxel
    private ImageTexture _texture;  // Textura para renderizado
    private bool _needsRedraw = true;  // Bandera de optimización

    private static readonly Dictionary<string, Color> ColorMap = new Dictionary<string, Color>
    {
        {"Red", Colors.Red},
        {"Blue", Colors.Blue},
        {"Green", Colors.Green},
        {"Yellow", Colors.Yellow},
        {"Orange", new Color(1, 0.65f, 0)},   // Naranja
        {"Purple", new Color(0.5f, 0, 0.5f)}, // Morado
        {"Black", Colors.Black},
        {"White", Colors.White},
        {"Transparent", Colors.Transparent}
    };

    // ===== INICIALIZACIÓN =====
    public override void _Ready()
    {
        InitializeCanvas();
        SetMinSize(new Vector2(400, 400));  // Tamaño mínimo del control
    }

    private void InitializeCanvas()
    {
        _pixels = new Color[CanvasSize, CanvasSize];
        Clear(); 
    }

    // Redimensionar el canvas (llamado desde UI)
    public void Resize(int newSize)
    {
        CanvasSize = newSize;
        InitializeCanvas();
        _needsRedraw = true;
        Update(); 
    }

    public void Clear()
    {
        for (int y = 0; y < CanvasSize; y++)
            for (int x = 0; x < CanvasSize; x++)
                _pixels[x, y] = BackgroundColor;
        
        _needsRedraw = true;
        Update();
    }

    public void SetPixel(int x, int y, string colorName)
    {
        if (IsWithinBounds(x, y) && ColorMap.ContainsKey(colorName))
        {
            _pixels[x, y] = ColorMap[colorName];
            _needsRedraw = true;  // Marcar para redibujar
        }
    }
    public string GetPixel(int x, int y)
    {
        if (!IsWithinBounds(x, y)) return "White";
        
        // Buscar el nombre del color (ineficiente pero funcional)
        foreach (var pair in ColorMap)
            if (_pixels[x, y] == pair.Value)
                return pair.Key;
        
        return "White";
    }
    // Dibujar línea con algoritmo Bresenham
    public void DrawLine(int startX, int startY, int endX, int endY, string color, int brushSize)
    {
        if (!ColorMap.ContainsKey(color)) return;
        
        // Ajustar tamaño impar del pincel
        brushSize = Math.Max(1, brushSize % 2 == 0 ? brushSize - 1 : brushSize);
        int radius = (brushSize - 1) / 2;
        
        // Algoritmo de línea
        int dx = Math.Abs(endX - startX);
        int dy = Math.Abs(endY - startY);
        int sx = startX < endX ? 1 : -1;
        int sy = startY < endY ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            DrawBrush(startX, startY, color, radius);
            if (startX == endX && startY == endY) break;
            
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                startX += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                startY += sy;
            }
        }
    }

    // Dibujar un "pincel" (grupo de píxeles)
    private void DrawBrush(int centerX, int centerY, string color, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int px = centerX + x;
                int py = centerY + y;
                
                if (IsWithinBounds(px, py))
                {
                    _pixels[px, py] = ColorMap[color];
                }
            }
        }
        _needsRedraw = true;
    }

    // Dibujar círculo con algoritmo de punto medio
    public void DrawCircle(int centerX, int centerY, int radius, string color, int brushSize)
    {
        if (!ColorMap.ContainsKey(color)) return;
        
        int x = radius;
        int y = 0;
        int err = 0;
        Color col = ColorMap[color];

        while (x >= y)
        {
            DrawBrush(centerX + x, centerY + y, color, brushSize);
            DrawBrush(centerX + y, centerY + x, color, brushSize);
			DrawBrush(centerX - x, centerY + y, color, brushSize);
			DrawBrush(centerX + x, centerY - y, color, brushSize);
			DrawBrush(centerX - y, centerY + x, color, brushSize);
			DrawBrush(centerX + y, centerY - x, color, brushSize);
			DrawBrush(centerX - x, centerY - y, color, brushSize);
			DrawBrush(centerX - y, centerY - x, color, brushSize);
            
            y++;
            err += 1 + 2 * y;
            if (2 * (err - x) + 1 > 0)
            {
                x--;
                err += 1 - 2 * x;
            }
        }
    }

    // Rellenado por inundación (Flood Fill)
    public void FloodFill(int x, int y, string color)
    {
        if (!IsWithinBounds(x, y) || !ColorMap.ContainsKey(color)) return;
        
        Color targetColor = _pixels[x, y];
        Color newColor = ColorMap[color];
        if (targetColor == newColor) return;

        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(new Vector2(x, y));
        
        while (stack.Count > 0)
        {
            Vector2 p = stack.Pop();
            int px = (int)p.x;
            int py = (int)p.y;
            
            if (!IsWithinBounds(px, py) || _pixels[px, py] != targetColor)
                continue;
            
            _pixels[px, py] = newColor;
            
            stack.Push(new Vector2(px + 1, py));
            stack.Push(new Vector2(px - 1, py));
            stack.Push(new Vector2(px, py + 1));
            stack.Push(new Vector2(px, py - 1));
        }
        _needsRedraw = true;
    }

    // Verificar límites del canvas
    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < CanvasSize && y >= 0 && y < CanvasSize;
    }

    // ===== RENDERIZADO =====
    public override void _Draw()
    {
        if (CanvasSize <= 0) return;
        
        // Calcular tamaño de píxel basado en el control
        Vector2 size = RectSize;
        _pixelSize = Mathf.Min(size.x, size.y) / CanvasSize;
        
        // Crear/actualizar textura
        if (_texture == null || _needsRedraw)
        {
            RenderToTexture();
            _needsRedraw = false;
        }
        
        // Dibujar textura
        DrawTextureRect(_texture, new Rect2(Vector2.Zero, size), false);
        
        // Dibujar cuadrícula si está habilitada
        if (ShowGrid)
        {
            DrawGrid();
        }
    }

    // Convertir matriz de píxeles a textura
    private void RenderToTexture()
    {
        Image img = new Image();
        img.Create(CanvasSize, CanvasSize, false, Image.Format.Rgba8);
        
        img.Lock();
        for (int y = 0; y < CanvasSize; y++)
        {
            for (int x = 0; x < CanvasSize; x++)
            {
                img.SetPixel(x, y, _pixels[x, y]);
            }
        }
        img.Unlock();
        
        _texture = new ImageTexture();
        _texture.CreateFromImage(img);
    }

    // Dibujar cuadrícula
    private void DrawGrid()
    {
        // Líneas verticales
        for (int x = 0; x <= CanvasSize; x++)
        {
            float posX = x * _pixelSize;
            DrawLine(
                new Vector2(posX, 0),
                new Vector2(posX, CanvasSize * _pixelSize),
                GridColor
            );
        }
        
        // Líneas horizontales
        for (int y = 0; y <= CanvasSize; y++)
        {
            float posY = y * _pixelSize;
            DrawLine(
                new Vector2(0, posY),
                new Vector2(CanvasSize * _pixelSize, posY),
                GridColor
            );
        }
    }
    public int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        if (!ColorMap.ContainsKey(color)) return 0;
        
        Color target = ColorMap[color];
        int count = 0;
        int minX = Mathf.Clamp(Math.Min(x1, x2), 0, CanvasSize - 1);
        int maxX = Mathf.Clamp(Math.Max(x1, x2), 0, CanvasSize - 1);
        int minY = Mathf.Clamp(Math.Min(y1, y2), 0, CanvasSize - 1);
        int maxY = Mathf.Clamp(Math.Max(y1, y2), 0, CanvasSize - 1);
        
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (_pixels[x, y] == target) count++;
            }
        }
        return count;
    }
    
    public bool IsColor(int x, int y, string color)
    {
        return IsWithinBounds(x, y) && 
               ColorMap.ContainsKey(color) && 
               _pixels[x, y] == ColorMap[color];
    }
}