using Godot;
using System;

public class MainController : Node2D
{
    private CodeEditor _codeEditor;
    private ErrorWindow _errorWindow;
    private PixelCanvas _pixelCanvas;
    private LineEdit _canvasDimensionInput;
    Scope scope = new Scope();
    private Interpreter _interpreter;
    private string _defaultFilePath = "user://default.pw";

    public override void _Ready()
    {
        _codeEditor = GetNode<CodeEditor>("CodeEditor");
        _errorWindow = GetNode<ErrorWindow>("ErrorWindow");
        _pixelCanvas = GetNode<PixelCanvas>("PixelCanvas");
        _canvasDimensionInput = GetNode<LineEdit>("VBoxContainer/HBoxContainer/CanvasDimensionInput");
        _interpreter = new Interpreter(100, scope);
    }
    private void _on_Run_pressed()
    {
        try
		{
            _errorWindow.ClearErrors();
			string code = _codeEditor.Text;
			
			var lexer = new Lexer(code);
			var parser = new Parser(lexer.tokens);
            
			var program = parser.ParseProgram();

            foreach (var function in Functions.FunctionMap)
            {
                scope.SetFunction(function.Key, function.Value);
            }

			_pixelCanvas.InitializePixels();
            program.Execute(scope);
			_pixelCanvas.Update();
		}
		catch (Exception ex)
		{
			Interpreter.Error.Add(ex);
            _errorWindow.DisplayErrors();
		}
	}
    private void _on_Save_pressed()
    {
        try
        {
            _errorWindow.ClearErrors();
            using (File file = new File())
            {
                Error err = file.Open(_defaultFilePath, File.ModeFlags.Write);

                if (err == Error.Ok)
                {
                    file.StoreString(_codeEditor.Text);
                    Interpreter.Error.Add(new Exception($"Archivo guardado: {_defaultFilePath}"));
                    _errorWindow.DisplayErrors();
                }
                else
                {
                    Interpreter.Error.Add(new Exception($"Error al guardar: {err}"));
                    _errorWindow.DisplayErrors();
                }
            }
        }
        catch (Exception ex)
        {
            Interpreter.Error.Add(ex);
            _errorWindow.DisplayErrors();
        }
    }
    private void _on_Load_pressed()
    {
        try
        {
            _errorWindow.ClearErrors();
            File file = new File();
            if (!file.FileExists(_defaultFilePath))
            {
                Interpreter.Error.Add(new Exception("Archivo no encontrado"));
            }
            Error err = file.Open(_defaultFilePath, File.ModeFlags.Read);
            if (err == Error.Ok)
            {
                _codeEditor.Text = file.GetAsText();
                file.Close();
                Interpreter.Error.Add(new Exception($"Archivo cargado: {_defaultFilePath}"));
                _errorWindow.DisplayErrors();
            }
            else
            {
                Interpreter.Error.Add(new Exception($"Error al cargar: {err}"));
                _errorWindow.DisplayErrors();
            }
        }
        catch (Exception ex)
        {
            Interpreter.Error.Add(ex);
            _errorWindow.DisplayErrors();
        }
    }
    private void _on_CanvasResize_pressed()
    {
        try
        {
            _errorWindow.ClearErrors();
            const int newSize = 100;
            _pixelCanvas.SetGridSize(newSize);
            if (_canvasDimensionInput != null)
            {
                _canvasDimensionInput.Text = newSize.ToString();
            }
            Interpreter.Error.Add(new Exception($"Canvas redimensionado a {newSize}x{newSize}"));
            _errorWindow.DisplayErrors();
        }
        catch (Exception ex)
        {
            Interpreter.Error.Add(ex);
            _errorWindow.DisplayErrors();
        }
    }
    private void _on_CanvasDimensionInput_text_changed()
    {
        if (int.TryParse(_canvasDimensionInput.Text, out int size) && size > 0)
        {
            _pixelCanvas.SetGridSize(newSize) = size;
        }
    }
}