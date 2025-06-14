using Godot;
using System;
using System.Collections.Generic;

public class Token
{
	public TokenType Type;
	public string Lexeme;
	public Token(TokenType type, string lexeme)
	{
		Type = type;
		Lexeme = lexeme;
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
		
		WhiteSpaceToken,
		NewLineToken,
		EOFToken
	}
	public static List<string> Functions = new List<string>
	{
		"Spawn", "Color", "Size","DrawLine", "DrawCircle", "DrawRectangle", "Fill", "GetActualX",
		"GetActualY", "GetCanvasSize", "GetColorCount", "IsBrushColor", "IsBrushSize", "IsCanvasColor",
	};
	public override string ToString() => $"{Type}: {Lexeme}";
}
