using System.Collections.Immutable;

namespace NonConTroll.CodeAnalysis
{
    public class EvaluationResult
    {
        public EvaluationResult( ImmutableArray<Diagnostic> diagnostics , object? value )
        {
            this.Diagnostics = diagnostics;
            this.Value       = value;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public object? Value { get; }
    }
}
