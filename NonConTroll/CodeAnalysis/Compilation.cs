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
        private BoundGlobalScope? globalScope;

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
        public ImmutableArray<FunctionSymbol> Functions => this.GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => this.GlobalScope.Variables;

        internal BoundGlobalScope GlobalScope {
            get {
                if( this.globalScope == null )
                {
                    var globalScope = Binder.BindGlobalScope( this.Previous?.GlobalScope , this.SyntaxTrees );

                    _ = Interlocked.CompareExchange( ref this.globalScope! , globalScope , null );
                }

                return this.globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission = this;
            var seenSymbolNames = new HashSet<string>();

            while( submission != null )
            {
                foreach( var function in submission.Functions )
                    if( seenSymbolNames.Add( function.Name ) )
                        yield return function;

                foreach( var variable in submission.Variables )
                    if( seenSymbolNames.Add( variable.Name ) )
                        yield return variable;

                submission = submission.Previous;
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

            var evaluator = new Evaluator( program , variables );
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
}
