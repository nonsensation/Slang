using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Lowering;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class Binder
    {
        private readonly DiagnosticBag DiagBag = new DiagnosticBag();
        public DiagnosticBag Diagnostics => this.DiagBag;

        private readonly FunctionSymbol? Function;

        private Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> LoopStack = new Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)>();
        private int LabelCounter;
        private BoundScope? Scope;

        public Binder( BoundScope parent , FunctionSymbol? function )
        {
            this.Scope = new BoundScope( parent );
            this.Function = function;

            if( function != null )
            {
                foreach( var p in function.Parameters )
                    _ = this.Scope.TryDeclareVariable( p );
            }
        }

        public static BoundGlobalScope BindGlobalScope( BoundGlobalScope? previous , CompilationUnitSyntax syntax )
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope, function: null);

            foreach( var function in syntax.Members.OfType<FunctionDeclarationSyntax>() )
                binder.BindFunctionDeclaration( function );

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach( var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>() )
            {
                var statement = binder.BindStatement(globalStatement.Statement);
                statements.Add( statement );
            }

            var functions = binder.Scope!.GetDeclaredFunctions();
            var variables = binder.Scope!.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if( previous != null )
                diagnostics = diagnostics.InsertRange( 0 , previous.Diagnostics );

            return new BoundGlobalScope( previous , diagnostics , functions , variables , statements.ToImmutable() );
        }

        public static BoundProgram BindProgram( BoundGlobalScope globalScope )
        {
            var parentScope = CreateParentScope(globalScope);
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            BoundGlobalScope? scope = globalScope;

            while( scope != null )
            {
                foreach( var function in scope.Functions )
                {
                    var binder = new Binder(parentScope, function);
                    var body = binder.BindStatement(function.Declaration!.Body);
                    var loweredBody = Lowerer.Lower(body);

                    if( function.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn( loweredBody ) )
                        binder.DiagBag.ReportAllPathsMustReturn( function.Declaration.Identifier.Span );

                    functionBodies.Add( function , loweredBody );

                    diagnostics.AddRange( binder.Diagnostics );
                }

                scope = scope.Previous;
            }

            var statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));

            return new BoundProgram( diagnostics.ToImmutable() , functionBodies.ToImmutable() , statement );
        }

        private void BindFunctionDeclaration( FunctionDeclarationSyntax syntax )
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string>();

            foreach( var parameterSyntax in syntax.Parameters )
            {
                var parameterName = parameterSyntax.Identifier.Text!;
                var parameterType = this.BindTypeClause(parameterSyntax.Type)!;

                if( !seenParameterNames.Add( parameterName ) )
                {
                    this.DiagBag.ReportParameterAlreadyDeclared( parameterSyntax.Span , parameterName );
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType);
                    parameters.Add( parameter );
                }
            }

            var type = this.BindTypeClause( syntax.ReturnType ) ?? TypeSymbol.Void;
            var function = new FunctionSymbol(syntax.Identifier.Text!, parameters.ToImmutable(), type, syntax);

            if( !this.Scope!.TryDeclareFunction( function ) )
                this.DiagBag.ReportSymbolAlreadyDeclared( syntax.Identifier.Span , function.Name );
        }

        private static BoundScope CreateParentScope( BoundGlobalScope? previous )
        {
            var stack = new Stack<BoundGlobalScope>();

            while( previous != null )
            {
                stack.Push( previous );

                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            while( stack.Count > 0 )
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);

                foreach( var f in previous.Functions )
                    _ = scope.TryDeclareFunction( f );

                foreach( var v in previous.Variables )
                    _ = scope.TryDeclareVariable( v );

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);

            foreach( var f in BuiltinFunctions.GetAll() )
                _ = result.TryDeclareFunction( f );

            return result;
        }

        private BoundStatement BindErrorStatement()
        {
            return new BoundExpressionStatement( new BoundErrorExpression() );
        }

        private BoundStatement BindStatement( StatementSyntax syntax )
        {
            switch( syntax.Kind )
            {
                case SyntaxKind.BlockStatement:
                    return this.BindBlockStatement( (BlockStatementSyntax)syntax );
                case SyntaxKind.VariableDeclaration:
                    return this.BindVariableDeclaration( (VariableDeclarationSyntax)syntax );
                case SyntaxKind.IfStatement:
                    return this.BindIfStatement( (IfStatementSyntax)syntax );
                case SyntaxKind.WhileStatement:
                    return this.BindWhileStatement( (WhileStatementSyntax)syntax );
                case SyntaxKind.DoWhileStatement:
                    return this.BindDoWhileStatement( (DoWhileStatementSyntax)syntax );
                case SyntaxKind.ForStatement:
                    return this.BindForStatement( (ForStatementSyntax)syntax );
                case SyntaxKind.BreakStatement:
                    return this.BindBreakStatement( (BreakStatementSyntax)syntax );
                case SyntaxKind.ContinueStatement:
                    return this.BindContinueStatement( (ContinueStatementSyntax)syntax );
                case SyntaxKind.ReturnStatement:
                    return this.BindReturnStatement( (ReturnStatementSyntax)syntax );
                case SyntaxKind.ExpressionStatement:
                    return this.BindExpressionStatement( (ExpressionStatementSyntax)syntax );
                default:
                    throw new Exception( $"Unexpected syntax {syntax.Kind}" );
            }
        }

        private BoundStatement BindBlockStatement( BlockStatementSyntax syntax )
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            this.Scope = new BoundScope( this.Scope );

            foreach( var statementSyntax in syntax.Statements )
            {
                var statement = this.BindStatement(statementSyntax);
                statements.Add( statement );
            }

            this.Scope = this.Scope.Parent;

            return new BoundBlockStatement( statements.ToImmutable() );
        }

        private BoundStatement BindVariableDeclaration( VariableDeclarationSyntax syntax )
        {
            var isReadOnly = syntax.Keyword.TkType == TokenType.Let;
            var type = this.BindTypeClause(syntax.TypeClause);
            var initializer = this.BindExpression(syntax.Initializer);
            var variableType = type ?? initializer.Type;
            var variable = this.BindVariable(syntax.Identifier, isReadOnly, variableType);
            var convertedInitializer = this.BindConversion(syntax.Initializer.Span, initializer, variableType);

            return new BoundVariableDeclaration( variable , convertedInitializer );
        }

        private TypeSymbol? BindTypeClause( TypeClauseSyntax? syntax )
        {
            if( syntax == null )
                return null;

            var type = this.LookupType(syntax.Identifier.Text!);
            if( type == null )
                this.DiagBag.ReportUndefinedType( syntax.Identifier.Span , syntax.Identifier.Text! );

            return type;
        }

        private BoundStatement BindIfStatement( IfStatementSyntax syntax )
        {
            var condition = this.BindExpression(syntax.Condition, TypeSymbol.Bool);
            var thenStatement = this.BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : this.BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement( condition , thenStatement , elseStatement );
        }

        private BoundStatement BindWhileStatement( WhileStatementSyntax syntax )
        {
            var condition = this.BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = this.BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement( condition , body , breakLabel , continueLabel );
        }

        private BoundStatement BindDoWhileStatement( DoWhileStatementSyntax syntax )
        {
            var body = this.BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            var condition = this.BindExpression(syntax.Condition, TypeSymbol.Bool);
            return new BoundDoWhileStatement( body , condition , breakLabel , continueLabel );
        }

        private BoundStatement BindForStatement( ForStatementSyntax syntax )
        {
            var lowerBound = this.BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = this.BindExpression(syntax.UpperBound, TypeSymbol.Int);

            this.Scope = new BoundScope( this.Scope );

            var variable = this.BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Int);
            var body = this.BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            this.Scope = this.Scope.Parent;

            return new BoundForStatement( variable , lowerBound , upperBound , body , breakLabel , continueLabel );
        }

        private BoundStatement BindLoopBody( StatementSyntax body , out BoundLabel breakLabel , out BoundLabel continueLabel )
        {
            this.LabelCounter++;
            breakLabel = new BoundLabel( $"break{this.LabelCounter}" );
            continueLabel = new BoundLabel( $"continue{this.LabelCounter}" );

            this.LoopStack.Push( (breakLabel, continueLabel) );
            var boundBody = this.BindStatement(body);
            _ = this.LoopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindBreakStatement( BreakStatementSyntax syntax )
        {
            if( this.LoopStack.Count == 0 )
            {
                this.DiagBag.ReportInvalidBreakOrContinue( syntax.Keyword.Span , syntax.Keyword.Text! );
                return this.BindErrorStatement();
            }

            var breakLabel = this.LoopStack.Peek().BreakLabel;
            return new BoundGotoStatement( breakLabel );
        }

        private BoundStatement BindContinueStatement( ContinueStatementSyntax syntax )
        {
            if( this.LoopStack.Count == 0 )
            {
                this.DiagBag.ReportInvalidBreakOrContinue( syntax.Keyword.Span , syntax.Keyword.Text! );
                return this.BindErrorStatement();
            }

            var continueLabel = this.LoopStack.Peek().ContinueLabel;
            return new BoundGotoStatement( continueLabel );
        }

        private BoundStatement BindReturnStatement( ReturnStatementSyntax syntax )
        {
            var expression = syntax.Expression == null ? null : this.BindExpression(syntax.Expression);

            if( this.Function == null )
            {
                this.DiagBag.ReportInvalidReturn( syntax.ReturnKeyword.Span );
            }
            else
            {
                if( this.Function.Type == TypeSymbol.Void )
                {
                    if( expression != null )
                        this.DiagBag.ReportInvalidReturnExpression( syntax.Expression!.Span , this.Function.Name );
                }
                else
                {
                    if( expression == null )
                        this.DiagBag.ReportMissingReturnExpression( syntax.ReturnKeyword.Span , this.Function.Type );
                    else
                        expression = this.BindConversion( syntax.Expression!.Span , expression , this.Function.Type );
                }
            }

            return new BoundReturnStatement( expression );
        }

        private BoundStatement BindExpressionStatement( ExpressionStatementSyntax syntax )
        {
            var expression = this.BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement( expression );
        }

        private BoundExpression BindExpression( ExpressionSyntax syntax , TypeSymbol targetType )
        {
            return this.BindConversion( syntax , targetType );
        }

        private BoundExpression BindExpression( ExpressionSyntax syntax , bool canBeVoid = false )
        {
            var result = this.BindExpressionpublic(syntax);

            if( !canBeVoid && result.Type == TypeSymbol.Void )
            {
                this.DiagBag.ReportExpressionMustHaveValue( syntax.Span );

                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionpublic( ExpressionSyntax syntax )
        {
            switch( syntax.Kind )
            {
                case SyntaxKind.ParenthesizedExpression:
                    return this.BindParenthesizedExpression( (ParenthesizedExpressionSyntax)syntax );
                case SyntaxKind.LiteralExpression:
                    return this.BindLiteralExpression( (LiteralExpressionSyntax)syntax );
                case SyntaxKind.NameExpression:
                    return this.BindNameExpression( (NameExpressionSyntax)syntax );
                case SyntaxKind.AssignmentExpression:
                    return this.BindAssignmentExpression( (AssignmentExpressionSyntax)syntax );
                case SyntaxKind.UnaryExpression:
                    return this.BindUnaryExpression( (UnaryExpressionSyntax)syntax );
                case SyntaxKind.BinaryExpression:
                    return this.BindBinaryExpression( (BinaryExpressionSyntax)syntax );
                case SyntaxKind.CallExpression:
                    return this.BindCallExpression( (CallExpressionSyntax)syntax );
                default:
                    throw new Exception( $"Unexpected syntax {syntax.Kind}" );
            }
        }

        private BoundExpression BindParenthesizedExpression( ParenthesizedExpressionSyntax syntax )
        {
            return this.BindExpression( syntax.Expression );
        }

        private BoundExpression BindLiteralExpression( LiteralExpressionSyntax syntax )
        {
            var value = default( object );

            switch( syntax.LiteralToken.TkType )
            {
                case TokenType.NumericLiteral:
                    if( !int.TryParse( syntax.LiteralToken.Text , out var intVal ) )
                    {
                        this.DiagBag.ReportExpressionInvalidNumericLiteral( syntax.Span , syntax.LiteralToken.Text! );

                        return new BoundErrorExpression();
                    }

                    value = intVal;

                    break;
                case TokenType.StringLiteral:
                    value = syntax.LiteralToken.Text;
                    break;
            }

            if( value == null )
            {
                this.DiagBag.ReportExpressionInvalidLiteral( syntax.Span );

                return new BoundErrorExpression();
            }

            return new BoundLiteralExpression( value );
        }

        private BoundExpression BindNameExpression( NameExpressionSyntax syntax )
        {
            var name = syntax.IdentifierToken.Text!;

            if( syntax.IdentifierToken.IsMissing )
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundErrorExpression();
            }

            if( !this.Scope!.TryLookupVariable( name , out var variable ) )
            {
                this.DiagBag.ReportUndefinedName( syntax.IdentifierToken.Span , name );

                return new BoundErrorExpression();
            }

            return new BoundVariableExpression( variable! );
        }

        private BoundExpression BindAssignmentExpression( AssignmentExpressionSyntax syntax )
        {
            var name = syntax.IdentifierToken.Text!;
            var boundExpression = this.BindExpression(syntax.Expression);

            if( !this.Scope!.TryLookupVariable( name , out var variable ) )
            {
                this.DiagBag.ReportUndefinedName( syntax.IdentifierToken.Span , name );

                return boundExpression;
            }

            if( variable!.IsReadOnly )
                this.DiagBag.ReportCannotAssign( syntax.EqualsToken.Span , name );

            var convertedExpression = this.BindConversion(syntax.Expression.Span, boundExpression, variable.Type);

            return new BoundAssignmentExpression( variable , convertedExpression );
        }

        private BoundExpression BindUnaryExpression( UnaryExpressionSyntax syntax )
        {
            var boundOperand = this.BindExpression(syntax.Operand);

            if( boundOperand.Type == TypeSymbol.Error )
                return new BoundErrorExpression();

            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.TkType , boundOperand.Type);

            if( boundOperator == null )
            {
                this.DiagBag.ReportUndefinedUnaryOperator( syntax.OperatorToken.Span , syntax.OperatorToken.Text! , boundOperand.Type );

                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression( boundOperator , boundOperand );
        }

        private BoundExpression BindBinaryExpression( BinaryExpressionSyntax syntax )
        {
            var boundLeft = this.BindExpression(syntax.Left);
            var boundRight = this.BindExpression(syntax.Right);

            if( boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error )
                return new BoundErrorExpression();

            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.TkType, boundLeft.Type, boundRight.Type);

            if( boundOperator == null )
            {
                this.DiagBag.ReportUndefinedBinaryOperator( syntax.OperatorToken.Span , syntax.OperatorToken.Text! , boundLeft.Type , boundRight.Type );
                return new BoundErrorExpression();
            }

            return new BoundBinaryExpression( boundLeft , boundOperator , boundRight );
        }

        private BoundExpression BindCallExpression( CallExpressionSyntax syntax )
        {
            var identifierText = syntax.Identifier.Text!;

            if( syntax.Arguments.Count == 1 && this.LookupType( identifierText ) is TypeSymbol type )
                return this.BindConversion( syntax.Arguments[ 0 ] , type , allowExplicit: true );

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach( var argument in syntax.Arguments )
            {
                var boundArgument = this.BindExpression(argument);

                boundArguments.Add( boundArgument );
            }

            if( !this.Scope!.TryLookupFunction( identifierText , out var function ) )
            {
                this.DiagBag.ReportUndefinedFunction( syntax.Identifier.Span , identifierText );

                return new BoundErrorExpression();
            }

            if( syntax.Arguments.Count != function!.Parameters.Length )
            {
                TextSpan span;

                if( syntax.Arguments.Count > function.Parameters.Length )
                {
                    SyntaxNode firstExceedingNode;

                    if( function.Parameters.Length > 0 )
                        firstExceedingNode = syntax.Arguments.GetSeparator( function.Parameters.Length - 1 )!;
                    else
                        firstExceedingNode = syntax.Arguments[ 0 ];

                    var lastExceedingArgument = syntax.Arguments[syntax.Arguments.Count - 1];

                    span = TextSpan.FromBounds( firstExceedingNode.Span.Start , lastExceedingArgument.Span.End );
                }
                else
                {
                    span = syntax.CloseParenthesisToken.Span;
                }

                this.DiagBag.ReportWrongArgumentCount( span , function.Name , function.Parameters.Length , syntax.Arguments.Count );

                return new BoundErrorExpression();
            }

            var hasErrors = false;

            for( var i = 0 ; i < syntax.Arguments.Count ; i++ )
            {
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];

                if( argument.Type != parameter.Type )
                {
                    if( argument.Type != TypeSymbol.Error )
                        this.DiagBag.ReportWrongArgumentType( syntax.Arguments[ i ].Span , parameter.Name , parameter.Type , argument.Type );

                    hasErrors = true;
                }
            }

            if( hasErrors )
                return new BoundErrorExpression();

            return new BoundCallExpression( function , boundArguments.ToImmutable() );
        }

        private BoundExpression BindConversion( ExpressionSyntax syntax , TypeSymbol type , bool allowExplicit = false )
        {
            var expression = this.BindExpression(syntax);

            return this.BindConversion( syntax.Span , expression , type , allowExplicit );
        }

        private BoundExpression BindConversion( TextSpan diagnosticSpan , BoundExpression expression , TypeSymbol type , bool allowExplicit = false )
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if( !conversion.Exists )
            {
                if( expression.Type != TypeSymbol.Error && type != TypeSymbol.Error )
                    this.DiagBag.ReportCannotConvert( diagnosticSpan , expression.Type , type );

                return new BoundErrorExpression();
            }

            if( !allowExplicit && conversion.IsExplicit )
            {
                this.DiagBag.ReportCannotConvertImplicit( diagnosticSpan , expression.Type , type );
            }

            if( conversion.IsIdentity )
                return expression;

            return new BoundConversionExpression( type , expression );
        }

        private VariableSymbol BindVariable( SyntaxToken identifier , bool isReadOnly , TypeSymbol type )
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = this.Function == null
                                ? (VariableSymbol) new GlobalVariableSymbol(name, isReadOnly, type)
                                : new LocalVariableSymbol(name, isReadOnly, type);

            if( declare && !this.Scope!.TryDeclareVariable( variable ) )
                this.DiagBag.ReportSymbolAlreadyDeclared( identifier.Span , name );

            return variable;
        }

        private TypeSymbol? LookupType( string name )
        {
            switch( name )
            {
                case "bool":
                    return TypeSymbol.Bool;
                case "int":
                    return TypeSymbol.Int;
                case "string":
                    return TypeSymbol.String;
                default:
                    return null;
            }
        }
    }
}
