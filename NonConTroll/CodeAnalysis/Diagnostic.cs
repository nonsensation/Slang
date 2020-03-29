using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis
{
    public class Diagnostic
    {
        public Diagnostic( TextLocation textLocation , string msg )
        {
            this.Location = textLocation;
            this.Msg = msg;
        }

        public TextLocation Location { get; }
        public string Msg { get; }

        public override string ToString() => this.Msg;
    }
}
