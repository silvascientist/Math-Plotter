using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEngine;

/*****
0.1.0
This version includes the defintion of the Token struct, which requires also the definition of the TokenType enum. The lists of supported operators and keywords are also defined.
0.2.0
This version will include the full lexer when finished.
*****/

public static class MathParser
{
    static readonly Dictionary<char, Func<float, float, float>> operatorExpressions = new Dictionary<char, Func<float, float, float>>
    {
        {'^', (x, z) => (float) Math.Pow(x,z)},
        {'*', (x, z) => x*z},
        {'/', (x, z) => x/z},
        {'+', (x, z) => x + z},
        {'-', (x, z) => x - z}
    };
    static readonly char[] operators = operatorExpressions.Keys.ToArray();
    static readonly char[] delimiters = new char[]
    {
        '(',')',','
    };
    static readonly Dictionary<string, Func<float, float>> unaryFuncExpressions = new Dictionary<string, Func<float, float>>
    {
        {"sin", x => (float) Math.Sin(x)},
        {"cos", x => (float) Math.Cos(x)},
        {"tan", x => (float) Math.Tan(x)},
        // {"cot", x => (float) Math.Cot(x)}, // Math.Cot() not recognized - find replacement
        {"sec", x => 1 / (float) Math.Cos(x)},
        {"csc", x => 1 / (float) Math.Sin(x)},
        {"asin", x => (float) Math.Asin(x)},
        {"acos", x => (float) Math.Acos(x)},
        {"atan", x => (float) Math.Atan(x)},
        // {"acot", x => (float) Math.Acot(x)}, // Math.Acot() not recognized - find replacement
        {"asec", x => (float) Math.Acos(1/x)},
        {"exp", x => (float) Math.Exp(x)},
        {"ln", x => (float) Math.Log(x)},
        {"log10", x => (float) Math.Log10(x)},
        // {"log2", x => (float) Math.Log2(x)}, // Math.Log2() not recognized - find replacement
        {"sqrt", x => (float) Math.Sqrt(x)},
        // {"cbrt", x => (float) Math.Cbrt(x)}, // Math.Cbrt() not recognized - find replacement
        {"abs", x => (float) Math.Abs(x)}
    };
    static readonly string[] unaryFunctions = unaryFuncExpressions.Keys.ToArray();
    static readonly string[] vars = new string[]
    {
        "x","z","t"
    };
    static readonly Dictionary<string, Func<float, float, float>> binaryFuncExpressions = new Dictionary<string, Func<float, float, float>>
    {
        {"log", (x, z) => (float) Math.Log(x, z)},
        {"atan2", (x, z) => (float) Math.Atan2(x, z)},
        // {"mod", (x, z) => (float) x % z},
        {"max", (x, z) => (float) Math.Max(x, z)},
        {"min", (x, z) => (float) Math.Min(x, z)},
    };
    static readonly string[] binaryFunctions = binaryFuncExpressions.Keys.ToArray();
    // Later on we might sort the keyword list alphabetically to speed up keyword matching.
    // For now we don't expect the list of keywords to grow to a size such that
    // it becomes a problem
    static readonly string[] keywords = unaryFunctions.Concat(binaryFunctions.Concat(vars)).ToArray();
    public enum TokenType
    {
        Operator,
        Int,
        Float,
        Delimiter,
        Identifier
    }
    public readonly struct Token
    {
        public readonly TokenType type;
        public readonly char opValue;
        public readonly int intValue;
        public readonly float floatValue;
        public readonly char delimiter;
        public readonly string identifier;
        public Token(TokenType tType, char CharValue)
        {
            type = tType;
            if (type == TokenType.Operator)
            {
                if (operators.Contains(CharValue))
                {
                    opValue = CharValue;
                    intValue = default;
                    floatValue = default;
                    delimiter = '?';
                    identifier = "?";
                }
                else
                    throw new System.ArgumentException($"Token of type Operator may not hold value \"{CharValue}\".", "CharValue");
            }
            else if (type == TokenType.Delimiter)
            {
                if (delimiters.Contains(CharValue))
                {
                    opValue = '?';
                    intValue = default;
                    floatValue = default;
                    delimiter = CharValue;
                    identifier = "?";
                }
                else
                    throw new ArgumentException("Token of type Delimiter must be either a parenthesis or a comma.", "CharValue");
            }
            else
                throw new ArgumentException($"Token type {type} does not hold a char.", "tType");
        }

        public Token(TokenType tType, int IntValue)
        {
            type = tType;
            if (type == TokenType.Int)
            {
                opValue = '?';
                intValue = IntValue;
                floatValue = default;
                delimiter = '?';
                identifier = "?";
            }
            else
                throw new ArgumentException();
        }

        public Token(TokenType tType, float FloatValue)
        {
            type = tType;
            if (type == TokenType.Float)
            {
                opValue = '?';
                intValue = default;
                floatValue = FloatValue;
                delimiter = '?';
                identifier = "?";
            }
            else
                throw new ArgumentException();
        }
        public Token(TokenType tType, string IdentifierValue)
        {
            type = tType;
            if (type == TokenType.Identifier)
            {
                opValue = '?';
                intValue = default;
                floatValue = default;
                delimiter = '?';
                identifier = IdentifierValue; // we're going to handle keyword matching in the lexer
            }
            else
                throw new ArgumentException($"Token type {type} does not hold a string.", "tType");
        }

        public dynamic DynamicValue() // primarily for debugging purposes
        {
            return type switch
            {
                TokenType.Operator => opValue,
                TokenType.Int => intValue,
                TokenType.Float => floatValue,
                TokenType.Delimiter => delimiter,
                _ => identifier
            };
        }
    }

