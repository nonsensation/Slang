using NonConTroll.CodeAnalysis.Text;


namespace NonConTroll.CodeAnalysis.Text
{
    public class TextLine
    {
        public TextLine( SourceText text , int start , int length , int lengthIncludingLineBreak )
        {
            this.Text = text;
            this.Start = start;
            this.Length = length;
            this.LengthIncludingLineBreak = lengthIncludingLineBreak;
        }

        public SourceText Text { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => this.Start + this.Length;
        public int LengthIncludingLineBreak { get; }
        public TextSpan Span => new TextSpan( this.Start , this.Length );
        public TextSpan SpanIncludingLineBreak
            => new TextSpan( this.Start , this.LengthIncludingLineBreak );

        public override string ToString() => Text.ToString( this.Span );
    }
}
