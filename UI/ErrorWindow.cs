using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class ErrorWindow : TextEdit
{
    public override void _Ready()
    {
        Text = "";
        Visible = false;
    }

    public void DisplayErrors()
    {
        if (Interpreter.Error.Count == 0)
        {
            Text = "";
            Visible = false;
            return;
        }

        // Unir todos los mensajes de error en una sola cadena
        string errorText = "ERRORES:\n";
        errorText += string.Join("\n", Interpreter.Error.Select(e => e.Message));

        Text = errorText;
        Visible = true;
    }
    
    public void ClearErrors()
    {
        Text = "";
        Visible = false;
    }
}