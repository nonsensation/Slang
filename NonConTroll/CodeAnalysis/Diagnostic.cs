using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis
{
    public class Diagnostic
    {
        public Diagnostic( TextSpan span , string msg )
        {
            this.Span = span;
            this.Msg = msg;
        }

        public TextSpan Span { get; }
        public string Msg { get; }

        public override string ToString() => this.Msg;
    }
}
