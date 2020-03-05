namespace NonConTroll.CodeAnalysis.Text
{
    public struct TextSpan
    {
        public TextSpan( int start , int length )
        {
            this.Start = start;
            this.Length = length;
        }

        public int Start { get; }
        public int Length { get; }
        public int End => this.Start + this.Length;

        public static TextSpan FromBounds( int start , int end )
            => new TextSpan( start , end - start );

        public override string ToString() => $"{this.Start}..{this.End}";
    }
}
