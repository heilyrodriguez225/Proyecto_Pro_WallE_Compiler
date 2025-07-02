using Godot;
using System;
using System.Collections.Generic;

public interface IASTNode
{
	object Execute(Scope scope);
}
public class ProgramNode : IASTNode
{
	public List<IASTNode> Statements { get; } = new List<IASTNode>();
	public object Execute(Scope scope)
	{
		var localScope = new Scope(scope);
		for (int i = 0; i < Statements.Count; i++)
		{
			if (Statements[i] is LabelNode labelNode)
			{
				scope.SetVariable($"Label_{ labelNode.LabelName}", i);
			}
		}
		int currentIndex = 0;
		while (currentIndex < Statements.Count)
		{
			var currentStatement = Statements[currentIndex];
			object result = null;
			if (!(currentStatement is LabelNode))
			{
				try
				{
					result = currentStatement.Execute(localScope);
				}
				catch (Exception ex)
				{
					Interpreter.Error.Add(new Exception($"Runtime error at line {currentIndex + 1}: {ex.Message}"));
				}
			}
			if (result != null && currentStatement is GoToNode)
			{
				currentIndex = (int)result;
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
		public IASTNode X { get; }
		public IASTNode Y { get; }
		public SpawnNode(IASTNode x, IASTNode y)
		{
			X = x; Y = y;
		}
		public object Execute(Scope scope)
		{
			var canvasSize = (int)scope.GetVariable("CanvasSize");
			var x = Convert.ToInt32(X.Execute(scope));
			var y = Convert.ToInt32(Y.Execute(scope));
			if (x < 0 || x >= canvasSize || y < 0 || y >= canvasSize)
		{
			Interpreter.Error.Add(new Exception($"Posición inicial inválida: ({X}, {Y})"));
		}
			WallEState state = new WallEState();
			state.X = x;
			state.Y = y;
			state.PixelCanvas = new PixelCanvas();
			scope.SetVariable("WallEState",state);
			scope.SetVariable("WallE_X", x);
			scope.SetVariable("WallE_Y", y);
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
	public object Execute(Scope scope)
	{
		scope.SetVariable(Variable, Expression.Execute(scope));
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
    public object Execute(Scope scope)
    {
        object leftResult = Left.Execute(scope);
        object rightResult = Right.Execute(scope);

        if (Operator.Lexeme == "&&" || Operator.Lexeme == "||")
        {
            bool leftBool = Convert.ToBoolean(leftResult);
            bool rightBool = Convert.ToBoolean(rightResult);
            
            if (Operator.Lexeme == "&&") return leftBool && rightBool;
            if (Operator.Lexeme == "||") return leftBool || rightBool;
        }
        double leftNum = Convert.ToDouble(leftResult);
        double rightNum = Convert.ToDouble(rightResult);

        switch (Operator.Lexeme)
        {
            case "+": return leftNum + rightNum;
            case "-": return leftNum - rightNum;
            case "*": return leftNum * rightNum;
            case "/":
                if (rightNum == 0)
				{
				Interpreter.Error.Add(new Exception("División por cero"));
					break;
				} 
                return leftNum / rightNum;
            case "%": return (int)leftNum % (int)rightNum;
            case "**": return Math.Pow(leftNum, rightNum);
            case "==": return leftNum == rightNum;
            case ">": return leftNum > rightNum;
            case "<": return leftNum < rightNum;
            case ">=": return leftNum >= rightNum;
            case "<=": return leftNum <= rightNum;
            default:
				 Interpreter.Error.Add(new Exception($"Operador no soportado: {Operator.Lexeme}"));
				 return null;
        }
		return null;
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
	public object Execute(Scope scope)
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
	public object Execute(Scope scope)
	{
		var res = scope.GetVariable(Name);
		if(res == null) Interpreter.Error.Add(new Exception("Variable no asignada"));
		return res;
	}
}
public class LabelNode : IASTNode
{
	public string LabelName { get; }
	public LabelNode(string name)
	{
		LabelName = name;
	}
	public object Execute(Scope scope)
	{
		return LabelName;
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
	public object Execute(Scope scope)
	{
		var func = scope.GetFunction(FunctionName);
		List<object> args = new List<object>();
		for (int i = 0; i < Arguments.Count; i++)
		{
			args.Add( Arguments[i].Execute(scope));
		}
		return func.DynamicInvoke(new object[] { args, scope });
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
	public object Execute(Scope scope)
	{
		bool condition = Convert.ToBoolean(Condition.Execute(scope));
		if (condition)
		{
			if (scope.GetVariable($"Label_{Label}") == null)
			{
				Interpreter.Error.Add(new Exception($"Etiqueta no existe: {Label}"));
			}
			int targetIndex = (int)scope.GetVariable($"Label_{Label}");
			return targetIndex;
		}
		return null;
	}
}
