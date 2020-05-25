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
        public SyntaxToken( SyntaxTree syntaxTree , SyntaxKind kind ,
                            int position , string? text ,
                            ImmutableArray<SyntaxTrivia> leadingTrivia ,
                            ImmutableArray<SyntaxTrivia> trailingTrivia )
            : base( syntaxTree , kind )
        {
             this.Position       = position;
            this.Text           = text ?? string.Empty;
            this.LeadingTrivia  = leadingTrivia;
            this.TrailingTrivia = trailingTrivia;
            this.IsMissing      = text == null;
        }

        public override TextSpan FullSpan
        {
            get
            {
                var start = this.LeadingTrivia.Any() ? LeadingTrivia.First().Span.Start : this.Span.Start;
                var end = this.TrailingTrivia.Any() ? this.TrailingTrivia.Last().Span.End : this.Span.End;

                return TextSpan.FromBounds( start , end );
            }
        }


        public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }
        public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

        public int Position { get; }
        public string Text { get; }

        /// <summary>A token is missing if it was inserted by the parser and doesn't appear in source.</summary>
        public bool IsMissing { get; }

        public override IEnumerable<SyntaxNode> GetChildren() => Array.Empty<SyntaxNode>();

        public override TextSpan Span => new TextSpan( this.Position , this.Text.Length );
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

        public TextSpan Span => new TextSpan( this.Position, this.Text?.Length ?? 0 );
    }

}
