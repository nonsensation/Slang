using System.Collections.Generic;
using System.Collections.Immutable;
using System;
using System.Linq;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using System.Diagnostics;

namespace NonConTroll.CodeAnalysis.Lowering
{
    internal class Lowerer : BoundTreeRewriter
    {
        private int LabelCount;

        private Lowerer()
        {
        }

        public static BoundBlockStatement Lower( FunctionSymbol function , BoundStatement statement )
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement( statement );
            var flattenedResult = Flatten( function , result );
            var removedDeadCodeResult = RemoveDeadCode( flattenedResult );

            return removedDeadCodeResult;
        }

        private static BoundBlockStatement Flatten( FunctionSymbol function , BoundStatement statement )
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();

            stack.Push( statement );

            while( stack.Count > 0 )
            {
                var current = stack.Pop();

                if( current is BoundBlockStatement block )
                {
                    foreach( var s in block.Statements.Reverse() )
                    {
                        stack.Push( s );
                    }
                }
                else
                {
                    builder.Add( current );
                }
            }

            if( function.ReturnType == BuiltinTypes.Void )
            {
                if( builder.Count == 0 || CanFallThrough( builder.Last() ) )
                {
                    builder.Add( new BoundReturnStatement( statement.Syntax , null ) );
                }
            }

