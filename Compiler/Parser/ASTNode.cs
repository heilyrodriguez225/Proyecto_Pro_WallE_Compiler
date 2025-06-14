using Godot;
using System;
using System.Collections.Generic;

public interface IASTNode
{
	object Execute(Dictionary<string, object> scope);
}
public class ProgramNode : IASTNode
{
	public Dictionary<string, object> globalScope = new Dictionary<string, object>();
	public List<IASTNode> Statements { get; } = new List<IASTNode>();
	public object Execute(Dictionary<string, object> scope)
	{
		for (int i = 0; i < Statements.Count; i++)
		{
			if (Statements[i] is LabelNode labelNode)
			{
				ScopeUtils.SetVariable(globalScope, $"Label_{labelNode.LabelName}", i);
			}
		}
		 var executionScope = new Dictionary<string, object>
        {
            ["parent"] = globalScope 
        };

		int currentIndex = 0;
		while (currentIndex < Statements.Count)
		{
			var currentStatement = Statements[currentIndex];
			if (!(currentStatement is LabelNode))
			{
				try
				{
					currentStatement.Execute(executionScope);
				}
				catch (Exception ex)
				{
					throw new Exception($"Runtime error at line {currentIndex + 1}: {ex.Message}");
				}
			}
			if (ScopeUtils.TryGetVariable(executionScope, "CurrentStatementIndex", out object nextIndex))
			{
				currentIndex = (int)nextIndex;
				ScopeUtils.RemoveVariable(executionScope, "CurrentStatementIndex");
			}
			else
			{
				currentIndex++;
			}
		}
		return null;
	}
}
public class SpawnNode : IASTNode
	{
		public int X { get; }
		public int Y { get; }
		public SpawnNode(int x, int y)
		{
			X = x; Y = y;
		}
		public object Execute(Dictionary<string, object> scope)
		{
			var canvasSize = (int)scope["CanvasSize"];
			if (X < 0 || X >= canvasSize || Y < 0 || Y >= canvasSize) throw new Exception($"Posición inicial inválida: ({X}, {Y})");
			scope["WallE_X"] = X;
			scope["WallE_Y"] = Y;
			return null;
		}
	}
//Representa una asignación de variable: variable ← expresión.
public class AssignmentNode : IASTNode
{
	public string Variable { get; }
	public IASTNode Expression { get; }
	public AssignmentNode(string var, IASTNode expr)
	{
		Variable = var;
		Expression = expr;
	}
	public object Execute(Dictionary<string, object> scope)
	{
		object value = Expression.Execute(scope);
		if (!ScopeUtils.UpdateVariableInHierarchy(scope, Variable, value))
        {
			scope[Variable] = value;
        }
		return null;
	}
}
public class BinaryExpressionNode : IASTNode
{
	public IASTNode Left { get; }
	public Token Operator { get; }
	public IASTNode Right { get; }
	public BinaryExpressionNode(IASTNode left, Token op, IASTNode right)
	{
		Left = left;
		Operator = op;
		Right = right;
	}
	public object Execute(Dictionary<string,object> scope)
	{
		dynamic left = Left.Execute(scope);
		dynamic right = Right.Execute(scope);
		switch (Operator.Lexeme)
		{
			case "+":
				return left + right;
			case "-":
				return left - right;
			case "*":
				return left * right;
			case "/":
				if (right == 0) throw new Exception("División por cero");
				return left / right;
			case "%":
				return left % right;
			case "**":
				if (left is int && right is int)
					return (int)Math.Pow(left, right);
				else return Math.Pow(left, right);
			case "==":
				return left == right;
			case "!=":
				return left != right;
			case ">":
				return left > right;
			case "<":
				return left < right;
			case ">=":
				return left >= right;
			case "<=":
				return left <= right;
			case "&&":
				return Convert.ToBoolean(left) && Convert.ToBoolean(right);
			case "||":
				return Convert.ToBoolean(left) || Convert.ToBoolean(right);
			default:
				throw new Exception($"Operador no soportado: {Operator.Lexeme}");
		}
	}
}
//Representa un valor literal (número, string, booleano).
public class LiteralNode : IASTNode
{
	public object Value { get; }
	public LiteralNode(object value)
	{
		Value = value;
	}
	public object Execute(Dictionary<string, object> scope)
	{
		return Value;
	}
}
public class VariableNode : IASTNode
{
	public string Name { get; }

	public VariableNode(string name)
	{
		Name = name;
	}
	public object Execute(Dictionary<string, object> scope)
	{
		if (!scope.ContainsKey(Name)) throw new Exception($"Variable no definida: {Name}");
		return scope[Name];
	}
}
public class LabelNode : IASTNode
{
	public string LabelName { get; }
	public LabelNode(string name)
	{
		LabelName = name;
	}
	public object Execute(Dictionary<string, object> scope)
	{
		return null;
	}
}
public class FunctionCallNode : IASTNode
{
	public string FunctionName { get; }
	public List<IASTNode> Arguments { get; }
	public FunctionCallNode(string name, List<IASTNode> args)
	{
		FunctionName = name;
		Arguments = args;
	}
	public object Execute(Dictionary<string, object> parentScope)
    {
        var functionScope = new Dictionary<string, object>
        {
            ["parent"] = parentScope, ["function"] = FunctionName,["args"] = Arguments.Count
        };
        var evaluatedArgs = new List<object>();
        foreach (var arg in Arguments)
        {
            evaluatedArgs.Add(arg.Execute(parentScope));
        }
        for (int i = 0; i < evaluatedArgs.Count; i++)
		{
            ScopeUtils.SetVariable(functionScope, $"arg{i}", evaluatedArgs[i]);
        }
        if (!Functions.FunctionMap.TryGetValue(FunctionName, out var function))
        {
            throw new Exception($"Función no definida: {FunctionName}");
        }
        try
        {
            return function(evaluatedArgs, functionScope);
        }
        finally
        {
            if (ScopeUtils.TryGetVariable(functionScope, "return", out object returnValue))
            {
                ScopeUtils.SetVariable(parentScope, $"{FunctionName}result", returnValue);
            }
        }
    }
}
public class GoToNode : IASTNode
{
	public string Label { get; }
	public IASTNode Condition { get; }
	public GoToNode(string label, IASTNode condition)
	{
		Label = label;
		Condition = condition;
	}
	public object Execute(Dictionary<string, object> scope)
	{
		bool condition = Convert.ToBoolean(Condition.Execute(scope));

		if (condition)
		{
			if (!scope.ContainsKey($"Label_{Label}")) throw new Exception($"Etiqueta no existe: {Label}");
			int targetIndex = (int)scope[$"Label_{Label}"];
			scope["CurrentStatementIndex"] = targetIndex - 1;
		}
		return null;
	}
}
