using System.Collections.Immutable;
using System.Linq;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public abstract class InvokationExpressionSyntax : ExpressionSyntax
    {
        public InvokationExpressionSyntax( SyntaxTree syntaxTree )
			: base( syntaxTree )
		{
        }
    }

    public class CallExpressionSyntax : InvokationExpressionSyntax
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

    public class InfixBinaryExpressionSyntax : InvokationExpressionSyntax
    {
        public InfixBinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax lhsExpression , SyntaxToken identifier , ExpressionSyntax rhsExpression )
			: base( syntaxTree )
		{
            this.Lhs        = lhsExpression;
            this.Identifier = identifier;
            this.Rhs        = rhsExpression;
        }

        public override SyntaxKind Kind => SyntaxKind.InfixBinaryCallExpression;

        public ExpressionSyntax Lhs { get; }
        public SyntaxToken Identifier { get; }
        public ExpressionSyntax Rhs { get; }
    }

    public class InfixUnaryExpressionSyntax : InvokationExpressionSyntax
    {
        public InfixUnaryExpressionSyntax( SyntaxTree syntaxTree ,  SyntaxToken identifier , ExpressionSyntax expression )
			: base( syntaxTree )
		{
            this.Identifier = identifier;
            this.Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.InfixUnaryCallExpression;
        public SyntaxToken Identifier { get; }
        public ExpressionSyntax Expression { get; }
    }
}
