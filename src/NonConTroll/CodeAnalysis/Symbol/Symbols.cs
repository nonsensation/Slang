using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Symbols
{
    public abstract class FunctionSymbol : Symbol
    {
        public FunctionSymbol( string name , ImmutableArray<ParameterSymbol> parameters , TypeSymbol returnType )
            : base( name )
        {
            this.Parameters = parameters;
            this.ReturnType = returnType;
        }

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
    }

    public sealed class DeclaredFunctionSymbol : FunctionSymbol
    {
        public DeclaredFunctionSymbol( string name , ImmutableArray<ParameterSymbol> parameters , TypeSymbol returnType , FunctionDeclarationSyntax declaration )
            : base( name , parameters , returnType )
        {
            this.Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.DeclaredFunction;
        public FunctionDeclarationSyntax Declaration { get; }
    }

    public sealed class BuiltinFunctionSymbol : FunctionSymbol
    {
        public BuiltinFunctionSymbol( string name , ImmutableArray<ParameterSymbol> parameters , TypeSymbol returnType , IdentifierNameSyntax? identifierName )
            : base( name , parameters , returnType )
        {
            this.IdentifierName = identifierName;
        }

        public override SymbolKind Kind => SymbolKind.BuiltinFunction;

        public IdentifierNameSyntax? IdentifierName { get; }
    }

    public sealed class GeneratedFunctionSymbol : FunctionSymbol
    {
        public GeneratedFunctionSymbol( string name , ImmutableArray<ParameterSymbol> parameters , TypeSymbol returnType )
            : base( name , parameters , returnType )
        {

        }

        public override SymbolKind Kind => SymbolKind.BuiltinFunction;
    }

    public abstract class VariableSymbol : Symbol
    {
        public VariableSymbol( string name , bool isReadOnly , TypeSymbol type )
            : base( name )
        {
            this.IsReadOnly = isReadOnly;
            this.Type = type;
        }

        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }
    }

    public class GlobalVariableSymbol : VariableSymbol
    {
        public GlobalVariableSymbol( string name , bool isReadOnly , TypeSymbol type )
            : base( name , isReadOnly , type )
        {
        }

        public override SymbolKind Kind => SymbolKind.GlobalVariable;
    }

    public class LocalVariableSymbol : VariableSymbol
    {
        public LocalVariableSymbol( string name , bool isReadOnly , TypeSymbol type )
            : base( name , isReadOnly , type )
        {
        }

        public override SymbolKind Kind => SymbolKind.LocalVariable;
    }

    // TODO make parameter kind of standalone, not a variable
    public sealed class ParameterSymbol : LocalVariableSymbol
    {
        public ParameterSymbol( string name , TypeSymbol type )
            : base( name , isReadOnly: true , type )
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }

    public abstract class TypeSymbol : Symbol
    {
        public TypeSymbol( string name )
            : base( name )
        {
        }
    }

    public class DeclaredTypeSymbol : TypeSymbol
    {
        public DeclaredTypeSymbol( string name )
            : base( name )
        {
        }

        public override SymbolKind Kind => SymbolKind.DeclaredType;
    }

    public class BuiltinTypeSymbol : TypeSymbol
    {
        public BuiltinTypeSymbol( string name )
            : base( name )
        {
        }

        public override SymbolKind Kind => SymbolKind.BuiltinType;
    }
}
