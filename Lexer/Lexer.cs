using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
public class Lexer
{
    public readonly string code; 
    public List<Token> tokens = new List<Token>();
    public int start = 0;
    public int current = 0;
    public int line = 1;
    public int column = 1;
    public Lexer(string code)
    {
        this.code = code + '$';
        Tokenize();
    }
    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }
        tokens.Add(new Token(Token.TokenType.EOFToken, "$", line, column));
        return tokens;
    }    
    private bool IsAtEnd()
    {
        if(code[current] == '$') return true;
        else return false;
    }
    private char Advance() 
    {
        var currentChar = code[current];
        current = Math.Min(current + 1, code.Length);
        return currentChar;
    }
    private void AddToken(Token.TokenType type)
    {
        string text = code.Substring(start, current - start);
        tokens.Add(new Token(type, text, line, column));
    }
    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (code[current] != expected) return false;
        
        current++;
        return true;
    }
    private void ScanToken()
    {
        char c = Advance();
        
        switch (c)
        {
            // Caracteres de un solo símbolo
            case '(': AddToken(Token.TokenType.SymbolToken); break;
            case ')': AddToken(Token.TokenType.SymbolToken); break;
            case ',': AddToken(Token.TokenType.SymbolToken); break;
            case '[': AddToken(Token.TokenType.SymbolToken); break;
            case ']': AddToken(Token.TokenType.SymbolToken); break;
            case '←': AddToken(Token.TokenType.SymbolToken); break;
            
            // Operadores
            case '+': AddToken(Token.TokenType.OperatorToken); break;
            case '/': AddToken(Token.TokenType.OperatorToken); break;
            case '%': AddToken(Token.TokenType.OperatorToken); break;
            case '&': if (Match('&')) AddToken(Token.TokenType.OperatorToken); break;
            case '|': if (Match('|')) AddToken(Token.TokenType.OperatorToken); break;
            case '=': if (Match('=')) AddToken(Token.TokenType.OperatorToken); break;
            case '-': if (Match('>')) AddToken(Token.TokenType.OperatorToken); 
                else AddToken(Token.TokenType.OperatorToken); break;
            case '*': if (Match('*')) AddToken (Token.TokenType.OperatorToken);
                else AddToken(Token.TokenType.OperatorToken); break;
            case '>': if (Match('=')) AddToken(Token.TokenType.OperatorToken);
                else AddToken (Token.TokenType.OperatorToken); break;
            case '<': if (Match('=')) AddToken(Token.TokenType.OperatorToken);
                else AddToken (Token.TokenType.OperatorToken); break;
            
            // Espacios en blanco
            case ' ':
            case '\r':
            case '\t':
                // Ignorar
                break;
                
            case '\n':
                AddToken(Token.TokenType.NewLineToken);
                line++;
                column = 1;
                break;
                
            // Literales de cadena
            case '"': String(); break;
                
            // Números
            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsLetter(c))
                {
                    Identifier();
                }
                else
                {
                    throw new Exception("Invalid token");
                }
                break;
        }
    }
    private void String()
    {
        while (code[current + 1] != '"' && !IsAtEnd())
        {
            if (code[current + 1] == '\n') line++;
            Advance();
        }
        
        /*if (IsAtEnd())
        {
            Error(line, "String sin terminar.");
            return;
        }*/
        
        // Cerrar comillas
        Advance();
        
        // Extraer el valor del string (sin las comillas)
        string value = code.Substring(start + 1, current - start - 2);
        AddToken(Token.TokenType.StringToken);
    }

    private void Number()
    {
        while (IsDigit(code[current + 1])) Advance();
        
        // Parte decimal
        if (code[current + 1] == '.' && IsDigit(code[current + 2]))
        {
            // Consumir el punto
            Advance();
            
            while (IsDigit(code[current + 1])) Advance();
        }
        AddToken(Token.TokenType.NumberToken);
    }
    private void Identifier()
    {
        while (IsLetterOrDigit(code[current + 1])) Advance();
        string text = code.Substring(start, current - start);
        // Verificar si es palabra clave
        if (Token.Functions.TryGetValue(text, out Token.TokenType type)) {
            AddToken(type);
        } else {
            AddToken(Token.TokenType.IdentifierToken);
        }
    }
    public bool IsDigit(char c) => c >= '0' && c <= '9';
    public bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    public bool IsLetterOrDigit(char c) => IsLetter(c) || IsDigit(c);
}