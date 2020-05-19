using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using NonConTroll.CodeAnalysis.Text;


namespace NonConTroll.CodeAnalysis.Syntax
{
    internal sealed partial class Parser
    {
        public readonly DiagnosticBag Diagnostics = new DiagnosticBag();

        private readonly SyntaxTree SyntaxTree;
        private SourceText Text => this.SyntaxTree.Text;
        private readonly ImmutableArray<SyntaxToken> Tokens;

        private int Position;

        public Parser( SyntaxTree syntaxTree )
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer( syntaxTree );
            var token = default( SyntaxToken );

            do
            {
                token = lexer.Lex();

                if( token.Kind != SyntaxKind.WhiteSpaceTrivia &&
                    !token.Kind.ToString().EndsWith( "Trivia" ) &&
                    token.Kind != SyntaxKind.None )
                {
                    tokens.Add( token );
                }
            }
            while( token.Kind != SyntaxKind.EndOfFile );

            this.SyntaxTree = syntaxTree;
            this.Tokens = tokens.ToImmutableArray();
            this.Diagnostics.AddRange( lexer.Diagnostics );
        }

        private SyntaxToken Current => this.Peek( 0 );

        private void Advance( int count = 1 )
            => this.Position += count;

        private SyntaxToken Peek( int offset = 1 )
            => this.Tokens.ElementAt( Math.Clamp( this.Position + offset , 0 , this.Tokens.Length - 1 ) );

        private SyntaxToken MatchToken( SyntaxKind tokenType )
        {
            if( this.Current.Kind == tokenType )
            {
                return this.NextToken();
            }

            this.Diagnostics.ReportUnexpectedToken( this.Current.Location , this.Current.Kind , tokenType );

            return this.CreateMissingToken( tokenType );
        }

        private SyntaxToken CreateMissingToken( SyntaxKind tokenType )
            => new SyntaxToken( this.SyntaxTree , tokenType , this.Current.Position , null );

        private SyntaxToken NextToken()
        {
            var current = this.Current;

            this.Advance();

            return current;
        }

        #region Parsing

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = this.ParseMembers();
            var endOfFileToken = this.MatchToken( SyntaxKind.EndOfFile );

