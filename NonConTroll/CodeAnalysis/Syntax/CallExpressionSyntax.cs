using System.Collections.Immutable;
using System.Linq;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public abstract class InvokationExpressionSyntax : ExpressionSyntax
    {
        public InvokationExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , SyntaxList<ExpressionSyntax> arguments )
			: base( syntaxTree )
		{
            this.Identifier = identifier;
            this.Arguments  = arguments;
        }

        public SyntaxToken Identifier { get; }
        public SyntaxList<ExpressionSyntax> Arguments { get; }
    }

    public class CallExpressionSyntax : InvokationExpressionSyntax
    {
        public CallExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifier , SyntaxToken openParenthesisToken , SeparatedSyntaxList<ExpressionSyntax> arguments , SyntaxToken closeParenthesisToken )
			: base( syntaxTree , identifier , arguments )
		{
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Arguments             = arguments;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CallExpression;

        public SyntaxToken OpenParenthesisToken { get; }
        public new SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }

    public class InfixBinaryExpressionSyntax : InvokationExpressionSyntax
    {
        public InfixBinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax lhsExpression , SyntaxToken identifier , ExpressionSyntax rhsExpression )
			: base( syntaxTree , identifier , new SyntaxList<ExpressionSyntax>( ImmutableArray.Create<SyntaxNode>( lhsExpression , rhsExpression ) ) )
		{
        }

        public override SyntaxKind Kind => SyntaxKind.InfixCallExpression;

        public ExpressionSyntax Lhs => this.Arguments.First();
        public ExpressionSyntax Rhs => this.Arguments.Last();
    }

    public class InfixUnaryExpressionSyntax : InvokationExpressionSyntax
    {
        public InfixUnaryExpressionSyntax( SyntaxTree syntaxTree ,  SyntaxToken identifier , ExpressionSyntax expression )
			: base( syntaxTree , identifier , new SyntaxList<ExpressionSyntax>( ImmutableArray.Create<SyntaxNode>( expression ) ) )
		{
        }

        public override SyntaxKind Kind => SyntaxKind.InfixCallExpression;
        public ExpressionSyntax Expression => this.Arguments.Single();
    }
}
