using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Options;

using NonConTroll.CodeAnalysis;
using NonConTroll.CodeAnalysis.IO;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll
{
    public class Program
    {
        public static int Main( string[] args )
        {
            var helpRequested  = false;
            var moduleName     = default( string? );
            var outputPath     = default( string? );
            var sourcePaths    = new List<string>();
            var referencePaths = new List<string>();

            var options = new OptionSet {
                "usage: NonConTroll.Compiler <source-files> [options]" ,
                { "-r=" , "The {path} of an assembly reference" , x => referencePaths.Add( x ) } ,
                { "-o=" , "The output {path} of the assembly to create" , x => outputPath = x } ,
                { "-m=" , "The name of the module" , x => moduleName = x } ,
                { "-?|-h|--help" , "Prints help" , x => helpRequested = true } ,
                { "<>" , x => sourcePaths.Add( x ) } ,
            };

            options.Parse( args );

            if( helpRequested )
            {
                options.WriteOptionDescriptions( Console.Out );

                return 0;
            }

            var paths = GetFilePaths( sourcePaths );

            if( !paths.Any() )
            {
                Console.Error.WriteLine( "error: need at least one source file" );

                return 1;
            }

            if( outputPath == null )
            {
                outputPath = Path.ChangeExtension( sourcePaths.First() , ".exe" );
            }

            if( moduleName == null )
            {
                moduleName = Path.GetFileNameWithoutExtension( outputPath );
            }

            var syntaxTrees = new List<SyntaxTree>();
            var hasErrors = false;

            foreach( var path in paths )
            {
                if( !File.Exists( path ) )
                {
                    Console.Error.WriteLine( $"error: file '{path}' doesn't exist" );

                    hasErrors = true;

                    continue;
                }

                var syntaxTree = SyntaxTree.Load( path );

                syntaxTrees.Add( syntaxTree );
            }

            if( hasErrors )
            {
                return 1;
            }

            var compilation = Compilation.Create( syntaxTrees.ToArray() );

            var diagnostics = compilation.Emit( moduleName , referencePaths.ToArray() , outputPath );

            // var result = compilation.Evaluate( new Dictionary<VariableSymbol,object>() );

            if( diagnostics.Any() )
            {
                Console.Error.WriteDiagnostics( diagnostics );

                return 1;
            }

            return 0;
        }

        private static IEnumerable<string> GetFilePaths( IEnumerable<string> paths )
        {
            var result = new SortedSet<string>();

            foreach( var path in paths )
            {
                if( Directory.Exists( path ) )
                {
                    result.UnionWith( Directory.EnumerateFiles( path , "*.sl" , SearchOption.AllDirectories ) );
                }
                else
                {
                    result.Add( path );
                }
            }

            return result;
        }
    }
}
