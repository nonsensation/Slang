namespace NonConTroll.CodeAnalysis.Syntax
{
    public class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax( SyntaxToken functionKeyword , SyntaxToken identifier , SyntaxToken openParenthesisToken , SeparatedSyntaxList<ParameterSyntax> parameters , SyntaxToken closeParenthesisToken , TypeClauseSyntax? returnType , BlockStatementSyntax body )
        {
            this.FunctionKeyword       = functionKeyword;
            this.Identifier            = identifier;
            this.OpenParenthesisToken  = openParenthesisToken;
            this.Parameters            = parameters;
            this.CloseParenthesisToken = closeParenthesisToken;
            this.ReturnType            = returnType;
            this.Body                  = body;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

        public SyntaxToken FunctionKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public TypeClauseSyntax? ReturnType { get; }
        public BlockStatementSyntax Body { get; }
    }
}
