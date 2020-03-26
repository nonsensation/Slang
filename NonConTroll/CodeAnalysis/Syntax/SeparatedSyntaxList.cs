using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;


namespace NonConTroll.CodeAnalysis.Syntax
{
    public abstract class SeparatedSyntaxList
    {
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
    }

    public class SeparatedSyntaxList<T>
        : SeparatedSyntaxList, IEnumerable<T>
        where T : SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> NodesAndSeparators;

        public SeparatedSyntaxList( ImmutableArray<SyntaxNode> nodesAndSeparators )
        {
            this.NodesAndSeparators = nodesAndSeparators;
        }

        public int Count => (this.NodesAndSeparators.Length + 1) / 2;

        public T this[ int idx ] => (T)this.NodesAndSeparators[ idx * 2 ];

        public SyntaxToken? GetSeparator( int idx )
        {
            if( idx == this.Count - 1 )
                return null;

            return (SyntaxToken)this.NodesAndSeparators[ idx * 2 + 1 ];
        }

        public override ImmutableArray<SyntaxNode> GetWithSeparators()
            => this.NodesAndSeparators;

        public IEnumerator<T> GetEnumerator()
        {
            for( var i = 0 ; i < this.Count ; i++ )
                yield return this[ i ];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
