using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> Diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator()
            => this.Diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        public void AddRange( IEnumerable<Diagnostic> diagnostics )
            => this.Diagnostics.AddRange( diagnostics );

        private void Report( TextLocation location, string message )
            => this.Diagnostics.Add( new Diagnostic( location, message ) );

        #region Reports

        public void ReportInvalidNumber( TextLocation location, string text, TypeSymbol type )
            => this.Report( location, $"The number {text} isn't valid {type}." );

        public void ReportBadCharacter( TextLocation location, char character )
            => this.Report( location, $"Bad character input: '{character}'." );

        public void ReportUnterminatedString( TextLocation location )
            => this.Report( location, $"Unterminated string literal." );

        public void ReportUnexpectedToken( TextLocation location, SyntaxKind tokenType, SyntaxKind expectedTokenType )
            => this.Report( location, $"Unexpected token <{tokenType}>, expected <{expectedTokenType}>." );

        public void ReportUndefinedUnaryOperator( TextLocation location, string operatorText, TypeSymbol operandType )
            => this.Report( location, $"Unary operator '{operatorText}' is not defined for type '{operandType}'." );

        public void ReportUndefinedBinaryOperator( TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType )
            => this.Report( location, $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'." );

        public void ReportParameterAlreadyDeclared( TextLocation location, string parameterName )
            => this.Report( location, $"A parameter with the name '{parameterName}' already exists." );

        public void ReportUndefinedType( TextLocation location, string name )
            => this.Report( location, $"Type '{name}' doesn't exist." );

        public void ReportCannotConvert( TextLocation location, TypeSymbol fromType, TypeSymbol toType )
            => this.Report( location, $"Cannot convert type '{fromType}' to '{toType}'." );

        internal void ReportInvalidReference( string reference )
        {
            throw new NotImplementedException();
        }

        public void ReportCannotConvertImplicit( TextLocation location, TypeSymbol fromType, TypeSymbol toType )
            => this.Report( location, $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)" );

        public void ReportSymbolAlreadyDeclared( TextLocation location, string name )
            => this.Report( location, $"'{name}' is already declared." );

        public void ReportCannotAssign( TextLocation location, string name )
            => this.Report( location, $"Variable '{name}' is read-only and cannot be assigned to." );

        public void ReportUndefinedFunction( TextLocation location, string name )
            => this.Report( location, $"Function '{name}' doesn't exist." );

        public void ReportWrongArgumentCount( TextLocation location, string name, int expectedCount, int actualCount )
            => this.Report( location, $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}." );

        internal void ReportCannotMixMainAndGlobalStatements( TextLocation location )
            => this.Report( location, $"Cannot declare a main() function when global statements are used." );

        internal void ReportInvalidMainSignature( TextLocation location )
            => this.Report( location, $"main() function must not take arguments and return void." );

        internal void ReportOnlyOneFileCanHaveGlobalStatements( TextLocation location )
            => this.Report( location, $"At most one file can have global statements." );

        internal void ReportRequiredTypeNotFound( string? minskName , string metadataName )
        {
            throw new NotImplementedException();
        }

        internal void ReportRequiredTypeAmbiguous( string? minskName , string metadataName , TypeDefinition[] foundTypes )
        {
            throw new NotImplementedException();
        }

        public void ReportWrongArgumentType( TextLocation location, DeclaredFunctionSymbol function, string name, TypeSymbol expectedType, TypeSymbol actualType )
            => this.Report( location, $"Parameter '{name}' for funnction '{function}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'." );

        public void ReportExpressionMustHaveValue( TextLocation location )
            => this.Report( location, "Expression must have a value." );

        public void ReportInvalidBreakOrContinue( TextLocation location, string text )
            => this.Report( location, $"The keyword '{text}' can only be used inside of loops." );

        public void ReportAllPathsMustReturn( TextLocation location )
            => this.Report( location, "Not all code paths return a value." );

        public void ReportInvalidReturn( TextLocation location )
            => this.Report( location, "The 'return' keyword can only be used inside of functions." );

        public void ReportInvalidReturnExpression( TextLocation location, string functionName )
            => this.Report( location, $"Since the function '{functionName}' does not return a value the 'return' keyword cannot be followed by an expression." );

        public void ReportMissingReturnExpression( TextLocation location, TypeSymbol returnType )
            => this.Report( location, $"An expression of type '{returnType}' is expected." );

        internal void ReportRequiredMethodNotFound( string typeName , string methodName , string[] parameterTypeNames )
        {
            throw new NotImplementedException();
        }

        public void ReportExpressionInvalidLiteral( TextLocation location )
            => this.Report( location, "Invalid literal." );

        public void ReportExpressionInvalidNumericLiteral( TextLocation location, string literalText )
            => this.Report( location, $"The numeric literal '{literalText}' is not a valid number." );

        public void ReportUndefinedVariable( TextLocation location, string name )
            => this.Report( location, $"Variable '{name}' doesn't exist." );

        public void ReportNotAVariable( TextLocation location, string name )
            => this.Report( location, $"'{name}' is not a variable." );

        public void ReportNotAFunction( TextLocation location, string name )
            => this.Report( location, $"'{name}' is not a function." );

        internal void ReportInvalidExpressionStatement( TextLocation location )
            => this.Report( location, $"Expected a statement, found an expression. (Only assignment, call, increment, decrement, match, await, and new object expressions can be used as a statement.)" );

        internal void ReportMissingPattern( TextLocation location )
            => this.Report( location, $"Pattern missing." );

        internal void ReportMultipleMatchAnyPattern( TextLocation location )
            => this.Report( location, $"Multiple match-any \"_\" pattern found." );

        internal void ReportInvalidReturnWithValueInGlobalStatements( TextLocation location )
            => this.Report( location, $"The 'return' keyword cannot be followed by an espression in global statements." );

        internal void ReportBuiltinTypeAmbiguous( TypeSymbol type , TypeDefinition[] foundTypes )
            => this.Report( default , $"The builtin type '{type}' was found in multiple references: '{string.Join( ", " , foundTypes.Select( x => x.Module.Assembly.Name ) )}'" );

        internal void ReportBuiltinTypeNotFound( TypeSymbol type )
            => this.Report( default , $"The builtin type '{type}' cannot be resolved among the given references." );

        public void ReportBadImagePath( string reference )
            => this.Report( default , $"The reference is not a valid .NET assembly: '{reference}'" );

        #endregion

    }
}
