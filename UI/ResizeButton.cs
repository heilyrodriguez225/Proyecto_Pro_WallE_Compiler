using Godot;
using System;

public class ResizeButton : Button
{
    private MainController _mainController;
    private LineEdit _dimensionInput;

    public override void _Ready()
    {
        _mainController = GetNode<MainController>("/root/MainController");
        _dimensionInput = GetNode<LineEdit>("../DimensionInput");
        Connect("pressed", this, nameof(OnPressed));
    }
    private void OnPressed()
    {
        if (int.TryParse(_dimensionInput.Text, out int newSize))
        {
            _mainController.InitializeCanvas(newSize);
        }
    }
}