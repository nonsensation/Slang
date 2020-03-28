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

        private void Report( TextSpan span , string message )
            => this.Diagnostics.Add( new Diagnostic( span , message ) );



        public void ReportInvalidNumber( TextSpan span , string text , TypeSymbol type )
            => this.Report( span , $"The number {text} isn't valid {type}." );
        public void ReportBadCharacter( int position , char character )
            => this.Report( new TextSpan( position , 1 ) , $"Bad character input: '{character}'." );

        public void ReportUnterminatedString( TextSpan span )
            => this.Report( span , $"Unterminated string literal." );

        public void ReportUnexpectedToken( TextSpan span , TokenType tokenType , TokenType expectedTokenType )
            => this.Report( span , $"Unexpected token <{tokenType}>, expected <{expectedTokenType}>." );

        public void ReportUndefinedUnaryOperator( TextSpan span , string operatorText , TypeSymbol operandType )
            => this.Report( span , $"Unary operator '{operatorText}' is not defined for type '{operandType}'." );

        public void ReportUndefinedBinaryOperator( TextSpan span , string operatorText , TypeSymbol leftType , TypeSymbol rightType )
            => this.Report( span , $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'." );

        public void ReportParameterAlreadyDeclared( TextSpan span , string parameterName )
            => this.Report( span , $"A parameter with the name '{parameterName}' already exists." );

        public void ReportUndefinedName( TextSpan span , string name )
            => this.Report( span , $"Variable '{name}' doesn't exist." );

        public void ReportUndefinedType( TextSpan span , string name )
            => this.Report( span , $"Type '{name}' doesn't exist." );

        public void ReportCannotConvert( TextSpan span , TypeSymbol fromType , TypeSymbol toType )
            => this.Report( span , $"Cannot convert type '{fromType}' to '{toType}'." );

        public void ReportCannotConvertImplicit( TextSpan span , TypeSymbol fromType , TypeSymbol toType )
            => this.Report( span , $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)" );

        public void ReportSymbolAlreadyDeclared( TextSpan span , string name )
            => this.Report( span , $"'{name}' is already declared." );

        public void ReportCannotAssign( TextSpan span , string name )
            => this.Report( span , $"Variable '{name}' is read-only and cannot be assigned to." );

        public void ReportUndefinedFunction( TextSpan span , string name )
            => this.Report( span , $"Function '{name}' doesn't exist." );

        public void ReportWrongArgumentCount( TextSpan span , string name , int expectedCount , int actualCount )
            => this.Report( span , $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}." );

        public void ReportWrongArgumentType( TextSpan span , string name , TypeSymbol expectedType , TypeSymbol actualType )
            => this.Report( span , $"Parameter '{name}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'." );

        public void ReportExpressionMustHaveValue( TextSpan span )
            => this.Report( span , "Expression must have a value." );

        public void ReportInvalidBreakOrContinue( TextSpan span , string text )
            => this.Report( span , $"The keyword '{text}' can only be used inside of loops." );

        public void ReportAllPathsMustReturn( TextSpan span )
            => this.Report( span , "Not all code paths return a value." );

        public void ReportInvalidReturn( TextSpan span )
            => this.Report( span , "The 'return' keyword can only be used inside of functions." );

        public void ReportInvalidReturnExpression( TextSpan span , string functionName )
            => this.Report( span , $"Since the function '{functionName}' does not return a value the 'return' keyword cannot be followed by an expression." );

        public void ReportMissingReturnExpression( TextSpan span , TypeSymbol returnType )
            => this.Report( span , $"An expression of type '{returnType}' is expected." );

        internal void ReportExpressionInvalidLiteral( TextSpan span )
            => this.Report( span , "Invalid literal." );

        internal void ReportExpressionInvalidNumericLiteral( TextSpan span , string literalText )
            => this.Report( span , $"The numeric literal '{literalText}' is not a valid number." );

        internal void ReportUndefinedVariable( TextSpan span , string name )
            => this.Report( span , $"Variable '{name}' doesn't exist." );

        internal void ReportNotAVariable( TextSpan span , string name )
            => this.Report( span , $"'{name}' is not a variable." );

        internal void ReportNotAFunction( TextSpan span , string name )
            => this.Report( span , $"'{name}' is not a function." );

    }
}