    public static Queue<Token> LexExpression(string mathExpression)
    {
        Queue<char> textStream = new Queue<char>(mathExpression);
        Queue<Token> tokenStream = new Queue<Token>(textStream.Count);

        char currChar;
        Token token;
        while (textStream.Count > 0)
        {
            currChar = textStream.Peek();
            if (delimiters.Contains(currChar)) // must be delimiter
            {
                token = new Token(TokenType.Delimiter, currChar);
                textStream.Dequeue();
            }
            else if (operators.Contains(currChar)) // must be operator
            {
                token = new Token(TokenType.Operator, currChar);
                textStream.Dequeue();
            }
            else if (currChar >= '0' && currChar <= '9') // must be numeric (int or float)
                token = LexNumber(textStream);
            else if (currChar >= 'a' && currChar <= 'z') // valid identifiers can only have lowercase letters
                token = LexIdentifier(textStream);
            else if (currChar == ' ')
            {
                textStream.Dequeue();
                continue;
            }
            else
                throw new ArgumentException("Invalid or misplaced character found in expression.", "mathExpression");
            tokenStream.Enqueue(token);
            Debug.Log($"{token.DynamicValue()}, {token.type}");
        }
        return tokenStream;
    }
    static string ReadDigits(Queue<char> textStream)
    {
        char currChar = textStream.Peek();
        string digits = "";
        while (currChar >= '0' && currChar <= '9')
        {
            textStream.Dequeue();
            digits += currChar.ToString();
            if (!textStream.TryPeek(out currChar))
                break;
        }
        return digits;
    }

    public static Token LexNumber(Queue<char> textStream)
    {
        string tokenString = ReadDigits(textStream);

        if (textStream.Count > 0)
        {
            char currChar = textStream.Peek();
            if (currChar == '.') // must be float 
            {
                textStream.Dequeue();
                if (textStream.Count == 0)
                    throw new ArgumentException("Expression ended at decimal point", "textStream");

                textStream.TryPeek(out currChar);
                if (currChar >= '0' && currChar <= '9')
                {
                    tokenString += '.' + ReadDigits(textStream);
                    textStream.TryPeek(out currChar);
                    if (currChar == '.')
                        throw new ArgumentException("Improperly placed decimal point found after floating point number", "textStream");

                    float floatValue = float.Parse(tokenString);
                    return new Token(TokenType.Float, floatValue);
                }
                else
                    throw new ArgumentException("Invalid character found after decimal point.", "textStream");
            }
        }
        // if we make it here, must be an int, end of token
        int intValue = int.Parse(tokenString);
        return new Token(TokenType.Int, intValue);
    }
    static Token LexIdentifier(Queue<char> textStream)
    {
        string tokenString = "";
        char currChar = textStream.Peek();
        while (((currChar >= 'a' && currChar <= 'z') || (currChar >= '0' && currChar <= '9')) && textStream.Count > 0)
        {
            textStream.Dequeue();
            tokenString += currChar.ToString();
            textStream.TryPeek(out currChar);
        }
        if (keywords.Contains(tokenString))
            return new Token(TokenType.Identifier, tokenString);
        else
            throw new ArgumentException($"Invalid identifier {tokenString}", "tokenString");
    }
    public class ExpressionAST
    {
        class TreeNode
        {
            public Token Token { get; }
            public int Arity => Operands.Count;
            public List<TreeNode> Operands { get; }

