using Godot;
using System;

public class RunButton : Button
{
    private MainController _mainController;

    public override void _Ready()
    {
        _mainController = GetNode<MainController>("/root/Main");
        Connect("pressed", this, nameof(OnPressed));
    }

    private void OnPressed()
    {
        _mainController.ExecuteCode();
        _mainController.InitializeCanvas(size);
    }
}
