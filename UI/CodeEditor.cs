using Godot;
using System;

public class CodeEditor : TextEdit
{
    private readonly Color _functionColor = new Color(0.16f, 0.55f, 0.88f); // Azul
    private readonly Color _stringColor = new Color(0.13f, 0.67f, 0.33f);   // Verde
    private readonly string[] _functions = {
        "Spawn", "Color", "Size", "DrawLine", "DrawCircle", "DrawRectangle", "Fill",
        "GetActualX", "GetActualY", "GetCanvasSize", "GetColorCount", "IsBrushColor",
        "IsBrushSize", "IsCanvasColor", "GoTo"
    };

    public override void _Ready()
    {
        ShowLineNumbers = true;       // Mostrar números de línea
        HighlightCurrentLine = true;   // Resaltar línea actual
        SyntaxHighlighting = true;     // Activar resaltado de sintaxis
        
        // 1. Región para strings (entre comillas dobles)
        AddColorRegion("\"", "\"", _stringColor);
        
        // 2. Palabras clave (funciones)
        foreach (string function in _functions)
        {
            AddKeywordColor(function, _functionColor);
        }
    }
}