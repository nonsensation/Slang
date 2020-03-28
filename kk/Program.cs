using System;
using System.Linq;

namespace kk
{
    class Program
    {
        static void Main( string[] args )
        {
            if( !args.Any() )
            {
                Console.Error.WriteLine( "usage: kk <source-files>" );

                return;
            }

            if( args.Length > 1 )
            {
                Console.Error.WriteLine( "Currently only a single file supported." );

                return;
            }


            Console.WriteLine( "Hello World!" );
        }
    }
}
