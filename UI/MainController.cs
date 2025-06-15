using Godot;
using System;

public class MainController : Control
{
    private CodeEditor _codeEditor;
    private PixelCanvas _pixelCanvas;
    private ErrorLabel _errorLabel;
    private Interpreter _interpreter;
    
    private bool _shiftPressed = false;
    private bool _altPressed = false;
    
    private string _defaultFilePath = "user://default.gw";

    public override void _Ready()
    {
        _codeEditor = GetNode<CodeEditor>("CodeEditor");
        _pixelCanvas = GetNode<PixelCanvas>("PixelCanvas");
        _errorLabel = GetNode<ErrorLabel>("ErrorLabel");
        InitializeCanvas(100);
    }
    public void InitializeCanvas(int size)
    {
        _pixelCanvas.SetGridSize(size);
        _pixelCanvas.InitializePixels();
        
        _interpreter = new Interpreter(size);
        _interpreter.WallEState.PixelCanvas = _pixelCanvas;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Scancode == (int)KeyList.Shift)
            {
                _shiftPressed = keyEvent.Pressed;
            }
            else if (keyEvent.Scancode == (int)KeyList.Alt)
            {
                _altPressed = keyEvent.Pressed;
            }
            
            if (_shiftPressed && _altPressed && keyEvent.Pressed)
            {
                if (keyEvent.Scancode != (int)KeyList.Shift && keyEvent.Scancode != (int)KeyList.Alt)
                {
                    ExecuteCode();
                    InitializeCanvas(100);
                }
            }
            
            if (keyEvent.Pressed)
            {
                switch (keyEvent.Scancode)
                {
                    case (int)KeyList.F1:
                        SaveFile();
                        break;
                    case (int)KeyList.F2:
                        LoadFile();
                        break;
                    case (int)KeyList.F3:
                        ResizeCanvas();
                        break;
                }
            }
        }
    }
    private void ExecuteCode()
    {
        try
        {
            _errorLabel.ClearError();
            string code = _codeEditor.Text;
            
            var lexer = new Lexer(code);
            var parser = new Parser(lexer.tokens);
            var program = parser.ParseProgram();
            
            // Reiniciar píxeles antes de ejecutar
            _pixelCanvas.InitializePixels();
            
            program.Execute(_interpreter.GlobalScope);
            _pixelCanvas.Update();
        }
        catch (Exception ex)
        {
            _errorLabel.DisplayError($"Error: {ex.Message}");
        }
    }
    private void SaveFile()
    {
        try
        {
            File file = new File();
            Error err = file.Open(_defaultFilePath, File.ModeFlags.Write);
            
            if (err == Error.Ok)
            {
                file.StoreString(_codeEditor.Text);
                file.Close();
                _errorLabel.DisplayError($"Archivo guardado: {_defaultFilePath}");
            }
            else
            {
                throw new Exception($"Error al guardar: {err}");
            }
        }
        catch (Exception ex)
        {
            _errorLabel.DisplayError($"Error F1: {ex.Message}");
        }
    }
    private void LoadFile()
    {
        try
        {
            File file = new File();
            if (!file.FileExists(_defaultFilePath))
            {
                throw new Exception("Archivo no encontrado");
            }
            
            Error err = file.Open(_defaultFilePath, File.ModeFlags.Read);
            if (err == Error.Ok)
            {
                _codeEditor.Text = file.GetAsText();
                file.Close();
                _errorLabel.DisplayError($"Archivo cargado: {_defaultFilePath}");
            }
            else
            {
                throw new Exception($"Error al cargar: {err}");
            }
        }
        catch (Exception ex)
        {
            _errorLabel.DisplayError($"Error F2: {ex.Message}");
        }
    }
    private void ResizeCanvas()
    {
        try
        {
            int newSize = 100;
            LineEdit sizeInput = GetNodeOrNull<LineEdit>("CanvasDimensionInput");
            
            if (sizeInput != null && int.TryParse(sizeInput.Text, out int parsedSize))
            {
                newSize = parsedSize;
            }
            
            if (newSize < 10) newSize = 10;
            if (newSize > 500) newSize = 500;
            
            InitializeCanvas(newSize);
            
            _errorLabel.DisplayError($"Canvas redimensionado: {newSize}x{newSize}");
        }
        catch (Exception ex)
        {
            _errorLabel.DisplayError($"Error F3: {ex.Message}");
        }
    }
}