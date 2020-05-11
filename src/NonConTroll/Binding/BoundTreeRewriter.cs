using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    public abstract class BoundTreeRewriter
    {
        #region Statements

        public virtual BoundStatement RewriteStatement( BoundStatement node )
        {
            switch( node )
            {
                case BoundBlockStatement b:           return this.RewriteBlockStatement( b );
                case BoundVariableDeclaration b:      return this.RewriteVariableDeclaration( b );
                case BoundIfStatement b:              return this.RewriteIfStatement( b );
                case BoundWhileStatement b:           return this.RewriteWhileStatement( b );
                case BoundDoWhileStatement b:         return this.RewriteDoWhileStatement( b );
                case BoundForStatement b:             return this.RewriteForStatement( b );
                case BoundLabelStatement b:           return this.RewriteLabelStatement( b );
                case BoundGotoStatement b:            return this.RewriteGotoStatement( b );
                case BoundConditionalGotoStatement b: return this.RewriteConditionalGotoStatement( b );
                case BoundReturnStatement b:          return this.RewriteReturnStatement( b );
                case BoundDeferStatement b:           return this.RewriteDeferStatement( b );
                case BoundExpressionStatement b:      return this.RewriteExpressionStatement( b );
                case BoundMatchStatement b:           return this.RewriteMatchStatement( b );
                default:
                    throw new Exception( $"Unexpected node: {node.Kind}" );
            }
        }

        protected virtual BoundStatement RewriteBlockStatement( BoundBlockStatement node )
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>( node.Statements.Length );
            var deferStmts = new List<BoundDeferStatement>();

            foreach( var stmt in node.Statements )
            {
                var statement = this.RewriteStatement( stmt );

                if( statement.Kind == BoundNodeKind.DeferStatement )
                {
                    deferStmts.Add( (BoundDeferStatement)statement );
                }
                else
                {
                    builder.Add( statement );
                }
            }

            builder.AddRange( deferStmts.AsEnumerable().Reverse() );

            return new BoundBlockStatement( builder.MoveToImmutable() );
        }

        protected virtual BoundStatement RewriteVariableDeclaration( BoundVariableDeclaration node )
        {
            var initializer = this.RewriteExpression( node.Initializer );

            if( initializer == node.Initializer )
            {
                return node;
            }

            return new BoundVariableDeclaration( node.Variable , initializer );
        }

        protected virtual BoundStatement RewriteIfStatement( BoundIfStatement node )
        {
            var condition = this.RewriteExpression( node.Condition );
            var thenStatement = this.RewriteStatement( node.ThenStatement );
            var elseStatement = node.ElseStatement == null ? null : this.RewriteStatement( node.ElseStatement);

            if( condition == node.Condition &&
                thenStatement == node.ThenStatement &&
                elseStatement == node.ElseStatement )
            {
                return node;
            }

            return new BoundIfStatement( condition , thenStatement , elseStatement );
        }

        protected virtual BoundStatement RewriteWhileStatement( BoundWhileStatement node )
        {
            var condition = this.RewriteExpression( node.Condition );
            var body = this.RewriteStatement( node.Body );

            if( condition == node.Condition && body == node.Body )
            {
                return node;
            }

            return new BoundWhileStatement( condition , body , node.BreakLabel , node.ContinueLabel );
        }

        protected virtual BoundStatement RewriteDoWhileStatement( BoundDoWhileStatement node )
        {
            var body = this.RewriteStatement( node.Body );
            var condition = this.RewriteExpression( node.Condition );

            if( body == node.Body && condition == node.Condition )
            {
                return node;
            }

            return new BoundDoWhileStatement( body , condition , node.BreakLabel , node.ContinueLabel );
        }

        protected virtual BoundStatement RewriteForStatement( BoundForStatement node )
        {
            var lowerBound = this.RewriteExpression( node.LowerBound );
            var upperBound = this.RewriteExpression( node.UpperBound );
            var body = this.RewriteStatement( node.Body );

            if( lowerBound == node.LowerBound &&
                upperBound == node.UpperBound &&
                body == node.Body )
            {
                return node;
            }

            return new BoundForStatement( node.Variable , lowerBound , upperBound , body , node.BreakLabel , node.ContinueLabel );
        }

        protected virtual BoundStatement RewriteLabelStatement( BoundLabelStatement node )
        {
            return node;
        }

        protected virtual BoundStatement RewriteGotoStatement( BoundGotoStatement node )
        {
            return node;
        }

        protected virtual BoundStatement RewriteConditionalGotoStatement( BoundConditionalGotoStatement node )
        {
            var condition = this.RewriteExpression( node.Condition );

            if( condition == node.Condition )
            {
                return node;
            }

            return new BoundConditionalGotoStatement( node.Label , condition , node.JumpIfTrue );
        }

        protected virtual BoundStatement RewriteReturnStatement( BoundReturnStatement node )
        {
            var expression = node.Expression == null ? null : this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
            {
                return node;
            }

            return new BoundReturnStatement( expression );
        }

        protected virtual BoundStatement RewriteDeferStatement( BoundDeferStatement node )
        {
            var expression = this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
            {
                return node;
            }

            return new BoundDeferStatement( expression );
        }

        protected virtual BoundStatement RewriteExpressionStatement( BoundExpressionStatement node )
        {
            var expression = this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
            {
                return node;
            }

            return new BoundExpressionStatement( expression );
        }

        protected virtual BoundStatement RewriteMatchStatement( BoundMatchStatement node )
        {
            var expr = this.RewriteExpression( node.Expression );
            var patternSections = this.RewritePatternSectionStatements( node.PatternSections );

            if( expr == node.Expression && patternSections.SequenceEqual( node.PatternSections ) )
            {
                return node;
            }

            return new BoundMatchStatement( expr , patternSections );
        }

        protected virtual BoundStatement RewritePatternSectionStatement( BoundPatternSectionStatement node )
        {
            var expr = this.RewriteStatement( node.Statement );
            var patterns = this.RewritePatterns( node.Patterns );

            if( expr == node.Statement && patterns.SequenceEqual( node.Patterns ) )
            {
                return node;
            }

            return new BoundPatternSectionStatement( patterns , expr );
        }

        public ImmutableArray<BoundPatternSectionStatement> RewritePatternSectionStatements( ImmutableArray<BoundPatternSectionStatement> patternSections )
        {
            var builder = ImmutableArray.CreateBuilder<BoundPatternSectionStatement>( patternSections.Count() );

            foreach( var patternSection in patternSections )
            {
                var newExpr = this.RewriteStatement( patternSection );

                builder.Add( patternSection );
            }

            return builder.MoveToImmutable();
        }

        #endregion

        #region Expressions

        public virtual BoundExpression RewriteExpression( BoundExpression node )
        {
            switch( node )
            {
                case BoundErrorExpression b:          return this.RewriteErrorExpression( b );
                case BoundLiteralExpression b:        return this.RewriteLiteralExpression( b );
                case BoundVariableExpression b:       return this.RewriteVariableExpression( b );
                case BoundAssignmentExpression b:     return this.RewriteAssignmentExpression( b );
                case BoundUnaryExpression b:          return this.RewriteUnaryExpression( b );
                case BoundBinaryExpression b:         return this.RewriteBinaryExpression( b );
                case BoundCallExpression b:           return this.RewriteCallExpression( b );
                case BoundConversionExpression b:     return this.RewriteConversionExpression( b );
                case BoundMatchExpression b:          return this.RewriteMatchExpression( b );
                case BoundPatternSectionExpression b: return this.RewritePatternSectionExpression( b );
                default:
                    throw new Exception( $"Unexpected node: {node.Kind}" );
            }
        }

        protected virtual BoundExpression RewriteErrorExpression( BoundErrorExpression node )
        {
            return node;
        }

        protected virtual BoundExpression RewriteLiteralExpression( BoundLiteralExpression node )
        {
            return node;
        }

        protected virtual BoundExpression RewriteVariableExpression( BoundVariableExpression node )
        {
            return node;
        }

        protected virtual BoundExpression RewriteMatchExpression( BoundMatchExpression node )
        {
            var expr = this.RewriteExpression( node.Expression );
            var patternSections = this.RewritePatternSectionExpressions( node.PatternSections );

            if( expr == node.Expression && patternSections.SequenceEqual( node.PatternSections ) )
            {
                return node;
            }

            return new BoundMatchExpression( expr , patternSections );
        }

        protected virtual BoundExpression RewritePatternSectionExpression( BoundPatternSectionExpression node )
        {
            var expr = this.RewriteExpression( node.Expression );
            var patterns = this.RewritePatterns( node.Patterns );

            if( expr == node.Expression && patterns.SequenceEqual( node.Patterns ) )
            {
                return node;
            }

            return new BoundPatternSectionExpression( patterns , expr );
        }

        public ImmutableArray<BoundPatternSectionExpression> RewritePatternSectionExpressions( ImmutableArray<BoundPatternSectionExpression> patternSections )
        {
            var builder = ImmutableArray.CreateBuilder<BoundPatternSectionExpression>( patternSections.Count() );

            foreach( var patternSection in patternSections )
            {
                var newExpr = this.RewriteExpression( patternSection );

                builder.Add( patternSection );
            }

            return builder.MoveToImmutable();
        }

        public ImmutableArray<BoundPattern> RewritePatterns( ImmutableArray<BoundPattern> patterns )
        {
            var builder = ImmutableArray.CreateBuilder<BoundPattern>( patterns.Count() );

            foreach( var pattern in patterns )
            {
                var newExpr = this.RewritePattern( pattern );

                builder.Add( pattern );
            }

            return builder.MoveToImmutable();
        }

        protected virtual BoundPattern RewritePattern( BoundPattern node )
        {
            switch( node )
            {
                case BoundInfixPattern pattern:    return this.RewriteInfixPattern( pattern );
                case BoundMatchAnyPattern pattern: return this.RewriteMatchAnyPattern( pattern );
                case BoundConstantPattern pattern: return this.RewriteConstantPattern( pattern );
                default:
                    throw new Exception( $"Unexpected node: {node.Kind}" );
            }
        }

        private BoundPattern RewriteInfixPattern( BoundInfixPattern node )
        {
            var expr = this.RewriteExpression( node.Expression );

            if( expr == node.Expression )
            {
                return node;
            }

            return new BoundInfixPattern( node.InfixFunction , expr );
        }

        private BoundPattern RewriteConstantPattern( BoundConstantPattern node )
        {
            var expr = this.RewriteExpression( node.Expression );

            if( expr == node.Expression )
            {
                return node;
            }

            return new BoundConstantPattern( expr );
        }

        private BoundPattern RewriteMatchAnyPattern( BoundMatchAnyPattern node )
        {
            return node;
        }

        protected virtual BoundExpression RewriteAssignmentExpression( BoundAssignmentExpression node )
        {
            var expression = this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
            {
                return node;
            }

            return new BoundAssignmentExpression( node.Variable , expression );
        }

        protected virtual BoundExpression RewriteUnaryExpression( BoundUnaryExpression node )
        {
            var operand = this.RewriteExpression( node.Operand );

            if( operand == node.Operand )
            {
                return node;
            }

            return new BoundUnaryExpression( node.Op , operand );
        }

        protected virtual BoundExpression RewriteBinaryExpression( BoundBinaryExpression node )
        {
            var lhs = this.RewriteExpression( node.Lhs );
            var rhs = this.RewriteExpression( node.Rhs );

            if( lhs == node.Lhs && rhs == node.Rhs )
            {
                return node;
            }

            return new BoundBinaryExpression( lhs , node.Operator , rhs );
        }

        protected virtual BoundExpression RewriteCallExpression( BoundCallExpression node )
        {
            var args = this.RewriteExpressions( node.Arguments );

            if( args.SequenceEqual( node.Arguments ) )
            {
                return node;
            }

            return new BoundCallExpression( node.Function , args.ToImmutableArray() );
        }

        protected virtual BoundExpression RewriteConversionExpression( BoundConversionExpression node )
        {
            var expression = this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
            {
                return node;
            }

            return new BoundConversionExpression( node.Type , expression );
        }

        public ImmutableArray<BoundExpression> RewriteExpressions( ImmutableArray<BoundExpression> expressions )
        {
            var builder = ImmutableArray.CreateBuilder<BoundExpression>( expressions.Count() );

            foreach( var expr in expressions )
            {
                var newExpr = this.RewriteExpression( expr );

                builder.Add( expr );
            }

            return builder.MoveToImmutable();
        }

        #endregion

#if false
        // rewrite ImmutableArray, if no rewrite is done, do nothing
        protected ImmutableArray<TBaseNode> RewriteNodes<TNode,TBaseNode>( ImmutableArray<TNode> nodes , Func<TNode,TBaseNode> func )
            where TBaseNode : BoundNode // TBaseNode might be BoundExpression
            where TNode : TBaseNode
        {
            var builder = default( ImmutableArray<TBaseNode>.Builder );

            for( var i = 0 ; i < nodes.Count() ; i++ )
            {
                var oldStatement = nodes.ElementAt( i );
                var newStatement = func( oldStatement );

                if( newStatement != oldStatement )
                {
                    if( builder == null )
                    {
                        builder = ImmutableArray.CreateBuilder<TBaseNode>( nodes.Count() );
                        builder.AddRange( nodes.Take( i ) );
                    }
                }

                if( builder != null )
                {
                    builder.Add( newStatement! );
                }
            }

            if( builder == null )
            {
                return nodes.ToImmutableArray<TBaseNode>(); // this defeats the purpose of this function..
            }

            return builder.MoveToImmutable();
        }

        #endif
    }
}
