using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
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
            new BoundUnaryOperator( SyntaxKind.ExmToken   , BoundUnaryOperatorKind.LogicalNegation , TypeSymbol.Bool ) ,
            new BoundUnaryOperator( SyntaxKind.PlusToken  , BoundUnaryOperatorKind.Identity        , TypeSymbol.Int  ) ,
            new BoundUnaryOperator( SyntaxKind.MinusToken , BoundUnaryOperatorKind.Negation        , TypeSymbol.Int  ) ,
            new BoundUnaryOperator( SyntaxKind.TildeToken , BoundUnaryOperatorKind.OnesComplement  , TypeSymbol.Int  ) ,
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

}
