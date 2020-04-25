using System;
using System.Collections;
using System.Collections.Generic;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> Diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator()
            => this.Diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        public void AddRange( DiagnosticBag diagnostics )
            => this.Diagnostics.AddRange( diagnostics.Diagnostics );

        private void Report( TextLocation location , string message )
            => this.Diagnostics.Add( new Diagnostic( location , message ) );

    #region Reports

        public void ReportInvalidNumber( TextLocation location , string text , TypeSymbol type )
            => this.Report( location , $"The number {text} isn't valid {type}." );

        public void ReportBadCharacter( TextLocation location , char character )
            => this.Report( location , $"Bad character input: '{character}'." );

        public void ReportUnterminatedString( TextLocation location )
            => this.Report( location , $"Unterminated string literal." );

        public void ReportUnexpectedToken( TextLocation location , TokenType tokenType , TokenType expectedTokenType )
            => this.Report( location , $"Unexpected token <{tokenType}>, expected <{expectedTokenType}>." );

        public void ReportUndefinedUnaryOperator( TextLocation location , string operatorText , TypeSymbol operandType )
            => this.Report( location , $"Unary operator '{operatorText}' is not defined for type '{operandType}'." );

        public void ReportUndefinedBinaryOperator( TextLocation location , string operatorText , TypeSymbol leftType , TypeSymbol rightType )
            => this.Report( location , $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'." );

        public void ReportParameterAlreadyDeclared( TextLocation location , string parameterName )
            => this.Report( location , $"A parameter with the name '{parameterName}' already exists." );

        public void ReportUndefinedType( TextLocation location , string name )
            => this.Report( location , $"Type '{name}' doesn't exist." );

        public void ReportCannotConvert( TextLocation location , TypeSymbol fromType , TypeSymbol toType )
            => this.Report( location , $"Cannot convert type '{fromType}' to '{toType}'." );

        public void ReportCannotConvertImplicit( TextLocation location , TypeSymbol fromType , TypeSymbol toType )
            => this.Report( location , $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)" );

        public void ReportSymbolAlreadyDeclared( TextLocation location , string name )
            => this.Report( location , $"'{name}' is already declared." );

        public void ReportCannotAssign( TextLocation location , string name )
            => this.Report( location , $"Variable '{name}' is read-only and cannot be assigned to." );

        public void ReportUndefinedFunction( TextLocation location , string name )
            => this.Report( location , $"Function '{name}' doesn't exist." );

        public void ReportWrongArgumentCount( TextLocation location , string name , int expectedCount , int actualCount )
            => this.Report( location , $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}." );

        public void ReportWrongArgumentType( TextLocation location , FunctionSymbol function , string name , TypeSymbol expectedType , TypeSymbol actualType )
            => this.Report( location , $"Parameter '{name}' for funnction '{function}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'." );

        public void ReportExpressionMustHaveValue( TextLocation location )
            => this.Report( location , "Expression must have a value." );

        public void ReportInvalidBreakOrContinue( TextLocation location , string text )
            => this.Report( location , $"The keyword '{text}' can only be used inside of loops." );

        public void ReportAllPathsMustReturn( TextLocation location )
            => this.Report( location , "Not all code paths return a value." );

        public void ReportInvalidReturn( TextLocation location )
            => this.Report( location , "The 'return' keyword can only be used inside of functions." );

        public void ReportInvalidReturnExpression( TextLocation location , string functionName )
            => this.Report( location , $"Since the function '{functionName}' does not return a value the 'return' keyword cannot be followed by an expression." );

        public void ReportMissingReturnExpression( TextLocation location , TypeSymbol returnType )
            => this.Report( location , $"An expression of type '{returnType}' is expected." );

        public void ReportExpressionInvalidLiteral( TextLocation location )
            => this.Report( location , "Invalid literal." );

        public void ReportExpressionInvalidNumericLiteral( TextLocation location , string literalText )
            => this.Report( location , $"The numeric literal '{literalText}' is not a valid number." );

        public void ReportUndefinedVariable( TextLocation location , string name )
            => this.Report( location , $"Variable '{name}' doesn't exist." );

        public void ReportNotAVariable( TextLocation location , string name )
            => this.Report( location , $"'{name}' is not a variable." );

        public void ReportNotAFunction( TextLocation location , string name )
            => this.Report( location , $"'{name}' is not a function." );

        internal void ReportInvalidExpressionStatement( TextLocation location )
            => this.Report( location , "Only assignment, call, (TODO: increment, decrement, await, and new object) expressions can be used as a statement." );

        #endregion

    }
}
