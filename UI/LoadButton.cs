using Godot;
using System;

public class LoadButton : Button
{
    private FileDialog _fileDialog;
    private CodeEditor _codeEditor;
    private Label _statusLabel;

    public override void _Ready()
    {
        _fileDialog = GetNode<FileDialog>("/root/Main/FileDialog");
        _codeEditor = GetNode<CodeEditor>("/root/Main/VBoxContainer/HSplitContainer/CodeEditor");
        _statusLabel = GetNode<Label>("/root/Main/VBoxContainer/StatusBar/Label");
        
        Connect("pressed", this, nameof(OnPressed));
    }

    private void OnPressed()
    {
        _fileDialog.Mode = FileDialog.ModeEnum.OpenFile;
        _fileDialog.Filters = new string[] { "*.gw; Archivos GW" };
        _fileDialog.Connect("file_selected", this, nameof(OnFileSelected), flags: (uint)ConnectFlags.Oneshot);
        _fileDialog.PopupCentered();
    }

    private void OnFileSelected(string path)
    {
        try
        {
            using (var file = new File())
            {
                if (file.Open(path, File.ModeFlags.Read) == Error.Ok)
                {
                    _codeEditor.Text = file.GetAsText();
                    _statusLabel.Text = $"Archivo cargado: {System.IO.Path.GetFileName(path)}";
                }
            }
        }
        catch (Exception e)
        {
            _statusLabel.Text = $"Error al cargar: {e.Message}";
        }
    }
}