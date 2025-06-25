using Godot;
using System;
using System.Collections.Generic;

public class Scope
{
    private Dictionary<string,object> variable;
    private Dictionary<string, Delegate> functions;
    private Scope parent;

    public Scope(Scope parent = null)
    {
        variable = new Dictionary<string,object>(); 
        functions = new Dictionary<string,Delegate>();
        this.parent = parent;
    }

    public void SetVariable(string name, object value)
    {
        variable[name] = value;
        if(parent != null && parent.GetVariable(name) != null)
            parent.SetVariable(name,value);
    }
    public object GetVariable(string name)
    {
        if(!variable.ContainsKey(name))
        {
            if(parent != null)
                return parent.GetVariable(name);
            else
                return null;
        }
        return variable[name];
    }
    public void SetFunction(string name, Delegate function)
    {
        functions[name] = function;
        if(parent != null && parent.GetFunction(name) != null)
        {
            parent.SetFunction(name,function);
        }
    }
    public Delegate GetFunction(string name)
    {
        if(!functions.ContainsKey(name))
        {
            if(parent != null)
                return parent.GetFunction(name);
            return null;
        }
        return functions[name];
    }
}