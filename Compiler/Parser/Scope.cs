using Godot;
using System;
using System.Collections.Generic;

public static class ScopeUtils
{
	public static bool UpdateVariableInHierarchy(Dictionary<string, object> currentScope, string variableName, object newValue)
	{
		if (currentScope.ContainsKey(variableName))
		{
			currentScope[variableName] = newValue;
			return true;
		}
		if (currentScope.TryGetValue("parent", out object parentObj) && parentObj is Dictionary<string, object> parentScope)
		{
			return UpdateVariableInHierarchy(parentScope, variableName, newValue);
		}

		return false;
	}
	public static bool SetVariable(Dictionary<string, object> scope, string variableName, object value)
	{
		if (UpdateVariableInHierarchy(scope, variableName, value))
		{
			return true;
		}
		scope[variableName] = value;
		return true;
	}
	public static bool TryGetVariable(Dictionary<string, object> currentScope, string variableName, out object value)
	{
		if (currentScope.TryGetValue(variableName, out value))
		{
			return true;
		}
		if (currentScope.TryGetValue("parent", out object parentObj) &&
			parentObj is Dictionary<string, object> parentScope)
		{
			return TryGetVariable(parentScope, variableName, out value);
		}
		value = null;
		return false;
	}
	public static bool RemoveVariable(Dictionary<string, object> scope, string variableName)
	{
		return scope.Remove(variableName);
	}
}
