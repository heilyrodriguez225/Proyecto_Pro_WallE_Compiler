using Godot;
using System;
using System.Collections.Generic;
public class MainController : Control
{
    public CodeEditor CodeEditor { get; private set; }
    public PixelCanvas PixelCanvas { get; private set; }
    public Label StatusLabel { get; private set; }
    
    public Interpreter Interpreter { get; private set; }

    public override void _Ready()
    {
        CodeEditor = GetNode<CodeEditor>("CodeEditor");
        PixelCanvas = GetNode<PixelCanvas>("PixelCanvas");
        StatusLabel = GetNode<Label>("CodeError/StatusLabel");
        
        // 4. Configurar estado inicial
        StatusLabel.Text = "Listo";
        
        InitializeCanvas(100);
    }
    public void InitializeCanvas(int size)
    {
        Interpreter = new Interpreter(size);
        
        // 6.2. Redimensionar el canvas visual
        PixelCanvas.SetGridSize(size);
        PixelCanvas.InitializePixels();
        
        Interpreter.WallEState.PixelCanvas = PixelCanvas;
       
        StatusLabel.Text = $"Canvas inicializado: {size}x{size}";
    }

    public void ExecuteCode()
    {
        try
        {
            StatusLabel.Text = "Ejecutando...";
            
            var lexer = new Lexer(CodeEditor.Text);
            var parser = new Parser(lexer.tokens);
            var program = parser.ParseProgram();
            
            program.Execute(Interpreter.GlobalScope);
            
            StatusLabel.Text = "Ejecución completada";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error : {ex.Message}");
        }
    }
}