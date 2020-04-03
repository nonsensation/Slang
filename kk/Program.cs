using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if( !args.Any() )
            {
                Console.Error.WriteLine( "usage: kk <source-files>" );

                return 1;
            }

            var paths = GetFilePaths( args );
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
                return 1;

            var compilation = new Compilation( syntaxTrees.ToArray() );
            var result = compilation.Evaluate( new Dictionary<VariableSymbol,object>() );

            if( result.Diagnostics.Any() )
            {
                Console.Error.WriteDiagnostics( result.Diagnostics );

                return 1;
            }

            if( result.Value != null )
                Console.WriteLine( result.Value );

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
