using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundUnaryOperator
    {
        private BoundUnaryOperator( TokenType syntaxKind , BoundUnaryOperatorKind kind , TypeSymbol operandType )
            : this( syntaxKind , kind , operandType , operandType )
        {
        }

        private BoundUnaryOperator( TokenType syntaxKind , BoundUnaryOperatorKind kind , TypeSymbol operandType , TypeSymbol resultType )
        {
            this.TkType = syntaxKind;
            this.Kind = kind;
            this.OperandType = operandType;
            this.Type = resultType;
        }

        public TokenType TkType { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol Type { get; }

        private static readonly BoundUnaryOperator[] _operators = {
            new BoundUnaryOperator( TokenType.Exm   , BoundUnaryOperatorKind.LogicalNegation , TypeSymbol.Bool ) ,
            new BoundUnaryOperator( TokenType.Plus  , BoundUnaryOperatorKind.Identity        , TypeSymbol.Int  ) ,
            new BoundUnaryOperator( TokenType.Minus , BoundUnaryOperatorKind.Negation        , TypeSymbol.Int  ) ,
            new BoundUnaryOperator( TokenType.Tilde , BoundUnaryOperatorKind.OnesComplement  , TypeSymbol.Int  ) ,
        };

        public static BoundUnaryOperator? Bind( TokenType tokenType , TypeSymbol operandType )
        {
            foreach( var op in _operators )
            {
                if( op.TkType == tokenType && op.OperandType == operandType )
                    return op;
            }

            return null;
        }
    }

}
