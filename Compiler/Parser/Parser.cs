using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public class Parser
{
	public List<Token> Tokens;
	private Token previous;
	public Parser(List<Token> tokens)
	{
		Tokens = tokens;
		previous = null;
	}
	public IASTNode ParseProgram()
	{
		var program = new ProgramNode();
		program.Statements.Add(ParseSpawn()); // Spawn obligatorio
		while (Tokens[0].Type != Token.TokenType.EOFToken)
		{
			if (Check(Token.TokenType.FunctionToken, Tokens[0].Lexeme))
				program.Statements.Add(ParseInstruction());
			else if (Check(Token.TokenType.GoToToken, Tokens[0].Lexeme))
				program.Statements.Add(ParseGoTo());
			else if (Check(Token.TokenType.IdentifierToken, Tokens[0].Lexeme))
			{
				if (Tokens[1].Lexeme == "<-")
					program.Statements.Add(ParseAssignment());
				else program.Statements.Add(ParseLabel());
			}
			else if (Consume(Token.TokenType.NewLineToken, Tokens[0].Lexeme)) continue;
			else
			{
				Interpreter.Error.Add(new Exception("Error of parsing"));
				break;
			}
		}
		return program;
	}
	private bool Check(Token.TokenType type, string lexeme)
	{
		if (Tokens.Count == 0) return false;
		bool typeMatches = Tokens[0].Type == type;
		bool lexemeMatches = Tokens[0].Lexeme == lexeme;
		return typeMatches && lexemeMatches;
	}
	private bool Consume(Token.TokenType type, string lexeme)
	{
		if (Check(type, lexeme))
		{
			SavePrevious();
			Tokens.Remove(Tokens[0]);
			return true;
		}
		else
		{
			return false;
		}
	}
	private void SavePrevious()
	{
		if (0 < Tokens.Count)
		{
			previous = Tokens[0];
		}
	}
	// Spawn(int x, int y)
	private IASTNode ParseSpawn()
	{
		Consume(Token.TokenType.FunctionToken, "Spawn");
		Consume(Token.TokenType.SymbolToken, "(");
		IASTNode x = ParseExpression();
		Consume(Token.TokenType.SymbolToken, ",");
		IASTNode y = ParseExpression();
		Consume(Token.TokenType.SymbolToken, ")");
		return new SpawnNode(x, y);
	}
	// Instrucción → Color | Size | DrawLine ...
	private IASTNode ParseInstruction()
	{
		string functionName = Tokens[0].Lexeme;
		Consume(Token.TokenType.FunctionToken, Tokens[0].Lexeme);
		Consume(Token.TokenType.SymbolToken, "(");
		List<IASTNode> parameters = new List<IASTNode>();
		while (!Check(Token.TokenType.SymbolToken, ")") && !Check(Token.TokenType.EOFToken, "$"))
		{
			IASTNode param = ParseExpression();
			parameters.Add(param);
			if (Check(Token.TokenType.SymbolToken, ",")) Consume (Token.TokenType.SymbolToken, ",");
			else break;
		}
		Consume(Token.TokenType.SymbolToken, ")");
		return new FunctionCallNode(functionName, parameters);
	}
	private IASTNode ParseAssignment()
	{
		string variableName = Tokens[0].Lexeme;
		Consume (Token.TokenType.IdentifierToken, Tokens[0].Lexeme);
		Consume(Token.TokenType.SymbolToken, "<-");
		IASTNode expression;
		expression = ParseExpression();
		return new AssignmentNode(variableName, expression);
	}
	private IASTNode ParseLabel()
	{
		string labelName = Tokens[0].Lexeme;
		Consume (Token.TokenType.IdentifierToken, Tokens[0].Lexeme);
		return new LabelNode (labelName);
	}
	private IASTNode ParseExpression()
	{
		return ParseBoolExpression();
	}
	private IASTNode ParseBoolExpression()
	{
		var left = ParseLogicalOr();
		while (Consume (Token.TokenType.OperatorToken, "&&"))
		{
			var op = previous;
			var right = ParseLogicalOr();
			left = new BinaryExpressionNode(left, op, right);
		}
		return left;
	}
	private IASTNode ParseLogicalOr()
	{
		var left = ParseComparison();
		while (Consume(Token.TokenType.OperatorToken, "||"))
		{
			var op = previous;
			var right = ParseComparison();
			left = new BinaryExpressionNode(left, op, right);
		}
		return left;
	}
	private IASTNode ParseComparison()
	{
		IASTNode left = ParseAlgebraicExpression();
		while (true)
		{
			if (Consume (Token.TokenType.OperatorToken, "==") || Consume (Token.TokenType.OperatorToken, "!=") || Consume (Token.TokenType.OperatorToken, ">") ||
				Consume (Token.TokenType.OperatorToken, "<") || Consume (Token.TokenType.OperatorToken, ">=") || Consume (Token.TokenType.OperatorToken, "<="))
			{
				Token op = previous;
				IASTNode right = ParseAlgebraicExpression();
				left = new BinaryExpressionNode(left, op, right);
			}
			else break;
		}
		return left;
	}
	private IASTNode ParseAlgebraicExpression()
	{
		return ParseAdditive();
	}
	private IASTNode ParseAdditive()
	{
		IASTNode left = ParseTerm();
		while (true)
		{
			if (Consume(Token.TokenType.OperatorToken, "+") || Consume(Token.TokenType.OperatorToken, "-"))
			{
				Token op = previous;
				IASTNode right = ParseTerm();
				left = new BinaryExpressionNode(left, op, right);
			}
			else break;
		}
		return left;
	}
	private IASTNode ParseTerm()
	{
		IASTNode left = ParseFactor(); // Factor maneja literales, variables o paréntesis
		while (Consume(Token.TokenType.OperatorToken, "*") || Consume(Token.TokenType.OperatorToken, "/") || Consume(Token.TokenType.OperatorToken, "%"))
		{
			Token op = previous;
			IASTNode right = ParseFactor();
			left = new BinaryExpressionNode(left, op, right);
		}
		return left;
	}
	private IASTNode ParseFactor()
	{
		if (Consume(Token.TokenType.NumberToken, Tokens[0].Lexeme))
		{
			int value = int.Parse(previous.Lexeme);
			return new LiteralNode(value);
		}
		else if (Consume(Token.TokenType.SymbolToken, "("))
		{
			IASTNode expr = ParseExpression();
			return expr;
		}
		else if (Tokens[0].Type == Token.TokenType.FunctionToken)
		{
			return ParseInstruction();
		}
		else if (Consume(Token.TokenType.IdentifierToken, Tokens[0].Lexeme))
		{
			string identifier = previous.Lexeme;
			return new VariableNode(identifier);
		}
		else if (Consume(Token.TokenType.StringToken, Tokens[0].Lexeme))
		{
			string stringObj = previous.Lexeme;
			if (stringObj.StartsWith("\"") && stringObj.EndsWith("\"")) stringObj = stringObj.Substring(1, stringObj.Length - 2);
			return new LiteralNode(stringObj);
		}
		else if (Consume(Token.TokenType.OperatorToken, "-"))
		{
			IASTNode right = ParseFactor();
			return new BinaryExpressionNode(new LiteralNode(0), previous, right);
		}
		else Interpreter.Error.Add(new Exception($"Token inesperado: {Tokens[0]} en ParseFactor"));
		return null;
	}
	private IASTNode ParseGoTo()
	{
		Consume (Token.TokenType.GoToToken, Tokens[0].Lexeme);
		Consume (Token.TokenType.SymbolToken, "[");
		string label = Tokens[0].Lexeme;
		Consume(Token.TokenType.IdentifierToken, Tokens[0].Lexeme);
		Consume (Token.TokenType.SymbolToken, "]");
		Consume (Token.TokenType.SymbolToken, "(");
		IASTNode condition = ParseBoolExpression();
		Consume (Token.TokenType.SymbolToken, ")");
		return new GoToNode(label, condition);
	} 
}
