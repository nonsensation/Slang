namespace NonConTroll.CodeAnalysis.Text
{
    public struct TextLocation
    {
        public TextLocation( SourceText text , TextSpan span )
        {
            this.Text = text;
            this.Span = span;
        }

        public SourceText Text { get; }
        public TextSpan Span { get; }

        public readonly string FileName => this.Text.FileName;

        public int StartLine => this.Text.GetLineIndex( this.Span.Start );
        public int EndLine => this.Text.GetLineIndex( this.Span.End );
        public int StartCharacter => this.Span.Start - this.Text.Lines[ this.StartLine ].Start;
        public int EndCharacter => this.Span.End - this.Text.Lines[ this.StartLine ].Start;
    }
}
