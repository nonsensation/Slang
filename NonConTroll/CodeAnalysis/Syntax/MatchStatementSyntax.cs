using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class MatchExpressionSyntax : ExpressionSyntax
    {
        public MatchExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken matchKeyword , ExpressionSyntax expression , ImmutableArray<PatternSectionSyntax> patternSections , bool isStatement )
            : base( syntaxTree )
        {
            this.MatchKeyword    = matchKeyword;
            this.Expression      = expression;
            this.PatternSections = patternSections;
            this.IsStatement     = isStatement;
        }

        public override SyntaxKind Kind => SyntaxKind.MatchExpression;

        public PatternSectionSyntax? DefaultPatternSection => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public SyntaxToken MatchKeyword { get; }
        public ExpressionSyntax Expression { get; }
        public ImmutableArray<PatternSectionSyntax> PatternSections { get; }
        public bool IsStatement { get; }
    }

    public class PatternSectionSyntax : ExpressionSyntax
    {
        public PatternSectionSyntax( SyntaxTree syntaxTree , SeparatedSyntaxList<PatternSyntax> patterns , SyntaxToken arrowToken , SyntaxNode result )
            : base( syntaxTree )
        {
            Debug.Assert( result is StatementSyntax || result is ExpressionSyntax );

            this.Patterns   = patterns;
            this.ArrowToken = arrowToken;
            this.Result     = result;
        }

        public override SyntaxKind Kind => SyntaxKind.PatternExpression;

        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == SyntaxKind.MatchAnyPattern );

        public SeparatedSyntaxList<PatternSyntax> Patterns { get; }
        public SyntaxToken ArrowToken { get; }
        public SyntaxNode Result { get; }
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
        public BoundMatchExpression( BoundExpression expression , ImmutableArray<BoundPatternSection> patternSections , bool isStatement )
        {
            this.Expression      = expression;
            this.PatternSections = patternSections;
            this.IsStatement     = isStatement;
        }

        public override BoundNodeKind Kind => BoundNodeKind.MatchExpression;
        public override TypeSymbol Type =>
            // TODO: get common type, if this.IsStatement == false
            ( this.PatternSections.FirstOrDefault()?.Result as BoundExpression )?.Type
            ?? TypeSymbol.Void;

        public BoundPatternSection? DefaultPattern => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public BoundExpression Expression { get; }
        public ImmutableArray<BoundPatternSection> PatternSections { get; }
        public bool IsStatement { get; }
    }

    public class BoundPatternSection : BoundNode
    {
        public BoundPatternSection( ImmutableArray<BoundPattern> patterns , BoundNode result )
        {
            Debug.Assert( result is BoundStatement || result is BoundExpression );

            this.Patterns = patterns;
            this.Result   = result;
        }

        public override BoundNodeKind Kind => BoundNodeKind.PatternSection;

        public ImmutableArray<BoundPattern> Patterns { get; }
        public BoundNode Result { get; }
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
