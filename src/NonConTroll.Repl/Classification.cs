using System;
using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll
{
    internal partial class NonConTrollRepl
    {
        internal enum Classification
        {
            Text,
            Keyword,
            Identifier,
            Number,
            String,
            Punctuation,
            Comment,
        }

        internal class ClassifiedSpan
        {
            public ClassifiedSpan( TextSpan span , Classification classification )
            {
                this.Span = span;
                this.Classification = classification;
            }

            public TextSpan Span { get; }
            public Classification Classification { get; }
        }

        internal class Classifier
        {
            public static ImmutableArray<ClassifiedSpan> Classify( SyntaxTree syntaxTree , TextSpan span )
            {
                var result = ImmutableArray.CreateBuilder<ClassifiedSpan>();

                ClassifyNode( syntaxTree.Root , span , result );

                return result.ToImmutable();
            }

            public static void ClassifyNode( SyntaxNode node , TextSpan span , ImmutableArray<ClassifiedSpan>.Builder result )
            {
                if( !node.FullSpan.OverlapsWith( span ) )
                {
                    return;
                }

                if( node is SyntaxToken token )
                {
                    ClassifyToken( token , span , result );
                }

                foreach( var child in node.GetChildren() )
                {
                    ClassifyNode( child , span , result );
                }
            }

            public static void ClassifyToken( SyntaxToken token , TextSpan span , ImmutableArray<ClassifiedSpan>.Builder result )
            {
                foreach( var trivia in token.LeadingTrivia )
                {
                    ClassifyTrivia( trivia , span , result );
                }

                AddClassification( token.Kind , token.Span , span , result );

                foreach( var trivia in token.TrailingTrivia )
                {
                    ClassifyTrivia( trivia , span , result );
                }
            }

            public static void ClassifyTrivia( SyntaxTrivia trivia , TextSpan span , ImmutableArray<ClassifiedSpan>.Builder result )
            {
                AddClassification( trivia.Kind , trivia.Span , span , result );
            }

            private static void AddClassification( SyntaxKind elementKind , TextSpan elementSpan , TextSpan span , ImmutableArray<ClassifiedSpan>.Builder result )
            {
                if( !elementSpan.OverlapsWith( span ) )
                {
                    return;
                }

                var classification = GetClassification( elementKind );
                var adjustedStart = Math.Max( elementSpan.Start , span.Start );
                var adjustedEnd = Math.Min( elementSpan.End , span.End );
                var adjustedSpan = TextSpan.FromBounds( adjustedStart , adjustedEnd );
                var classifiedSpan = new ClassifiedSpan( adjustedSpan , classification );

                result.Add( classifiedSpan );
            }

            private static Classification GetClassification( SyntaxKind kind )
            {
                if( kind.IsKeyword() ) { return Classification.Keyword; }
                else if( kind.IsPunctuation() ) { return Classification.Punctuation; }
                else if( kind == SyntaxKind.Identifier ) { return Classification.Identifier; }
                else if( kind == SyntaxKind.NumericLiteral ) { return Classification.Number; }
                else if( kind == SyntaxKind.StringLiteral ) { return Classification.String; }
                else if( kind == SyntaxKind.CommentTrivia ) { return Classification.Comment; }
                else { return Classification.Text; }
            }
        }


    }
}
