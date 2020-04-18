using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram Program;
        private readonly Dictionary<VariableSymbol, object> Globals;
        private readonly Dictionary<FunctionSymbol, BoundBlockStatement> Functions = new Dictionary<FunctionSymbol, BoundBlockStatement>();
        private readonly Stack<Dictionary<VariableSymbol, object>> Locals = new Stack<Dictionary<VariableSymbol, object>>();

        private Random? Random;
        private object? LastValue;

        public Evaluator( BoundProgram program , Dictionary<VariableSymbol , object> variables )
        {
            this.Program = program;
            this.Globals = variables;
            this.Locals.Push( new Dictionary<VariableSymbol , object>() );

            var c = this.Program;

            while( c != null )
            {
                foreach( var f in c.Functions )
                    this.Functions.Add( f.Key , f.Value );

                c = c.Previous;
            }
        }

        public object? Evaluate()
        {
            return this.EvaluateStatement( this.Program.Statement );
        }

        private object? EvaluateStatement( BoundBlockStatement body )
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for( var i = 0 ; i < body.Statements.Length ; i++ )
            {
                if( body.Statements[ i ] is BoundLabelStatement l )
                    labelToIndex.Add( l.Label , i + 1 );
            }

            var index = 0;

            while( index < body.Statements.Length )
            {
                var s = body.Statements[ index ];

                switch( s.Kind )
                {
                    case BoundNodeKind.VariableDeclaration:
                    {
                        this.EvaluateVariableDeclaration( (BoundVariableDeclaration)s );

                        index++;
                    }
                    break;

                    case BoundNodeKind.ExpressionStatement:
                    {
                        this.EvaluateExpressionStatement( (BoundExpressionStatement)s );

                        index++;
                    }
                    break;

                    case BoundNodeKind.GotoStatement:
                    {
                        var gs = (BoundGotoStatement)s;

                        index = labelToIndex[ gs.Label ];
                    }
                    break;

                    case BoundNodeKind.ConditionalGotoStatement:
                    {
                        var cgs = (BoundConditionalGotoStatement)s;
                        var condition = (bool)this.EvaluateExpression( cgs.Condition )!;

                        if( condition == cgs.JumpIfTrue )
                            index = labelToIndex[ cgs.Label ];
                        else
                            index++;
                    }
                    break;

                    case BoundNodeKind.LabelStatement:
                    {
                        index++;
                    }
                    break;

                    case BoundNodeKind.DeferStatement:
                    {
                        this.EvaluateExpression( ((BoundDeferStatement)s).Expression );

                        index++;
                    }
                    break;

                    case BoundNodeKind.ReturnStatement:
                    {
                        var rs = (BoundReturnStatement)s;

                        if( rs.Expression != null )
                            this.LastValue = this.EvaluateExpression( rs.Expression );

                        return this.LastValue;
                    }

                    default:
                    {
                        throw new Exception( $"Unexpected node {s.Kind}" );
                    }
                }
            }

            return this.LastValue;
        }

        private void EvaluateVariableDeclaration( BoundVariableDeclaration node )
        {
            var value = this.EvaluateExpression( node.Initializer )!;

			this.LastValue = value;
			this.Assign( node.Variable , value );
        }

        private void EvaluateExpressionStatement( BoundExpressionStatement node )
        {
			this.LastValue = this.EvaluateExpression( node.Expression );
        }

        private object? EvaluateExpression( BoundExpression node )
        {
            switch( node.Kind )
            {
                case BoundNodeKind.LiteralExpression:    return this.EvaluateLiteralExpression( (BoundLiteralExpression)node );
                case BoundNodeKind.VariableExpression:   return this.EvaluateVariableExpression( (BoundVariableExpression)node );
                case BoundNodeKind.AssignmentExpression: return this.EvaluateAssignmentExpression( (BoundAssignmentExpression)node );
                case BoundNodeKind.UnaryExpression:      return this.EvaluateUnaryExpression( (BoundUnaryExpression)node );
                case BoundNodeKind.BinaryExpression:     return this.EvaluateBinaryExpression( (BoundBinaryExpression)node );
                case BoundNodeKind.CallExpression:       return this.EvaluateCallExpression( (BoundCallExpression)node );
                case BoundNodeKind.ConversionExpression: return this.EvaluateConversionExpression( (BoundConversionExpression)node );
                default:
                    throw new Exception( $"Unexpected node {node.Kind}" );
            }
        }

        private object EvaluateLiteralExpression( BoundLiteralExpression n )
        {
            return n.Value;
        }

        private object EvaluateVariableExpression( BoundVariableExpression v )
        {
            if( v.Variable.Kind == SymbolKind.GlobalVariable )
            {
                return this.Globals[ v.Variable ];
            }
            else
            {
                var locals = this.Locals.Peek();

                return locals[ v.Variable ];
            }
        }

        private object EvaluateAssignmentExpression( BoundAssignmentExpression a )
        {
            var value = this.EvaluateExpression( a.Expression )!;

			this.Assign( a.Variable , value );

            return value;
        }

        private object EvaluateUnaryExpression( BoundUnaryExpression u )
        {
            var operand = this.EvaluateExpression( u.Operand )!;

            switch( u.Op.Kind )
            {
                case BoundUnaryOperatorKind.Identity:        return  (int)operand;
                case BoundUnaryOperatorKind.Negation:        return -(int)operand;
                case BoundUnaryOperatorKind.LogicalNegation: return !(bool)operand;

                default:
                    throw new Exception( $"Unexpected unary operator {u.Op}" );
            }
        }

        private object EvaluateBinaryExpression( BoundBinaryExpression binExpr )
        {
            var lhs = this.EvaluateExpression( binExpr.Lhs )!;
            var rhs = this.EvaluateExpression( binExpr.Rhs )!;

            switch( binExpr.Operator.Kind )
            {
                case BoundBinaryOperatorKind.Addition:
                    if( binExpr.Type == TypeSymbol.Int )
                        return (int)lhs + (int)rhs;
                    else
                        return ((string)lhs).Replace( "\"" , "" )
                             + ((string)rhs).Replace( "\"" , "" );

                case BoundBinaryOperatorKind.Equals:          return  Equals( lhs , rhs );
                case BoundBinaryOperatorKind.NotEquals:       return !Equals( lhs , rhs );
                case BoundBinaryOperatorKind.Subtraction:     return (bool)lhs && (bool)rhs;
                case BoundBinaryOperatorKind.LogicalOr:       return (bool)lhs || (bool)rhs;
                case BoundBinaryOperatorKind.Less:            return (int)lhs  <  (int)rhs;
                case BoundBinaryOperatorKind.LessOrEquals:    return (int)lhs  <= (int)rhs;
                case BoundBinaryOperatorKind.Greater:         return (int)lhs  >  (int)rhs;
                case BoundBinaryOperatorKind.GreaterOrEquals: return (int)lhs  >= (int)rhs;

                default:
                    throw new Exception( $"Unexpected binary operator {binExpr.Operator}" );
            }
        }

        private object? EvaluateCallExpression( BoundCallExpression node )
        {
            if( node.Function == BuiltinFunctions.Input )
            {
                return Console.ReadLine();
            }
            else if( node.Function == BuiltinFunctions.Print )
            {
                var message = (string)this.EvaluateExpression( node.Arguments[ 0 ] )!;

                Console.WriteLine( message );

                return null;
            }
            else if( node.Function == BuiltinFunctions.Rnd )
            {
                var max = (int)this.EvaluateExpression( node.Arguments[ 0 ] )!;

                if( this.Random == null )
					this.Random = new Random();

                return this.Random.Next( max );
            }
            else
            {
                var locals = new Dictionary<VariableSymbol, object>();

                for( var i = 0 ; i < node.Arguments.Length ; i++ )
                {
                    var parameter = node.Function.Parameters[ i ];
                    var value = this.EvaluateExpression( node.Arguments[ i ] )!;

                    locals.Add( parameter , value );
                }

				this.Locals.Push( locals );

                var statement = this.Functions[ node.Function ];
                var result = this.EvaluateStatement( statement );

                _ = this.Locals.Pop();

                return result;
            }
        }

        private object EvaluateConversionExpression( BoundConversionExpression node )
        {
            var value = this.EvaluateExpression( node.Expression );

            if( node.Type == TypeSymbol.Bool )
                return Convert.ToBoolean( value );
            else if( node.Type == TypeSymbol.Int )
                return Convert.ToInt32( value );
            else if( node.Type == TypeSymbol.String )
                return Convert.ToString( value )!;
            else
                throw new Exception( $"Unexpected type {node.Type}" );
        }

        private void Assign( VariableSymbol variable , object value )
        {
            if( variable.Kind == SymbolKind.GlobalVariable )
            {
				this.Globals[ variable ] = value;
            }
            else
            {
                var locals = this.Locals.Peek();

                locals[ variable ] = value;
            }
        }

        public sealed class EvaluationResult
        {
            public EvaluationResult( ImmutableArray<Diagnostic> diagnostics , object value )
            {
				this.Diagnostics = diagnostics;
				this.Value = value;
            }

            public ImmutableArray<Diagnostic> Diagnostics { get; }
            public object Value { get; }
        }
    }
}
