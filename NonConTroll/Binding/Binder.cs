using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        private readonly Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> LoopStack = new Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)>();
        private bool IsScript;
        private int LabelCounter;
        private BoundScope? Scope;

        public Binder( bool isScript , BoundScope parent , FunctionSymbol? function )
        {
            this.IsScript = isScript;
            this.Scope    = new BoundScope( parent );
            this.Function = function;

            if( function != null )
            {
                foreach( var p in function.Parameters )
                {
                    _ = this.Scope.TryDeclareVariable( p );
                }
            }
        }

        public static BoundGlobalScope BindGlobalScope( bool isScript , BoundGlobalScope? previous , ImmutableArray<SyntaxTree> syntaxTrees )
        {
            var parentScope = CreateParentScope( previous );
            var binder = new Binder( isScript , parentScope , function: null );

            foreach( var function in syntaxTrees.SelectMany( x => x.Root.Members ).OfType<FunctionDeclarationSyntax>() )
            {
                binder.BindFunctionDeclaration( function );
            }

            var statements = syntaxTrees
                .SelectMany( x => x.Root.Members )
                .OfType<GlobalStatementSyntax>()
                .Select( x => binder.BindStatement( x.Statement ) )
                .ToImmutableArray();
            var functions = binder.Scope!.GetDeclaredFunctions();
            var variables = binder.Scope!.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if( previous != null )
            {
                diagnostics = diagnostics.InsertRange( 0 , previous.Diagnostics );
            }

            return new BoundGlobalScope( previous , diagnostics , functions , variables , statements );
        }

        public static BoundProgram BindProgram( bool isScript , BoundProgram? previous , BoundGlobalScope globalScope )
        {
            var parentScope    = CreateParentScope( globalScope );
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics    = ImmutableArray.CreateBuilder<Diagnostic>();

            foreach( var function in globalScope.Functions )
            {
                var binder      = new Binder( isScript , parentScope , function );
                var body        = binder.BindGlobalStatement( function.Declaration!.Body );
                var loweredBody = Lowerer.Lower( body );

                if( function.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn( loweredBody ) )
                {
                    binder.DiagBag.ReportAllPathsMustReturn( function.Declaration.Identifier.Location );
                }

                functionBodies.Add( function , loweredBody );
                diagnostics.AddRange( binder.Diagnostics );
            }

            var statement = Lowerer.Lower( new BoundBlockStatement( globalScope.Statements ) );

            return new BoundProgram( previous , diagnostics.ToImmutable() , functionBodies.ToImmutable() , statement );
        }

        private void BindFunctionDeclaration( FunctionDeclarationSyntax syntax )
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string>();

            foreach( var parameterSyntax in syntax.Parameters )
            {
                var parameterName = parameterSyntax.Identifier.Text!;
                var parameterType = this.BindTypeClause( parameterSyntax.Type )!;

                if( !seenParameterNames.Add( parameterName ) )
                {
                    this.DiagBag.ReportParameterAlreadyDeclared( parameterSyntax.Location , parameterName );
                }
                else
                {
                    parameters.Add( new ParameterSymbol( parameterName , parameterType ) );
                }
            }

            var returnType = this.BindTypeClause( syntax.ReturnType ) ?? TypeSymbol.Void;
            var function   = new FunctionSymbol( syntax.Identifier.Text! , parameters.ToImmutable() , returnType , syntax );

            if( function.Declaration!.Identifier.Text != null &&
                !this.Scope!.TryDeclareFunction( function ) )
            {
                this.DiagBag.ReportSymbolAlreadyDeclared( syntax.Identifier.Location , function.Name );
            }
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

                var scope = new BoundScope( parent );

                foreach( var f in previous.Functions )
                {
                    _ = scope.TryDeclareFunction( f );
                }

                foreach( var v in previous.Variables )
                {
                    _ = scope.TryDeclareVariable( v );
                }

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope( null );

            foreach( var f in BuiltinFunctions.GetAll() )
            {
                _ = result.TryDeclareFunction( f );
            }

            return result;
        }

        private BoundStatement BindErrorStatement()
        {
            return new BoundExpressionStatement( new BoundErrorExpression() );
        }

        private BoundStatement BindStatement( StatementSyntax syntax , bool isGlobal = false )
        {
            var result = this.BindStatementInternal( syntax );

            if( !this.IsScript || !isGlobal )
            {
                if( result is BoundExpressionStatement exprStmt )
                {
                    var isAllowedExpression =
                        exprStmt.Expression.Kind == BoundNodeKind.ErrorExpression ||
                        exprStmt.Expression.Kind == BoundNodeKind.AssignmentExpression ||
                        exprStmt.Expression.Kind == BoundNodeKind.CallExpression;

                    // TODO: incement, decrement, await, new object

                    if( !isAllowedExpression )
                    {
                        this.Diagnostics.ReportInvalidExpressionStatement( syntax.Location );
                    }
                }
            }

            return result;
        }

        private BoundStatement BindGlobalStatement( StatementSyntax syntax )
            => this.BindStatement( syntax , true );

        private BoundStatement BindStatementInternal( StatementSyntax syntax )
        {
            switch( syntax )
            {
                case BlockStatementSyntax s:      return this.BindBlockStatement( s );
                case VariableDeclarationSyntax s: return this.BindVariableDeclaration( s );
                case IfStatementSyntax s:         return this.BindIfStatement( s );
                case WhileStatementSyntax s:      return this.BindWhileStatement( s );
                case DoWhileStatementSyntax s:    return this.BindDoWhileStatement( s );
                case ForStatementSyntax s:        return this.BindForStatement( s );
                case BreakStatementSyntax s:      return this.BindBreakStatement( s );
                case ContinueStatementSyntax s:   return this.BindContinueStatement( s );
                case ReturnStatementSyntax s:     return this.BindReturnStatement( s );
                case ExpressionStatementSyntax s: return this.BindExpressionStatement( s );
                case DeferStatementSyntax s:      return this.BindDeferStatement( s );
                case MatchStatementSyntax s:      return this.BindMatchStatement( s );
                default:
                    throw new Exception( $"Unexpected syntax {syntax.Kind}" );
            }
        }

        private BoundStatement BindDeferStatement( DeferStatementSyntax syntax )
        {
            var expr = this.BindExpression( syntax.Expression , TypeSymbol.Void );

            return new BoundDeferStatement( expr );
        }

        private BoundStatement BindBlockStatement( BlockStatementSyntax syntax )
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            this.Scope = new BoundScope( this.Scope );

            foreach( var statementSyntax in syntax.Statements )
            {
                statements.Add( this.BindGlobalStatement( statementSyntax ) );
            }

            this.Scope = this.Scope.Parent;

            return new BoundBlockStatement( statements.ToImmutable() );
        }

        private BoundStatement BindVariableDeclaration( VariableDeclarationSyntax syntax )
        {
            var isReadOnly           = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var type                 = this.BindTypeClause( syntax.TypeClause );
            var initializer          = this.BindExpression( syntax.Initializer );
            var variableType         = type ?? initializer.Type;
            var variable             = this.BindVariableDeclaration( syntax.Identifier , isReadOnly , variableType );
            var convertedInitializer = this.BindConversion( syntax.Initializer.Location , initializer , variableType );

            return new BoundVariableDeclaration( variable , convertedInitializer );
        }

        private VariableSymbol BindVariableDeclaration( SyntaxToken identifier , bool isReadOnly , TypeSymbol type )
        {
            var name = identifier.Text ?? "?";
            var variable = this.Function == null
                                ? (VariableSymbol)new GlobalVariableSymbol( name , isReadOnly , type )
                                : new LocalVariableSymbol( name , isReadOnly , type );

            if( !identifier.IsMissing && !this.Scope!.TryDeclareVariable( variable ) )
            {
                this.Diagnostics.ReportSymbolAlreadyDeclared( identifier.Location , name );
            }

            return variable;
        }

        private TypeSymbol? BindTypeClause( TypeClauseSyntax? syntax )
        {
            // TODO: how to handle inferred types with no identifier?

            if( syntax == null )
            {
                return null;
            }

            var identifier = syntax.TypeName.Identifier!;
            var type = this.LookupType( identifier.Text! );

            if( type == null )
            {
                this.DiagBag.ReportUndefinedType( identifier.Location , identifier.Text! );
            }

            return type;
        }

        private BoundStatement BindIfStatement( IfStatementSyntax syntax )
        {
            var condition     = this.BindExpression( syntax.Condition , TypeSymbol.Bool );
            var thenStatement = this.BindGlobalStatement( syntax.ThenStatement );
            var elseStatement = syntax.ElseClause == null ? null : this.BindGlobalStatement( syntax.ElseClause.ElseStatement );

            return new BoundIfStatement( condition , thenStatement , elseStatement );
        }

        private BoundStatement BindWhileStatement( WhileStatementSyntax syntax )
        {
            var condition = this.BindExpression( syntax.Condition , TypeSymbol.Bool );
            var body      = this.BindLoopBody( syntax.Body , out var breakLabel , out var continueLabel );

            return new BoundWhileStatement( condition , body , breakLabel , continueLabel );
        }

        private BoundStatement BindDoWhileStatement( DoWhileStatementSyntax syntax )
        {
            var body      = this.BindLoopBody( syntax.Body , out var breakLabel , out var continueLabel );
            var condition = this.BindExpression( syntax.Condition , TypeSymbol.Bool );

            return new BoundDoWhileStatement( body , condition , breakLabel , continueLabel );
        }

        private BoundStatement BindForStatement( ForStatementSyntax syntax )
        {
            var lowerBound = this.BindExpression( syntax.LowerBound , TypeSymbol.Int );
            var upperBound = this.BindExpression( syntax.UpperBound , TypeSymbol.Int );

            this.Scope = new BoundScope( this.Scope );

            var variable = this.BindVariableDeclaration( syntax.Identifier , isReadOnly: true , TypeSymbol.Int );
            var body     = this.BindLoopBody( syntax.Body , out var breakLabel , out var continueLabel );

            this.Scope = this.Scope.Parent;

            return new BoundForStatement( variable , lowerBound , upperBound , body , breakLabel , continueLabel );
        }

        private BoundStatement BindLoopBody( StatementSyntax body , out BoundLabel breakLabel , out BoundLabel continueLabel )
        {
            this.LabelCounter++;

            breakLabel    = new BoundLabel( $"break{this.LabelCounter}" );
            continueLabel = new BoundLabel( $"continue{this.LabelCounter}" );

            this.LoopStack.Push( (breakLabel , continueLabel) );

            var boundBody = this.BindGlobalStatement( body );

            _ = this.LoopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindBreakStatement( BreakStatementSyntax syntax )
        {
            if( this.LoopStack.Count == 0 )
            {
                this.DiagBag.ReportInvalidBreakOrContinue( syntax.Keyword.Location , syntax.Keyword.Text! );

                return this.BindErrorStatement();
            }

            return new BoundGotoStatement( this.LoopStack.Peek().BreakLabel );
        }

        private BoundStatement BindContinueStatement( ContinueStatementSyntax syntax )
        {
            if( this.LoopStack.Count == 0 )
            {
                this.DiagBag.ReportInvalidBreakOrContinue( syntax.Keyword.Location , syntax.Keyword.Text! );

                return this.BindErrorStatement();
            }

            return new BoundGotoStatement( this.LoopStack.Peek().ContinueLabel );
        }

        private BoundStatement BindReturnStatement( ReturnStatementSyntax syntax )
        {
            var expression = syntax.Expression == null ? null : this.BindExpression( syntax.Expression );

            if( this.Function == null )
            {
                this.DiagBag.ReportInvalidReturn( syntax.ReturnKeyword.Location );
            }
            else
            {
                if( this.Function.ReturnType == TypeSymbol.Void )
                {
                    if( expression != null )
                    {
                        this.DiagBag.ReportInvalidReturnExpression( syntax.Expression!.Location , this.Function.Name );
                    }
                }
                else
                {
                    if( expression == null )
                    {
                        this.DiagBag.ReportMissingReturnExpression( syntax.ReturnKeyword.Location , this.Function.ReturnType );
                    }
                    else
                    {
                        expression = this.BindConversion( syntax.Expression!.Location , expression , this.Function.ReturnType );
                    }
                }
            }

            return new BoundReturnStatement( expression );
        }

        private BoundStatement BindExpressionStatement( ExpressionStatementSyntax syntax )
        {
            var expression = this.BindExpression( syntax.Expression , canBeVoid: true );

            return new BoundExpressionStatement( expression );
        }

        private BoundExpression BindExpression( ExpressionSyntax syntax , TypeSymbol targetType )
        {
            return this.BindConversion( syntax , targetType );
        }

        private BoundExpression BindExpression( ExpressionSyntax syntax , bool canBeVoid = false )
        {
            var result = this.BindExpressionInternal( syntax );

            if( !canBeVoid && result.Type == TypeSymbol.Void )
            {
                this.DiagBag.ReportExpressionMustHaveValue( syntax.Location );

                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionInternal( ExpressionSyntax syntax )
        {
            switch( syntax )
            {
                case ParenthesizedExpressionSyntax s: return this.BindParenthesizedExpression( s );
                case LiteralExpressionSyntax s:       return this.BindLiteralExpression( s );
                case NameExpressionSyntax s:          return this.BindNameExpression( s );
                case AssignmentExpressionSyntax s:    return this.BindAssignmentExpression( s );
                case UnaryExpressionSyntax s:         return this.BindUnaryExpression( s );
                case BinaryExpressionSyntax s:        return this.BindBinaryExpression( s );
                case CallExpressionSyntax s:          return this.BindInvokationExpression( s );
                case MatchExpressionSyntax s:         return this.BindMatchExpression( s );
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

            switch( syntax.LiteralToken.Kind )
            {
                case SyntaxKind.NumericLiteral:
                {
                    if( !int.TryParse( syntax.LiteralToken.Text , out var val ) )
                    {
                        this.DiagBag.ReportExpressionInvalidNumericLiteral( syntax.Location , syntax.LiteralToken.Text! );

                        return new BoundErrorExpression();
                    }

                    value = val;
                }
                break;
                case SyntaxKind.TrueKeywordLiteral:
                {
                    value = true;
                }
                break;
                case SyntaxKind.FalseKeywordLiteral:
                {
                    value = false;
                }
                break;
                case SyntaxKind.StringLiteral:
                {
                    value = syntax.LiteralToken.Text;
                }
                break;
            }

            if( value == null )
            {
                this.DiagBag.ReportExpressionInvalidLiteral( syntax.Location );

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

            var variable = this.BindVariableReference( syntax.IdentifierToken );

            if( variable == null )
            {
                return new BoundErrorExpression();
            }

            return new BoundVariableExpression( variable );
        }

        private BoundExpression BindAssignmentExpression( AssignmentExpressionSyntax syntax )
        {
            var name = syntax.IdentifierToken.Text!;
            var boundExpression = this.BindExpression( syntax.Expression );
            var variable = this.BindVariableReference( syntax.IdentifierToken );

            if( variable == null )
            {
                return boundExpression;
            }

            if( variable!.IsReadOnly )
            {
                this.DiagBag.ReportCannotAssign( syntax.EqualsToken.Location , name );
            }

            var convertedExpression = this.BindConversion( syntax.Expression.Location , boundExpression , variable.Type );

            return new BoundAssignmentExpression( variable , convertedExpression );
        }

        private BoundExpression BindUnaryExpression( UnaryExpressionSyntax syntax )
        {
            // TODO
            // if( syntax.OperatorToken.TkType == TokenType.Identifier )
            // {
            //     var infixSyntax = new InfixUnaryExpressionSyntax( syntax.SyntaxTree , syntax.OperatorToken , syntax.Expression );
            //
            //     return this.BindInvokationExpression( infixSyntax );
            // }

            var boundOperand = this.BindExpression( syntax.Expression );

            if( boundOperand.Type == TypeSymbol.Error )
            {
                return new BoundErrorExpression();
            }

            var boundOperator = BoundUnaryOperator.Bind( syntax.OperatorToken.Kind , boundOperand.Type );

            if( boundOperator == null )
            {
                this.DiagBag.ReportUndefinedUnaryOperator( syntax.OperatorToken.Location , syntax.OperatorToken.Text! , boundOperand.Type );

                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression( boundOperator , boundOperand );
        }

        private BoundExpression BindBinaryExpression( BinaryExpressionSyntax syntax )
        {
            if( syntax.OperatorToken.Kind == SyntaxKind.Identifier )
            {
                var infixSyntax = new InfixBinaryExpressionSyntax( syntax.SyntaxTree , syntax.Lhs , syntax.OperatorToken , syntax.Rhs );

                return this.BindInvokationExpression( infixSyntax );
            }

            var boundLhs = this.BindExpression( syntax.Lhs );
            var boundRhs = this.BindExpression( syntax.Rhs );

            if( boundLhs.Type == TypeSymbol.Error ||
                boundRhs.Type == TypeSymbol.Error )
            {
                return new BoundErrorExpression();
            }

            var boundOperator = BoundBinaryOperator.Bind( syntax.OperatorToken.Kind , boundLhs.Type , boundRhs.Type );

            if( boundOperator == null )
            {
                this.DiagBag.ReportUndefinedBinaryOperator( syntax.OperatorToken.Location , syntax.OperatorToken.Text! , boundLhs.Type , boundRhs.Type );

                return new BoundErrorExpression();
            }

            return new BoundBinaryExpression( boundLhs , boundOperator , boundRhs );
        }

        private BoundExpression BindInvokationExpression( InvokationExpressionSyntax syntax )
        {
            switch( syntax )
            {
                case CallExpressionSyntax s : return this.BindCallExpression( s );

                default: throw new Exception();
            }
        }

        private BoundExpression BindCallExpression( CallExpressionSyntax syntax )
        {
            var identifierText = syntax.Identifier.Text!;

            // TODO: cast
            // right now: casting MyType( 1 )
            if( syntax.Arguments.Count() == 1 && this.LookupType( identifierText ) is TypeSymbol type )
            {
                return this.BindConversion( syntax.Arguments.First() , type , allowExplicit: true );
            }

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach( var argument in syntax.Arguments )
            {
                boundArguments.Add( this.BindExpression( argument ) );
            }

            var symbol = this.Scope!.TryLookupSymbol( syntax.Identifier.Text! );

            if( symbol == null )
            {
                this.DiagBag.ReportUndefinedFunction( syntax.Identifier.Location , identifierText );

                return new BoundErrorExpression();
            }

            var function = symbol as FunctionSymbol;

            if( function == null )
            {
                this.DiagBag.ReportNotAFunction( syntax.Identifier.Location , identifierText );

                return new BoundErrorExpression();
            }

            if( syntax is CallExpressionSyntax callSyntax )
            {
                var args =  callSyntax.Arguments;

                if( args.Count != function.Parameters.Length )
                {
                    var span = default( TextSpan );

                    if( args.Count > function.Parameters.Length )
                    {
                        var firstExceedingNode = default( SyntaxNode );

                        if( function.Parameters.Any() )
                        {
                            firstExceedingNode = args.GetSeparator( function.Parameters.Length - 1 )!;
                        }
                        else
                        {
                            firstExceedingNode = args.First();
                        }

                        var lastExceedingArgument = syntax.Arguments.Last();

                        span = TextSpan.FromBounds( firstExceedingNode.Span.Start , lastExceedingArgument.Span.End );
                    }
                    else
                    {
                        span = callSyntax.CloseParenthesisToken.Span;
                    }

                    var location = new TextLocation( syntax.SyntaxTree.Text , span );

                    this.DiagBag.ReportWrongArgumentCount( location , function.Name , function.Parameters.Length , syntax.Arguments.Count );

                    return new BoundErrorExpression();
                }
            }
            // else if( syntax is InfixBinaryExpressionSyntax infixBinarySyntax )
            // {
            //     var args =  infixBinarySyntax.Arguments;

            //     // TODO: check only 2 arguments allowed for now
            //     // TODO: maybe infix for unary operators too?
            //     // var x = not 1;

            //     if( args.Count != function.Parameters.Length )
            //     {
            //         var location = new TextLocation( syntax.SyntaxTree.Text , syntax.Span );

            //         this.DiagBag.ReportWrongArgumentCount( location , function.Name , function.Parameters.Length , syntax.Arguments.Count );

            //         return new BoundErrorExpression();
            //     }
            // }
            // else if( syntax is InfixUnaryExpressionSyntax infixUnarySyntax )
            // {
            //     var args =  infixUnarySyntax.Arguments;

            //     if( args.Count != function.Parameters.Length )
            //     {
            //         var location = new TextLocation( syntax.SyntaxTree.Text , syntax.Span );

            //         this.DiagBag.ReportWrongArgumentCount( location , function.Name , function.Parameters.Length , syntax.Arguments.Count );

            //         return new BoundErrorExpression();
            //     }
            // }

            var hasErrors = false;

            for( var i = 0 ; i < syntax.Arguments.Count ; i++ )
            {
                var argument = boundArguments[ i ];
                var parameter = function.Parameters[ i ];

                if( argument.Type != parameter.Type )
                {
                    if( argument.Type != TypeSymbol.Error )
                    {
                        this.DiagBag.ReportWrongArgumentType( syntax.Arguments[ i ].Location , function , parameter.Name , parameter.Type , argument.Type );
                    }

                    hasErrors = true;
                }
            }

            if( hasErrors )
                return new BoundErrorExpression();

            return new BoundCallExpression( function , boundArguments.ToImmutable() );
        }

        private bool ValidatePattern( ImmutableArray<PatternSyntax> patterns )
        {
            var hasErrors = false;
            var foundMatchAnyCase = false;

            foreach( var pattern in patterns )
            {
                switch( pattern )
                {
                    case MatchAnyPatternSyntax p:
                    {
                        if( foundMatchAnyCase )
                        {
                            hasErrors = true;

                            this.DiagBag.ReportMultipleMatchAnyPattern( p.Location );
                        }

                        foundMatchAnyCase = true;
                    }
                    break;
                }
            }

            return !hasErrors;
        }

        private BoundExpression BindMatchExpression( MatchExpressionSyntax syntax )
        {
            var matchExpr = this.BindExpression( syntax.Expression );
            var boundPatternSections = this.BindPatternSectionExpressions( syntax.PatternSections );

            _ = ValidatePattern( syntax.PatternSections.SelectMany( x => x.Patterns ).ToImmutableArray() );

            return new BoundMatchExpression( matchExpr , boundPatternSections.ToImmutableArray() );
        }

        private BoundStatement BindMatchStatement( MatchStatementSyntax syntax )
        {
            var matchExpr = this.BindExpression( syntax.Expression );
            var boundPatternSections = this.BindPatternSectionExpressions( syntax.PatternSections );

            _ = ValidatePattern( syntax.PatternSections.SelectMany( x => x.Patterns ).ToImmutableArray() );

            return new BoundMatchStatement( matchExpr , boundPatternSections.ToImmutableArray() );
        }

        private ImmutableArray<BoundPatternSectionExpression> BindPatternSectionExpressions( ImmutableArray<PatternSectionExpressionSyntax> sections )
        {
            var builder = ImmutableArray.CreateBuilder<BoundPatternSectionExpression>( sections.Count() );

            foreach( var section in sections )
            {
                var boundSection = this.BindPatternSectionExpression( section );

                builder.Add( boundSection );
            }

            return builder.MoveToImmutable();
        }

        private ImmutableArray<BoundPatternSectionStatement> BindPatternSectionExpressions( ImmutableArray<PatternSectionStatementSyntax> sections )
        {
            var builder = ImmutableArray.CreateBuilder<BoundPatternSectionStatement>( sections.Count() );

            foreach( var section in sections )
            {
                var boundSection = this.BindPatternSectionStatement( section );

                builder.Add( boundSection );
            }

            return builder.MoveToImmutable();
        }

        private BoundPatternSectionExpression BindPatternSectionExpression( PatternSectionExpressionSyntax syntax )
        {
            var result = this.BindExpression( syntax.Expression , canBeVoid: false );
            var patterns = this.BindPatterns( syntax.Patterns );

            return new BoundPatternSectionExpression( patterns , result );
        }

        private BoundPatternSectionStatement BindPatternSectionStatement( PatternSectionStatementSyntax syntax )
        {
            var result = this.BindStatement( syntax.Statement );
            var patterns = this.BindPatterns( syntax.Patterns );

            return new BoundPatternSectionStatement( patterns , result );
        }

        private ImmutableArray<BoundPattern> BindPatterns( IEnumerable<PatternSyntax> patterns )
        {
            var boundPatternList = new List<BoundPattern>( patterns.Count() );

            foreach( var patternSyntax in patterns )
            {
                var boundPattern = this.BindPattern( patternSyntax );

                boundPatternList.Add( boundPattern );
            }

            return boundPatternList.ToImmutableArray();
        }

        private BoundPattern BindPattern( PatternSyntax patternSyntax )
        {
            switch( patternSyntax )
            {
                case ConstantPatternSyntax s:
                {
                    var patternExpr  = this.BindExpression( s.Expression );
                    var boundPattern = new BoundConstantPattern( patternExpr );

                    return boundPattern;
                }
                case MatchAnyPatternSyntax s:
                {
                    var boundPattern = new BoundMatchAnyPattern();

                    return boundPattern;
                }
                case InfixPatternSyntax s:
                {
                    throw new NotImplementedException();
                }

                default:
                {
                    throw new Exception( "" );
                }
            }
        }

        private BoundExpression BindConversion( ExpressionSyntax syntax , TypeSymbol type , bool allowExplicit = false )
        {
            var expression = this.BindExpression( syntax );

            return this.BindConversion( syntax.Location , expression , type , allowExplicit );
        }

        private BoundExpression BindConversion( TextLocation location , BoundExpression expression , TypeSymbol type , bool allowExplicit = false )
        {
            var conversion = Conversion.Classify( expression.Type , type );

            if( !conversion.Exists )
            {
                if( expression.Type != TypeSymbol.Error &&
                    type != TypeSymbol.Error )
                {
                    this.DiagBag.ReportCannotConvert( location , expression.Type , type );
                }

                return new BoundErrorExpression();
            }

            if( !allowExplicit && conversion.IsExplicit )
            {
                this.DiagBag.ReportCannotConvertImplicit( location , expression.Type , type );
            }

            if( conversion.IsIdentity )
            {
                return expression;
            }

            return new BoundConversionExpression( type , expression );
        }

        private VariableSymbol BindVariable( SyntaxToken identifier , bool isReadOnly , TypeSymbol type )
        {
            var name     = identifier.Text ?? "?";
            var declare  = !identifier.IsMissing;
            var variable = this.Function == null
                ? (VariableSymbol)new GlobalVariableSymbol( name , isReadOnly , type )
                : (VariableSymbol)new LocalVariableSymbol( name , isReadOnly , type );

            if( declare && !this.Scope!.TryDeclareVariable( variable ) )
            {
                this.DiagBag.ReportSymbolAlreadyDeclared( identifier.Location , name );
            }

            return variable;
        }

        private VariableSymbol? BindVariableReference( SyntaxToken identifierToken )
        {
            var name = identifierToken.Text!;

            switch( this.Scope!.TryLookupSymbol( name ) )
            {
                case VariableSymbol variable:
                    return variable;
                case null:
                    this.Diagnostics.ReportUndefinedVariable( identifierToken.Location , name );
                    return null;
                default:
                    this.Diagnostics.ReportNotAVariable( identifierToken.Location , name );
                    return null;
            }
        }

        private TypeSymbol? LookupType( string name )
        {
            switch( name )
            {
                case "bool":   return TypeSymbol.Bool;
                case "int":    return TypeSymbol.Int;
                case "string": return TypeSymbol.String;
                default:       return null;
            }
        }
    }
}
