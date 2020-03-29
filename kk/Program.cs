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
        public static void Main( string[] args )
        {
            if( !args.Any() )
            {
                Console.Error.WriteLine( "usage: kk <source-files>" );

                return;
            }

            var paths = GetFilePaths( args );
            var syntaxTrees = new List<SyntaxTree>();
            var hasErrors = false;

            foreach( var path in paths )
            {
                if( !File.Exists( path ) )
                {
                    Console.WriteLine( $"error: file '{path}' doesn't exist" );

                    hasErrors = true;

                    continue;
                }

                var syntaxTree = SyntaxTree.Load( path );

                syntaxTrees.Add( syntaxTree );
            }

            if( hasErrors )
                return;

            var compilation = new Compilation( syntaxTrees.ToArray() );
            var result = compilation.Evaluate( new Dictionary<VariableSymbol,object>() );

            if( result.Diagnostics.Any() )
            {
                Console.Error.WriteDiagnostics( result.Diagnostics );

                return;
            }

            if( result.Value != null )
                Console.WriteLine( result.Value );
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
