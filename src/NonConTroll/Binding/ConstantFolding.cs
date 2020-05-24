using System;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal static class ConstantFolding
    {
        public static BoundConstant? Fold( BoundUnaryOperator op , BoundExpression rhs )
        {
            if( rhs.ConstantValue != null )
            {
                var value = rhs.ConstantValue.Value;

                switch( op.Kind )
                {
                    case BoundUnaryOperatorKind.Identity:        return new BoundConstant( /*(int)*/value );
                    case BoundUnaryOperatorKind.Negation:        return new BoundConstant( -(int)value );
                    case BoundUnaryOperatorKind.LogicalNegation: return new BoundConstant( !(bool)value );
                    default:
                        throw new Exception( $"Unexpected unary operator '{op.Kind}' for type '{rhs.Type}'" );

                }
            }

            return null;
        }

        public static BoundConstant? Fold( BoundExpression lhsExpr , BoundBinaryOperator op , BoundExpression rhsExpr )
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
                switch( op.Kind )
                {
                    case BoundBinaryOperatorKind.LogicalAnd:      return new BoundConstant( lhsBool && rhsBool );
                    case BoundBinaryOperatorKind.LogicalOr:       return new BoundConstant( lhsBool || rhsBool );
                    case BoundBinaryOperatorKind.Equals:          return new BoundConstant( lhsBool == rhsBool );
                    case BoundBinaryOperatorKind.NotEquals:       return new BoundConstant( lhsBool != rhsBool );
                    default:
                        throw new Exception( $"Unexpected binary operator '{op.Kind}' for types '{lhsExpr.Type}' and '{rhsExpr.Type}'" );
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
                        throw new Exception( $"Unexpected binary operator '{op.Kind}' for types '{lhsExpr.Type}' and '{rhsExpr.Type}'" );
                }
            }
            else if( lhsValue is string lhsStr && rhsValue is string rhsStr )
            {
                // flatten string concat

                switch( op.Kind )
                {
                    case BoundBinaryOperatorKind.Addition:        return new BoundConstant( lhsStr + rhsStr );
                    case BoundBinaryOperatorKind.Equals:          return new BoundConstant( string.Equals( lhsStr , rhsStr ) );
                    case BoundBinaryOperatorKind.NotEquals:       return new BoundConstant( !string.Equals( lhsStr , rhsStr ) );
                    default:
                        throw new Exception( $"Unexpected binary operator '{op.Kind}' for types '{lhsExpr.Type}' and '{rhsExpr.Type}'" );
                }
            }

            return null; //is this an error?
        }
    }
}
