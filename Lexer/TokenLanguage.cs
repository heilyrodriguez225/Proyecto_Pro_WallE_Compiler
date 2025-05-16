public class Token
{
    public TokenType Type;
    public string Lexeme;
    public int Line;
    public int Column;
    public Token(TokenType type, string lexeme, int line, int column)
    {
        Type = type;
        Lexeme = lexeme;
        Line = line;
        Column = column;
    }
    public enum TokenType
    {
        FunctionToken,
        /*Spawn, Color, Size, DrawLine, DrawCircle, DrawRectangle, Fill,
        GetActualX, GetActualY, GetCanvasSize, GetColorCount, IsBrushColor, 
        IsBrushSize, IsCanvasColor, */
        GoToToken,
        // GoTo
        SymbolToken,
        /*LeftArrow(←), LeftParen, RightParen, Comma, LeftBracket([), RightBracket(]) */
        NumberToken,
        StringToken,
        IdentifierToken,
        OperatorToken,
        /*Plus(+), Minus(-), Multiply(*), Divide(/), Power(**), Modulo(%), And(&&),
         Or(||), Equal(==), GreaterEqual(>=), LessEqual(<=), Greater(>), Less(<)*/        
        
        NewLineToken,
        EOFToken
    }
    public static readonly Dictionary<string, TokenType> Functions = new Dictionary<string, TokenType>
    {
        {"Spawn", TokenType.FunctionToken},
        {"Color", TokenType.FunctionToken},
        {"Size", TokenType.FunctionToken},
        {"DrawLine", TokenType.FunctionToken},
        {"DrawCircle", TokenType.FunctionToken},
        {"DrawRectangle", TokenType.FunctionToken},
        {"Fill", TokenType.FunctionToken},
        {"GetActualX", TokenType.FunctionToken},
        {"GetActualY", TokenType.FunctionToken},
        {"GetCanvasSize", TokenType.FunctionToken},
        {"GetColorCount", TokenType.FunctionToken},
        {"IsBrushColor", TokenType.FunctionToken},
        {"IsBrushSize", TokenType.FunctionToken},
        {"IsCanvasColor", TokenType.FunctionToken}
    };
    public override string ToString() => $"{Type}: {Lexeme}";
}
