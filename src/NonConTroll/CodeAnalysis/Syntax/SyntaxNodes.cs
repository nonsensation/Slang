using System.Collections.Immutable;
using System.Linq;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public sealed partial class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifierToken , SyntaxToken equalsToken , ExpressionSyntax expression )
            : base( syntaxTree , SyntaxKind.AssignmentExpression )
        {
            this.IdentifierToken = identifierToken;
            this.EqualsToken     = equalsToken;
            this.Expression      = expression;
        }

        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax( SyntaxTree syntaxTree , ImmutableArray<MemberDeclarationSyntax> members , SyntaxToken endOfFileToken )
            : base( syntaxTree , SyntaxKind.CompilationUnit )
        {
            this.Members = members;
            this.EndOfFileToken = endOfFileToken;
        }

        public ImmutableArray<MemberDeclarationSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }
    }

    public abstract class InvokationExpressionSyntax : ExpressionSyntax
    {
        public InvokationExpressionSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {
        }
    }

    public sealed partial class CallExpressionSyntax : InvokationExpressionSyntax
    {
        public CallExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , SyntaxToken openParenthesisToken , SeparatedSyntaxList<ExpressionSyntax> arguments , SyntaxToken closeParenthesisToken )
            : base( syntaxTree , SyntaxKind.CallExpression )
        {
            this.Identifier            = identifier;
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Arguments             = arguments;
            this.CloseParenthesisToken = closeParenthesisToken;
        }


        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }

    // public sealed partial class InfixBinaryExpressionSyntax : InvokationExpressionSyntax
    // {
    //     public InfixBinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax lhsExpression , SyntaxToken identifier , ExpressionSyntax rhsExpression )
    //         : base( syntaxTree , SyntaxKind.InfixBinaryCallExpression )
    //     {
    //         this.Lhs        = lhsExpression;
    //         this.Identifier = identifier;
    //         this.Rhs        = rhsExpression;
    //     }

    //     public ExpressionSyntax Lhs { get; }
    //     public SyntaxToken Identifier { get; }
    //     public ExpressionSyntax Rhs { get; }
    // }

    // public sealed partial class InfixUnaryExpressionSyntax : InvokationExpressionSyntax
    // {
    //     public InfixUnaryExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , ExpressionSyntax expression )
    //         : base( syntaxTree , SyntaxKind.InfixUnaryCallExpression )
    //     {
    //         this.Identifier = identifier;
    //         this.Expression = expression;
    //     }

    //     public SyntaxToken Identifier { get; }
    //     public ExpressionSyntax Expression { get; }
    // }

    public sealed partial class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword )
            : base( syntaxTree , SyntaxKind.ContinueStatement )
        {
            this.Keyword = keyword;
        }

        public SyntaxToken Keyword { get; }
    }

    public sealed partial class DoWhileStatementSyntax : StatementSyntax
    {
        public DoWhileStatementSyntax( SyntaxTree syntaxTree , SyntaxToken doKeyword , StatementSyntax body , SyntaxToken whileKeyword , ExpressionSyntax condition )
            : base( syntaxTree , SyntaxKind.DoWhileStatement )
        {
            this.DoKeyword    = doKeyword;
            this.Body         = body;
            this.WhileKeyword = whileKeyword;
            this.Condition    = condition;
        }

        public SyntaxToken DoKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
    }

    public sealed partial class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax( SyntaxTree syntaxTree , SyntaxToken elseKeyword , StatementSyntax elseStatement )
            : base( syntaxTree , SyntaxKind.ElseClause )
        {
            this.ElseKeyword   = elseKeyword;
            this.ElseStatement = elseStatement;
        }

        public SyntaxToken ElseKeyword { get; }
        public StatementSyntax ElseStatement { get; }
    }

    public sealed partial class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax( SyntaxTree syntaxTree , ExpressionSyntax expression )
            : base( syntaxTree , SyntaxKind.ExpressionStatement )
        {
            this.Expression = expression;
        }

        public ExpressionSyntax Expression { get; }
    }

    public abstract class ExpressionSyntax : SyntaxNode
    {
        public ExpressionSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {
        }
    }

    public sealed partial class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword , SyntaxToken identifier , SyntaxToken equalsToken , ExpressionSyntax lowerBound , SyntaxToken toKeyword , ExpressionSyntax upperBound , StatementSyntax body )
            : base( syntaxTree , SyntaxKind.ForStatement )
        {
            this.Keyword     = keyword;
            this.Identifier  = identifier;
            this.EqualsToken = equalsToken;
            this.LowerBound  = lowerBound;
            this.ToKeyword   = toKeyword;
            this.UpperBound  = upperBound;
            this.Body        = body;
        }

        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax UpperBound { get; }
        public StatementSyntax Body { get; }
    }

    public sealed partial class FunctionDeclarationSyntax : MemberDeclarationSyntax
    {
        public FunctionDeclarationSyntax( SyntaxTree syntaxTree , SyntaxToken functionKeyword , SyntaxToken identifier , SyntaxToken openParenthesisToken , SeparatedSyntaxList<ParameterSyntax> parameters , SyntaxToken closeParenthesisToken , TypeClauseSyntax? returnType , BlockStatementSyntax body )
            : base( syntaxTree , SyntaxKind.FunctionDeclaration )
        {
            this.FunctionKeyword       = functionKeyword;
            this.Identifier            = identifier;
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Parameters            = parameters;
            this.CloseParenthesisToken = closeParenthesisToken;
            this.ReturnType            = returnType;
            this.Body                  = body;
        }


        public SyntaxToken FunctionKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public TypeClauseSyntax? ReturnType { get; }
        public BlockStatementSyntax Body { get; }
    }

    public sealed partial class GlobalStatementSyntax : MemberDeclarationSyntax
    {
        public GlobalStatementSyntax( SyntaxTree syntaxTree , StatementSyntax statement )
            : base( syntaxTree , SyntaxKind.GlobalStatement )
        {
            this.Statement = statement;
        }

        public StatementSyntax Statement { get; }
    }

    public sealed partial class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax( SyntaxTree syntaxTree , SyntaxToken ifKeyword , ExpressionSyntax condition , StatementSyntax thenStatement , ElseClauseSyntax? elseClause )
            : base( syntaxTree , SyntaxKind.IfStatement )
        {
            this.IfKeyword     = ifKeyword;
            this.Condition     = condition;
            this.ThenStatement = thenStatement;
            this.ElseClause    = elseClause;
        }

        public SyntaxToken IfKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax? ElseClause { get; }
    }

    public sealed partial class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken literalToken )
            : base( syntaxTree , SyntaxKind.LiteralExpression )
        {
            this.LiteralToken = literalToken;
        }

        public SyntaxToken LiteralToken { get; }
    }

    public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax left , SyntaxToken operatorToken , ExpressionSyntax right )
            : base( syntaxTree , SyntaxKind.BinaryExpression )
        {
            this.Lhs          = left;
            this.OperatorToken = operatorToken;
            this.Rhs         = right;
        }

        public ExpressionSyntax Lhs { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Rhs { get; }
    }


    public sealed partial class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax( SyntaxTree syntaxTree , SyntaxToken colonToken , TypeNameSyntax typeName )
            : base( syntaxTree , SyntaxKind.TypeClause )
        {
            this.ColonToken = colonToken;
            this.TypeName   = typeName;
        }

        public SyntaxToken ColonToken { get; }
        public TypeNameSyntax TypeName { get; }
    }


    public abstract class TypeSyntax : ExpressionSyntax
    {
        public TypeSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {
        }
    }

    public sealed partial class TypeNameSyntax : TypeSyntax
    {
        public TypeNameSyntax( SyntaxTree syntaxTree , SyntaxToken? identifier /*, ImmutableArray<TypeSpecifierSyntax> typeSpecifier , ImmutableArray<ArrayTypeSpecifierSyntax> arrayTypeSpecifier */)
            : base( syntaxTree , SyntaxKind.Type )
        {
            this.Identifier         = identifier;
            // this.TypeSpecifier      = typeSpecifier;
            // this.ArrayTypeSpecifier = arrayTypeSpecifier;
        }

        public SyntaxToken? Identifier { get; }
        // public ImmutableArray<TypeSpecifierSyntax> TypeSpecifier { get; }
        // public ImmutableArray<ArrayTypeSpecifierSyntax> ArrayTypeSpecifier { get; }

    }

    // public sealed partial class TupleTypeSyntax : TypeSyntax
    // {
    //     public TupleTypeSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree , SyntaxKind.TupleType )
    //     {
    //     }
    // }

    // public sealed partial class TupleTypeElementSyntax : TypeSyntax
    // {
    //     public TupleTypeElementSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree , SyntaxKind.TupleElementType )
    //     {
    //     }
    // }

    // public sealed partial class ArrayTypeSpecifierSyntax : SyntaxNode
    // {
    //     public ArrayTypeSpecifierSyntax( SyntaxTree syntaxTree , SyntaxToken openBracket , ExpressionSyntax rankExpression , SyntaxToken closeBracket )
    //         : base( syntaxTree , SyntaxKind.ArrayTypeSpecifier )
    //     {
    //         this.OpenBracket    = openBracket;
    //         this.RankExpression = rankExpression;
    //         this.CloseBracket   = closeBracket;
    //     }

    //     public SyntaxToken OpenBracket { get; }
    //     public ExpressionSyntax RankExpression { get; }
    //     public SyntaxToken CloseBracket { get; }

    // }

    // public sealed partial class TypeSpecifierSyntax : SyntaxNode // TODO: these should be separate classes for ref/null/ptr
    // {
    //     public TypeSpecifierSyntax( SyntaxTree syntaxTree , SyntaxToken specifierToken , SyntaxKind specifierKind )
    //         : base( syntaxTree , SyntaxKind.SpecifierKind )
    //     {
    //         this.SpecifierToken = specifierToken;
    //         this.SpecifierKind  = specifierKind;
    //     }

    //     public override SyntaxKind Kind => this.SpecifierKind;

    //     public SyntaxToken SpecifierToken { get; }

    //     private SyntaxKind SpecifierKind { get; }
    // }

    // public sealed partial class TupleExpressionSyntax : ExpressionSyntax
    // {
    //     public TupleExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken openParen , SeparatedSyntaxList<IdentifierNameSyntax> arguments , SyntaxToken closeParen )
    //         : base( syntaxTree , SyntaxKind.TupleExpression )
    //     {
    //         this.OpenParen  = openParen;
    //         this.Arguments  = arguments;
    //         this.CloseParen = closeParen;
    //     }

    //     public SyntaxToken OpenParen { get; }
    //     public SeparatedSyntaxList<IdentifierNameSyntax> Arguments { get; }
    //     public SyntaxToken CloseParen { get; }

    // }

    public abstract class StatementSyntax : SyntaxNode
    {
        public StatementSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {
        }
    }

    public sealed partial class NameExpressionSyntax : IdentifierNameSyntax
    {
        public NameExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifierToken )
            : base( syntaxTree , SyntaxKind.NameExpression )
        {
            this.IdentifierNameToken = identifierToken;
        }

        public SyntaxToken IdentifierNameToken { get; }
    }

    public abstract class NameSyntax : TypeSyntax
    {
        public NameSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {
        }
    }

    public sealed partial class MatchExpressionSyntax : ExpressionSyntax
    {
        public MatchExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken matchKeyword , ExpressionSyntax expression , ImmutableArray<PatternSectionExpressionSyntax> patternSections )
            : base( syntaxTree , SyntaxKind.MatchExpression )
        {
            this.MatchKeyword    = matchKeyword;
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }


        public PatternSectionExpressionSyntax? DefaultPatternSection => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public SyntaxToken MatchKeyword { get; }
        public ExpressionSyntax Expression { get; }
        public ImmutableArray<PatternSectionExpressionSyntax> PatternSections { get; }
    }

    public sealed partial class MatchStatementSyntax : StatementSyntax
    {
        public MatchStatementSyntax( SyntaxTree syntaxTree , SyntaxToken matchKeyword , ExpressionSyntax expression , ImmutableArray<PatternSectionStatementSyntax> patternSections )
            : base( syntaxTree , SyntaxKind.MatchStatement )
        {
            this.MatchKeyword    = matchKeyword;
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }


        public PatternSectionStatementSyntax? DefaultPatternSection => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public SyntaxToken MatchKeyword { get; }
        public ExpressionSyntax Expression { get; }
        public ImmutableArray<PatternSectionStatementSyntax> PatternSections { get; }
    }

    public sealed partial class PatternSectionExpressionSyntax : ExpressionSyntax
    {
        public PatternSectionExpressionSyntax( SyntaxTree syntaxTree , SeparatedSyntaxList<PatternSyntax> patterns , SyntaxToken arrowToken , ExpressionSyntax expression )
            : base( syntaxTree , SyntaxKind.PatternSectionExpression )
        {
            this.Patterns   = patterns;
            this.ArrowToken = arrowToken;
            this.Expression = expression;
        }


        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == SyntaxKind.MatchAnyPattern );

        public SeparatedSyntaxList<PatternSyntax> Patterns { get; }
        public SyntaxToken ArrowToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class PatternSectionStatementSyntax : ExpressionSyntax
    {
        public PatternSectionStatementSyntax( SyntaxTree syntaxTree , SeparatedSyntaxList<PatternSyntax> patterns , SyntaxToken arrowToken , StatementSyntax statement )
            : base( syntaxTree , SyntaxKind.PatternSectionStatement )
        {
            this.Patterns   = patterns;
            this.ArrowToken = arrowToken;
            this.Statement  = statement;
        }


        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == SyntaxKind.MatchAnyPattern );

        public SeparatedSyntaxList<PatternSyntax> Patterns { get; }
        public SyntaxToken ArrowToken { get; }
        public StatementSyntax Statement { get; }
    }

    public abstract class PatternSyntax : SyntaxNode
    {
        public PatternSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {

        }
    }

    public sealed partial class MatchAnyPatternSyntax : PatternSyntax
    {
        public MatchAnyPatternSyntax( SyntaxTree syntaxTree , SyntaxToken underscoreToken )
            : base( syntaxTree , SyntaxKind.MatchAnyPattern )
        {
            this.UnderscoreToken = underscoreToken;
        }


        public SyntaxToken UnderscoreToken { get; }
    }


    public sealed partial class ConstantPatternSyntax : PatternSyntax
    {
        public ConstantPatternSyntax( SyntaxTree syntaxTree , ExpressionSyntax expression )
            : base( syntaxTree , SyntaxKind.ConstantPattern )
        {
            this.Expression = expression;
        }


        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class InfixPatternSyntax : PatternSyntax
    {
        public InfixPatternSyntax( SyntaxTree syntaxTree , SyntaxToken infixIdentifierToken , ExpressionSyntax expression )
            : base( syntaxTree , SyntaxKind.InfixPattern )
        {
            this.InfixIdentifierToken = infixIdentifierToken;
            this.Expression           = expression;
        }


        public SyntaxToken InfixIdentifierToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public abstract class MemberDeclarationSyntax : SyntaxNode
    {
        public MemberDeclarationSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {
        }
    }

    public abstract class IdentifierNameSyntax : NameSyntax
    {
        public IdentifierNameSyntax( SyntaxTree syntaxTree , SyntaxKind kind )
            : base( syntaxTree , kind )
        {
        }
    }


    // public sealed partial class QualifiedNameSyntax : NameSyntax
    // {
    //     public QualifiedNameSyntax( SyntaxTree syntaxTree , NameSyntax lhsName , SyntaxToken dotToken , NameSyntax rhsName )
    //         : base( syntaxTree , SyntaxKind.QualifiedName )
    //     {
    //         this.LhsName  = lhsName;
    //         this.DotToken = dotToken;
    //         this.RhsName  = rhsName;
    //     }

    //     public NameSyntax LhsName { get; }
    //     public SyntaxToken DotToken { get; }
    //     public NameSyntax RhsName { get; }
    // }

    public sealed partial class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , TypeClauseSyntax type )
            : base( syntaxTree , SyntaxKind.Parameter )
        {
            this.Identifier = identifier;
            this.Type       = type;
        }

        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax Type { get; }
    }

    public sealed partial class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        public ParenthesizedExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken openParenthesisToken , ExpressionSyntax expression , SyntaxToken closeParenthesisToken )
            : base( syntaxTree , SyntaxKind.ParenthesizedExpression )
        {
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Expression            = expression;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }

    public sealed partial class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax( SyntaxTree syntaxTree , SyntaxToken returnKeyword , ExpressionSyntax? expression )
            : base( syntaxTree , SyntaxKind.ReturnStatement )
        {
            this.ReturnKeyword = returnKeyword;
            this.Expression    = expression;
        }

        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax? Expression { get; }
    }

    // public sealed partial class DiscardSyntax : SyntaxNode
    // {
    //     public DiscardSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree , SyntaxKind.Discard )
    //     {
    //     }
    // }

    // public sealed partial class AnnotationSyntax : SyntaxNode
    // {
    //     public AnnotationSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree , SyntaxKind.Annotation )
    //     {
    //     }
    // }

    public sealed partial class DeferStatementSyntax : StatementSyntax
    {
        public DeferStatementSyntax( SyntaxTree syntaxTree , SyntaxToken deferKeyword , ExpressionSyntax expression )
            : base( syntaxTree , SyntaxKind.DeferStatement )
        {
            this.DeferKeyword = deferKeyword;
            this.Expression   = expression;
        }


        public SyntaxToken DeferKeyword { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken operatorToken , ExpressionSyntax expression )
            : base( syntaxTree , SyntaxKind.UnaryExpression )
        {
            this.OperatorToken = operatorToken;
            this.Expression       = expression;
        }

        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class VariableDeclarationSyntax : StatementSyntax
    {
        public VariableDeclarationSyntax( SyntaxTree syntaxTree , SyntaxToken keyword , SyntaxToken identifier , TypeClauseSyntax? typeClause , SyntaxToken equalsToken , ExpressionSyntax initializer )
            : base( syntaxTree , SyntaxKind.VariableDeclaration )
        {
            this.Keyword = keyword;
            this.Identifier = identifier;
            this.TypeClause = typeClause;
            this.EqualsToken = equalsToken;
            this.Initializer = initializer;
        }

        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax? TypeClause { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }
    }

    public sealed partial class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax( SyntaxTree syntaxTree , SyntaxToken whileKeyword , ExpressionSyntax condition , StatementSyntax body )
            : base( syntaxTree , SyntaxKind.WhileStatement )
        {
            this.WhileKeyword = whileKeyword;
            this.Condition    = condition;
            this.Body         = body;
        }

        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
    }

    public sealed partial class BlockStatementSyntax : StatementSyntax
    {
        public BlockStatementSyntax( SyntaxTree syntaxTree , SyntaxToken openBraceToken , ImmutableArray<StatementSyntax> statements , SyntaxToken closeBraceToken )
            : base( syntaxTree , SyntaxKind.BlockStatement )
        {
            this.OpenBraceToken  = openBraceToken;
            this.Statements      = statements;
            this.CloseBraceToken = closeBraceToken;
        }

        public SyntaxToken OpenBraceToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseBraceToken { get; }
    }

    public sealed partial class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword )
            : base( syntaxTree , SyntaxKind.BreakStatement )
        {
            this.Keyword = keyword;
        }

        public SyntaxToken Keyword { get; }
    }
}
