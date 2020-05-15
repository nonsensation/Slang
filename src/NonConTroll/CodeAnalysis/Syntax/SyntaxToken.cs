using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class SyntaxToken : SyntaxNode
    {
        public SyntaxToken( SyntaxTree syntaxTree ,
                            SyntaxKind kind ,
                            int position ,
                            string? text )
                            // ImmutableArray<SyntaxTrivia> leadingTrivia ,
                            // ImmutableArray<SyntaxTrivia> trailingTrivia )
            : base( syntaxTree )
        {
            this.Kind     = kind;
            this.Position = position;
            this.Text     = text;
            // this.LeadingTrivia  = leadingTrivia;
            // this.TrailingTrivia = trailingTrivia;
        }

        public override SyntaxKind Kind { get; }
        // public override TextSpan Span => new TextSpan( this.Position , this.Text?.Length ?? 0 );
        // // public override TextSpan FullSpan {
        // //     get {

        // //     }
        // // }

        public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }
        public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

        public int Position { get; }
        public string? Text { get; }

        /// <summary>A token is missing if it was inserted by the parser and doesn't appear in source.</summary>
        public bool IsMissing => this.Text == null;

        public override IEnumerable<SyntaxNode> GetChildren() => Array.Empty<SyntaxNode>();
    }

    public sealed class SyntaxTrivia
    {
        public SyntaxTrivia( SyntaxTree syntaxTree , SyntaxKind kind , int position , string text )
        {
            this.Kind = kind;
            this.Position = position;
            this.Text = text;
        }

        public SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
    }

}
