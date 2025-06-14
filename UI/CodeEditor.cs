using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CodeEditor : TextEdit
{
    // ===== CONFIGURACIÓN DE COLORES =====
    private readonly Color _functionColor = new Color(0.16f, 0.55f, 0.88f); // Azul
    private readonly Color _stringColor = new Color(0.13f, 0.67f, 0.33f);   // Verde
    private readonly Color _keywordColor = new Color(0.88f, 0.16f, 0.39f);  // Rosa

    // Lista de funciones y palabras clave
    private readonly string[] _functions = {
        "Spawn", "Color", "Size", "DrawLine", "DrawCircle", "DrawRectangle", "Fill",
        "GetActualX", "GetActualY", "GetCanvasSize", "GetColorCount", "IsBrushColor",
        "IsBrushSize", "IsCanvasColor", "GoTo"
    };

    // ===== AUTOCOMPLETADO =====
    private PopupMenu _autoCompleteMenu;
    private List<string> _autoCompleteOptions = new List<string>();
    private int _autoCompleteStart = -1;

    public override void _Ready()
    {
        // Configuración básica
        ShowLineNumbers = true;
        HighlightCurrentLine = true;
        SyntaxHighlighting = true;
        
        // Crear menú de autocompletado
        _autoCompleteMenu = new PopupMenu();
        AddChild(_autoCompleteMenu);
        _autoCompleteMenu.Connect("id_pressed", this, nameof(OnAutoCompleteSelected));
        
        // Conectar señales
        Connect("text_changed", this, nameof(OnTextChanged));
        Connect("gui_input", this, nameof(OnGuiInput));

        // Configurar colores de sintaxis
        AddColorRegion("\"", "\"", _stringColor);
        foreach (string function in _functions)
        {
            AddKeywordColor(function, _functionColor);
        }
    }

    // ===== MANEJO DE TEXTO =====
    private void OnTextChanged()
    {
        CheckAutoComplete();
    }

    // ===== AUTOCOMPLETADO =====
    private void CheckAutoComplete()
    {
        int line = CursorGetLine();
        int column = CursorGetColumn();
        string lineText = GetLine(line).Substring(0, column);

        // Buscar palabra parcial
        if (lineText.Length > 0 && char.IsLetter(lineText[^1]))
        {
            int start = lineText.Length - 1;
            while (start > 0 && (char.IsLetterOrDigit(lineText[start - 1]) || lineText[start - 1] == '-'))
            {
                start--;
            }

            string partial = lineText.Substring(start);
            _autoCompleteOptions.Clear();

            // Buscar coincidencias
            foreach (string function in _functions)
            {
                if (function.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
                {
                    _autoCompleteOptions.Add(function);
                }
            }

            if (_autoCompleteOptions.Count > 0)
            {
                _autoCompleteMenu.Clear();
                foreach (string option in _autoCompleteOptions)
                {
                    _autoCompleteMenu.AddItem(option);
                }

                Vector2 cursorPos = new Vector2(
                    GetColumnXOffset(line, column),
                    (line + 1) * GetLineHeight()
                );

                _autoCompleteStart = start;
                _autoCompleteMenu.RectPosition = cursorPos;
                _autoCompleteMenu.Popup_();
            }
        }
        else
        {
            _autoCompleteMenu.Hide();
        }
    }

    private void OnAutoCompleteSelected(int id)
    {
        string selected = _autoCompleteMenu.GetItemText(id);
        int line = CursorGetLine();
        int column = CursorGetColumn();
        
        // Reemplazar texto parcial
        string lineText = GetLine(line);
        string newLine = lineText.Substring(0, _autoCompleteStart) + selected + lineText.Substring(column);
        SetLine(line, newLine);
        
        // Posicionar cursor después de la función
        CursorSetColumn(_autoCompleteStart + selected.Length);
        _autoCompleteMenu.Hide();
    }

    // ===== MANEJO DE ENTRADA =====
    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Tab → 4 espacios
            if (keyEvent.Scancode == (uint)KeyList.Tab)
            {
                InsertTextAtCursor("    ");
                GetTree().SetInputAsHandled();
            }
            // Enter en autocompletado
            else if (keyEvent.Scancode == (uint)KeyList.Enter && _autoCompleteMenu.Visible)
            {
                OnAutoCompleteSelected(_autoCompleteMenu.GetSelectedId());
                GetTree().SetInputAsHandled();
            }
        }
    }
}
