using System.Collections.Immutable;
using System.Linq;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public sealed partial class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifierToken , SyntaxToken equalsToken , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.IdentifierToken = identifierToken;
            this.EqualsToken     = equalsToken;
            this.Expression      = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax( SyntaxTree syntaxTree , ImmutableArray<MemberDeclarationSyntax> members , SyntaxToken endOfFileToken )
            : base( syntaxTree )
        {
            this.Members = members;
            this.EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<MemberDeclarationSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }
    }

    public abstract class InvokationExpressionSyntax : ExpressionSyntax
    {
        public InvokationExpressionSyntax( SyntaxTree syntaxTree )
            : base( syntaxTree )
        {
        }
    }

    public sealed partial class CallExpressionSyntax : InvokationExpressionSyntax
    {
        public CallExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , SyntaxToken openParenthesisToken , SeparatedSyntaxList<ExpressionSyntax> arguments , SyntaxToken closeParenthesisToken )
            : base( syntaxTree )
        {
            this.Identifier            = identifier;
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Arguments             = arguments;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CallExpression;

        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }

    // public sealed partial class InfixBinaryExpressionSyntax : InvokationExpressionSyntax
    // {
    //     public InfixBinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax lhsExpression , SyntaxToken identifier , ExpressionSyntax rhsExpression )
    //         : base( syntaxTree )
    //     {
    //         this.Lhs        = lhsExpression;
    //         this.Identifier = identifier;
    //         this.Rhs        = rhsExpression;
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.InfixBinaryCallExpression;

    //     public ExpressionSyntax Lhs { get; }
    //     public SyntaxToken Identifier { get; }
    //     public ExpressionSyntax Rhs { get; }
    // }

    // public sealed partial class InfixUnaryExpressionSyntax : InvokationExpressionSyntax
    // {
    //     public InfixUnaryExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , ExpressionSyntax expression )
    //         : base( syntaxTree )
    //     {
    //         this.Identifier = identifier;
    //         this.Expression = expression;
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.InfixUnaryCallExpression;
    //     public SyntaxToken Identifier { get; }
    //     public ExpressionSyntax Expression { get; }
    // }

    public sealed partial class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword )
            : base( syntaxTree )
        {
            this.Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
        public SyntaxToken Keyword { get; }
    }

    public sealed partial class DoWhileStatementSyntax : StatementSyntax
    {
        public DoWhileStatementSyntax( SyntaxTree syntaxTree , SyntaxToken doKeyword , StatementSyntax body , SyntaxToken whileKeyword , ExpressionSyntax condition )
            : base( syntaxTree )
        {
            this.DoKeyword    = doKeyword;
            this.Body         = body;
            this.WhileKeyword = whileKeyword;
            this.Condition    = condition;
        }

        public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
        public SyntaxToken DoKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
    }

    public sealed partial class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax( SyntaxTree syntaxTree , SyntaxToken elseKeyword , StatementSyntax elseStatement )
            : base( syntaxTree )
        {
            this.ElseKeyword   = elseKeyword;
            this.ElseStatement = elseStatement;
        }

        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public SyntaxToken ElseKeyword { get; }
        public StatementSyntax ElseStatement { get; }
    }

    public sealed partial class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax( SyntaxTree syntaxTree , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public ExpressionSyntax Expression { get; }
    }

    public abstract class ExpressionSyntax : SyntaxNode
    {
        public ExpressionSyntax( SyntaxTree syntaxTree ) : base( syntaxTree )
        {
        }
    }

    public sealed partial class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword , SyntaxToken identifier , SyntaxToken equalsToken , ExpressionSyntax lowerBound , SyntaxToken toKeyword , ExpressionSyntax upperBound , StatementSyntax body )
            : base( syntaxTree )
        {
            this.Keyword     = keyword;
            this.Identifier  = identifier;
            this.EqualsToken = equalsToken;
            this.LowerBound  = lowerBound;
            this.ToKeyword   = toKeyword;
            this.UpperBound  = upperBound;
            this.Body        = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;
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
            : base( syntaxTree )
        {
            this.FunctionKeyword       = functionKeyword;
            this.Identifier            = identifier;
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Parameters            = parameters;
            this.CloseParenthesisToken = closeParenthesisToken;
            this.ReturnType            = returnType;
            this.Body                  = body;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

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
            : base( syntaxTree )
        {
            this.Statement = statement;
        }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        public StatementSyntax Statement { get; }
    }

    public sealed partial class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax( SyntaxTree syntaxTree , SyntaxToken ifKeyword , ExpressionSyntax condition , StatementSyntax thenStatement , ElseClauseSyntax? elseClause )
            : base( syntaxTree )
        {
            this.IfKeyword     = ifKeyword;
            this.Condition     = condition;
            this.ThenStatement = thenStatement;
            this.ElseClause    = elseClause;
        }

        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public SyntaxToken IfKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax? ElseClause { get; }
    }

    public sealed partial class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken literalToken )
            : base( syntaxTree )
        {
            this.LiteralToken = literalToken;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        public SyntaxToken LiteralToken { get; }
    }

    public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax left , SyntaxToken operatorToken , ExpressionSyntax right )
            : base( syntaxTree )
        {
            this.Lhs          = left;
            this.OperatorToken = operatorToken;
            this.Rhs         = right;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public ExpressionSyntax Lhs { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Rhs { get; }
    }


    public sealed partial class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax( SyntaxTree syntaxTree , SyntaxToken colonToken , TypeNameSyntax typeName )
            : base( syntaxTree )
        {
            this.ColonToken = colonToken;
            this.TypeName   = typeName;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; }
        public TypeNameSyntax TypeName { get; }
    }


    public abstract class TypeSyntax : ExpressionSyntax
    {
        public TypeSyntax( SyntaxTree syntaxTree ) : base( syntaxTree )
        {
        }
    }

    public sealed partial class TypeNameSyntax : TypeSyntax
    {
        public TypeNameSyntax( SyntaxTree syntaxTree , SyntaxToken? identifier /*, ImmutableArray<TypeSpecifierSyntax> typeSpecifier , ImmutableArray<ArrayTypeSpecifierSyntax> arrayTypeSpecifier */)
            : base( syntaxTree )
        {
            this.Identifier         = identifier;
            // this.TypeSpecifier      = typeSpecifier;
            // this.ArrayTypeSpecifier = arrayTypeSpecifier;
        }

        public SyntaxToken? Identifier { get; }
        // public ImmutableArray<TypeSpecifierSyntax> TypeSpecifier { get; }
        // public ImmutableArray<ArrayTypeSpecifierSyntax> ArrayTypeSpecifier { get; }

        public override SyntaxKind Kind => SyntaxKind.Type;
    }

    // public sealed partial class TupleTypeSyntax : TypeSyntax
    // {
    //     public TupleTypeSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree )
    //     {
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.TupleType;
    // }

    // public sealed partial class TupleTypeElementSyntax : TypeSyntax
    // {
    //     public TupleTypeElementSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree )
    //     {
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.TupleElementType;
    // }

    // public sealed partial class ArrayTypeSpecifierSyntax : SyntaxNode
    // {
    //     public ArrayTypeSpecifierSyntax( SyntaxTree syntaxTree , SyntaxToken openBracket , ExpressionSyntax rankExpression , SyntaxToken closeBracket )
    //         : base( syntaxTree )
    //     {
    //         this.OpenBracket    = openBracket;
    //         this.RankExpression = rankExpression;
    //         this.CloseBracket   = closeBracket;
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.ArrayTypeSpecifier;

    //     public SyntaxToken OpenBracket { get; }
    //     public ExpressionSyntax RankExpression { get; }
    //     public SyntaxToken CloseBracket { get; }

    // }

    // public sealed partial class TypeSpecifierSyntax : SyntaxNode // TODO: these should be separate classes for ref/null/ptr
    // {
    //     public TypeSpecifierSyntax( SyntaxTree syntaxTree , SyntaxToken specifierToken , SyntaxKind specifierKind )
    //         : base( syntaxTree )
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
    //         : base( syntaxTree )
    //     {
    //         this.OpenParen  = openParen;
    //         this.Arguments  = arguments;
    //         this.CloseParen = closeParen;
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.TupleExpression;

    //     public SyntaxToken OpenParen { get; }
    //     public SeparatedSyntaxList<IdentifierNameSyntax> Arguments { get; }
    //     public SyntaxToken CloseParen { get; }

    // }

    public abstract class StatementSyntax : SyntaxNode
    {
        public StatementSyntax( SyntaxTree syntaxTree ) : base( syntaxTree )
        {
        }
    }

    public sealed partial class NameExpressionSyntax : IdentifierNameSyntax
    {
        public NameExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifierToken )
            : base( syntaxTree )
        {
            this.IdentifierNameToken = identifierToken;
        }

        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        public SyntaxToken IdentifierNameToken { get; }
    }

    public abstract class NameSyntax : TypeSyntax
    {
        public NameSyntax( SyntaxTree syntaxTree ) : base( syntaxTree )
        {
        }
    }

    public sealed partial class MatchExpressionSyntax : ExpressionSyntax
    {
        public MatchExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken matchKeyword , ExpressionSyntax expression , ImmutableArray<PatternSectionExpressionSyntax> patternSections )
            : base( syntaxTree )
        {
            this.MatchKeyword    = matchKeyword;
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public override SyntaxKind Kind => SyntaxKind.MatchExpression;

        public PatternSectionExpressionSyntax? DefaultPatternSection => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public SyntaxToken MatchKeyword { get; }
        public ExpressionSyntax Expression { get; }
        public ImmutableArray<PatternSectionExpressionSyntax> PatternSections { get; }
    }

    public sealed partial class MatchStatementSyntax : StatementSyntax
    {
        public MatchStatementSyntax( SyntaxTree syntaxTree , SyntaxToken matchKeyword , ExpressionSyntax expression , ImmutableArray<PatternSectionStatementSyntax> patternSections )
            : base( syntaxTree )
        {
            this.MatchKeyword    = matchKeyword;
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public override SyntaxKind Kind => SyntaxKind.MatchStatement;

        public PatternSectionStatementSyntax? DefaultPatternSection => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public SyntaxToken MatchKeyword { get; }
        public ExpressionSyntax Expression { get; }
        public ImmutableArray<PatternSectionStatementSyntax> PatternSections { get; }
    }

    public sealed partial class PatternSectionExpressionSyntax : ExpressionSyntax
    {
        public PatternSectionExpressionSyntax( SyntaxTree syntaxTree , SeparatedSyntaxList<PatternSyntax> patterns , SyntaxToken arrowToken , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.Patterns   = patterns;
            this.ArrowToken = arrowToken;
            this.Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.PatternSectionExpression;

        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == SyntaxKind.MatchAnyPattern );

        public SeparatedSyntaxList<PatternSyntax> Patterns { get; }
        public SyntaxToken ArrowToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class PatternSectionStatementSyntax : ExpressionSyntax
    {
        public PatternSectionStatementSyntax( SyntaxTree syntaxTree , SeparatedSyntaxList<PatternSyntax> patterns , SyntaxToken arrowToken , StatementSyntax statement )
            : base( syntaxTree )
        {
            this.Patterns   = patterns;
            this.ArrowToken = arrowToken;
            this.Statement  = statement;
        }

        public override SyntaxKind Kind => SyntaxKind.PatternSectionStatement;

        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == SyntaxKind.MatchAnyPattern );

        public SeparatedSyntaxList<PatternSyntax> Patterns { get; }
        public SyntaxToken ArrowToken { get; }
        public StatementSyntax Statement { get; }
    }

    public abstract class PatternSyntax : SyntaxNode
    {
        public PatternSyntax( SyntaxTree syntaxTree ) : base( syntaxTree ) { }
    }

    public sealed partial class MatchAnyPatternSyntax : PatternSyntax
    {
        public MatchAnyPatternSyntax( SyntaxTree syntaxTree , SyntaxToken underscoreToken )
            : base( syntaxTree )
        {
            this.UnderscoreToken = underscoreToken;
        }

        public override SyntaxKind Kind => SyntaxKind.MatchAnyPattern;

        public SyntaxToken UnderscoreToken { get; }
    }


    public sealed partial class ConstantPatternSyntax : PatternSyntax
    {
        public ConstantPatternSyntax( SyntaxTree syntaxTree , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ConstantPattern;

        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class InfixPatternSyntax : PatternSyntax
    {
        public InfixPatternSyntax( SyntaxTree syntaxTree , SyntaxToken infixIdentifierToken , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.InfixIdentifierToken = infixIdentifierToken;
            this.Expression           = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.InfixPattern;

        public SyntaxToken InfixIdentifierToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public abstract class MemberDeclarationSyntax : SyntaxNode
    {
        public MemberDeclarationSyntax( SyntaxTree syntaxTree ) : base( syntaxTree )
        {
        }
    }

    public abstract class IdentifierNameSyntax : NameSyntax
    {
        public IdentifierNameSyntax( SyntaxTree syntaxTree ) : base( syntaxTree ) { }
    }


    // public sealed partial class QualifiedNameSyntax : NameSyntax
    // {
    //     public QualifiedNameSyntax( SyntaxTree syntaxTree , NameSyntax lhsName , SyntaxToken dotToken , NameSyntax rhsName )
    //         : base( syntaxTree )
    //     {
    //         this.LhsName  = lhsName;
    //         this.DotToken = dotToken;
    //         this.RhsName  = rhsName;
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.QualifiedName;
    //     public NameSyntax LhsName { get; }
    //     public SyntaxToken DotToken { get; }
    //     public NameSyntax RhsName { get; }
    // }

    public sealed partial class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , TypeClauseSyntax type )
            : base( syntaxTree )
        {
            this.Identifier = identifier;
            this.Type       = type;
        }

        public override SyntaxKind Kind => SyntaxKind.Parameter;
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax Type { get; }
    }

    public sealed partial class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        public ParenthesizedExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken openParenthesisToken , ExpressionSyntax expression , SyntaxToken closeParenthesisToken )
            : base( syntaxTree )
        {
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Expression            = expression;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }

    public sealed partial class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax( SyntaxTree syntaxTree , SyntaxToken returnKeyword , ExpressionSyntax? expression )
            : base( syntaxTree )
        {
            this.ReturnKeyword = returnKeyword;
            this.Expression    = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax? Expression { get; }
    }

    // public sealed partial class DiscardSyntax : SyntaxNode
    // {
    //     public DiscardSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree )
    //     {
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.Discard;
    // }

    // public sealed partial class AnnotationSyntax : SyntaxNode
    // {
    //     public AnnotationSyntax( SyntaxTree syntaxTree )
    //         : base( syntaxTree )
    //     {
    //     }

    //     public override SyntaxKind Kind => SyntaxKind.Annotation;
    // }

    public sealed partial class DeferStatementSyntax : StatementSyntax
    {
        public DeferStatementSyntax( SyntaxTree syntaxTree , SyntaxToken deferKeyword , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.DeferKeyword = deferKeyword;
            this.Expression   = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.DeferStatement;

        public SyntaxToken DeferKeyword { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken operatorToken , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.OperatorToken = operatorToken;
            this.Expression       = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Expression { get; }
    }

    public sealed partial class VariableDeclarationSyntax : StatementSyntax
    {
        public VariableDeclarationSyntax( SyntaxTree syntaxTree , SyntaxToken keyword , SyntaxToken identifier , TypeClauseSyntax? typeClause , SyntaxToken equalsToken , ExpressionSyntax initializer )
            : base( syntaxTree )
        {
            this.Keyword = keyword;
            this.Identifier = identifier;
            this.TypeClause = typeClause;
            this.EqualsToken = equalsToken;
            this.Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax? TypeClause { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }
    }

    public sealed partial class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax( SyntaxTree syntaxTree , SyntaxToken whileKeyword , ExpressionSyntax condition , StatementSyntax body )
            : base( syntaxTree )
        {
            this.WhileKeyword = whileKeyword;
            this.Condition    = condition;
            this.Body         = body;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
    }

    public sealed partial class BlockStatementSyntax : StatementSyntax
    {
        public BlockStatementSyntax( SyntaxTree syntaxTree , SyntaxToken openBraceToken , ImmutableArray<StatementSyntax> statements , SyntaxToken closeBraceToken )
            : base( syntaxTree )
        {
            this.OpenBraceToken  = openBraceToken;
            this.Statements      = statements;
            this.CloseBraceToken = closeBraceToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public SyntaxToken OpenBraceToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseBraceToken { get; }
    }

    public sealed partial class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword )
            : base( syntaxTree )
        {
            this.Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public SyntaxToken Keyword { get; }
    }
}
