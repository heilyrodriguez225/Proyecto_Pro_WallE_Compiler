using Godot;
using System;

public class ErrorLabel : Label
{
    private static ErrorLabel _instance;
    public static ErrorLabel Instance => _instance;
    
    public override void _Ready()
    {
        _instance = this;
        Text = "";
        Visible = false;
        
    }
    public static void ShowException(Exception exception)
    {
        if (_instance != null)
        {
            _instance.DisplayError(exception.Message);
        }
        else
        {
            GD.PushError("ErrorLabel no instanciado!");
        }
    }
    
    public void DisplayError(string errorMessage)
    {
        Text = $"ERROR: {errorMessage}";
        Visible = true;
    }
    
    public void ClearError()
    {
        Text = "";
        Visible = false;
    }
}