            public TreeNode(Token value)
            {
                if (value.type == TokenType.Identifier)
                {
                    if (!vars.Contains(value.identifier))
                        throw new ArgumentException($"Identifier {value.identifier} is not a valid variable name", "value");
                }
                else if (value.type != TokenType.Int && value.type != TokenType.Float)
                    throw new ArgumentException($"A leaf node may not hold a token of type {value.type}.", "value");
                this.Token = value;
                this.Operands = new List<TreeNode>();
            }
            public TreeNode(Token function, TreeNode operand)
            {
                if (function.type != TokenType.Identifier)
                    throw new ArgumentException("An internal node must have an identifier as its root token.", "function");
                else if (!unaryFunctions.Contains(function.identifier))
                    throw new ArgumentException($"{function} is not a supported unary function.", "function");

                this.Token = function;
                this.Operands = new List<TreeNode> { operand };
            }
            public TreeNode(Token function, TreeNode operand1, TreeNode operand2)
            {
                if (function.type == TokenType.Identifier)
                    if (!binaryFunctions.Contains(function.identifier))
                        throw new ArgumentException($"{function.identifier} is not a supported binary function.", "function");
                    else if (function.type != TokenType.Operator)
                        throw new ArgumentException("An internal node must have an operator or a binary function identifier as its root token.", "function");
                    else if (!unaryFunctions.Contains(function.identifier) && !operators.Contains(function.identifier[0]))
                        throw new ArgumentException($"{function} is not a supported unary function", "function");

                this.Token = function;
                this.Operands = new List<TreeNode> { operand1, operand2 };

            }
            public float NodeEval(float x, float z)
            {
                InvalidOperationException invalidToken = new InvalidOperationException($"Token type {this.Token.type} is not valid for a node of arity {Arity}.");
                switch (this.Arity)
                {
                    case 0:
                        Token value = Token;
                        return value.type switch
                        {
                            TokenType.Int => value.intValue,
                            TokenType.Float => value.floatValue,
                            TokenType.Identifier => value.identifier switch
                            {
                                "x" => x,
                                "z" => z,
                                "t" => throw new InvalidOperationException("Parameter t not found."),
                                _ => throw new InvalidOperationException($"Identifier {value.type} is not a valid expression variable."),
                            },
                            _ => throw invalidToken,
                        };
                    case 1:
                        Func<float, float> function = unaryFuncExpressions[this.Token.identifier];
                        return function(Operands[0].NodeEval(x, z));
                    case 2:
                        switch (this.Token.type)
                        {
                            case TokenType.Operator:
                                Func<float, float, float> op = operatorExpressions[Token.opValue];
                                float operand1 = Operands[0].NodeEval(x, z);
                                float operand2 = Operands[1].NodeEval(x, z);
                                return op(operand1, operand2);
                            case TokenType.Identifier:
                                Func<float, float, float> binaryFunction = binaryFuncExpressions[Token.identifier];
                                return binaryFunction(Operands[0].NodeEval(x, z), Operands[1].NodeEval(x, z));
                            default:
                                throw invalidToken;
                        }
                    default:
                        throw new InvalidOperationException($"Invalid state: {this} has too many leaves for an expression node. You should do an analysis to determine how this state was achieved.");
                }
            }
            public float NodeEval(float x, float z, float t)
            {
                InvalidOperationException invalidToken = new InvalidOperationException($"Token type {this.Token.type} is not valid for a node of arity {Arity}.");
                switch (Arity)
                {
                    case 0:
                        Token value = this.Token;
                        return value.type switch
                        {
                            TokenType.Int => value.intValue,
                            TokenType.Float => value.floatValue,
                            TokenType.Identifier => value.identifier switch
                            {
                                "x" => x,
                                "z" => z,
                                "t" => t,
                                _ => throw new InvalidOperationException($"Identifier {value.type} is not a valid expression variable."),
                            },
                            _ => throw invalidToken,
                        };
                    case 1:
                        Func<float, float> function = unaryFuncExpressions[this.Token.identifier];
                        return function(Operands[0].NodeEval(x, z, t));
                    case 2:
                        switch (this.Token.type)
                        {
                            case TokenType.Operator:
                                Func<float, float, float> op = operatorExpressions[this.Token.opValue];
                                return op(Operands[0].NodeEval(x, z, t), Operands[1].NodeEval(x, z, t));
                            case TokenType.Identifier:
                                Func<float, float, float> binaryFunction = binaryFuncExpressions[this.Token.identifier];
                                return binaryFunction(Operands[0].NodeEval(x, z, t), Operands[1].NodeEval(x, z, t));
                            default:
                                throw invalidToken;
                        }
                    default:
                        throw new InvalidOperationException($"Invalid state: {this} has too many leaves for an expression node. You should do an analysis to determine how this state was achieved.");
                }
            }
            public void PrintSubTree()
            {
                foreach (TreeNode node in Operands)
                {
                    Token t = node.Token;
                    dynamic val = t.type switch
                    {
                        TokenType.Operator => t.opValue,
                        TokenType.Int => t.intValue,
                        TokenType.Float => t.floatValue,
                        TokenType.Delimiter => t.delimiter,
                        _ => t.identifier
                    };
                    Debug.Log($"{val}, {t.type}");
                }
                foreach (TreeNode node in Operands)
                    node.PrintSubTree();
            }
        }

