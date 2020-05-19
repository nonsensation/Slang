using System;
using System.Diagnostics;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundLabel
    {
        public BoundLabel( string name )
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString() => this.Name;
    }

    internal sealed class BoundNop
    {

    }

    internal static class ConstantFolding
    {
        public static BoundConstant? ComputeConstant( BoundUnaryOperator op , BoundExpression rhs )
        {
            if( rhs.ConstantValue != null )
            {
                var value = rhs.ConstantValue.Value;

                switch( op.Kind )
                {
                    case BoundUnaryOperatorKind.Identity:        return new BoundConstant( /*(int)*/value );
                    case BoundUnaryOperatorKind.LogicalNegation: return new BoundConstant( -(int)value );
                    case BoundUnaryOperatorKind.Negation:        return new BoundConstant( !(bool)value );
                    default:
                        throw new Exception( $"Unexpected unary operator {op.Kind}" );
                }
            }

            return null;
        }

        public static BoundConstant? ComputeConstant( BoundExpression lhsExpr , BoundBinaryOperator op , BoundExpression rhsExpr )
        {
            if( lhsExpr.ConstantValue == null || rhsExpr.ConstantValue == null )
            {
                return null;
            }

            var lhsValue = lhsExpr.ConstantValue.Value;
            var rhsValue = rhsExpr.ConstantValue.Value;


            // Logical bin ops

            if( lhsValue is bool lhsBool && rhsValue is bool rhsBool )
            {
                if( op.Kind == BoundBinaryOperatorKind.LogicalAnd )
                {
                    if( !lhsBool || !rhsBool )
                    {
                        return new BoundConstant( false );
                    }
                }

                if( op.Kind == BoundBinaryOperatorKind.LogicalOr )
                {
                    if( lhsBool || rhsBool )
                    {
                        return new BoundConstant( true );
                    }
                }
            }
            else if( lhsValue is int lhsInt && rhsValue is int rhsInt )
            {
                switch( op.Kind )
                {
                    case BoundBinaryOperatorKind.Addition:        return new BoundConstant( lhsInt + rhsInt );
                    case BoundBinaryOperatorKind.Subtraction:     return new BoundConstant( lhsInt - rhsInt );
                    case BoundBinaryOperatorKind.Multiplication:  return new BoundConstant( lhsInt * rhsInt );
                    case BoundBinaryOperatorKind.Division:        return new BoundConstant( lhsInt / rhsInt );
                    case BoundBinaryOperatorKind.Less:            return new BoundConstant( lhsInt < rhsInt );
                    case BoundBinaryOperatorKind.Greater:         return new BoundConstant( lhsInt > rhsInt );
                    case BoundBinaryOperatorKind.LessOrEquals:    return new BoundConstant( lhsInt <= rhsInt );
                    case BoundBinaryOperatorKind.GreaterOrEquals: return new BoundConstant( lhsInt >= rhsInt );
                    case BoundBinaryOperatorKind.Equals:          return new BoundConstant( lhsInt == rhsInt );
                    case BoundBinaryOperatorKind.NotEquals:       return new BoundConstant( lhsInt != rhsInt );
                    default:
                        throw new Exception( $"Unexpected binary operator {op.Kind} for type " );
                }
            }
            else if( lhsValue is string lhsStr && rhsValue is string rhsStr )
            {
                // flatten string concat

                Debug.Assert( op.Kind == BoundBinaryOperatorKind.Addition );

                return new BoundConstant( lhsStr + rhsStr );
            }

            return null; //is this an error?
        }
    }
}
