using Godot;
using System;

public class MainController : Control
{

    private CodeEditor _codeEditor;
    private PixelCanvas _pixelCanvas;
    private LineEdit _dimensionInput;
    private FileDialog _fileDialog;
    private Button _runButton;
    private Button _loadButton;
    private Button _saveButton;
    private Button _resizeButton;
    private Label _statusLabel;
    
    // Componentes del interprete
    private Interpreter _interpreter;
    private Lexer _lexer;
    private Parser _parser;
    private ProgramNode _program;

    public override void _Ready()
    {
        // Obtener referencias usando rutas nodales
        _codeEditor = GetNode<CodeEditor>("VBoxContainer/HSplitContainer/CodeEditor");
        _pixelCanvas = GetNode<PixelCanvas>("VBoxContainer/HSplitContainer/PixelCanvas");
        _dimensionInput = GetNode<LineEdit>("VBoxContainer/HBoxContainer/DimensionInput");
        _fileDialog = GetNode<FileDialog>("FileDialog");
        _runButton = GetNode<Button>("VBoxContainer/HBoxContainer/RunButton");
        _loadButton = GetNode<Button>("VBoxContainer/HBoxContainer/LoadButton");
        _saveButton = GetNode<Button>("VBoxContainer/HBoxContainer/SaveButton");
        _resizeButton = GetNode<Button>("VBoxContainer/HBoxContainer/ResizeButton");
        _statusLabel = GetNode<Label>("VBoxContainer/StatusBar/Label");
        
        // Configuración inicial
        _dimensionInput.Text = "100";
        _statusLabel.Text = "Listo";
        
        // Conectar señales usando lambdas para mayor claridad
        _runButton.Connect("pressed", this, nameof(OnRunButtonPressed));
        _loadButton.Connect("pressed", this, nameof(OnLoadButtonPressed));
        _saveButton.Connect("pressed", this, nameof(OnSaveButtonPressed));
        _resizeButton.Connect("pressed", this, nameof(OnResizeButtonPressed));
        _fileDialog.Connect("file_selected", this, nameof(OnFileSelected));
        
        // Inicializar con tamaño por defecto
        OnResizeButtonPressed();
    }

    // ===== MANEJADORES DE BOTONES =====
    private void OnRunButtonPressed()
    {
        try
        {
            _statusLabel.Text = "Ejecutando...";
            
            // 1. Análisis léxico
            _lexer = new Lexer(_codeEditor.Text);
            
            // 2. Análisis sintáctico
            _parser = new Parser(_lexer.tokens);
            _program = _parser.ParseProgram();
            
            // 3. Ejecución
            _program.Execute(_interpreter.GlobalScope);
            
            // 4. Actualizar canvas desde el estado
            _pixelCanvas.UpdateFromState(_interpreter.WallEState);
            
            _statusLabel.Text = "Ejecución completada";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
            HighlightErrorLine(ex);
        }
    }

    private void OnLoadButtonPressed()
    {
        _fileDialog.Mode = FileDialog.ModeEnum.OpenFile;
        _fileDialog.Filters = new string[] { "*.pw; Archivos Pixel Wall-E" };
        _fileDialog.PopupCentered();
    }

    private void OnSaveButtonPressed()
    {
        _fileDialog.Mode = FileDialog.ModeEnum.SaveFile;
        _fileDialog.Filters = new string[] { "*.pw; Archivos Pixel Wall-E" };
        _fileDialog.PopupCentered();
    }

    private void OnResizeButtonPressed()
    {
        if (int.TryParse(_dimensionInput.Text, out int size) && size > 0)
        {
            // 1. Crear nuevo intérprete
            _interpreter = new Interpreter(size);
            
            // 2. Redimensionar canvas
            _pixelCanvas.Resize(size);
            _pixelCanvas.Clear();
            
            _statusLabel.Text = $"Canvas redimensionado a {size}x{size}";
        }
        else
        {
            _statusLabel.Text = "Tamaño inválido. Use número > 0";
        }
    }

    // ===== MANEJO DE ARCHIVOS =====
    private void OnFileSelected(string path)
    {
        if (_fileDialog.Mode == FileDialog.ModeEnum.OpenFile)
        {
            LoadFile(path);
        }
        else
        {
            SaveFile(path);
        }
    }

    private void LoadFile(string path)
    {
        try
        {
            using (File file = new File())
            {
                if (file.Open(path, File.ModeFlags.Read) == Error.Ok)
                {
                    _codeEditor.Text = file.GetAsText();
                    _statusLabel.Text = $"Cargado: {System.IO.Path.GetFileName(path)}";
                }
            }
        }
        catch (Exception e)
        {
            _statusLabel.Text = $"Error cargando: {e.Message}";
        }
    }

    private void SaveFile(string path)
    {
        try
        {
            if (!path.EndsWith(".pw")) path += ".pw";
            
            using (File file = new File())
            {
                if (file.Open(path, File.ModeFlags.Write) == Error.Ok)
                {
                    file.StoreString(_codeEditor.Text);
                    _statusLabel.Text = $"Guardado: {System.IO.Path.GetFileName(path)}";
                }
            }
        }
        catch (Exception e)
        {
            _statusLabel.Text = $"Error guardando: {e.Message}";
        }
    }

    // ===== UTILIDADES =====
    private void HighlightErrorLine(Exception ex)
    {
        // Buscar número de línea en el mensaje de error
        string[] parts = ex.Message.Split(' ');
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int lineNumber) && lineNumber > 0)
            {
                _codeEditor.CursorSetLine(lineNumber - 1);
                _codeEditor.CursorSetColumn(0);
                break;
            }
        }
    }
}
