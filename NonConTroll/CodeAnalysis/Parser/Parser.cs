using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using NonConTroll.CodeAnalysis.Text;


namespace NonConTroll.CodeAnalysis.Syntax
{
    public partial class Parser
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

                if( !token.TkType.IsTokenKind( TokenKind.WhiteSpace ) &&
                    !token.TkType.IsTokenKind( TokenKind.Documentation ) &&
                    token.TkType != TokenType.None )
                {
                    tokens.Add( token );
                }
            }
            while( token.TkType != TokenType.EndOfFile );

            this.SyntaxTree = syntaxTree;
            this.Tokens = tokens.ToImmutableArray();
            this.Diagnostics.AddRange( lexer.Diagnostics );
        }

        private SyntaxToken Current => this.Peek( 0 );

        private void Advance( int count = 1 )
            => this.Position += count;

        private SyntaxToken Peek( int offset )
            => this.Tokens.ElementAt( Math.Clamp( this.Position + offset , 0 , this.Tokens.Length - 1 ) );

        private SyntaxToken MatchToken( TokenType tokenType )
        {
            if( this.Current.TkType == tokenType )
            {
                return this.NextToken();
            }

            this.Diagnostics.ReportUnexpectedToken( this.Current.Location , this.Current.TkType , tokenType );

            return new SyntaxToken( this.SyntaxTree , tokenType , this.Current.Position , null );
        }

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
            var endOfFileToken = this.MatchToken( TokenType.EndOfFile );

            return new CompilationUnitSyntax( this.SyntaxTree , members , endOfFileToken );
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while( this.Current.TkType != TokenType.EndOfFile )
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

        private MemberSyntax ParseMember()
        {
            if( this.Current.TkType == TokenType.Func )
            {
                return this.ParseFunctionDeclaration();
            }

            return this.ParseGlobalStatement();
        }

        private MemberSyntax ParseFunctionDeclaration()
        {
            var functionKeyword = this.MatchToken( TokenType.Func );
            var identifier      = this.MatchToken( TokenType.Identifier );
            var openParenToken  = this.MatchToken( TokenType.OpenParen );
            var parameters      = this.ParseParameterList();
            var closeParenToken = this.MatchToken( TokenType.CloseParen );
            var type            = this.ParseOptionalTypeClause();
            var body            = this.ParseBlockStatement();

            return new FunctionDeclarationSyntax( this.SyntaxTree , functionKeyword , identifier , openParenToken , parameters , closeParenToken , type , body );
        }


        private SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
            => this.ParseSeparatedList( this.ParseParameter );

        private SeparatedSyntaxList<T> ParseSeparatedList<T>(
                Func<T> parseParamFunc ,
                TokenType openListToken = TokenType.OpenParen ,
                TokenType seperatorToken = TokenType.Comma ,
                TokenType closeListToken = TokenType.CloseParen )
                where T : SyntaxNode
        {
            //Debug.Assert( this.Current.TkType == openListToken );

            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var parseNextParameter = true;

            while( parseNextParameter &&
                   this.Current.TkType != closeListToken &&
                   this.Current.TkType != TokenType.EndOfFile )
            {
                var parameter = parseParamFunc();

                nodesAndSeparators.Add( parameter );

                if( this.Current.TkType == seperatorToken )
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
            var identifier = this.MatchToken( TokenType.Identifier );
            var type = this.ParseTypeClause();

            return new ParameterSyntax( this.SyntaxTree , identifier , type );
        }

        private MemberSyntax ParseGlobalStatement()
        {
            var statement = this.ParseStatement();

            return new GlobalStatementSyntax( this.SyntaxTree , statement );
        }

        private StatementSyntax ParseStatement()
        {
            switch( this.Current.TkType )
            {
                case TokenType.OpenBrace: return this.ParseBlockStatement();
                case TokenType.Let:       return this.ParseVariableDeclaration( TokenType.Let );
                case TokenType.Var:       return this.ParseVariableDeclaration( TokenType.Var );
                case TokenType.If:        return this.ParseIfStatement();
                case TokenType.While:     return this.ParseWhileStatement();
                case TokenType.Do:        return this.ParseDoWhileStatement();
                case TokenType.For:       return this.ParseForStatement();
                case TokenType.Break:     return this.ParseBreakStatement();
                case TokenType.Continue:  return this.ParseContinueStatement();
                case TokenType.Return:    return this.ParseReturnStatement();
                case TokenType.Defer:     return this.ParseDeferStatement();
                default:                  return this.ParseExpressionStatement();
            }
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            var openBraceToken = this.MatchToken( TokenType.OpenBrace);

            while( this.Current.TkType != TokenType.EndOfFile &&
                   this.Current.TkType != TokenType.CloseBrace )
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

            var closeBraceToken = this.MatchToken( TokenType.CloseBrace );

            return new BlockStatementSyntax( this.SyntaxTree , openBraceToken , statements.ToImmutable() , closeBraceToken );
        }

        private StatementSyntax ParseVariableDeclaration( TokenType expectedTokenType )
        {
            var keyword     = this.MatchToken( expectedTokenType );
            var identifier  = this.MatchToken( TokenType.Identifier );
            var typeClause  = this.ParseOptionalTypeClause();
            var equals      = this.MatchToken( TokenType.Eq );
            var initializer = this.ParseExpression();

            return new VariableDeclarationSyntax( this.SyntaxTree , keyword , identifier , typeClause , equals , initializer );
        }

        private TypeClauseSyntax? ParseOptionalTypeClause()
        {
            if( this.Current.TkType != TokenType.Colon )
            {
                return null;
            }

            return this.ParseTypeClause();
        }

        private TypeNameSyntax /* TypeSyntax */ ParseType()
        {
            if( this.Current.TkType == TokenType.OpenParen )
            {
                //return this.ParseTupleType();
            }

            var typeIdentifier     = default( SyntaxToken? );
            var typeSpecifier      = ImmutableArray.CreateBuilder<TypeSpecifierSyntax>();
            var arrayTypeSpecifier = ImmutableArray.CreateBuilder<ArrayTypeSpecifierSyntax>();
            var done = false;

            while( !done )
            {
                switch( this.Current.TkType )
                {
                    case TokenType.Ref:
                    case TokenType.Ptr:
                    //case TokenType.Qm:
                    case TokenType.Null:
                        typeSpecifier.Add( this.ParseTypeSpecifier() );
                        break;
                    case TokenType.OpenBracket:
                        arrayTypeSpecifier.Add( this.ParseArrayTypeSpecifier() );
                        break;
                    default:
                        typeIdentifier = this.MatchToken( TokenType.Identifier );
                        done = true;
                        break;
                }
            }

            return new TypeNameSyntax( this.SyntaxTree , typeIdentifier , typeSpecifier.ToImmutable() , arrayTypeSpecifier.ToImmutable() );
        }

        private TypeSpecifierSyntax ParseTypeSpecifier()
        {
            switch( this.Current.TkType )
            {
                case TokenType.Ref:  return new TypeSpecifierSyntax( this.SyntaxTree , this.MatchToken( TokenType.Ref )   , TypeSpecifierKind.Reference );
                case TokenType.Ptr:  return new TypeSpecifierSyntax( this.SyntaxTree , this.MatchToken( TokenType.Ptr )   , TypeSpecifierKind.Pointer   );
                case TokenType.Null: return new TypeSpecifierSyntax( this.SyntaxTree , this.MatchToken( TokenType.Null )  , TypeSpecifierKind.Nullable  );
                //case TokenType.Qm:   return new TypeSpecifierSyntax( this.MatchToken( TokenType.Colon ) , TypeSpecifierKind.Nullable  );
                default:
                    break;
            }

            throw new Exception( "" );
        }

        private ArrayTypeSpecifierSyntax ParseArrayTypeSpecifier()
        {
            var openBracketToken  = this.MatchToken( TokenType.OpenBracket );
            var rankExpression    = this.ParseExpression();
            var closeBracketToken = this.MatchToken( TokenType.CloseBracket );

            return new ArrayTypeSpecifierSyntax( this.SyntaxTree , openBracketToken , rankExpression , closeBracketToken );
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colonToken = this.MatchToken( TokenType.Colon );
            var type       = this.ParseType();

            return new TypeClauseSyntax( this.SyntaxTree , colonToken , type );
        }

        private StatementSyntax ParseIfStatement()
        {
            var keyword     = this.MatchToken( TokenType.If );
            var condition   = this.ParseExpression();
            var statement   = this.ParseStatement();
            var elseClause  = this.ParseElseClause();

            return new IfStatementSyntax( this.SyntaxTree , keyword , condition , statement , elseClause );
        }

        private ElseClauseSyntax? ParseElseClause()
        {
            if( this.Current.TkType != TokenType.Else )
            {
                return null;
            }

            var keyword   = this.NextToken();
            var statement = this.ParseStatement();

            return new ElseClauseSyntax( this.SyntaxTree , keyword , statement );
        }

        private StatementSyntax ParseWhileStatement()
        {
            var keyword     = this.MatchToken( TokenType.While );
            var condition   = this.ParseExpression();
            var body        = this.ParseStatement();

            return new WhileStatementSyntax( this.SyntaxTree , keyword , condition , body );
        }

        private StatementSyntax ParseDoWhileStatement()
        {
            var doKeyword    = this.MatchToken( TokenType.Do );
            var body         = this.ParseStatement();
            var whileKeyword = this.MatchToken( TokenType.While );
            var condition    = this.ParseExpression();

            return new DoWhileStatementSyntax( this.SyntaxTree , doKeyword , body , whileKeyword , condition );
        }

        private StatementSyntax ParseForStatement()
        {
            var keyword     = this.MatchToken( TokenType.For );
            var identifier  = this.MatchToken( TokenType.Identifier );
            var equalsToken = this.MatchToken( TokenType.Eq );
            var lowerBound  = this.ParseExpression();
            var toKeyword   = this.MatchToken( TokenType.To );
            var upperBound  = this.ParseExpression();
            var body        = this.ParseStatement();

            return new ForStatementSyntax( this.SyntaxTree , keyword , identifier , equalsToken , lowerBound , toKeyword , upperBound , body );
        }

        private StatementSyntax ParseBreakStatement()
        {
            var keyword = this.MatchToken( TokenType.Break );

            return new BreakStatementSyntax( this.SyntaxTree , keyword );
        }

        private StatementSyntax ParseContinueStatement()
        {
            var keyword = this.MatchToken( TokenType.Continue );

            return new ContinueStatementSyntax( this.SyntaxTree , keyword );
        }

        private StatementSyntax ParseReturnStatement()
        {
            var keyword     = this.MatchToken( TokenType.Return );
            var keywordLine = this.Text.GetLineIndex( keyword.Span.Start );
            var currentLine = this.Text.GetLineIndex( this.Current.Span.Start );
            var isEof       = this.Current.TkType == TokenType.EndOfFile;
            var sameLine    = !isEof && keywordLine == currentLine;
            var expression  = sameLine ? this.ParseExpression() : null;

            return new ReturnStatementSyntax( this.SyntaxTree , keyword , expression );
        }

        private StatementSyntax ParseDeferStatement()
        {
            var keyword     = this.MatchToken( TokenType.Defer );
            var expression  = this.ParseExpression();

            return new DeferStatementSyntax( this.SyntaxTree , keyword , expression );
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = this.ParseExpression();

            return new ExpressionStatementSyntax( this.SyntaxTree , expression );
        }

        private ExpressionSyntax ParseExpression()
        {
            return this.ParseAssignmentExpression();
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if( this.Peek( 0 ).TkType == TokenType.Identifier &&
                this.Peek( 1 ).TkType == TokenType.Eq )
            {
                var identifierToken = this.NextToken();
                var operatorToken   = this.NextToken();
                var right           = this.ParseAssignmentExpression();

                return new AssignmentExpressionSyntax( this.SyntaxTree , identifierToken , operatorToken , right );
            }

            return this.ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression( int parentPrecedence = 0 )
        {
            var left = default( ExpressionSyntax );
            var unaryOperatorPrecedence = this.Current.TkType.GetUnaryOperatorPrecedence();

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
                var precedence = this.Current.TkType.GetBinaryOperatorPrecedence();

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
            switch( this.Current.TkType )
            {
                case TokenType.OpenParen:      return this.ParseParenthesizedExpression();
                case TokenType.False:          return this.ParseBooleanLiteral( this.Current.TkType );
                case TokenType.True:           return this.ParseBooleanLiteral( this.Current.TkType );
                case TokenType.NumericLiteral: return this.ParseNumberLiteral();
                case TokenType.StringLiteral:  return this.ParseStringLiteral();
                case TokenType.Identifier:
                default:                       return this.ParseNameOrCallExpression();
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var left       = this.MatchToken( TokenType.OpenParen );
            var expression = this.ParseExpression();
            var right      = this.MatchToken( TokenType.CloseParen );

            return new ParenthesizedExpressionSyntax( this.SyntaxTree , left , expression , right );
        }

        private ExpressionSyntax ParseBooleanLiteral( TokenType expectedTokenType )
        {
            var keywordToken = this.MatchToken( expectedTokenType );

            return new LiteralExpressionSyntax( this.SyntaxTree , keywordToken );
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = this.MatchToken( TokenType.NumericLiteral );

            return new LiteralExpressionSyntax( this.SyntaxTree , numberToken );
        }

        private ExpressionSyntax ParseStringLiteral()
        {
            var stringToken = this.MatchToken( TokenType.StringLiteral );

            return new LiteralExpressionSyntax( this.SyntaxTree , stringToken );
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if( this.Peek( 0 ).TkType == TokenType.Identifier &&
                this.Peek( 1 ).TkType == TokenType.OpenParen )
                return this.ParseCallExpression();

            return this.ParseNameExpression();
        }

        private ExpressionSyntax ParseCallExpression()
        {
            var identifier            = this.MatchToken( TokenType.Identifier );
            var openParenthesisToken  = this.MatchToken( TokenType.OpenParen );
            var arguments             = this.ParseArguments();
            var closeParenthesisToken = this.MatchToken( TokenType.CloseParen );

            return new CallExpressionSyntax( this.SyntaxTree , identifier , openParenthesisToken , arguments , closeParenthesisToken );
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var parseNextArgument = true;

            while( parseNextArgument &&
                   this.Current.TkType != TokenType.CloseParen &&
                   this.Current.TkType != TokenType.EndOfFile )
            {
                nodesAndSeparators.Add( this.ParseExpression() );

                if( this.Current.TkType == TokenType.Comma )
                {
                    nodesAndSeparators.Add( this.MatchToken( TokenType.Comma ) );
                }
                else
                {
                    parseNextArgument = false;
                }
            }

            return new SeparatedSyntaxList<ExpressionSyntax>( nodesAndSeparators.ToImmutable() );
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = this.MatchToken( TokenType.Identifier );

            return new NameExpressionSyntax( this.SyntaxTree , identifierToken );
        }

        #endregion
    }
}