            return new CompilationUnitSyntax( this.SyntaxTree , members , endOfFileToken );
        }

        private ImmutableArray<MemberDeclarationSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();

            while( this.Current.Kind != SyntaxKind.EndOfFile )
            {
                var startToken = this.Current;

                members.Add( this.ParseMember() );

                // If ParseMember() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if( this.Current == startToken )
                {
                    _ = this.NextToken();
                }
            }

            return members.ToImmutable();
        }

        private MemberDeclarationSyntax ParseMember()
        {
            if( this.Current.Kind == SyntaxKind.FuncKeyword )
            {
                return this.ParseFunctionDeclaration();
            }

            return this.ParseGlobalStatement();
        }

        private MemberDeclarationSyntax ParseFunctionDeclaration()
        {
            var functionKeyword = this.MatchToken( SyntaxKind.FuncKeyword );
            var identifier      = this.MatchToken( SyntaxKind.Identifier );
            var openParenToken  = this.MatchToken( SyntaxKind.OpenParenToken );
            var parameters      = this.ParseParameterList();
            var closeParenToken = this.MatchToken( SyntaxKind.CloseParenToken );
            var type            = this.ParseOptionalTypeClause();
            var body            = this.ParseBlockStatement();

            return new FunctionDeclarationSyntax( this.SyntaxTree , functionKeyword , identifier , openParenToken , parameters , closeParenToken , type , body );
        }


        private SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
            => this.ParseSeparatedList( this.ParseParameter );

        private SeparatedSyntaxList<T> ParseSeparatedList<T>(
                Func<T> parseParamFunc ,
                SyntaxKind openListToken = SyntaxKind.OpenParenToken ,
                SyntaxKind seperatorToken = SyntaxKind.CommaToken ,
                SyntaxKind closeListToken = SyntaxKind.CloseParenToken )
                where T : SyntaxNode
        {
            //Debug.Assert( this.Current.TkType == openListToken );

            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var parseNextParameter = true;

            while( parseNextParameter &&
                   this.Current.Kind != closeListToken &&
                   this.Current.Kind != SyntaxKind.EndOfFile )
            {
                var parameter = parseParamFunc();

                nodesAndSeparators.Add( parameter );

                if( this.Current.Kind == seperatorToken )
                {
                    nodesAndSeparators.Add( this.MatchToken( seperatorToken ) );
                }
                else
                {
                    parseNextParameter = false;
                }
            }

            //Debug.Assert( this.Current.TkType == closeListToken );

            return new SeparatedSyntaxList<T>( nodesAndSeparators.ToImmutable() );
        }

        private ParameterSyntax ParseParameter()
        {
            var identifier = this.MatchToken( SyntaxKind.Identifier );
            var type = this.ParseTypeClause();

            return new ParameterSyntax( this.SyntaxTree , identifier , type );
        }

        private MemberDeclarationSyntax ParseGlobalStatement()
        {
            var statement = this.ParseStatement();

            return new GlobalStatementSyntax( this.SyntaxTree , statement );
        }

        private StatementSyntax ParseStatement()
        {
            switch( this.Current.Kind )
            {
                case SyntaxKind.OpenBraceToken:  return this.ParseBlockStatement();
                case SyntaxKind.LetKeyword:      return this.ParseVariableDeclaration( SyntaxKind.LetKeyword );
                case SyntaxKind.VarKeyword:      return this.ParseVariableDeclaration( SyntaxKind.VarKeyword );
                case SyntaxKind.IfKeyword:       return this.ParseIfStatement();
                case SyntaxKind.WhileKeyword:    return this.ParseWhileStatement();
                case SyntaxKind.DoKeyword:       return this.ParseDoWhileStatement();
                case SyntaxKind.ForKeyword:      return this.ParseForStatement();
                case SyntaxKind.BreakKeyword:    return this.ParseBreakStatement();
                case SyntaxKind.ContinueKeyword: return this.ParseContinueStatement();
                case SyntaxKind.ReturnKeyword:   return this.ParseReturnStatement();
                case SyntaxKind.DeferKeyword:    return this.ParseDeferStatement();
                case SyntaxKind.MatchKeyword:    return this.ParseMatchStatement();

                case SyntaxKind.BooleanLiteral:
                case SyntaxKind.NumericLiteral:
                case SyntaxKind.StringLiteral:
                default:                  return this.ParseExpressionStatement();
            }
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = this.ParseExpression();

            return new ExpressionStatementSyntax( this.SyntaxTree , expression );
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            var openBraceToken = this.MatchToken( SyntaxKind.OpenBraceToken);

            while( this.Current.Kind != SyntaxKind.EndOfFile &&
                   this.Current.Kind != SyntaxKind.CloseBraceToken )
            {
                var startToken = this.Current;

                statements.Add( this.ParseStatement() );

                // If ParseStatement() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if( this.Current == startToken )
                {
                    _ = this.NextToken();
                }
            }

            var closeBraceToken = this.MatchToken( SyntaxKind.CloseBraceToken );

            return new BlockStatementSyntax( this.SyntaxTree , openBraceToken , statements.ToImmutable() , closeBraceToken );
        }

        private StatementSyntax ParseVariableDeclaration( SyntaxKind expectedTokenType )
        {
            var keyword     = this.MatchToken( expectedTokenType );
            var identifier  = this.MatchToken( SyntaxKind.Identifier );
            var typeClause  = this.ParseOptionalTypeClause();
            var equals      = this.MatchToken( SyntaxKind.EqToken );
            var initializer = this.ParseExpression();

            return new VariableDeclarationSyntax( this.SyntaxTree , keyword , identifier , typeClause , equals , initializer );
        }

        private TypeClauseSyntax? ParseOptionalTypeClause()
        {
            if( this.Current.Kind != SyntaxKind.ColonToken )
            {
                return null;
            }

            return this.ParseTypeClause();
        }

        private TypeNameSyntax ParseType()
        {
            if( this.Current.Kind == SyntaxKind.OpenParenToken )
            {
                // TODO
                //return this.ParseTupleType();
            }

            var typeIdentifier     = default( SyntaxToken? );
            var typeSpecifier      = ImmutableArray.CreateBuilder<TypeSpecifierSyntax>();
            var arrayTypeSpecifier = ImmutableArray.CreateBuilder<ArrayTypeSpecifierSyntax>();
            var done = false;

            while( !done )
            {
                switch( this.Current.Kind )
                {
                    case SyntaxKind.RefKeyword:
                    case SyntaxKind.PtrKeyword:
                    //case TokenType.Qm:
                    case SyntaxKind.NullKeywordLiteral:
                        typeSpecifier.Add( this.ParseTypeSpecifier() );
                        break;
                    case SyntaxKind.OpenBracketToken:
                        arrayTypeSpecifier.Add( this.ParseArrayTypeSpecifier() );
                        break;
                    default:
                        typeIdentifier = this.MatchToken( SyntaxKind.Identifier );
                        done = true;
                        break;
                }
            }

            return new TypeNameSyntax( this.SyntaxTree , typeIdentifier , typeSpecifier.ToImmutable() , arrayTypeSpecifier.ToImmutable() );
        }

        private TypeSpecifierSyntax ParseTypeSpecifier()
        {
            switch( this.Current.Kind )
            {
                case SyntaxKind.RefKeyword:         return new TypeSpecifierSyntax( this.SyntaxTree , this.MatchToken( SyntaxKind.RefKeyword )          , SyntaxKind.ReferenceTypeSpecifier );
                case SyntaxKind.PtrKeyword:         return new TypeSpecifierSyntax( this.SyntaxTree , this.MatchToken( SyntaxKind.PtrKeyword )          , SyntaxKind.PointerTypeSpecifier   );
                case SyntaxKind.NullKeywordLiteral: return new TypeSpecifierSyntax( this.SyntaxTree , this.MatchToken( SyntaxKind.NullKeywordLiteral )  , SyntaxKind.NullableTypeSpecifier  );
                //case TokenType.Qm:   return new TypeSpecifierSyntax( this.MatchToken( TokenType.Colon ) , TypeSpecifierKind.Nullable  );
                default:
                    break;
            }

            throw new Exception( "" );
        }

        private ArrayTypeSpecifierSyntax ParseArrayTypeSpecifier()
        {
            var openBracketToken  = this.MatchToken( SyntaxKind.OpenBracketToken );
            var rankExpression    = this.ParseExpression();
            var closeBracketToken = this.MatchToken( SyntaxKind.CloseBracketToken );

            return new ArrayTypeSpecifierSyntax( this.SyntaxTree , openBracketToken , rankExpression , closeBracketToken );
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colonToken = this.MatchToken( SyntaxKind.ColonToken );
            var type       = this.ParseType();

            return new TypeClauseSyntax( this.SyntaxTree , colonToken , type );
        }

        private StatementSyntax ParseIfStatement()
        {
            var keyword     = this.MatchToken( SyntaxKind.IfKeyword );
            var condition   = this.ParseExpression();
            var statement   = this.ParseStatement();
            var elseClause  = this.ParseElseClause();

            return new IfStatementSyntax( this.SyntaxTree , keyword , condition , statement , elseClause );
        }

        private ElseClauseSyntax? ParseElseClause()
        {
            if( this.Current.Kind != SyntaxKind.ElseKeyword )
            {
                return null;
            }

            var keyword   = this.NextToken();
            var statement = this.ParseStatement();

            return new ElseClauseSyntax( this.SyntaxTree , keyword , statement );
        }

        private StatementSyntax ParseWhileStatement()
        {
            var keyword     = this.MatchToken( SyntaxKind.WhileKeyword );
            var condition   = this.ParseExpression();
            var body        = this.ParseStatement();

            return new WhileStatementSyntax( this.SyntaxTree , keyword , condition , body );
        }

        private StatementSyntax ParseDoWhileStatement()
        {
            var doKeyword    = this.MatchToken( SyntaxKind.DoKeyword );
            var body         = this.ParseStatement();
            var whileKeyword = this.MatchToken( SyntaxKind.WhileKeyword );
            var condition    = this.ParseExpression();

            return new DoWhileStatementSyntax( this.SyntaxTree , doKeyword , body , whileKeyword , condition );
        }

        private StatementSyntax ParseForStatement()
        {
            var keyword     = this.MatchToken( SyntaxKind.ForKeyword );
            var identifier  = this.MatchToken( SyntaxKind.Identifier );
            var equalsToken = this.MatchToken( SyntaxKind.EqToken );
            var lowerBound  = this.ParseExpression();
            var toKeyword   = this.MatchToken( SyntaxKind.ToKeyword );
            var upperBound  = this.ParseExpression();
            var body        = this.ParseStatement();

            return new ForStatementSyntax( this.SyntaxTree , keyword , identifier , equalsToken , lowerBound , toKeyword , upperBound , body );
        }

        private StatementSyntax ParseBreakStatement()
        {
            var keyword = this.MatchToken( SyntaxKind.BreakKeyword );

            return new BreakStatementSyntax( this.SyntaxTree , keyword );
        }

        private StatementSyntax ParseContinueStatement()
        {
            var keyword = this.MatchToken( SyntaxKind.ContinueKeyword );

            return new ContinueStatementSyntax( this.SyntaxTree , keyword );
        }

        private StatementSyntax ParseReturnStatement()
        {
            var keyword     = this.MatchToken( SyntaxKind.ReturnKeyword );
            var keywordLine = this.Text.GetLineIndex( keyword.Span.Start );
            var currentLine = this.Text.GetLineIndex( this.Current.Span.Start );
            var isEof       = this.Current.Kind == SyntaxKind.EndOfFile;
            var sameLine    = !isEof && keywordLine == currentLine;
            var expression  = sameLine ? this.ParseExpression() : null;

            return new ReturnStatementSyntax( this.SyntaxTree , keyword , expression );
        }

        private StatementSyntax ParseDeferStatement()
        {
            var keyword     = this.MatchToken( SyntaxKind.DeferKeyword );
            var expression  = this.ParseExpression();

            return new DeferStatementSyntax( this.SyntaxTree , keyword , expression );
        }

        private ExpressionSyntax ParseExpression()
        {
            return this.ParseAssignmentExpression();
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if( this.Current.Kind == SyntaxKind.Identifier )
            {
                var peek = this.Peek();

                if( peek.Kind == SyntaxKind.EqToken )
                {
                    var identifierToken = this.NextToken();
                    var operatorToken   = this.NextToken();
                    var right           = this.ParseAssignmentExpression();

                    return new AssignmentExpressionSyntax( this.SyntaxTree , identifierToken , operatorToken , right );
                }
            }

            return this.ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression( int parentPrecedence = 0 )
        {
            var left = default( ExpressionSyntax );
            var unaryOperatorPrecedence = this.Current.Kind.GetUnaryOperatorPrecedence();

            if( unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence )
            {
                var operatorToken = this.NextToken();
                var operand       = this.ParseBinaryExpression( unaryOperatorPrecedence );

                left = new UnaryExpressionSyntax( this.SyntaxTree , operatorToken , operand );
            }
            else
            {
                left = this.ParsePrimaryExpression();
            }

            while( true )
            {
                var precedence = this.Current.Kind.GetBinaryOperatorPrecedence();

                if( precedence == 0 || precedence <= parentPrecedence )
                {
                    break;
                }

                var operatorToken = this.NextToken();
                var right         = this.ParseBinaryExpression( precedence );

                left = new BinaryExpressionSyntax( this.SyntaxTree , left , operatorToken , right );
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch( this.Current.Kind )
            {
                case SyntaxKind.OpenParenToken :      return this.ParseParenthesizedExpression();
                case SyntaxKind.FalseKeywordLiteral : return this.ParseBooleanLiteral( this.Current.Kind );
                case SyntaxKind.TrueKeywordLiteral :  return this.ParseBooleanLiteral( this.Current.Kind );
                case SyntaxKind.NumericLiteral :      return this.ParseNumberLiteral();
                case SyntaxKind.StringLiteral :       return this.ParseStringLiteral();
                case SyntaxKind.UnderscoreToken :     return this.ParseUnderscoreLiteral();
                case SyntaxKind.MatchKeyword :        return this.ParseMatchExpression();
                case SyntaxKind.Identifier :          return this.ParseNameOrCallExpression();

                default: return this.ParseNameOrCallExpression();
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var left       = this.MatchToken( SyntaxKind.OpenParenToken );
            var expression = this.ParseExpression();
            var right      = this.MatchToken( SyntaxKind.CloseParenToken );

            return new ParenthesizedExpressionSyntax( this.SyntaxTree , left , expression , right );
        }

        private ExpressionSyntax ParseBooleanLiteral( SyntaxKind expectedTokenType )
        {
            var keywordToken = this.MatchToken( expectedTokenType );

            return new LiteralExpressionSyntax( this.SyntaxTree , keywordToken );
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = this.MatchToken( SyntaxKind.NumericLiteral );

            return new LiteralExpressionSyntax( this.SyntaxTree , numberToken );
        }

        private ExpressionSyntax ParseStringLiteral()
        {
            var stringToken = this.MatchToken( SyntaxKind.StringLiteral );

            return new LiteralExpressionSyntax( this.SyntaxTree , stringToken );
        }

        private ExpressionSyntax ParseUnderscoreLiteral()
        {
            var keywordToken = this.MatchToken( SyntaxKind.UnderscoreToken );

            return new LiteralExpressionSyntax( this.SyntaxTree , keywordToken );
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if( this.Current.Kind == SyntaxKind.Identifier )
            {
                var peek = this.Peek();

                if( peek.Kind == SyntaxKind.OpenParenToken )
                {
                    return this.ParseCallExpression();
                }
            }

            return this.ParseNameExpression();
        }

        private ExpressionSyntax ParseCallExpression()
        {
            var identifier            = this.MatchToken( SyntaxKind.Identifier );
            var openParenthesisToken  = this.MatchToken( SyntaxKind.OpenParenToken );
            var arguments             = this.ParseArguments();
            var closeParenthesisToken = this.MatchToken( SyntaxKind.CloseParenToken );

            return new CallExpressionSyntax( this.SyntaxTree , identifier , openParenthesisToken , arguments , closeParenthesisToken );
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = this.MatchToken( SyntaxKind.Identifier );

            return new NameExpressionSyntax( this.SyntaxTree , identifierToken );
        }

        private SeparatedSyntaxList<T> ParseSeparatedNodeList<T>( Func<T> parseFunc ) where T : SyntaxNode
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var parseNextArgument = true;

            while( parseNextArgument &&
                   this.Current.Kind != SyntaxKind.CloseParenToken &&
                   this.Current.Kind != SyntaxKind.EndOfFile )
            {
                var node = parseFunc();

                nodesAndSeparators.Add( node );

                if( this.Current.Kind == SyntaxKind.CommaToken )
                {
                    nodesAndSeparators.Add( this.MatchToken( SyntaxKind.CommaToken ) );
                }
                else
                {
                    parseNextArgument = false;
                }
            }

            return new SeparatedSyntaxList<T>( nodesAndSeparators.ToImmutable() );
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
            => this.ParseSeparatedNodeList( this.ParseExpression );

        private SeparatedSyntaxList<PatternSyntax> ParsePatternList()
            => this.ParseSeparatedNodeList( this.ParsePattern );

        private MatchExpressionSyntax ParseMatchExpression()
        {
            var matchKeyword    = this.MatchToken( SyntaxKind.MatchKeyword );
            var variableExpr    = this.ParseExpression();
            var openBraceTok    = this.MatchToken( SyntaxKind.OpenBraceToken );
            var patterns        = this.ParsePatternSectionSyntax( this.ParsePatternSectionExpression );
            var closeBraceToken = this.MatchToken( SyntaxKind.CloseBraceToken );

            return new MatchExpressionSyntax( this.SyntaxTree , matchKeyword , variableExpr , patterns );
        }

        private MatchStatementSyntax ParseMatchStatement()
        {
            var matchKeyword    = this.MatchToken( SyntaxKind.MatchKeyword );
            var variableExpr    = this.ParseExpression();
            var openBraceTok    = this.MatchToken( SyntaxKind.OpenBraceToken );
            var patterns        = this.ParsePatternSectionSyntax( this.ParsePatternSectionStatement );
            var closeBraceToken = this.MatchToken( SyntaxKind.CloseBraceToken );

            return new MatchStatementSyntax( this.SyntaxTree , matchKeyword , variableExpr , patterns );
        }

        private ImmutableArray<T> ParsePatternSectionSyntax<T>( Func<T> parsePatternSectionFunc )
        {
            // is this actually good?
            Debug.Assert( typeof( T ) == typeof( PatternSectionExpressionSyntax ) ||
                          typeof( T ) == typeof( PatternSectionStatementSyntax ) );

            var patterns = new List<T>();

            while( true )
            {
                if( this.Current.Kind == SyntaxKind.CloseBraceToken )
                    break;

                var pattern = parsePatternSectionFunc();

                patterns.Add( pattern );
            }

            return patterns.ToImmutableArray();
        }

        private PatternSectionExpressionSyntax ParsePatternSectionExpression()
        {
            var patternList = this.ParsePatternList();
            var arrowToken  = this.MatchToken( SyntaxKind.EqGtToken );
            var exprOrStmt  = this.ParseExpression();

            return new PatternSectionExpressionSyntax( this.SyntaxTree , patternList , arrowToken , exprOrStmt );
        }

        private PatternSectionStatementSyntax ParsePatternSectionStatement()
        {
            var patternList = this.ParsePatternList();
            var arrowToken  = this.MatchToken( SyntaxKind.EqGtToken );
            var exprOrStmt  = this.ParseStatement();

            return new PatternSectionStatementSyntax( this.SyntaxTree , patternList , arrowToken , exprOrStmt );
        }

        private PatternSyntax ParsePattern()
        {
            switch( this.Current.Kind )
            {
                // handle expected errors
                // case TokenType.Comma:
                // case TokenType.Semicolon:
                // case TokenType.CloseBrace:
                // case TokenType.CloseParen:
                // case TokenType.CloseBracket:
                // case TokenType.EqGt:
                default: // TODO: check if default or any above case is better
                {
                    this.Diagnostics.ReportMissingPattern( this.Current.Location );

                    var missingToken   = this.CreateMissingToken( SyntaxKind.Identifier );
                    var nameExpression = new NameExpressionSyntax( this.SyntaxTree , missingToken );
                    var pattern        = new ConstantPatternSyntax( this.SyntaxTree , nameExpression );

                    return pattern;
                }

                case SyntaxKind.StringLiteral:
                case SyntaxKind.NumericLiteral:

                case SyntaxKind.TrueKeywordLiteral:
                case SyntaxKind.FalseKeywordLiteral:
                case SyntaxKind.NullKeywordLiteral:

                case SyntaxKind.Identifier:
                {
                    var expr = this.ParsePrimaryExpression();

                    if( expr is UnaryExpressionSyntax unaryExpr )
                    {
                        var pattern = new InfixPatternSyntax( this.SyntaxTree , unaryExpr.OperatorToken , unaryExpr.Expression );

                        return pattern;
                    }
                    else if( expr is NameExpressionSyntax nameExpr ||
                             expr is LiteralExpressionSyntax literalExpr )
                    {
                        var pattern = new ConstantPatternSyntax( this.SyntaxTree , expr );

                        return pattern;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                case SyntaxKind.UnderscoreToken:
                {
                    var underscoreToken = this.MatchToken( SyntaxKind.UnderscoreToken );
                    var pattern = new MatchAnyPatternSyntax( this.SyntaxTree , underscoreToken );

                    return pattern;
                }
            }
        }


        #endregion
    }
}
