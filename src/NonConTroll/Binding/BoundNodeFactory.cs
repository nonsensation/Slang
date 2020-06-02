using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using System.Diagnostics;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal static class BoundNodeFactory
    {
        public static BoundVariableDeclaration ConstantDeclaration( SyntaxNode syntax , string name , BoundExpression initExpr )
            => VariableDeclarationInternal( syntax , name , initExpr , isReadOnly: true );

        public static BoundVariableDeclaration VariableDeclaration( SyntaxNode syntax , string name , BoundExpression initExpr )
            => VariableDeclarationInternal( syntax , name , initExpr , isReadOnly: false );

        public static BoundVariableDeclaration VariableDeclarationInternal( SyntaxNode syntax , string name , BoundExpression initExpr , bool isReadOnly )
        {
            var symbol = new LocalVariableSymbol( name , isReadOnly , initExpr.Type , initExpr.ConstantValue );

            return new BoundVariableDeclaration( syntax , symbol , initExpr );
        }

        public static BoundVariableDeclaration VariableDeclaration( SyntaxNode syntax , VariableSymbol symbol , BoundExpression initExpr )
        {
            return new BoundVariableDeclaration( syntax , symbol , initExpr );
        }

        public static BoundBlockStatement Block( SyntaxNode syntax , params BoundStatement[] stmts )
        {
            return new BoundBlockStatement( syntax , ImmutableArray.Create( stmts ) );
        }

        public static BoundStatement If( SyntaxNode syntax , BoundExpression condition , BoundStatement thenStmt , BoundLabel endLabel )
        {
            return Block( syntax ,
                          GotoFalse( syntax , endLabel , condition ) ,
                          thenStmt ,
                          Label( syntax , endLabel ) );
        }

        public static BoundStatement If( SyntaxNode syntax , BoundExpression condition , BoundStatement thenStmt , BoundStatement? elseStmt , BoundLabel elseLabel , BoundLabel endLabel )
        {
            if( elseStmt == null )
            {
                return If( syntax , condition , thenStmt , endLabel );
            }

            return Block( syntax ,
                          GotoFalse( syntax , elseLabel , condition ) ,
                          thenStmt ,
                          Goto( syntax , endLabel ) ,
                          Label( syntax , elseLabel ) ,
                          elseStmt ,
                          Label( syntax , endLabel ) );
        }

        public static BoundGotoStatement Goto( SyntaxNode syntax , BoundLabel label )
        {

            return new BoundGotoStatement( syntax , label );
        }

        public static BoundConditionalGotoStatement GotoTrue( SyntaxNode syntax , BoundLabel label , BoundExpression condition )
        {
            return new BoundConditionalGotoStatement( syntax , label , condition , true );
        }

        public static BoundConditionalGotoStatement GotoFalse( SyntaxNode syntax , BoundLabel label , BoundExpression condition )
        {
            return new BoundConditionalGotoStatement( syntax , label , condition , false );
        }

        public static BoundLabelStatement Label( SyntaxNode syntax , BoundLabel label )
        {

            return new BoundLabelStatement( syntax , label );
        }

        public static BoundWhileStatement While( SyntaxNode syntax , BoundExpression condition , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
        {
            return new BoundWhileStatement( syntax , condition , body , breakLabel , continueLabel );
        }

        public static BoundExpressionStatement Increment( SyntaxNode syntax , BoundVariableExpression varExpr )
        {
            var incrByOne = Add( syntax , varExpr , Literal( syntax , 1 ) );
            var incrAssign = new BoundAssignmentExpression( syntax , varExpr.Variable , incrByOne );

            return new BoundExpressionStatement( syntax , incrAssign );
        }

        public static BoundExpressionStatement Decrement( SyntaxNode syntax , BoundVariableExpression varExpr )
        {
            var incrByOne = Sub( syntax , varExpr , Literal( syntax , 1 ) );
            var incrAssign = new BoundAssignmentExpression( syntax , varExpr.Variable , incrByOne );

            return new BoundExpressionStatement( syntax , incrAssign );
        }

        public static BoundUnaryExpression Not( SyntaxNode syntax , BoundExpression expr )
            => Unary( syntax , SyntaxKind.ExmToken , expr );
        public static BoundUnaryExpression Negate( SyntaxNode syntax , BoundExpression expr )
            => Unary( syntax , SyntaxKind.MinusToken , expr );

        public static BoundBinaryExpression Equal( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.EqEqToken , rhs );
        public static BoundBinaryExpression NotEqual( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.ExmEqToken , rhs );
        public static BoundBinaryExpression Add( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.PlusToken , rhs );
        public static BoundBinaryExpression Sub( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.MinusToken , rhs );
        public static BoundBinaryExpression Mul( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.StarToken , rhs );
        public static BoundBinaryExpression Div( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.SlashToken , rhs );
        public static BoundBinaryExpression Greater( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.GtToken , rhs );
        public static BoundBinaryExpression GreaterOrEqual( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.GtEqToken , rhs );
        public static BoundBinaryExpression Less( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.LtToken , rhs );
        public static BoundBinaryExpression LessOrEqual( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.LtEqToken , rhs );
        public static BoundBinaryExpression Or( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.PipePipeToken , rhs );
        public static BoundBinaryExpression And( SyntaxNode syntax , BoundExpression lhs , BoundExpression rhs )
            => Binary( syntax , lhs , SyntaxKind.AndAndToken , rhs );

        public static BoundBinaryExpression Binary( SyntaxNode syntax , BoundExpression lhs , SyntaxKind kind , BoundExpression rhs )
        {
            var op = BoundBinaryOperator.Bind( kind , lhs.Type , rhs.Type )!;


            return new BoundBinaryExpression( syntax , lhs , op , rhs );
        }

        public static BoundUnaryExpression Unary( SyntaxNode syntax , SyntaxKind kind , BoundExpression expr )
        {
            var op = BoundUnaryOperator.Bind( kind , expr.Type )!;


            return new BoundUnaryExpression( syntax , op , expr );
        }

        public static BoundLiteralExpression Literal( SyntaxNode syntax , object literal )
        {
            Debug.Assert( literal is string || literal is bool || literal is int );


            return new BoundLiteralExpression( syntax , literal );
        }

        public static BoundVariableExpression Variable( SyntaxNode syntax , VariableSymbol symbol )
        {

            return new BoundVariableExpression( syntax , symbol );
        }

        public static BoundVariableExpression Variable( SyntaxNode syntax , BoundVariableDeclaration varDecl )
        {

            return new BoundVariableExpression( syntax , varDecl.Variable );
        }
    }
}
