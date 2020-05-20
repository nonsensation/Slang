using System.Diagnostics;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundLabel
    {
        public BoundLabel( string name )
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString() => this.Name;
    }

    internal sealed class BoundNop
    {

    }
}
