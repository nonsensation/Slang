using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class MatchExpressionSyntax : ExpressionSyntax
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

    public class MatchStatementSyntax : StatementSyntax
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

    public class PatternSectionExpressionSyntax : ExpressionSyntax
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

    public class PatternSectionStatementSyntax : ExpressionSyntax
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
        public PatternSyntax( SyntaxTree syntaxTree ) : base( syntaxTree ) {}
    }

    public class MatchAnyPatternSyntax : PatternSyntax
    {
        public MatchAnyPatternSyntax( SyntaxTree syntaxTree , SyntaxToken underscoreToken )
            : base( syntaxTree )
        {
            this.UnderscoreToken = underscoreToken;
        }

        public override SyntaxKind Kind => SyntaxKind.MatchAnyPattern;

        public SyntaxToken UnderscoreToken { get; }
    }

    public class ConstantPatternSyntax : PatternSyntax
    {
        public ConstantPatternSyntax( SyntaxTree syntaxTree , ExpressionSyntax expression )
            : base( syntaxTree )
        {
            this.Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ConstantPattern;

        public ExpressionSyntax Expression { get; }
    }

    public class InfixPatternSyntax : PatternSyntax
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
}

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundMatchExpression : BoundExpression
    {
        public BoundMatchExpression( BoundExpression expression , ImmutableArray<BoundPatternSectionExpression> patternSections )
        {
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public override BoundNodeKind Kind => BoundNodeKind.MatchExpression;
        public override TypeSymbol Type =>
            // TODO: get common type, if this.IsStatement == false
            ( this.PatternSections.FirstOrDefault()?.Expression as BoundExpression )?.Type
            ?? TypeSymbol.Void;

        public BoundPatternSectionExpression? DefaultPattern => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public BoundExpression Expression { get; }
        public ImmutableArray<BoundPatternSectionExpression> PatternSections { get; }
    }

    public class BoundMatchStatement : BoundStatement
    {
        public BoundMatchStatement( BoundExpression expression , ImmutableArray<BoundPatternSectionStatement> patternSections )
        {
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public override BoundNodeKind Kind => BoundNodeKind.MatchExpression;

        public BoundPatternSectionStatement? DefaultPattern => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public BoundExpression Expression { get; }
        public ImmutableArray<BoundPatternSectionStatement> PatternSections { get; }
    }

    public class BoundPatternSectionExpression : BoundExpression
    {
        public BoundPatternSectionExpression( ImmutableArray<BoundPattern> patterns , BoundExpression expression )
        {
            this.Patterns   = patterns;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.PatternSectionExpression;
        public override TypeSymbol Type => this.Expression.Type;

        public ImmutableArray<BoundPattern> Patterns { get; }
        public BoundExpression Expression { get; }
        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == BoundNodeKind.MatchAnyPattern );
    }

    public class BoundPatternSectionStatement : BoundStatement
    {
        public BoundPatternSectionStatement( ImmutableArray<BoundPattern> patterns , BoundStatement statement )
        {
            this.Patterns  = patterns;
            this.Statement = statement;
        }

        public override BoundNodeKind Kind => BoundNodeKind.PatternSectionStatement;

        public ImmutableArray<BoundPattern> Patterns { get; }
        public BoundStatement Statement { get; }
        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == BoundNodeKind.MatchAnyPattern );
    }

    public abstract class BoundPattern : BoundNode
    {
    }

    public class BoundMatchAnyPattern : BoundPattern
    {
        public override BoundNodeKind Kind => BoundNodeKind.MatchAnyPattern;
    }

    public class BoundConstantPattern : BoundPattern
    {
        public BoundConstantPattern( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConstantPattern;

        public BoundExpression Expression { get; }
    }

    public class BoundInfixPattern : BoundPattern
    {
        public BoundInfixPattern( FunctionSymbol infixFunction , BoundExpression expression )
        {
            this.InfixFunction = infixFunction;
            this.Expression    = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.InfixPattern;

        public FunctionSymbol InfixFunction { get; }
        public BoundExpression Expression { get; }
    }
}
