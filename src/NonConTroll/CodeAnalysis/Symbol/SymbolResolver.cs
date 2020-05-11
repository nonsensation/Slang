using System.Collections.Generic;
using System.IO;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Symbols
{
    public class SymbolResolver
    {
        public static void Resolve( IEnumerable<Symbol> symbols )
        {
            var done = false;

            while( !done )
            {
                done = true;

                foreach( var s in symbols )
                {
                    if( s.IsResolved )
                        continue;



                    if( !s.IsResolved )
                        done = false;
                }
            }
        }
    }
}
