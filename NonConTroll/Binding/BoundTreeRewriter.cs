using System;
using System.Collections.Immutable;

namespace NonConTroll.CodeAnalysis.Binding
{
    public abstract class BoundTreeRewriter
    {
        public virtual BoundStatement RewriteStatement( BoundStatement node )
        {
            switch( node.Kind )
            {
                case BoundNodeKind.BlockStatement:           return this.RewriteBlockStatement( (BoundBlockStatement)node );
                case BoundNodeKind.VariableDeclaration:      return this.RewriteVariableDeclaration( (BoundVariableDeclaration)node );
                case BoundNodeKind.IfStatement:              return this.RewriteIfStatement( (BoundIfStatement)node );
                case BoundNodeKind.WhileStatement:           return this.RewriteWhileStatement( (BoundWhileStatement)node );
                case BoundNodeKind.DoWhileStatement:         return this.RewriteDoWhileStatement( (BoundDoWhileStatement)node );
                case BoundNodeKind.ForStatement:             return this.RewriteForStatement( (BoundForStatement)node );
                case BoundNodeKind.LabelStatement:           return this.RewriteLabelStatement( (BoundLabelStatement)node );
                case BoundNodeKind.GotoStatement:            return this.RewriteGotoStatement( (BoundGotoStatement)node );
                case BoundNodeKind.ConditionalGotoStatement: return this.RewriteConditionalGotoStatement( (BoundConditionalGotoStatement)node );
                case BoundNodeKind.ReturnStatement:          return this.RewriteReturnStatement( (BoundReturnStatement)node );
                case BoundNodeKind.ExpressionStatement:      return this.RewriteExpressionStatement( (BoundExpressionStatement)node );
                default:
                    throw new Exception( $"Unexpected node: {node.Kind}" );
            }
        }

        protected virtual BoundStatement RewriteBlockStatement( BoundBlockStatement node )
        {
            var builder = default( ImmutableArray<BoundStatement>.Builder );

            for( var i = 0 ; i < node.Statements.Length ; i++ )
            {
                var oldStatement = node.Statements[ i ];
                var newStatement = this.RewriteStatement( oldStatement );

                if( newStatement != oldStatement )
                {
                    if( builder == null )
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>( node.Statements.Length );

                        for( var j = 0 ; j < i ; j++ )
                            builder.Add( node.Statements[ j ] );
                    }
                }

                if( builder != null )
                    builder.Add( newStatement );
            }

            if( builder == null )
                return node;

            return new BoundBlockStatement( builder.MoveToImmutable() );
        }

        protected virtual BoundStatement RewriteVariableDeclaration( BoundVariableDeclaration node )
        {
            var initializer = this.RewriteExpression( node.Initializer );

            if( initializer == node.Initializer )
                return node;

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
                return node;

            return new BoundIfStatement( condition , thenStatement , elseStatement );
        }

        protected virtual BoundStatement RewriteWhileStatement( BoundWhileStatement node )
        {
            var condition = this.RewriteExpression( node.Condition );
            var body = this.RewriteStatement( node.Body );
            if( condition == node.Condition &&
                body == node.Body )
                return node;

            return new BoundWhileStatement( condition , body , node.BreakLabel , node.ContinueLabel );
        }

        protected virtual BoundStatement RewriteDoWhileStatement( BoundDoWhileStatement node )
        {
            var body = this.RewriteStatement( node.Body );
            var condition = this.RewriteExpression( node.Condition );

            if( body == node.Body &&
                condition == node.Condition )
                return node;

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
                return node;

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
                return node;

            return new BoundConditionalGotoStatement( node.Label , condition , node.JumpIfTrue );
        }

        protected virtual BoundStatement RewriteReturnStatement( BoundReturnStatement node )
        {
            var expression = node.Expression == null ? null : this.RewriteExpression(node.Expression);

            if( expression == node.Expression )
                return node;

            return new BoundReturnStatement( expression );
        }

        protected virtual BoundStatement RewriteExpressionStatement( BoundExpressionStatement node )
        {
            var expression = this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
                return node;

            return new BoundExpressionStatement( expression );
        }

        public virtual BoundExpression RewriteExpression( BoundExpression node )
        {
            switch( node.Kind )
            {
                case BoundNodeKind.ErrorExpression:      return this.RewriteErrorExpression( (BoundErrorExpression)node );
                case BoundNodeKind.LiteralExpression:    return this.RewriteLiteralExpression( (BoundLiteralExpression)node );
                case BoundNodeKind.VariableExpression:   return this.RewriteVariableExpression( (BoundVariableExpression)node );
                case BoundNodeKind.AssignmentExpression: return this.RewriteAssignmentExpression( (BoundAssignmentExpression)node );
                case BoundNodeKind.UnaryExpression:      return this.RewriteUnaryExpression( (BoundUnaryExpression)node );
                case BoundNodeKind.BinaryExpression:     return this.RewriteBinaryExpression( (BoundBinaryExpression)node );
                case BoundNodeKind.CallExpression:       return this.RewriteCallExpression( (BoundCallExpression)node );
                case BoundNodeKind.ConversionExpression: return this.RewriteConversionExpression( (BoundConversionExpression)node );
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

        protected virtual BoundExpression RewriteAssignmentExpression( BoundAssignmentExpression node )
        {
            var expression = this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
                return node;

            return new BoundAssignmentExpression( node.Variable , expression );
        }

        protected virtual BoundExpression RewriteUnaryExpression( BoundUnaryExpression node )
        {
            var operand = this.RewriteExpression( node.Operand );

            if( operand == node.Operand )
                return node;

            return new BoundUnaryExpression( node.Op , operand );
        }

        protected virtual BoundExpression RewriteBinaryExpression( BoundBinaryExpression node )
        {
            var lhs = this.RewriteExpression( node.Lhs );
            var rhs = this.RewriteExpression( node.Rhs );

            if( lhs == node.Lhs &&
                rhs == node.Rhs )
                return node;

            return new BoundBinaryExpression( lhs , node.Op , rhs );
        }

        protected virtual BoundExpression RewriteCallExpression( BoundCallExpression node )
        {
            var builder = default( ImmutableArray<BoundExpression>.Builder );

            for( var i = 0 ; i < node.Arguments.Length ; i++ )
            {
                var oldArgument = node.Arguments[ i ];
                var newArgument = this.RewriteExpression( oldArgument );

                if( newArgument != oldArgument )
                {
                    if( builder == null )
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>( node.Arguments.Length );

                        for( var j = 0 ; j < i ; j++ )
                            builder.Add( node.Arguments[ j ] );
                    }
                }

                if( builder != null )
                    builder.Add( newArgument );
            }

            if( builder == null )
                return node;

            return new BoundCallExpression( node.Function , builder.MoveToImmutable() );
        }

        protected virtual BoundExpression RewriteConversionExpression( BoundConversionExpression node )
        {
            var expression = this.RewriteExpression( node.Expression );

            if( expression == node.Expression )
                return node;

            return new BoundConversionExpression( node.Type , expression );
        }
    }
}
