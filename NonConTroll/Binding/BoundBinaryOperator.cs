using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundBinaryOperator
    {
        private BoundBinaryOperator( TokenType tokenType , BoundBinaryOperatorKind kind , TypeSymbol type )
            : this( tokenType , kind , type , type , type )
        {
        }

        private BoundBinaryOperator( TokenType tokenType , BoundBinaryOperatorKind kind , TypeSymbol operandType , TypeSymbol resultType )
            : this( tokenType , kind , operandType , operandType , resultType )
        {
        }

        private BoundBinaryOperator( TokenType tokenType , BoundBinaryOperatorKind kind , TypeSymbol leftType , TypeSymbol rightType , TypeSymbol resultType )
        {
            this.TkType    = tokenType;
            this.Kind      = kind;
            this.LeftType  = leftType;
            this.RightType = rightType;
            this.Type      = resultType;
        }

        public TokenType TkType { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol Type { get; }

        private static readonly BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator( TokenType.Plus     , BoundBinaryOperatorKind.Addition        , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( TokenType.Minus    , BoundBinaryOperatorKind.Subtraction     , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( TokenType.Star     , BoundBinaryOperatorKind.Multiplication  , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( TokenType.Slash    , BoundBinaryOperatorKind.Division        , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( TokenType.And      , BoundBinaryOperatorKind.BitwiseAnd      , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( TokenType.Pipe     , BoundBinaryOperatorKind.BitwiseOr       , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( TokenType.Caret    , BoundBinaryOperatorKind.BitwiseXor      , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( TokenType.EqEq     , BoundBinaryOperatorKind.Equals          , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( TokenType.ExmEq    , BoundBinaryOperatorKind.NotEquals       , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( TokenType.Lt       , BoundBinaryOperatorKind.Less            , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( TokenType.LtEq     , BoundBinaryOperatorKind.LessOrEquals    , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( TokenType.Gt       , BoundBinaryOperatorKind.Greater         , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( TokenType.GtEq     , BoundBinaryOperatorKind.GreaterOrEquals , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( TokenType.And      , BoundBinaryOperatorKind.BitwiseAnd      , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( TokenType.AndAnd   , BoundBinaryOperatorKind.LogicalAnd      , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( TokenType.Pipe     , BoundBinaryOperatorKind.BitwiseOr       , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( TokenType.PipePipe , BoundBinaryOperatorKind.LogicalOr       , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( TokenType.Caret    , BoundBinaryOperatorKind.BitwiseXor      , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( TokenType.EqEq     , BoundBinaryOperatorKind.Equals          , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( TokenType.ExmEq    , BoundBinaryOperatorKind.NotEquals       , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( TokenType.Plus     , BoundBinaryOperatorKind.Addition        , TypeSymbol.String                   ) ,
            new BoundBinaryOperator( TokenType.EqEq     , BoundBinaryOperatorKind.Equals          , TypeSymbol.String , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( TokenType.ExmEq    , BoundBinaryOperatorKind.NotEquals       , TypeSymbol.String , TypeSymbol.Bool ) ,
        };

        public static BoundBinaryOperator? Bind( TokenType tokenType , TypeSymbol leftType , TypeSymbol rightType )
        {
            foreach( var op in _operators )
            {
                if( op.TkType == tokenType && op.LeftType == leftType && op.RightType == rightType )
                    return op;
            }

            return null;
        }
    }

}