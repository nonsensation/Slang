using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class SyntaxToken : SyntaxNode
    {
        public SyntaxToken( TokenType tokenType , int position , string? text )
        {
            this.TkType = tokenType;
            this.Position = position;
            this.Text = text;
        }

        public TokenType TkType { get; }
        public override SyntaxKind Kind { get; }
        public override TextSpan Span => new TextSpan( Position , Text?.Length ?? 0 );

        public int Position { get; }
        public string? Text { get; }

        /// <summary>A token is missing if it was inserted by the parser and doesn't appear in source.</summary>
        public bool IsMissing => Text == null;
    }
}
