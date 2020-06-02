using System.Collections.Generic;
using System.Collections.Immutable;
using System;
using System.Linq;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;
using static NonConTroll.CodeAnalysis.Binding.BoundNodeFactory;

namespace NonConTroll.CodeAnalysis.Lowering
{
    internal partial class Lowerer : BoundTreeRewriter
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
                    foreach( var syntax in block.Statements.Reverse() )
                    {
                        stack.Push( syntax );
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

        private BoundLabel GenerateLabel()
        {
            var name = $"Label{++this.LabelCount}";

            return new BoundLabel( name );
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

                var endLabel = this.GenerateLabel();
                var result = Block( syntax ,
                                    GotoFalse( syntax , endLabel , node.Condition ) ,
                                    node.ThenStatement ,
                                    Label( syntax , endLabel ) );

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

                var elseLabel = this.GenerateLabel();
                var endLabel = this.GenerateLabel();
                var result = Block( syntax ,
                                    GotoFalse( syntax , elseLabel , node.Condition ) ,
                                    node.ThenStatement ,
                                    Goto( syntax , endLabel ) ,
                                    Label( syntax , elseLabel ) ,
                                    node.ElseStatement ,
                                    Label( syntax , endLabel ) );

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

            var bodyLabel = this.GenerateLabel();
            var result = Block( syntax ,
                                Goto( syntax , node.ContinueLabel ) ,
                                Label( syntax , bodyLabel ) ,
                                node.Body ,
                                Label( syntax , node.ContinueLabel ) ,
                                GotoTrue( syntax , bodyLabel , node.Condition ) ,
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

            var bodyLabel = this.GenerateLabel();
            var result = Block( syntax ,
                                Label( syntax , bodyLabel ) ,
                                node.Body ,
                                Label( syntax , node.ContinueLabel ) ,
                                GotoTrue( syntax , bodyLabel , node.Condition ) ,
                                Label( syntax , node.BreakLabel ) );

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
            var upperBound = ConstantDeclaration( syntax , "upperBound" , node.UpperBound );
            var result = Block( syntax ,
                lowerBound ,
                upperBound ,
                While( syntax ,
                     LessOrEqual( syntax , Variable( syntax , lowerBound ) , Variable( syntax , upperBound ) ) ,
                     Block( syntax ,
                            node.Body ,
                            Label( syntax , node.ContinueLabel ) ,
                            Increment( syntax , Variable( syntax , lowerBound ) ) ) ,
                     node.BreakLabel ,
                     continueLabel: this.GenerateLabel() )
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

                var ifStmt = If( syntax , prevCondition! , patternSection.Statement , prevStmt , elseLabel: this.GenerateLabel() , endLabel: this.GenerateLabel() );

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
                    expr = Equal( syntax , matchExpr , constantPattern.Expression );
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
                    return new BoundNopStatement( syntax );
                }
            }

            return base.RewriteConditionalGotoStatement( node );
        }

        protected override BoundStatement RewriteExpressionStatement( BoundExpressionStatement node )
        {
            var newNode = base.RewriteExpressionStatement( node );

            return new BoundSequencePointStatement( newNode.Syntax , newNode , newNode.Syntax.Location );
        }

        #endregion
    }
}
