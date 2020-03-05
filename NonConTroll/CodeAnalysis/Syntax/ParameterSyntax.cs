namespace NonConTroll.CodeAnalysis.Syntax
{
    public class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax( SyntaxToken identifier , TypeClauseSyntax type )
        {
            this.Identifier = identifier;
            this.Type       = type;
        }

        public override SyntaxKind Kind => SyntaxKind.Parameter;
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax Type { get; }
    }
}
