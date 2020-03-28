using System.Collections.Immutable;


namespace NonConTroll.CodeAnalysis.Syntax
{
    public class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax( SyntaxTree syntaxTree , ImmutableArray<MemberSyntax> members , SyntaxToken endOfFileToken )
			: base( syntaxTree )
		{
            this.Members = members;
            this.EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }
    }
}
