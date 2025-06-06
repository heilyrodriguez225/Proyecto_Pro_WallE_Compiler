public class Parser
{
    public List<Token> Tokens;
    private int current;
    public Parser(List<Token> tokens)
    {
        Tokens = tokens;
        current = 0;
    }
    public ProgramNode ParseProgram()
    {
        var program = new ProgramNode();
        program.Statements.Add(ParseSpawn()); // Spawn obligatorio
        while (Tokens[current].Type != Token.TokenType.EOFToken)
        {
            if (Match(Token.TokenType.FunctionToken))
                program.Statements.Add(ParseInstruction());
            else if (Match(Token.TokenType.IdentifierToken))
                program.Statements.Add(ParseAssignment());
            else if (Match(Token.TokenType.GoToToken))
                program.Statements.Add(ParseGoTo());
            else throw new Exception("Error of parsing");
        }
        return program;
    }
    //Verifica el token actual sin avanzar.
    private bool Check(Token.TokenType type, string lexeme = null)
    {
        if (current == Tokens.Count) return false;
        bool typeMatches = Tokens[current].Type == type;
        bool lexemeMatches = Tokens[current].Lexeme == lexeme;
        return typeMatches && lexemeMatches;
    }
    //Verifica y avanza si hay coincidencia.
    private bool Match(Token.TokenType type, string lexeme = null)
    {
        if (Check(type, lexeme))
        {
            current++;
            return true;
        }
        return false;
    }
    private Token Consume(Token.TokenType expectedType, string expectedLexeme = null)
    {
        // Verifica si el token actual coincide con el tipo y lexema esperados
        if (Check(expectedType, expectedLexeme))
        {
            // Devuelve el token actual y avanza al siguiente
            return Tokens[current++];
        }
        else throw new Exception("Error of parsing");
    }
    // Spawn → "Spawn" "(" Numero "," Numero ")"
    private SpawnNode ParseSpawn()
    {
        Consume(Token.TokenType.FunctionToken, "Spawn");
        Consume(Token.TokenType.SymbolToken, "(");
        int x = int.Parse(Consume(Token.TokenType.NumberToken).Lexeme);
        Consume(Token.TokenType.SymbolToken, ",");
        int y = int.Parse(Consume(Token.TokenType.NumberToken).Lexeme);
        Consume(Token.TokenType.SymbolToken, ")");
        Consume(Token.TokenType.NewLineToken);
        return new SpawnNode(x, y);
    }

    // Instrucción → Color | Size | DrawLine | ...
    private IASTNode ParseInstruction()
    {
        Token functionToken = Consume(Token.TokenType.FunctionToken);
        var functionName = functionToken.Lexeme;
        Consume(Token.TokenType.SymbolToken, "(");
        List<IASTNode> parameters = new List<IASTNode>();
        while (!Check(Token.TokenType.SymbolToken, ")"))
        {
            IASTNode param = ParseExpression();
            parameters.Add(param);
            if (!Match(Token.TokenType.SymbolToken, ",")) break;
        }
        Consume(Token.TokenType.SymbolToken, ")");
        return new FunctionCallNode(functionName, parameters);
    }
    private AssignmentNode ParseAssignment()
    {
        Token varToken = Consume(Token.TokenType.IdentifierToken);
        string variableName = varToken.Lexeme;
        Consume(Token.TokenType.SymbolToken, "←");
        IASTNode expression = ParseExpression();
        Consume(Token.TokenType.NewLineToken);
        return new AssignmentNode(variableName, expression);
    }
    private IASTNode ParseExpression()
    {
        if (Tokens[current + 1].Lexeme == "&&" || Tokens[current + 1].Lexeme == "||" || Tokens[current + 1].Lexeme == "==" || 
        Tokens[current + 1].Lexeme == ">=" || Tokens[current + 1].Lexeme == "<=" || Tokens[current + 1].Lexeme == "<" || Tokens[current + 1].Lexeme == ">")
        return ParseBoolExpression();
    else
        return ParseAlgebraicExpression();
    }
    private IASTNode ParseBoolExpression()
    {
        var left = ParseLogicalAnd();
        while (Match(Token.TokenType.OperatorToken, "||"))
        {
            var op = Tokens[current - 1];
            var right = ParseLogicalAnd();
            left = new BinaryExpressionNode(left, op, right);
        }
        return left;
    }
    private IASTNode ParseLogicalAnd()
    {
        var left = ParseComparison();
        while (Match(Token.TokenType.OperatorToken, "&&"))
        {
            var op = Tokens[current - 1];
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
            if (Match(Token.TokenType.OperatorToken, "==") || Match(Token.TokenType.OperatorToken, "!=") || Match(Token.TokenType.OperatorToken, ">") ||
                Match(Token.TokenType.OperatorToken, "<") || Match(Token.TokenType.OperatorToken, ">=") || Match(Token.TokenType.OperatorToken, "<="))
            {
                Token op = Tokens[current - 1];
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
            if (Match(Token.TokenType.OperatorToken, "+") || Match(Token.TokenType.OperatorToken, "-"))
            {
                Token op = Tokens[current - 1];
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
        while (Match(Token.TokenType.OperatorToken, "*") || Match(Token.TokenType.OperatorToken, "/") || Match(Token.TokenType.OperatorToken, "%"))
        {
            Token op = Tokens[current - 1];
            IASTNode right = ParseFactor();
            left = new BinaryExpressionNode(left, op, right);
        }
        return left;
    }
    private IASTNode ParseFactor()
    {
        if (Match(Token.TokenType.NumberToken))
        {
            int value = int.Parse(Tokens[current - 1].Lexeme);
            return new LiteralNode(value);
        }
        if (Match(Token.TokenType.SymbolToken, "("))
        {
            IASTNode expr = ParseExpression();
            Consume(Token.TokenType.SymbolToken, ")");
            return expr;
        }
        if (Match(Token.TokenType.IdentifierToken))
        {
            string identifier = Tokens[current - 1].Lexeme;
            return new VariableNode(identifier);
        }
        if (Match(Token.TokenType.OperatorToken, "-"))
        {
            IASTNode right = ParseFactor();
            return new BinaryExpressionNode(new LiteralNode(0), Tokens[current - 1], right);
        }
        throw new Exception($"Token inesperado: {Tokens[current]}");
    }
    private GoToNode ParseGoTo()
    {
        Consume(Token.TokenType.GoToToken);
        Consume(Token.TokenType.SymbolToken, "[");
        string label = Consume(Token.TokenType.IdentifierToken).Lexeme;
        Consume(Token.TokenType.SymbolToken, "]");
        Consume(Token.TokenType.SymbolToken, "(");
        IASTNode condition = ParseBoolExpression();
        Consume(Token.TokenType.SymbolToken, ")");
        return new GoToNode(label, condition);
    } 
}