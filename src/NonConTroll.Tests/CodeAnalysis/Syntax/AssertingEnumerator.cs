using System;
using System.Collections.Generic;
using System.Linq;
using NonConTroll.CodeAnalysis.Syntax;
using Xunit;

namespace NonConTroll.Tests.CodeAnalysis.Syntax
{
    internal sealed class AssertingEnumerator : IDisposable
    {
        private readonly IEnumerator<SyntaxNode> Enumerator;
        private bool HasErrors;

        public AssertingEnumerator( SyntaxNode node )
        {
            this.Enumerator = Flatten( node ).GetEnumerator();
        }

        private bool MarkFailed()
        {
            this.HasErrors = true;

            return false;
        }

        public void Dispose()
        {
            if( !HasErrors )
            {
                Assert.False( this.Enumerator.MoveNext() );
            }

            this.Enumerator.Dispose();
        }

        private static IEnumerable<SyntaxNode> Flatten( SyntaxNode node )
        {
            var stack = new Stack<SyntaxNode>();

            stack.Push( node );

            while( stack.Count > 0 )
            {
                var n = stack.Pop();

                yield return n;

                foreach( var child in n.GetChildren().Reverse() )
                {
                    stack.Push( child );
                }
            }
        }

        public void AssertNode( SyntaxKind kind )
        {
            try
            {
                Assert.True( this.Enumerator.MoveNext() );
                Assert.Equal( kind , this.Enumerator.Current.Kind );
                Assert.IsNotType<SyntaxToken>( this.Enumerator.Current );
            }
            catch when( MarkFailed() )
            {
                throw;
            }
        }

        public void AssertToken( SyntaxKind kind , string text )
        {
            try
            {
                Assert.True( this.Enumerator.MoveNext() );
                Assert.Equal( kind , this.Enumerator.Current.Kind );

                var token = Assert.IsType<SyntaxToken>( this.Enumerator.Current );

                Assert.Equal( text , token.Text );
            }
            catch when( MarkFailed() )
            {
                throw;
            }
        }
    }
}
