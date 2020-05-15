using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    #region Binary Op

    public enum BoundBinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        LogicalAnd,
        LogicalOr,
        Equals,
        NotEquals,
        Less,
        LessOrEquals,
        Greater,
        GreaterOrEquals,
        Infix,
    }

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
            new BoundBinaryOperator( SyntaxKind.PlusToken     , BoundBinaryOperatorKind.Addition        , BuiltinTypes.Int                      ) ,
            new BoundBinaryOperator( SyntaxKind.MinusToken    , BoundBinaryOperatorKind.Subtraction     , BuiltinTypes.Int                      ) ,
            new BoundBinaryOperator( SyntaxKind.StarToken     , BoundBinaryOperatorKind.Multiplication  , BuiltinTypes.Int                      ) ,
            new BoundBinaryOperator( SyntaxKind.SlashToken    , BoundBinaryOperatorKind.Division        , BuiltinTypes.Int                      ) ,

            new BoundBinaryOperator( SyntaxKind.EqEqToken     , BoundBinaryOperatorKind.Equals          , BuiltinTypes.Int    , BuiltinTypes.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.ExmEqToken    , BoundBinaryOperatorKind.NotEquals       , BuiltinTypes.Int    , BuiltinTypes.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.LtToken       , BoundBinaryOperatorKind.Less            , BuiltinTypes.Int    , BuiltinTypes.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.LtEqToken     , BoundBinaryOperatorKind.LessOrEquals    , BuiltinTypes.Int    , BuiltinTypes.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.GtToken       , BoundBinaryOperatorKind.Greater         , BuiltinTypes.Int    , BuiltinTypes.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.GtEqToken     , BoundBinaryOperatorKind.GreaterOrEquals , BuiltinTypes.Int    , BuiltinTypes.Bool ) ,

            new BoundBinaryOperator( SyntaxKind.AndAndToken   , BoundBinaryOperatorKind.LogicalAnd      , BuiltinTypes.Bool                     ) ,
            new BoundBinaryOperator( SyntaxKind.PipePipeToken , BoundBinaryOperatorKind.LogicalOr       , BuiltinTypes.Bool                     ) ,
            new BoundBinaryOperator( SyntaxKind.EqEqToken     , BoundBinaryOperatorKind.Equals          , BuiltinTypes.Bool                     ) ,
            new BoundBinaryOperator( SyntaxKind.ExmEqToken    , BoundBinaryOperatorKind.NotEquals       , BuiltinTypes.Bool                     ) ,

            new BoundBinaryOperator( SyntaxKind.PlusToken     , BoundBinaryOperatorKind.Addition        , BuiltinTypes.String                   ) ,

            new BoundBinaryOperator( SyntaxKind.EqEqToken     , BoundBinaryOperatorKind.Equals          , BuiltinTypes.String , BuiltinTypes.Bool ) ,
            new BoundBinaryOperator( SyntaxKind.ExmEqToken    , BoundBinaryOperatorKind.NotEquals       , BuiltinTypes.String , BuiltinTypes.Bool ) ,
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

    #endregion

    #region Unary Op

    public enum BoundUnaryOperatorKind
    {
        Identity,
        Negation,
        LogicalNegation,
        OnesComplement
    }

    public class BoundUnaryOperator
    {
        private BoundUnaryOperator( SyntaxKind syntaxKind , BoundUnaryOperatorKind kind , TypeSymbol operandType )
            : this( syntaxKind , kind , operandType , operandType )
        {
        }

        private BoundUnaryOperator( SyntaxKind syntaxKind , BoundUnaryOperatorKind kind , TypeSymbol operandType , TypeSymbol resultType )
        {
            this.TkType = syntaxKind;
            this.Kind = kind;
            this.OperandType = operandType;
            this.Type = resultType;
        }

        public SyntaxKind TkType { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol Type { get; }

        private static readonly BoundUnaryOperator[] _operators = {
            new BoundUnaryOperator( SyntaxKind.ExmToken   , BoundUnaryOperatorKind.LogicalNegation , BuiltinTypes.Bool ) ,
            new BoundUnaryOperator( SyntaxKind.PlusToken  , BoundUnaryOperatorKind.Identity        , BuiltinTypes.Int  ) ,
            new BoundUnaryOperator( SyntaxKind.MinusToken , BoundUnaryOperatorKind.Negation        , BuiltinTypes.Int  ) ,
            new BoundUnaryOperator( SyntaxKind.TildeToken , BoundUnaryOperatorKind.OnesComplement  , BuiltinTypes.Int  ) ,
        };

        public static BoundUnaryOperator? Bind( SyntaxKind tokenType , TypeSymbol operandType )
        {
            foreach( var op in _operators )
            {
                if( op.TkType == tokenType && op.OperandType == operandType )
                    return op;
            }

            return null;
        }
    }

    #endregion
}