            return new BoundBlockStatement( statement.Syntax , builder.ToImmutable() );
        }

        private static bool CanFallThrough( BoundStatement boundStatement )
        {
            return boundStatement.Kind != BoundNodeKind.ReturnStatement &&
                   boundStatement.Kind != BoundNodeKind.GotoStatement;
        }

        private static BoundBlockStatement RemoveDeadCode( BoundBlockStatement node )
        {
            var controlFlow = ControlFlowGraph.Create( node );
            var reachableStatements = new HashSet<BoundStatement>(
                controlFlow.Blocks.SelectMany( b => b.Statements ) );
            var builder = node.Statements.ToBuilder();

            for( int i = builder.Count - 1 ; i >= 0 ; i-- )
            {
                if( !reachableStatements.Contains( builder[ i ] ) )
                {
                    builder.RemoveAt( i );
                }
            }

            var syntax = node.Syntax;

            return new BoundBlockStatement( syntax , builder.ToImmutable() );
        }

        #region Rewrite

        protected override BoundStatement RewriteIfStatement( BoundIfStatement node )
        {
            var syntax = node.Syntax;

            if( node.ElseStatement == null )
            {
                // if <condition>
                //      <then>
                //
                // ---->
                //
                // gotoFalse <condition> end
                // <then>
                // end:

                var endLabel = Label( syntax );

                var result =  Block( syntax ,
                    GotoIfFalse( syntax , endLabel , node.Condition ) ,
                    node.ThenStatement ,
                    endLabel );

                return this.RewriteStatement( result );
            }
            else
            {
                // if <condition>
                //      <then>
                // else
                //      <else>
                //
                // ---->
                //
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:

                var elseLabel = Label( syntax );
                var endLabel = Label( syntax );

                var result = Block( syntax ,
                    GotoIfFalse( syntax , elseLabel , node.Condition ) ,
                    node.ThenStatement ,
                    Goto( syntax , endLabel ) ,
                    elseLabel ,
                    node.ElseStatement ,
                    endLabel );

                return this.RewriteStatement( result );
            }
        }

        protected override BoundStatement RewriteWhileStatement( BoundWhileStatement node )
        {
            var syntax = node.Syntax;

            // while <condition>
            //      <body>
            //
            // ----->
            //
            // goto continue
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = Label( syntax );

            var result = Block( syntax ,
                Goto( syntax , node.ContinueLabel ) ,
                bodyLabel ,
                node.Body ,
                Label( syntax , node.ContinueLabel ) ,
                GotoIfTrue( syntax , bodyLabel , node.Condition ) ,
                Label( syntax , node.BreakLabel ) );

            return this.RewriteStatement( result );
        }

        protected override BoundStatement RewriteDoWhileStatement( BoundDoWhileStatement node )
        {
            var syntax = node.Syntax;

            // do
            //      <body>
            // while <condition>
            //
            // ----->
            //
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = Label( syntax );

            var result = Block( syntax ,
                bodyLabel ,
                node.Body ,
                Label( syntax , node.ContinueLabel ) ,
                GotoIfTrue( syntax , bodyLabel , node.Condition ) ,
                Label( syntax , node.BreakLabel )
                );

            return this.RewriteStatement( result );
        }

        protected override BoundStatement RewriteForStatement( BoundForStatement node )
        {
            var syntax = node.Syntax;

            // for <var> = <lower> to <upper>
            //      <body>
            //
            // ---->
            //
            // {
            //      var <var> = <lower>
            //      let upperBound = <upper>
            //      while( <var> <= upperBound )
            //      {
            //          <body>
            //          continue:
            //          <var> = <var> + 1
            //      }
            //      break:
            // }

            var lowerBound = VariableDeclaration( syntax , node.Variable , node.LowerBound );
            var upperBound = ConstantDeclaration( syntax , "upperBound" , node.UpperBound , BuiltinTypes.Int );

            var result = Block( syntax ,
                lowerBound ,
                upperBound ,
                While( syntax ,
                    CompareLessThanOrEqual( syntax , Variable( syntax , lowerBound ) , Variable( syntax , upperBound ) ) ,
                Block( syntax ,
                    node.Body ,
                    Label( syntax , node.ContinueLabel ) ,
                    Increment( syntax , Variable( syntax , lowerBound ) )
                ) ,
                node.BreakLabel )
                );

            return this.RewriteStatement( result );
        }

        protected override BoundStatement RewriteMatchStatement( BoundMatchStatement node )
        {
            var syntax = node.Syntax;

            /*
                match <expr> {
                    <exprA> => <stmtA> ,
                    <exprB> , <exprC> => <stmtBC> ,
                    _ => <stmtMatchAny> ,
                }

                ---->

                let <var> = <expr>

                if <var> == <exprA>
                    <stmtA>
                elif <var> == <exprB> or <var> == <exprC>
                    <stmtBC>
                else
                    <stmtMatchAny>
            */

            var exprVariableDecl = ConstantDeclaration( syntax , "matchExpression" , node.Expression );
            var exprVariableExpr = Variable( syntax , exprVariableDecl );

            var prevStmt = default( BoundStatement );

            if( node.DefaultPattern != null )
            {
                if( node.PatternSections.Count() == 1 )
                {
                    // match has only 1 section containing the match-any case
                    // all other scenarios should be a copile error
                    var matchAnySection = node.PatternSections.Single();

                    var boundBlockStmt = Block( syntax ,
                        exprVariableDecl ,
                        matchAnySection.Statement );

                    return this.RewriteStatement( boundBlockStmt );
                }

                prevStmt = this.RewriteStatement( node.DefaultPattern.Statement );
            }

            foreach( var patternSection in node.PatternSections.Reverse() )
            {
                if( node.DefaultPattern != null && node.DefaultPattern == patternSection )
                {
                    continue;
                }

                var prevCondition = default( BoundExpression );

                foreach( var pattern in patternSection.Patterns.Reverse() )
                {
                    var condition = this.ConvertPatternToExpression( pattern , exprVariableExpr );

                    if( prevCondition != null )
                    {
                        condition = Or( syntax , condition , prevCondition );
                    }

                    prevCondition = condition;
                }

                var ifStmt = If( syntax , prevCondition! , patternSection.Statement , prevStmt );

                prevStmt = ifStmt;
            }

            var blockStmt = Block( syntax ,
                exprVariableDecl ,
                prevStmt! );

            return this.RewriteStatement( blockStmt );
        }

        private BoundExpression ConvertPatternToExpression( BoundPattern pattern , BoundExpression matchExpr )
        {
            var syntax = pattern.Syntax;
            var expr = default( BoundExpression );

            switch( pattern )
            {
                case BoundMatchAnyPattern b:
                {
                    expr = Literal( syntax , true );
                }
                break;
                case BoundConstantPattern constantPattern:
                {
                    expr = CompareEqual( syntax , matchExpr , constantPattern.Expression );
                }
                break;

                // TODO: when infix function parsing (binary and unary!) is done
                // case BoundInfixPattern infixPattern:
                // {
                //     var comparison = BoundBinaryOperator.Bind( TokenType.Eq , matchExpr.Type , infixPattern.Expression.Type )!;
                //
                //     var args = ImmutableArray.Create( matchExpr , infixPattern.Expression );
                //     var expr = new BoundCallExpression( infixFuncSymbol , args );
                //
                //     return expr;
                // }
                // break;

                default:
                {
                    throw new Exception( "unreachable" );
                }
            }

            return this.RewriteExpression( expr! );
        }

        protected override BoundStatement RewriteConditionalGotoStatement( BoundConditionalGotoStatement node )
        {
            var syntax = node.Syntax;

            if( node.Condition.ConstantValue != null )
            {
                var value = (bool)node.Condition.ConstantValue.Value;
                var condition = node.JumpIfTrue ? value : !value;

                if( condition )
                {
                    return Goto( syntax , node.Label );
                }
                else
                {
                    //return new BoundNop();
                }
            }

            return base.RewriteConditionalGotoStatement( node );
        }

        #endregion

        #region Factory methods

        private BoundBlockStatement Block( SyntaxNode syntax , params BoundStatement[] stmts )
        {
            return new BoundBlockStatement( syntax , ImmutableArray.Create( stmts ) );
        }

        private BoundUnaryExpression Not( SyntaxNode syntax , BoundExpression expr )
            => UnExpr( syntax , SyntaxKind.ExmToken , expr );
        private BoundUnaryExpression Neg( SyntaxNode syntax , BoundExpression expr )
            => UnExpr( syntax , SyntaxKind.MinusToken , expr );

        private BoundBinaryExpression CompareEqual( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.EqEqToken , rhs );
        private BoundBinaryExpression CompareNotEqual( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.ExmEqToken , rhs );
        private BoundBinaryExpression Add( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.PlusToken , rhs );
        private BoundBinaryExpression Sub( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.MinusToken , rhs );
        private BoundBinaryExpression Mul( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.StarToken , rhs );
        private BoundBinaryExpression Div( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.SlashToken , rhs );
        private BoundBinaryExpression CompareGreaterThan( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.GtToken , rhs );
        private BoundBinaryExpression CompareGreaterThanOrEqual( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.GtEqToken , rhs );
        private BoundBinaryExpression CompareLessThan( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.LtToken , rhs );
        private BoundBinaryExpression CompareLessThanOrEqual( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.LtEqToken , rhs );
        private BoundBinaryExpression Or( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.PipePipeToken , rhs );
        private BoundBinaryExpression And( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => BinExpr( syntax , lhs , SyntaxKind.AndAndToken , rhs );

        private BoundBinaryExpression BinExpr( SyntaxNode syntax , BoundExpression lhs , SyntaxKind kind , BoundExpression rhs )
        {
            var op = BoundBinaryOperator.Bind( kind , lhs.Type , rhs.Type )!;


            return new BoundBinaryExpression( syntax , lhs , op , rhs );
        }

        private BoundUnaryExpression UnExpr( SyntaxNode syntax , SyntaxKind kind , BoundExpression expr )
        {
            var op = BoundUnaryOperator.Bind( kind , expr.Type )!;


            return new BoundUnaryExpression( syntax , op , expr );
        }

        private BoundLiteralExpression Literal( SyntaxNode syntax , object literal )
        {
            Debug.Assert( literal is string || literal is bool || literal is int );


            return new BoundLiteralExpression( syntax , literal );
        }

        private BoundStatement If( SyntaxNode syntax , BoundExpression condition , BoundStatement thenStmt )
        {
            var endLabel = Label( syntax );
            var gotoFalse = GotoIfFalse( syntax , endLabel , condition );

            return Block( syntax , gotoFalse , thenStmt , endLabel );
        }

        private BoundStatement If( SyntaxNode syntax , BoundExpression condition , BoundStatement thenStmt , BoundStatement? elseStmt )
        {
            if( elseStmt == null )
            {
                return If( syntax , condition , thenStmt );
            }

            var elseLabel = Label( syntax );
            var endLabel = Label( syntax );
            var gotoFalse = GotoIfFalse( syntax , elseLabel , condition );
            var gotoEndStmt = Goto( syntax , endLabel );

            return Block( syntax , gotoFalse , thenStmt , gotoEndStmt , elseLabel , elseStmt , endLabel );
        }

        private BoundGotoStatement Goto( SyntaxNode syntax , BoundLabelStatement label )
        {

            return new BoundGotoStatement( syntax , label.Label );
        }

        private BoundGotoStatement Goto( SyntaxNode syntax , BoundLabel label )
        {

            return new BoundGotoStatement( syntax , label );
        }

        private BoundConditionalGotoStatement GotoIf( SyntaxNode syntax , BoundLabelStatement label , BoundExpression condition , bool jumpIfTrue )
        {

            return new BoundConditionalGotoStatement( syntax , label.Label , condition , jumpIfTrue );
        }

        private BoundConditionalGotoStatement GotoIfTrue( SyntaxNode syntax , BoundLabelStatement label , BoundExpression condition )
            => GotoIf( syntax , label , condition , jumpIfTrue: true );

        private BoundConditionalGotoStatement GotoIfFalse( SyntaxNode syntax , BoundLabelStatement label , BoundExpression condition )
            => GotoIf( syntax , label , condition , jumpIfTrue: false );

        private BoundLabel GenerateLabel()
        {
            var name = $"Label{++this.LabelCount}";

            return new BoundLabel( name );
        }

        private BoundLabelStatement Label( SyntaxNode syntax )
        {
            var label = this.GenerateLabel();

            return new BoundLabelStatement( syntax , label );
        }

        private BoundLabelStatement Label( SyntaxNode syntax , BoundLabel label )
        {

            return new BoundLabelStatement( syntax , label );
        }

        private BoundVariableDeclaration ConstantDeclaration( SyntaxNode syntax , string name , BoundExpression initExpr , TypeSymbol? type = null )
            => VariableDeclarationInternal( syntax , name , initExpr , type , isReadOnly: true );

        private BoundVariableDeclaration VariableDeclaration( SyntaxNode syntax , string name , BoundExpression initExpr , TypeSymbol? type = null )
            => VariableDeclarationInternal( syntax , name , initExpr , type , isReadOnly: false );

        private BoundVariableDeclaration VariableDeclarationInternal( SyntaxNode syntax , string name , BoundExpression initExpr , TypeSymbol? type , bool isReadOnly )
        {
            var symbol = Symbol( syntax , name , type ?? initExpr.Type , isReadOnly , initExpr.ConstantValue );

            return new BoundVariableDeclaration( syntax , symbol , initExpr );
        }

        private BoundVariableDeclaration VariableDeclaration( SyntaxNode syntax , VariableSymbol symbol , BoundExpression initExpr )
        {

            return new BoundVariableDeclaration( syntax , symbol , initExpr );
        }

        private BoundVariableExpression Variable( SyntaxNode syntax , VariableSymbol symbol )
        {

            return new BoundVariableExpression( syntax , symbol );
        }

        private BoundVariableExpression Variable( SyntaxNode syntax , BoundVariableDeclaration varDecl )
        {

            return new BoundVariableExpression( syntax , varDecl.Variable );
        }

        private LocalVariableSymbol Symbol( SyntaxNode syntax , string name , TypeSymbol type , bool isReadOnly = true , BoundConstant? constant = null )
        {
            return new LocalVariableSymbol( name , isReadOnly , type , constant );
        }

        private BoundWhileStatement While( SyntaxNode syntax , BoundExpression condition , BoundStatement body , BoundLabel breakLabel )
        {
            var continueLabel = this.GenerateLabel();

            return new BoundWhileStatement( syntax , condition , body , breakLabel , continueLabel );
        }

        private BoundExpressionStatement Increment( SyntaxNode syntax , BoundVariableExpression varExpr )
        {
            var incrByOne = Add( syntax , varExpr , Literal( syntax , 1 ) );
            var incrAssign = new BoundAssignmentExpression( syntax , varExpr.Variable , incrByOne );

            return new BoundExpressionStatement( syntax , incrAssign );
        }

        private BoundExpressionStatement Decrement( SyntaxNode syntax , BoundVariableExpression varExpr )
        {
            var incrByOne = Sub( syntax , varExpr , Literal( syntax , 1 ) );
            var incrAssign = new BoundAssignmentExpression( syntax , varExpr.Variable , incrByOne );

            return new BoundExpressionStatement( syntax , incrAssign );
        }

        #endregion
    }
}
