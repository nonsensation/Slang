using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundBinaryOperator
    {
        private BoundBinaryOperator( SyntaxKind tokenType , BoundBinaryOperatorKind kind , TypeSymbol type )
            : this( tokenType , kind , type , type , type )
        {
        }

        private BoundBinaryOperator( SyntaxKind tokenType , BoundBinaryOperatorKind kind , TypeSymbol operandType , TypeSymbol resultType )
            : this( tokenType , kind , operandType , operandType , resultType )
        {
        }

        private BoundBinaryOperator( SyntaxKind tokenType , BoundBinaryOperatorKind kind , TypeSymbol lhsType , TypeSymbol rhsType , TypeSymbol resultType )
        {
            this.TkType  = tokenType;
            this.Kind    = kind;
            this.LhsType = lhsType;
            this.RhsType = rhsType;
            this.Type    = resultType;
        }

        public SyntaxKind TkType { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LhsType { get; }
        public TypeSymbol RhsType { get; }
        public TypeSymbol Type { get; }

        private static readonly BoundBinaryOperator[] Operators =
        {
            new BoundBinaryOperator( SyntaxKind.PlusToken     , BoundBinaryOperatorKind.Addition        , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( SyntaxKind.MinusToken    , BoundBinaryOperatorKind.Subtraction     , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( SyntaxKind.StarToken     , BoundBinaryOperatorKind.Multiplication  , TypeSymbol.Int                      ) ,
            new BoundBinaryOperator( SyntaxKind.SlashToken    , BoundBinaryOperatorKind.Division        , TypeSymbol.Int                      ) ,

            new BoundBinaryOperator( SyntaxKind.EqEqToken     , BoundBinaryOperatorKind.Equals          , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.ExmEqToken    , BoundBinaryOperatorKind.NotEquals       , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.LtToken       , BoundBinaryOperatorKind.Less            , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.LtEqToken     , BoundBinaryOperatorKind.LessOrEquals    , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.GtToken       , BoundBinaryOperatorKind.Greater         , TypeSymbol.Int    , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.GtEqToken     , BoundBinaryOperatorKind.GreaterOrEquals , TypeSymbol.Int    , TypeSymbol.Bool ) ,

            new BoundBinaryOperator( SyntaxKind.AndAndToken   , BoundBinaryOperatorKind.LogicalAnd      , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( SyntaxKind.PipePipeToken , BoundBinaryOperatorKind.LogicalOr       , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( SyntaxKind.EqEqToken     , BoundBinaryOperatorKind.Equals          , TypeSymbol.Bool                     ) ,
            new BoundBinaryOperator( SyntaxKind.ExmEqToken    , BoundBinaryOperatorKind.NotEquals       , TypeSymbol.Bool                     ) ,

            new BoundBinaryOperator( SyntaxKind.PlusToken     , BoundBinaryOperatorKind.Addition        , TypeSymbol.String                   ) ,

            new BoundBinaryOperator( SyntaxKind.EqEqToken     , BoundBinaryOperatorKind.Equals          , TypeSymbol.String , TypeSymbol.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.ExmEqToken    , BoundBinaryOperatorKind.NotEquals       , TypeSymbol.String , TypeSymbol.Bool ) ,
        };

        public static BoundBinaryOperator? Bind( SyntaxKind tokenType , TypeSymbol lhsType , TypeSymbol rhsType )
        {
            foreach( var op in Operators )
            {
                if( op.TkType == tokenType &&
                    op.LhsType == lhsType &&
                    op.RhsType == rhsType )
                {
                    return op;
                }
            }

            return null;
        }
    }

}
