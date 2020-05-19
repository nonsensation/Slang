using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.Emit;

namespace NonConTroll.CodeAnalysis
{
    public sealed class Compilation
    {
        private Compilation( bool isScript , Compilation? previous , params SyntaxTree[]? syntaxTrees )
        {
            this.IsScript    = isScript;
            this.Previous    = previous;
            this.SyntaxTrees = syntaxTrees == null ?
                ImmutableArray<SyntaxTree>.Empty :
                syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create( params SyntaxTree[]? syntaxTrees )
            => new Compilation( isScript: false , null , syntaxTrees );

        public static Compilation CreateScript( Compilation? previous , params SyntaxTree[]? syntaxTrees )
            => new Compilation( isScript: true , previous , syntaxTrees );

        private BoundGlobalScope? globalScope;

        internal bool IsScript { get; }
        internal Compilation? Previous { get; }
        internal BuiltinFunctionSymbol? MainFunction => this.GlobalScope.MainFunction;
        internal ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        internal ImmutableArray<FunctionSymbol> Functions => this.GlobalScope.Functions;
        internal ImmutableArray<VariableSymbol> Variables => this.GlobalScope.Variables;

        internal BoundGlobalScope GlobalScope {
            get {
                if( this.globalScope == null )
                {
                    var globalScope = Binding.Binder.BindGlobalScope( this.IsScript , this.Previous?.GlobalScope , this.SyntaxTrees );

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
                var bindingFlags =
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic;
                var builtinFunctions = typeof( BuiltinFunctions )
                    .GetFields( bindingFlags )
                    .Where( x => x.FieldType == typeof( DeclaredFunctionSymbol ) )
                    .Select( x => (DeclaredFunctionSymbol)x.GetValue( obj: null )! )
                    .Where( x => x != null )
                    .ToList();

                foreach( var function in submission.Functions )
                {
                    if( seenSymbolNames.Add( function.Name ) )
                    {
                        yield return function;
                    }
                }

                foreach( var variable in submission.Variables )
                {
                    if( seenSymbolNames.Add( variable.Name ) )
                    {
                        yield return variable;
                    }
                }

                foreach( var builtin in builtinFunctions )
                {
                    if( seenSymbolNames.Add( builtin.Name ) )
                    {
                        yield return builtin;
                    }
                }

                submission = submission.Previous;
            }
        }

        private BoundProgram GetProgram()
            => Binding.Binder.BindProgram( this.IsScript , this.Previous?.GetProgram() , this.GlobalScope );

        public EvaluationResult Evaluate( Dictionary<VariableSymbol , object> variables )
        {
            if( this.GlobalScope.Diagnostics.Any() )
            {
                return new EvaluationResult( this.GlobalScope.Diagnostics , null );
            }

            var program = this.GetProgram();

#if false // CFG Graphviz output
            var appPath      = Environment.GetCommandLineArgs()[ 0 ];
            var appDirectory = Path.GetDirectoryName( appPath )!;
            var cfgPath      = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) , "Slang" , "cfg.dot" );//Path.Combine( appDirectory , "cfg.dot" );
            var cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any()
                             ? program.Functions.Last().Value
                             : program.Statement;
            var cfg = ControlFlowGraph.Create( cfgStatement );

            using var streamWriter = new StreamWriter( cfgPath );

            cfg.WriteTo( streamWriter );
#endif

            if( program.Diagnostics.Any() )
            {
                return new EvaluationResult( program.Diagnostics.ToImmutableArray() , null );
            }

            var evaluator = new Evaluator( program , variables );
            var value = evaluator.Evaluate();

            return new EvaluationResult( ImmutableArray<Diagnostic>.Empty , value );
        }

        public void EmitTree( TextWriter writer )
        {
            var program = this.GetProgram();

            if( this.GlobalScope.MainFunction != null )
            {
                this.EmitTree( this.GlobalScope.MainFunction , writer );
            }
            else if( this.GlobalScope.EvalFunction != null )
            {
                this.EmitTree( this.GlobalScope.EvalFunction , writer );
            }
            else
            {
                throw new Exception( "unreachable" );
            }
        }

        public void EmitTree( FunctionSymbol symbol , TextWriter writer )
        {
            var program = this.GetProgram();

            symbol.WriteTo( writer );
            writer.WriteLine();

            if( !program.Functions.TryGetValue( symbol , out var body ) )
            {
                return;
            }

            body.WriteTo( writer );
        }

        public ImmutableArray<Diagnostic> Emit( string moduleName , string[] references , string outputPath )
        {
            var program = this.GetProgram();

            if( program.Diagnostics.Any() )
            {
                return program.Diagnostics;
            }

            // var diagnostics = this.Emit( program , moduleName , references , outputPath );

            // return diagnostics;

            throw new Exception();
        }
    }
}
