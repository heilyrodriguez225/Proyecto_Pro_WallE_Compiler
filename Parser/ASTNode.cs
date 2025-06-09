public interface IASTNode
{
    public object Execute(Dictionary<string, object> scope);
}
public class ProgramNode : IASTNode
{
    public List<IASTNode> Statements { get; } = new List<IASTNode>();
    public object Execute(Dictionary<string, object> scope)
    {
        foreach (var statement in Statements)
        {
            try
            {
                statement.Execute(scope);
            }
            catch (Exception ex)
            {
                throw new Exception($"Runtime error: {ex.Message}");
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
        scope[Variable] = Expression.Execute(scope);
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
                if (left is string || right is string)
                    return $"{left}{right}"; // Concatenación
                else if (left is int && right is int)
                    return (int)left + (int)right;
                else
                    return (double)left + (double)right;
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
        if(!scope.ContainsKey(Name)) throw new Exception($"Variable no definida: {Name}");
        return scope[Name];
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
    public object Execute(Dictionary<string, object> scope)
    {
        var args = Arguments.Select(arg => arg.Execute(scope)).ToList();
        if (!Functions.FunctionMap.ContainsKey(FunctionName))
            throw new Exception($"Función no existe: {FunctionName}");
        return Functions.FunctionMap[FunctionName](args, scope);
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