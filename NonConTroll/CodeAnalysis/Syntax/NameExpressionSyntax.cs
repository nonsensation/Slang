namespace NonConTroll.CodeAnalysis.Syntax
{
    public class NameExpressionSyntax : IdentifierNameSyntax
    {
        public NameExpressionSyntax( SyntaxToken identifierToken )
            : base( identifierToken )
        {
        }

        public override SyntaxKind Kind => SyntaxKind.NameExpression;
    }

    public abstract class NameSyntax : TypeSyntax
    {
    }

    public class IdentifierNameSyntax : NameSyntax
    {
        public IdentifierNameSyntax( SyntaxToken identifierToken )
        {
            this.IdentifierToken = identifierToken;
        }

        public override SyntaxKind Kind => SyntaxKind.IdentifierName;
        public SyntaxToken IdentifierToken { get; }    }


    public class QualifiedNameSyntax : NameSyntax
    {
        public QualifiedNameSyntax( NameSyntax lhsName , SyntaxToken dotToken , NameSyntax rhsName )
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
