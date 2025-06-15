using Godot;

public class CanvasResizeInput : LineEdit
{
    // Señal para notificar cambios de tamaño
    [Signal] public delegate void SizeChanged(int newSize);
    private int _minSize = 1;

    public override void _Ready()
    {
        Text = "100";
        
        Connect("text_entered", this, nameof(OnTextEntered));
        Connect("focus_exited", this, nameof(OnFocusExited));
    }
    private void OnTextEntered(string newText)
    {
        ApplyNewSize(newText);
        ReleaseFocus();  // Quitar foco después de aplicar
    }
    private void OnFocusExited()
    {
        ApplyNewSize(Text);
    }
    private void ApplyNewSize(string sizeText)
    {
        if (!int.TryParse(sizeText, out int newSize))
        {
            RestoreValidSize();
            return;
        }
        if (newSize < _minSize)
        {
            newSize = _minSize;
        }
        EmitSignal(nameof(SizeChanged), newSize);
    
        Text = newSize.ToString();
    }

    private void RestoreValidSize()
    {
        if (!int.TryParse(Text, out int currentSize))
        {
            Text = "100";  // Valor por defecto
        }
    }
}