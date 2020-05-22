using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis
{
    public class Diagnostic
    {
        public Diagnostic( TextLocation textLocation , string msg )
        {
            this.Location = textLocation;
            this.Message = msg;
        }

        public TextLocation Location { get; }
        public string Message { get; }

        public override string ToString() => this.Message;
    }
}
