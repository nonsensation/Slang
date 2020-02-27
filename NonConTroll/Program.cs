using System;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll
{
    public class Program
    {
        public static void Main( string[] args )
        {
            Console.WriteLine( "Hello World!" );
        }
    }
}

namespace NonConTroll.CodeAnalysis
{
    public class Lexer
    {
        private readonly string Text;

        private int Position;
        private TokenType TokenType;

        public Lexer( string text )
        {
            this.Text = text;
        }
    }
}

namespace NonConTroll.CodeAnalysis.Syntax
{
    public enum TokenType
    {

    }
}
