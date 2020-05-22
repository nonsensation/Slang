using System.IO;

namespace NonConTroll.CodeAnalysis.Binding
{
    public abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }

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
