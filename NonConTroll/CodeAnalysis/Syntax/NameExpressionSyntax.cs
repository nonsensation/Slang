namespace NonConTroll.CodeAnalysis.Syntax
{
    public class NameExpressionSyntax : IdentifierNameSyntax
    {
        public NameExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifierToken )
			: base( syntaxTree , identifierToken )
        {
        }

        public override SyntaxKind Kind => SyntaxKind.NameExpression;
    }

    public abstract class NameSyntax : TypeSyntax
    {
        public NameSyntax( SyntaxTree syntaxTree ) : base( syntaxTree )
        {
        }
    }

    public class IdentifierNameSyntax : NameSyntax
    {
        public IdentifierNameSyntax( SyntaxTree syntaxTree , SyntaxToken identifierToken )
			: base( syntaxTree )
        {
            this.IdentifierToken = identifierToken;
        }

        public override SyntaxKind Kind => SyntaxKind.IdentifierName;
        public SyntaxToken IdentifierToken { get; }    }


    public class QualifiedNameSyntax : NameSyntax
    {
        public QualifiedNameSyntax( SyntaxTree syntaxTree , NameSyntax lhsName , SyntaxToken dotToken , NameSyntax rhsName )
			: base( syntaxTree )
        {
            this.LhsName  = lhsName;
            this.DotToken = dotToken;
            this.RhsName  = rhsName;
        }

        public override SyntaxKind Kind => SyntaxKind.QualifiedName;
        public NameSyntax LhsName { get; }
        public SyntaxToken DotToken { get; }
        public NameSyntax RhsName { get; }
    }

}
