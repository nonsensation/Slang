using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;


namespace NonConTroll.CodeAnalysis.Syntax
{
    public interface ISyntaxList
    {
        ImmutableArray<SyntaxNode> GetNodes();
    }

    public class SyntaxList<T>
        : ISyntaxList , IEnumerable<T>
        where T : SyntaxNode
    {

        public SyntaxList( ImmutableArray<SyntaxNode> nodes )
        {
            this.Nodes = nodes;
        }

        protected readonly ImmutableArray<SyntaxNode> Nodes;

        public ImmutableArray<SyntaxNode> GetNodes()
            => this.Nodes;

        public int Count => this.Nodes.Length;

        public T this[ int idx ] => (T)this.Nodes[ idx ];

        public IEnumerator<T> GetEnumerator()
        {
            for( var i = 0 ; i < this.Count ; i++ )
                yield return this[ i ];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }

    public interface ISeparatedSyntaxList : ISyntaxList
    {
        ImmutableArray<SyntaxNode> GetNodesWithSeparators();
    }

    public class SeparatedSyntaxList<T>
        : SyntaxList<T> , IEnumerable<T> , ISeparatedSyntaxList
        where T : SyntaxNode
    {
        public SeparatedSyntaxList( ImmutableArray<SyntaxNode> nodes )
            : base( nodes )
        {
        }

        public new int Count => (this.Nodes.Length + 1) / 2;

        public new T this[ int idx ] => (T)this.Nodes[ idx * 2 ];

        public SyntaxToken? GetSeparator( int idx )
        {
            if( idx == this.Count - 1 )
                return null;

            return (SyntaxToken)this.Nodes[ idx * 2 + 1 ];
        }

        public ImmutableArray<SyntaxNode> GetNodesWithSeparators()
            => this.Nodes;

        public new IEnumerator<T> GetEnumerator()
        {
            for( var i = 0 ; i < this.Count ; i++ )
                yield return this[ i ];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