        TreeNode root;
        public Token rootToken => root.Token;
        public ExpressionAST(Token value)
        {
            root = new TreeNode(value);
        }
        public ExpressionAST(Token value, ExpressionAST child)
        {
            root = new TreeNode(value, child.root);
        }
        public ExpressionAST(Token value, ExpressionAST child1, ExpressionAST child2)
        {
            root = new TreeNode(value, child1.root, child2.root);
        }
        public float ASTeval(float x, float z)
        {
            return root.NodeEval(x, z);
        }
        public float ASTeval(float x, float z, float t)
        {
            return root.NodeEval(x, z, t);
        }
        public void PrintTree()
        {
            Token t = root.Token;
            dynamic val = t.type switch
            {
                TokenType.Operator => t.opValue,
                TokenType.Int => t.intValue,
                TokenType.Float => t.floatValue,
                TokenType.Delimiter => t.delimiter, 
                _ => t.identifier
            };
            root.PrintSubTree();
        }
    }

    static ExpressionAST ParseTerm(Queue<Token> tokenStream)
    {
        Token currToken = tokenStream.Peek();
        switch (currToken.type)
        {
            case TokenType.Identifier:
                if (vars.Contains(currToken.identifier))
                {
                    tokenStream.Dequeue();
                    return new ExpressionAST(currToken);
                }
                else
                    return ParseFunction(tokenStream);

            case TokenType.Int:
                tokenStream.Dequeue();
                return new ExpressionAST(currToken);

            case TokenType.Float:
                tokenStream.Dequeue();
                return new ExpressionAST(currToken);

            case TokenType.Delimiter:
                return ParseParenthetical(tokenStream);

            default: // must be operator
                throw new ArgumentException("Extraneous operator found.", "tokenStream");
        }
    }
    static ExpressionAST ParseFunction(Queue<Token> tokenStream)
    {
        Token functionToken = tokenStream.Dequeue();
        string functionIdentifier = functionToken.identifier;
        if (functionToken.type != TokenType.Identifier)
            throw new ArgumentException("Initial token of a function expression must be an identifier", "tokenStream");
        else if (unaryFunctions.Contains(functionIdentifier))
        {
            ExpressionAST argumentTree = ParseParenthetical(tokenStream);
            return new ExpressionAST(functionToken, argumentTree);
        }
        else if (binaryFunctions.Contains(functionIdentifier))
        {
            switch (tokenStream.Dequeue())
            {
                case {delimiter: '('}:
                    ExpressionAST argument1 = ParseSum(tokenStream);
                    Token comma = tokenStream.Dequeue();
                    if (comma.delimiter != ',')
                        throw new ArgumentException("Invalid token found in binary function expression, a comma is expected", "tokenStream");
                    ExpressionAST argument2 = ParseSum(tokenStream);
                    Token rightParen = tokenStream.Dequeue();
                    if (rightParen.delimiter != ')')
                        throw new ArgumentException("Invalid token found in binary function expression, ')' is expected", "tokenStream");
                    return new ExpressionAST(functionToken, argument1, argument2);

                default:
                    throw new ArgumentException("Invalid token found after function identifier, '(' is expected", "tokenStream");
            }
        }
        else // must be a var (this is type-checked during token initialization)
            throw new ArgumentException($"Variable identifier {functionIdentifier} cannot be used as a function identifier", "tokenStream");
    }
    static ExpressionAST ParseParenthetical(Queue<Token> tokenStream)
    {
        switch (tokenStream.Dequeue())
        {
            case { delimiter: '(' }:
                ExpressionAST expression = ParseSum(tokenStream);
                Token t = tokenStream.Dequeue();                 
                switch (t)
                {
                    case { delimiter: ')' }:
                        return expression;
                    default:
                        dynamic tval = t.type switch
                        {
                            TokenType.Delimiter => t.delimiter,
                            TokenType.Identifier => t.identifier,
                            TokenType.Float => t.floatValue,
                            TokenType.Int => t.intValue,
                            TokenType.Operator => t.opValue,
                            _ => null
                        };
                        throw new ArgumentException($"Invalid token {tval} in parenthesized expression; ')' is expected.", "tokenStream");
                }
            default:
                throw new ArgumentException("Invalid token in parenthesized expression; '(' is expected.", "tokenStream");
        }
    }
    static ExpressionAST ParseSum(Queue<Token> tokenStream)
    {
        ExpressionAST sum = ParseProduct(tokenStream);
        while (tokenStream.TryPeek(out Token expectedPlusOrMinus))
        {
            switch (expectedPlusOrMinus.opValue)
            {
                case '+':
                    tokenStream.Dequeue();
                    ExpressionAST summand = ParseProduct(tokenStream);
                    sum = new ExpressionAST(expectedPlusOrMinus, sum, summand);
                    break;

                case '-':
                    goto case '+';

                default:
                    return sum;
            }
        }

        return sum;
    }
    static ExpressionAST ParseProduct(Queue<Token> tokenStream)
    {
        ExpressionAST product = ParseExponent(tokenStream);
        while (tokenStream.TryPeek(out Token expectedTimesOrDivide))
        {
            switch (expectedTimesOrDivide.opValue)
            {
                case '*':
                    tokenStream.Dequeue();
                    ExpressionAST newFactor = ParseExponent(tokenStream);
                    product = new ExpressionAST(expectedTimesOrDivide, product, newFactor); // muls and divs are left-associative
                    break;

                case '/':
                    goto case '*';

                default:
                    return product;
            }
        }

        return product;
    }
    static ExpressionAST ParseExponent(Queue<Token> tokenStream)
    {
        ExpressionAST expBase = ParseTerm(tokenStream); 
        if (tokenStream.TryPeek(out Token expectedCaret))
        {
            if (expectedCaret.opValue == '^')
            {
                tokenStream.Dequeue();
                ExpressionAST exponent = ParseExponent(tokenStream); // recursing like this guarantees right-associativity
                return new ExpressionAST(expectedCaret, expBase, exponent);
            }
            else
                return expBase;
        }
        else
            return expBase;
    }
    public static ExpressionAST ParseExpression(string mathExpression)
    {
        Queue<Token> tokenStream = LexExpression(mathExpression);
        return ParseSum(tokenStream);
    }
}