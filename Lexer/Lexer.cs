using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
public class Lexer
{
    public string Code;
    public List<Token> tokens = new List<Token>();
    public static List<Regex> TokenRegex = new List<Regex>
    {
        new Regex(@"\$"),
        new Regex(@"\r|\n"),
        new Regex(@""".*"""), 
        new Regex(@"(\*\*|&&|\|\||==|>=|<=|!=|>|<|\+|-|\*|/|%)"),
        new Regex(@"←|\(|\)|,|\[|\]"),
        new Regex(@"[-]?(\d+\.\d+|\.\d+|\d+)"),
        new Regex(@"[a-zA-Z][a-zA-Z0-9_]*"),
        new Regex(@"\s+")       
    };
    public Dictionary<Regex, Token.TokenType> tokenPatterns = new Dictionary<Regex, Token.TokenType>
    {
        {TokenRegex[0], Token.TokenType.EOFToken},
        {TokenRegex[1], Token.TokenType.NewLineToken},
        {TokenRegex[2], Token.TokenType.StringToken},
        {TokenRegex[3], Token.TokenType.OperatorToken},
        {TokenRegex[4], Token.TokenType.SymbolToken},
        {TokenRegex[5], Token.TokenType.NumberToken},
        {TokenRegex[6], Token.TokenType.IdentifierToken},
        {TokenRegex[7], Token.TokenType.WhiteSpaceToken}
        
    };
    public Lexer(string code)
    {
        Code = code + "$"; // Añadir EOF 
        Tokenize();
    }
    public void Tokenize()
    {
        int initialPosition = 0;
        while (initialPosition < Code.Length)
        {
            int maxMatch = 0;
            Token.TokenType selectedType = Token.TokenType.EOFToken;
            string matchedLexeme = "";
            bool thereWasAMatch = false;
            for (int i = 0; i < TokenRegex.Count; i++)
            {
                Match match = TokenRegex[i].Match(Code, initialPosition);
                if (match.Success && match.Index == initialPosition && match.Length > maxMatch)
                {
                    maxMatch = match.Length;
                    selectedType = tokenPatterns[TokenRegex[i]];
                    matchedLexeme = match.Value;
                    thereWasAMatch = true;
                }
                if (selectedType == Token.TokenType.IdentifierToken)
                {
                    if (matchedLexeme == "GoTo")
                    {
                        selectedType = Token.TokenType.GoToToken;
                    }
                    else if (Token.Functions.Contains(matchedLexeme))
                    {
                        selectedType = Token.TokenType.FunctionToken;
                    }
                }
            }
            if (thereWasAMatch == false) throw new Exception("Error of lexing ");
            if (selectedType != Token.TokenType.WhiteSpaceToken )
                tokens.Add(new Token(selectedType, matchedLexeme));
            initialPosition += maxMatch;
        }
    }
}