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

        public IEnumerable<SyntaxNode> GetChildren()
        {
            var flags = BindingFlags.Public
                      | BindingFlags.Instance;
            var properties = this.GetType().GetProperties( flags );

            foreach( var property in properties )
            {
                if( typeof( SyntaxNode ).IsAssignableFrom( property.PropertyType ) )
                {
                    var child = (SyntaxNode?)property.GetValue( this );

                    if( child != null )
                    {
                        yield return child;
                    }
                }
                else if( typeof( ISyntaxList ).IsAssignableFrom( property.PropertyType ) )
                {
                    var separatedSyntaxList = (ISyntaxList?)property.GetValue( this );

                    if( separatedSyntaxList != null )
                    {
                        foreach( var child in separatedSyntaxList.GetNodes() )
                        {
                            yield return child;
                        }
                    }
                }
                else if( typeof( ISeparatedSyntaxList ).IsAssignableFrom( property.PropertyType ) )
                {
                    var separatedSyntaxList = (ISeparatedSyntaxList?)property.GetValue( this );

                    if( separatedSyntaxList != null )
                    {
                        foreach( var child in separatedSyntaxList.GetNodesWithSeparators() )
                        {
                            yield return child;
                        }
                    }
                }
                else if( typeof( IEnumerable<SyntaxNode> ).IsAssignableFrom( property.PropertyType ) )
                {
                    var children = (IEnumerable<SyntaxNode>?)property.GetValue( this );

                    if( children != null )
                    {
                        foreach( var child in children )
                        {
                            if( child != null )
                            {
                                yield return child;
                            }
                        }
                    }
                }
            }
        }

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

            if( isToConsole )
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

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
