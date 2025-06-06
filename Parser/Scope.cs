public static class ScopeUtils
{
    public static bool UpdateVariableInHierarchy(Dictionary<string, object> currentScope, string variableName, object newValue)
    {
        if (currentScope.ContainsKey(variableName))
        {
            currentScope[variableName] = newValue;
            return true;
        }
        if (!currentScope.TryGetValue("parent", out var parentObj) || !(parentObj is Dictionary<string, object> parentScope))
        {
            return false;
        }
        return UpdateVariableInHierarchy(parentScope, variableName, newValue);
    }
}