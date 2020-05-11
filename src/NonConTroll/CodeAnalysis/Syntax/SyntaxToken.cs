using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class SyntaxToken : SyntaxNode
    {
        public SyntaxToken( SyntaxTree syntaxTree , SyntaxKind kind , int position , string? text )
            : base( syntaxTree )
        {
            this.Kind     = kind;
            this.Position = position;
            this.Text     = text;
        }

        public override SyntaxKind Kind { get; }
        public override TextSpan Span => new TextSpan( this.Position , this.Text?.Length ?? 0 );

        public int Position { get; }
        public string? Text { get; }

        /// <summary>A token is missing if it was inserted by the parser and doesn't appear in source.</summary>
        public bool IsMissing => this.Text == null;
    }
}
