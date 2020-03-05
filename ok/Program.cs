
using NonConTroll.CodeAnalysis;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;


namespace NonConTroll
{
    class Program
    {
        static void Main()
        {
            var repl = new NonConTrollRepl();

            repl.Run();
        }
    }
}
