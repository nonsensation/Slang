using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class Conversion
    {
        public static readonly Conversion None     = new Conversion( exists: false , isIdentity: false , isImplicit: false );
        public static readonly Conversion Identity = new Conversion( exists: true  , isIdentity: true  , isImplicit: true  );
        public static readonly Conversion Implicit = new Conversion( exists: true  , isIdentity: false , isImplicit: true  );
        public static readonly Conversion Explicit = new Conversion( exists: true  , isIdentity: false , isImplicit: false );

        private Conversion( bool exists , bool isIdentity , bool isImplicit )
        {
            this.Exists = exists;
            this.IsIdentity = isIdentity;
            this.IsImplicit = isImplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => this.Exists && !this.IsImplicit;

        public static Conversion Classify( TypeSymbol from , TypeSymbol to )
        {
            if( from == to )
                return Conversion.Identity;

            if( from == TypeSymbol.Bool || from == TypeSymbol.Int )
            {
                if( to == TypeSymbol.String )
                    return Conversion.Explicit;
            }

            if( from == TypeSymbol.String )
            {
                if( to == TypeSymbol.Bool || to == TypeSymbol.Int )
                    return Conversion.Explicit;
            }

            return Conversion.None;
        }
    }

}
