using System.Collections.Immutable;


namespace NonConTroll.CodeAnalysis.Syntax
{
    public class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax( ImmutableArray<MemberSyntax> members , SyntaxToken endOfFileToken )
        {
            this.Members = members;
            this.EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }
    }
}
