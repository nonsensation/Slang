using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax( SyntaxToken colonToken , TypeNameSyntax typeName )
        {
            this.ColonToken = colonToken;
            this.TypeName   = typeName;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; }
        public TypeNameSyntax TypeName { get; }
    }


    public abstract class TypeSyntax : ExpressionSyntax
    {
    }

    public class TypeNameSyntax : TypeSyntax
    {
        public TypeNameSyntax( SyntaxToken? identifier , ImmutableArray<TypeSpecifierSyntax> typeSpecifier , ImmutableArray<ArrayTypeSpecifierSyntax> arrayTypeSpecifier )
        {
            this.Identifier         = identifier;
            this.TypeSpecifier      = typeSpecifier;
            this.ArrayTypeSpecifier = arrayTypeSpecifier;
        }

        public SyntaxToken? Identifier { get; }
        public ImmutableArray<TypeSpecifierSyntax> TypeSpecifier { get; }
        public ImmutableArray<ArrayTypeSpecifierSyntax> ArrayTypeSpecifier { get; }

        public override SyntaxKind Kind => SyntaxKind.Type;
    }

    public class TupleTypeSyntax : TypeSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.TupleType;

    }

    public class TupleTypeElementSyntax : TypeSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.TupleElementType;

    }

    public class ArrayTypeSpecifierSyntax : SyntaxNode
    {
        public ArrayTypeSpecifierSyntax( SyntaxToken openBracket , ExpressionSyntax rankExpression , SyntaxToken closeBracket )
        {
            this.OpenBracket    = openBracket;
            this.RankExpression = rankExpression;
            this.CloseBracket   = closeBracket;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayTypeSpecifier;

        public SyntaxToken OpenBracket { get; }
        public ExpressionSyntax RankExpression { get; }
        public SyntaxToken CloseBracket { get; }

    }

    public class TypeSpecifierSyntax : SyntaxNode
    {
        public TypeSpecifierSyntax( SyntaxToken specifierToken , TypeSpecifierKind specifierKind )
        {
            this.SpecifierToken = specifierToken;
            this.SpecifierKind  = specifierKind;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeSpecifier;

        public SyntaxToken SpecifierToken { get; }
        public TypeSpecifierKind SpecifierKind { get; }

    }

    public enum TypeSpecifierKind
    {
        Reference,
        Pointer,
        Nullable,
    }

    public class TupleExpressionSyntax : ExpressionSyntax
    {
        public TupleExpressionSyntax( SyntaxToken openParen , SeparatedSyntaxList<IdentifierNameSyntax> arguments , SyntaxToken closeParen )
        {
            this.OpenParen  = openParen;
            this.Arguments  = arguments;
            this.CloseParen = closeParen;
        }

        public override SyntaxKind Kind => SyntaxKind.TupleExpression;

        public SyntaxToken OpenParen { get; }
        public SeparatedSyntaxList<IdentifierNameSyntax> Arguments { get; }
        public SyntaxToken CloseParen { get; }

    }

    public class DiscardSyntax : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Discard;

    }

    public class AnnotationSyntax : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Annotation;

    }

    public class DeferStatementSyntax : StatementSyntax
    {
        public DeferStatementSyntax( SyntaxToken deferKeyword , ExpressionSyntax expression )
        {
            this.DeferKeyword = deferKeyword;
            this.Expression   = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.DeferStatement;

        public SyntaxToken DeferKeyword { get; }
        public ExpressionSyntax Expression { get; }
    }

}
