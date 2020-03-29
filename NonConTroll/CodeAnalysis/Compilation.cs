using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis
{
    public class Compilation
    {
        private BoundGlobalScope? _globalScope;

        public Compilation( params SyntaxTree[] syntaxTrees )
            : this( null , syntaxTrees )
        {
        }

        private Compilation( Compilation? previous , params SyntaxTree[] syntaxTrees )
        {
            this.Previous    = previous;
            this.SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public Compilation? Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

        internal BoundGlobalScope GlobalScope {
            get {
                if( this._globalScope == null )
                {
                    var globalScope = Binder.BindGlobalScope( this.Previous?.GlobalScope , this.SyntaxTrees );

                    _ = Interlocked.CompareExchange( ref this._globalScope! , globalScope , null );
                }

                return this._globalScope;
            }
        }

        public Compilation ContinueWith( SyntaxTree syntaxTree )
        {
            return new Compilation( this , syntaxTree );
        }

        public EvaluationResult Evaluate( Dictionary<VariableSymbol , object> variables )
        {
            var diagnostics = this.SyntaxTrees
                .SelectMany( x => x.Diagnostics )
                .Concat( this.GlobalScope.Diagnostics )
                .ToImmutableArray();

            if( diagnostics.Any() )
                return new EvaluationResult( diagnostics , null );

            var program      = Binder.BindProgram( this.GlobalScope );
            var appPath      = Environment.GetCommandLineArgs()[ 0 ];
            var appDirectory = Path.GetDirectoryName( appPath )!;
            var cfgPath      = Path.Combine( appDirectory , "cfg.dot" );
            var cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any()
                             ? program.Functions.Last().Value
                             : program.Statement;
            var cfg = ControlFlowGraph.Create( cfgStatement );

            using( var streamWriter = new StreamWriter( cfgPath ) )
                cfg.WriteTo( streamWriter );

            if( program.Diagnostics.Any() )
                return new EvaluationResult( program.Diagnostics.ToImmutableArray() , null );

            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult( ImmutableArray<Diagnostic>.Empty , value );
        }

        public void EmitTree( TextWriter writer )
        {
            var program = Binder.BindProgram(this.GlobalScope);

            if( program.Statement.Statements.Any() )
            {
                program.Statement.WriteTo( writer );
            }
            else
            {
                foreach( var functionBody in program.Functions )
                {
                    if( !this.GlobalScope.Functions.Contains( functionBody.Key ) )
                        continue;

                    functionBody.Key.WriteTo( writer );
                    functionBody.Value.WriteTo( writer );
                }
            }
        }
    }







    internal sealed class Evaluator
    {
        private readonly BoundProgram _program;
        private readonly Dictionary<VariableSymbol, object> _globals;
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new Stack<Dictionary<VariableSymbol, object>>();

        private Random? _random;
        private object? _lastValue;

        public Evaluator( BoundProgram program , Dictionary<VariableSymbol , object> variables )
        {
            this._program = program;
            this._globals = variables;
            this._locals.Push( new Dictionary<VariableSymbol , object>() );
        }

        public object? Evaluate()
        {
            return this.EvaluateStatement( this._program.Statement );
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
                var s = body.Statements[index];

                switch( s.Kind )
                {
                    case BoundNodeKind.VariableDeclaration:
						this.EvaluateVariableDeclaration( (BoundVariableDeclaration)s );
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
						this.EvaluateExpressionStatement( (BoundExpressionStatement)s );
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        var gs = (BoundGotoStatement)s;
                        index = labelToIndex[ gs.Label ];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        var cgs = (BoundConditionalGotoStatement)s;
                        var condition = (bool)this.EvaluateExpression(cgs.Condition)!;
                        if( condition == cgs.JumpIfTrue )
                            index = labelToIndex[ cgs.Label ];
                        else
                            index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    case BoundNodeKind.ReturnStatement:
                        var rs = (BoundReturnStatement)s;
						this._lastValue = rs.Expression == null ? null : this.EvaluateExpression( rs.Expression );
                        return this._lastValue;
                    default:
                        throw new Exception( $"Unexpected node {s.Kind}" );
                }
            }

            return this._lastValue;
        }

        private void EvaluateVariableDeclaration( BoundVariableDeclaration node )
        {
            var value = this.EvaluateExpression(node.Initializer)!;

			this._lastValue = value;

			this.Assign( node.Variable , value );
        }

        private void EvaluateExpressionStatement( BoundExpressionStatement node )
        {
			this._lastValue = this.EvaluateExpression( node.Expression );
        }

        private object? EvaluateExpression( BoundExpression node )
        {
            switch( node.Kind )
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression( (BoundLiteralExpression)node );
                case BoundNodeKind.VariableExpression:
                    return this.EvaluateVariableExpression( (BoundVariableExpression)node );
                case BoundNodeKind.AssignmentExpression:
                    return this.EvaluateAssignmentExpression( (BoundAssignmentExpression)node );
                case BoundNodeKind.UnaryExpression:
                    return this.EvaluateUnaryExpression( (BoundUnaryExpression)node );
                case BoundNodeKind.BinaryExpression:
                    return this.EvaluateBinaryExpression( (BoundBinaryExpression)node );
                case BoundNodeKind.CallExpression:
                    return this.EvaluateCallExpression( (BoundCallExpression)node );
                case BoundNodeKind.ConversionExpression:
                    return this.EvaluateConversionExpression( (BoundConversionExpression)node );
                default:
                    throw new Exception( $"Unexpected node {node.Kind}" );
            }
        }

        private static object EvaluateLiteralExpression( BoundLiteralExpression n )
        {
            return n.Value;
        }

        private object EvaluateVariableExpression( BoundVariableExpression v )
        {
            if( v.Variable.Kind == SymbolKind.GlobalVariable )
            {
                return this._globals[ v.Variable ];
            }
            else
            {
                var locals = this._locals.Peek();

                return locals[ v.Variable ];
            }
        }

        private object? EvaluateAssignmentExpression( BoundAssignmentExpression a )
        {
            var value = this.EvaluateExpression(a.Expression)!;

			this.Assign( a.Variable , value );

            return value;
        }

        private object? EvaluateUnaryExpression( BoundUnaryExpression u )
        {
            var operand = this.EvaluateExpression(u.Operand)!;

            switch( u.Op.Kind )
            {
                case BoundUnaryOperatorKind.Identity:
                    return (int)operand;
                case BoundUnaryOperatorKind.Negation:
                    return -(int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                case BoundUnaryOperatorKind.OnesComplement:
                    return ~(int)operand;
                default:
                    throw new Exception( $"Unexpected unary operator {u.Op}" );
            }
        }

        private object EvaluateBinaryExpression( BoundBinaryExpression b )
        {
            var lhs = this.EvaluateExpression( b.Lhs )!;
            var rhs = this.EvaluateExpression( b.Rhs )!;

            switch( b.Op.Kind )
            {
                case BoundBinaryOperatorKind.Addition:
                    if( b.Type == TypeSymbol.Int )
                        return (int)lhs + (int)rhs;
                    else
                        return ((string)lhs).Replace( "\"" , "" ) + ((string)rhs).Replace( "\"" , "" );

                case BoundBinaryOperatorKind.Subtraction:     return (bool)lhs && (bool)rhs;
                case BoundBinaryOperatorKind.LogicalOr:       return (bool)lhs || (bool)rhs;
                case BoundBinaryOperatorKind.Equals:          return Equals( lhs , rhs );
                case BoundBinaryOperatorKind.NotEquals:       return !Equals( lhs , rhs );
                case BoundBinaryOperatorKind.Less:            return (int)lhs < (int)rhs;
                case BoundBinaryOperatorKind.LessOrEquals:    return (int)lhs <= (int)rhs;
                case BoundBinaryOperatorKind.Greater:         return (int)lhs > (int)rhs;
                case BoundBinaryOperatorKind.GreaterOrEquals: return (int)lhs >= (int)rhs;

                case BoundBinaryOperatorKind.BitwiseXor:
                    if( b.Type == TypeSymbol.Int )
                        return (int)lhs ^ (int)rhs;
                    else
                        return (bool)lhs ^ (bool)rhs;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if( b.Type == TypeSymbol.Int )
                        return (int)lhs | (int)rhs;
                    else
                        return (bool)lhs | (bool)rhs;
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if( b.Type == TypeSymbol.Int )
                        return (int)lhs & (int)rhs;
                    else
                        return (bool)lhs & (bool)rhs;

                default:
                    throw new Exception( $"Unexpected binary operator {b.Op}" );
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
                var message = (string)this.EvaluateExpression(node.Arguments[0])!;

                Console.WriteLine( message );

                return null;
            }
            else if( node.Function == BuiltinFunctions.Rnd )
            {
                var max = (int)this.EvaluateExpression(node.Arguments[0])!;

                if( this._random == null )
					this._random = new Random();

                return this._random.Next( max );
            }
            else
            {
                var locals = new Dictionary<VariableSymbol, object>();

                 for( var i = 0 ; i < node.Arguments.Length ; i++ )
                {
                    var parameter = node.Function.Parameters[i];
                    var value = this.EvaluateExpression(node.Arguments[i])!;

                    locals.Add( parameter , value );
                }

				this._locals.Push( locals );

                var statement = this._program.Functions[node.Function];
                var result = this.EvaluateStatement(statement);

                _ = this._locals.Pop();

                return result;
            }
        }

        private object? EvaluateConversionExpression( BoundConversionExpression node )
        {
            var value = this.EvaluateExpression(node.Expression);

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
				this._globals[ variable ] = value;
            }
            else
            {
                var locals = this._locals.Peek();

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
