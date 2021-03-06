using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public abstract class SeparatedSyntaxList
    {
        public abstract ImmutableArray<SyntaxNode> GetNodesWithSeparators();
    }

    public sealed class SeparatedSyntaxList<T>
        : SeparatedSyntaxList, IEnumerable<T>
        where T : SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> NodesAndSeparators;

        public SeparatedSyntaxList( ImmutableArray<SyntaxNode> nodesAndSeparators )
        {
            this.NodesAndSeparators = nodesAndSeparators;
        }

        public int Count => (this.NodesAndSeparators.Length + 1) / 2;

        public T this[ int index ] => (T)this.NodesAndSeparators[ index * 2 ];

        public SyntaxToken GetSeparator( int index )
        {
            if( index < 0 || index >= this.Count - 1 )
            {
                throw new ArgumentOutOfRangeException();
            }

            return (SyntaxToken)this.NodesAndSeparators[ index * 2 + 1 ];
        }

        public override ImmutableArray<SyntaxNode> GetNodesWithSeparators() => this.NodesAndSeparators;

        public IEnumerator<T> GetEnumerator()
        {
            for( var i = 0 ; i < this.Count ; i++ )
            {
                yield return this[ i ];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
