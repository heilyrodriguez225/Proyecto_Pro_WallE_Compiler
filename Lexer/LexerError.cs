public class LexerError : Exception
{
    public int Position { get; }
    public int InvalidChar { get; }
    public LexerError(int position, char invalidChar) : base($"Error léxico en posición {position}: Carácter inesperado '{invalidChar}'")
    {
        Position = position;
        InvalidChar = invalidChar;
    }
}