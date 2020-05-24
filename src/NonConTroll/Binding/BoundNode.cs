using System.IO;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    public abstract class BoundNode
    {
        public BoundNodeKind Kind { get; }
        public SyntaxNode Syntax { get; }

        public BoundNode( BoundNodeKind kind , SyntaxNode syntax )
        {
            this.Kind = kind;
            this.Syntax = syntax;
        }

        public override string ToString()
        {
            using var writer = new StringWriter();

            this.WriteTo( writer );

            return writer.ToString();
        }
    }

    internal sealed class BoundConstant
    {
        public BoundConstant( object value )
        {
            this.Value = value;
        }

        public object Value { get; }
    }
}
