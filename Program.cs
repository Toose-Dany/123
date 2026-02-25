using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string sourceCode = @"
                Var a, b, c
                a = 10
                b = -20
                c = (a + b) * 3 - 100 / 4
                Print c
            ";

            Console.WriteLine("Исходный код:");
            Console.WriteLine(sourceCode);
            Console.WriteLine(new string('-', 50));

            try
            {
                var compiler = new Compiler();
                compiler.Execute(sourceCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.ReadKey();
        }
    }

    
    public class Compiler
    {
        private Lexer _lexer;
        private Parser _parser;
        private Interpreter _interpreter;

        public void Execute(string sourceCode)
        {
            // 1. Лексический анализ
            _lexer = new Lexer(sourceCode);
            var tokens = _lexer.Tokenize();

            Console.WriteLine("Токены:");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
            Console.WriteLine(new string('-', 50));

            // 2. Синтаксический анализ
            _parser = new Parser(tokens);
            var ast = _parser.ParseProgram();

            Console.WriteLine("Дерево разбора (упрощенно):");
            PrintAst(ast);
            Console.WriteLine(new string('-', 50));

            // 3. Интерпретация
            _interpreter = new Interpreter();
            _interpreter.Execute(ast);
        }

        private void PrintAst(AstNode node, int indent = 0)
        {
            Console.WriteLine(new string(' ', indent * 2) + node);
            foreach (var child in node.Children)
            {
                PrintAst(child, indent + 1);
            }
        }
    }

    #region Лексический анализатор (Токенизатор)

    public enum TokenType
    {
        Var,           
        Print,         
        Identifier,    
        Number,        
        Assign,        
        Plus,          
        Minus,         
        Multiply,      
        Divide,        
        LParen,        
        RParen,        
        Comma,         
        EndOfFile      
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }

        public override string ToString()
        {
            return $"[{Line}:{Position}] {Type} '{Value}'";
        }
    }

    public class Lexer
    {
        private readonly string _source;
        private int _position;
        private int _line;
        private int _lineStart;

        public Lexer(string source)
        {
            _source = source;
            _position = 0;
            _line = 1;
            _lineStart = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            Token token;

            do
            {
                token = NextToken();
                tokens.Add(token);
            } while (token.Type != TokenType.EndOfFile);

            return tokens;
        }

        private Token NextToken()
        {
            SkipWhitespace();

            if (_position >= _source.Length)
                return new Token
                {
                    Type = TokenType.EndOfFile,
                    Value = "",
                    Line = _line,
                    Position = _position - _lineStart
                };

            char current = _source[_position];
            int startPos = _position; 

            
            if (char.IsLetter(current))
            {
                string identifier = ReadWhile(c => char.IsLetterOrDigit(c));
                TokenType type = identifier switch
                {
                    "Var" => TokenType.Var,
                    "Print" => TokenType.Print,
                    _ => TokenType.Identifier
                };
                return new Token
                {
                    Type = type,
                    Value = identifier,
                    Line = _line,
                    Position = startPos - _lineStart
                };
            }

            
            if (char.IsDigit(current))
            {
                string number = ReadWhile(c => char.IsDigit(c));
                return new Token
                {
                    Type = TokenType.Number,
                    Value = number,
                    Line = _line,
                    Position = startPos - _lineStart
                };
            }

            
            _position++;

            TokenType tokenType;
            string tokenValue;

            switch (current)
            {
                case '=': tokenType = TokenType.Assign; tokenValue = "="; break;
                case '+': tokenType = TokenType.Plus; tokenValue = "+"; break;
                case '-': tokenType = TokenType.Minus; tokenValue = "-"; break;
                case '*': tokenType = TokenType.Multiply; tokenValue = "*"; break;
                case '/': tokenType = TokenType.Divide; tokenValue = "/"; break;
                case '(': tokenType = TokenType.LParen; tokenValue = "("; break;
                case ')': tokenType = TokenType.RParen; tokenValue = ")"; break;
                case ',': tokenType = TokenType.Comma; tokenValue = ","; break;
                default:
                    throw new Exception($"Неизвестный символ '{current}' в строке {_line}");
            }

            return new Token
            {
                Type = tokenType,
                Value = tokenValue,
                Line = _line,
                Position = startPos - _lineStart
            };
        }

        private void SkipWhitespace()
        {
            while (_position < _source.Length && char.IsWhiteSpace(_source[_position]))
            {
                if (_source[_position] == '\n')
                {
                    _line++;
                    _lineStart = _position + 1;
                }
                _position++;
            }
        }

        private string ReadWhile(Func<char, bool> predicate)
        {
            int start = _position;
            while (_position < _source.Length && predicate(_source[_position]))
                _position++;
            return _source.Substring(start, _position - start);
        }
    }

    #endregion

    #region Синтаксический анализатор (Парсер)

    
    public abstract class AstNode
    {
        public virtual List<AstNode> Children => new List<AstNode>();
    }

    public class ProgramNode : AstNode
    {
        public VarDeclarationNode VarDeclaration { get; set; }
        public List<AssignmentNode> Assignments { get; set; } = new List<AssignmentNode>();
        public PrintNode PrintStatement { get; set; }

        public override List<AstNode> Children
        {
            get
            {
                var children = new List<AstNode> { VarDeclaration };
                children.AddRange(Assignments);
                children.Add(PrintStatement);
                return children;
            }
        }

        public override string ToString() => "Program";
    }

    public class VarDeclarationNode : AstNode
    {
        public List<string> Variables { get; set; } = new List<string>();

        public override string ToString() => $"VarDeclaration: {string.Join(", ", Variables)}";
    }

    public class AssignmentNode : AstNode
    {
        public string Variable { get; set; }
        public ExpressionNode Expression { get; set; }

        public override List<AstNode> Children => new List<AstNode> { Expression };

        public override string ToString() => $"Assignment: {Variable} =";
    }

    public class PrintNode : AstNode
    {
        public string Variable { get; set; }

        public override string ToString() => $"Print: {Variable}";
    }

    public abstract class ExpressionNode : AstNode { }

    public class BinaryOperationNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public TokenType Operator { get; set; }
        public ExpressionNode Right { get; set; }

        public override List<AstNode> Children => new List<AstNode> { Left, Right };

        public override string ToString() => $"BinOp: {Operator}";
    }

    public class UnaryOperationNode : ExpressionNode
    {
        public TokenType Operator { get; set; }
        public ExpressionNode Operand { get; set; }

        public override List<AstNode> Children => new List<AstNode> { Operand };

        public override string ToString() => $"UnaryOp: {Operator}";
    }

    public class IdentifierNode : ExpressionNode
    {
        public string Name { get; set; }

        public override string ToString() => $"Identifier: {Name}";
    }

    public class NumberNode : ExpressionNode
    {
        public int Value { get; set; }

        public override string ToString() => $"Number: {Value}";
    }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        private Token Current => _tokens[_position];
        private Token Previous => _tokens[_position - 1];

        
        public ProgramNode ParseProgram()
        {
            var program = new ProgramNode();

            
            program.VarDeclaration = ParseVarDeclaration();

            
            program.Assignments = ParseAssignments();

            
            program.PrintStatement = ParsePrintStatement();

            
            if (Current.Type != TokenType.EndOfFile)
                throw new Exception($"Ожидался конец программы, найден {Current.Type}");

            return program;
        }

        
        private VarDeclarationNode ParseVarDeclaration()
        {
            Expect(TokenType.Var);
            var variables = ParseVariableList();
            return new VarDeclarationNode { Variables = variables };
        }

        
        private List<string> ParseVariableList()
        {
            var variables = new List<string>();

            
            var ident = Expect(TokenType.Identifier);
            variables.Add(ident.Value);

            
            while (Current.Type == TokenType.Comma)
            {
                NextToken(); // пропус ','
                ident = Expect(TokenType.Identifier);
                variables.Add(ident.Value);
            }

            return variables;
        }

        
        private List<AssignmentNode> ParseAssignments()
        {
            var assignments = new List<AssignmentNode>();

            while (Current.Type == TokenType.Identifier && _tokens[_position + 1].Type == TokenType.Assign)
            {
                assignments.Add(ParseAssignment());
            }

            return assignments;
        }

        
        private AssignmentNode ParseAssignment()
        {
            var identToken = Expect(TokenType.Identifier);
            Expect(TokenType.Assign);
            var expr = ParseExpression();

            return new AssignmentNode
            {
                Variable = identToken.Value,
                Expression = expr
            };
        }

        
        private PrintNode ParsePrintStatement()
        {
            Expect(TokenType.Print);
            var identToken = Expect(TokenType.Identifier);
            return new PrintNode { Variable = identToken.Value };
        }

        
        private ExpressionNode ParseExpression()
        {
            
            if (Current.Type == TokenType.Minus)
            {
                NextToken();
                var subExpr = ParseSubExpression();
                return new UnaryOperationNode
                {
                    Operator = TokenType.Minus,
                    Operand = subExpr
                };
            }

            return ParseSubExpression();
        }

        
        private ExpressionNode ParseSubExpression()
        {
            return ParseBinaryExpression(0);
        }

        
        private readonly Dictionary<TokenType, int> _precedence = new Dictionary<TokenType, int>
        {
            { TokenType.Plus, 1 },
            { TokenType.Minus, 1 },
            { TokenType.Multiply, 2 },
            { TokenType.Divide, 2 }
        };

        private ExpressionNode ParseBinaryExpression(int contextPrecedence)
        {
            ExpressionNode left;

            
            if (Current.Type == TokenType.LParen)
            {
                NextToken(); // пропус '('
                left = ParseExpression();
                Expect(TokenType.RParen);
            }
            else
            {
                left = ParseOperand();
            }

            
            while (true)
            {
                TokenType opType = Current.Type;

                
                if (!IsBinaryOperator(opType))
                    break;

                int currentPrecedence = _precedence[opType];
                if (currentPrecedence <= contextPrecedence)
                    break;

                NextToken(); 

                
                var right = ParseBinaryExpression(currentPrecedence);

                left = new BinaryOperationNode
                {
                    Left = left,
                    Operator = opType,
                    Right = right
                };
            }

            return left;
        }

        private bool IsBinaryOperator(TokenType type)
        {
            return type == TokenType.Plus || type == TokenType.Minus ||
                   type == TokenType.Multiply || type == TokenType.Divide;
        }

        
        private ExpressionNode ParseOperand()
        {
            if (Current.Type == TokenType.Identifier)
            {
                var token = NextToken();
                return new IdentifierNode { Name = token.Value };
            }
            else if (Current.Type == TokenType.Number)
            {
                var token = NextToken();
                return new NumberNode { Value = int.Parse(token.Value) };
            }

            throw new Exception($"Ожидался идентификатор или число, найден {Current.Type}");
        }

        
        private Token NextToken()
        {
            return _tokens[_position++];
        }

        private Token Expect(TokenType expected)
        {
            if (Current.Type != expected)
                throw new Exception($"Ожидался {expected}, найден {Current.Type} в строке {Current.Line}");

            return NextToken();
        }
    }

    #endregion

    #region Интерпретатор

    public class Interpreter
    {
        private Dictionary<string, int> _variables;

        public void Execute(ProgramNode program)
        {
            _variables = new Dictionary<string, int>();

            
            foreach (var varName in program.VarDeclaration.Variables)
            {
                _variables[varName] = 0;
            }

            Console.WriteLine("Выполнение программы:");

            
            foreach (var assignment in program.Assignments)
            {
                int value = EvaluateExpression(assignment.Expression);
                _variables[assignment.Variable] = value;
                Console.WriteLine($"{assignment.Variable} = {value}");
            }

            
            if (program.PrintStatement != null)
            {
                string varName = program.PrintStatement.Variable;
                if (!_variables.ContainsKey(varName))
                    throw new Exception($"Переменная '{varName}' не объявлена");

                int result = _variables[varName];
                Console.WriteLine($"Результат печати: {result}");
            }
        }

        private int EvaluateExpression(ExpressionNode expr)
        {
            switch (expr)
            {
                case NumberNode number:
                    return number.Value;

                case IdentifierNode identifier:
                    if (!_variables.ContainsKey(identifier.Name))
                        throw new Exception($"Переменная '{identifier.Name}' не объявлена");
                    return _variables[identifier.Name];

                case UnaryOperationNode unary:
                    int operand = EvaluateExpression(unary.Operand);
                    return unary.Operator == TokenType.Minus ? -operand : operand;

                case BinaryOperationNode binary:
                    int left = EvaluateExpression(binary.Left);
                    int right = EvaluateExpression(binary.Right);

                    return binary.Operator switch
                    {
                        TokenType.Plus => left + right,
                        TokenType.Minus => left - right,
                        TokenType.Multiply => left * right,
                        TokenType.Divide => left / right, 
                        _ => throw new Exception($"Неизвестный оператор {binary.Operator}")
                    };

                default:
                    throw new Exception($"Неизвестный тип узла {expr.GetType()}");
            }
        }
    }

    #endregion
}