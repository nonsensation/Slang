using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NonConTroll.CodeAnalysis.Text;


namespace NonConTroll.CodeAnalysis.Syntax
{
    public abstract partial class SyntaxNode
    {
        protected SyntaxNode( SyntaxTree syntaxTree )
        {
            this.SyntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree { get; }

        public abstract SyntaxKind Kind { get; }

        public TextLocation Location => new TextLocation( this.SyntaxTree.Text , this.Span );

        public virtual TextSpan Span {
            get {
                var children = this.GetChildren();
                var first    = children.First();
                var last     = children.Last();

                return TextSpan.FromBounds( first.Span.Start , last.Span.End );
            }
        }

        public virtual TextSpan FullSpan {
            get {
                var children = this.GetChildren();
                var first    = children.First();
                var last     = children.Last();

                return TextSpan.FromBounds( first.Span.Start , last.Span.End );
            }
        }

        public abstract IEnumerable<SyntaxNode> GetChildren();

        public SyntaxToken GetLastToken()
        {
            if( this is SyntaxToken token )
            {
                return token;
            }

            // A syntax node should always contain at least 1 token.
            return this.GetChildren().Last().GetLastToken();
        }

        public void WriteTo( TextWriter writer )
        {
            PrettyPrint( writer , this );
        }

        private static void PrettyPrint( TextWriter writer , SyntaxNode node , string indent = "" , bool isLast = true )
        {
            var isToConsole = writer == Console.Out;
            var marker = isLast ? "└──" : "├──";
            var token = node as SyntaxToken;

            if( isToConsole )
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            if( token != null )
            {
                foreach( var trivia in token.LeadingTrivia )
                {
                    var isLastTriva = trivia == token.TrailingTrivia.Last();
                    var triviaMarker = isLastTriva ? "└──" : "├──";

                    writer.Write( indent );
                    writer.Write( isLastTriva );
                    writer.Write( trivia.Kind );
                }
            }

            var hasTrailingTrivia = token != null && token.TrailingTrivia.Any();

            writer.Write( indent );
            writer.Write( marker );

            if( isToConsole )
            {
                Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.DarkCyan : ConsoleColor.Cyan;
            }

            writer.Write( node.Kind );

            if( isToConsole )
            {
                Console.ResetColor();
            }

            writer.WriteLine();

            if( token != null )
            {
                foreach( var trivia in token.TrailingTrivia )
                {
                    var isLastTriva = trivia == token.TrailingTrivia.Last();
                    var triviaMarker = isLastTriva ? "└──" : "├──";

                    writer.Write( indent );
                    writer.Write( triviaMarker );
                    writer.Write( trivia.Kind );
                }
            }

            indent += isLast ? "   " : "│  ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach( var child in node.GetChildren() )
            {
                PrettyPrint( writer , child , indent , child == lastChild );
            }
        }

        public override string ToString()
        {
            using var writer = new StringWriter();

			this.WriteTo( writer );

            return writer.ToString();
        }
    }
}
