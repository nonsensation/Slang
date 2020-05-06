using System;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression( object value )
        {
            this.Value = value;

            if( value is bool b )
            {
                this.Type = TypeSymbol.Bool;
            }
            else if( value is int i )
            {
                this.Type = TypeSymbol.Int;
            }
            else if( value is string str )
            {
                this.Type = TypeSymbol.String;
                this.Value = str[ 1 .. ^1 ]; // strip escaped quotes "\"str\"\ -> "str"
            }
            else
            {
                throw new Exception( $"Unexpected literal '{value}' of type {value.GetType()}" );
            }
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        public object Value { get; }
    }

}
