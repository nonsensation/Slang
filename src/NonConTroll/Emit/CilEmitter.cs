using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.IO;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Emit
{
    public abstract class Emitter
    {
    }

    internal sealed class CilEmitter : Emitter
    {
        private DiagnosticBag Diagnostics = new DiagnosticBag();

        private readonly Dictionary<TypeSymbol , TypeReference> KnownTypes;

        private readonly TypeReference RandomReference;
        private readonly MethodReference DebuggableAttributeCtorReference;
        private readonly MethodReference ObjectEqualsReference;
        private readonly MethodReference ConsoleReadLineReference;
        private readonly MethodReference ConsoleWriteLineReference;
        private readonly MethodReference StringConcat2Reference;
        private readonly MethodReference StringConcat3Reference;
        private readonly MethodReference StringConcat4Reference;
        private readonly MethodReference StringConcatArrayReference;
        private readonly MethodReference ConvertToBooleanReference;
        private readonly MethodReference ConvertToInt32Reference;
        private readonly MethodReference ConvertToStringReference;
        private readonly MethodReference RandomCtorReference;
        private readonly MethodReference RandomNextReference;
        private readonly AssemblyDefinition AssemblyDefinition;
        private readonly Dictionary<FunctionSymbol , MethodDefinition> Methods = new Dictionary<FunctionSymbol , MethodDefinition>();
        private readonly Dictionary<VariableSymbol , VariableDefinition> Locals = new Dictionary<VariableSymbol , VariableDefinition>();
        private readonly Dictionary<BoundLabel , int> Labels = new Dictionary<BoundLabel , int>();
        private readonly List<(int InstructionIndex, BoundLabel Target)> Fixups = new List<(int InstructionIndex, BoundLabel Target)>();

        private TypeDefinition TypeDefinition;
        private FieldDefinition? RandomFieldDefinition;

        private CilEmitter( string moduleName , string[] references )
        {
            var assemblies = new List<AssemblyDefinition>();

            foreach( var reference in references )
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly( reference );

                    assemblies.Add( assembly );
                }
                catch( BadImageFormatException )
                {
                    this.Diagnostics.ReportInvalidReference( reference );
                }
            }

            var builtInTypes = new List<(TypeSymbol type, string MetadataName)>()
            {
                (BuiltinTypes.Any, "System.Object"),
                (BuiltinTypes.Bool, "System.Boolean"),
                (BuiltinTypes.Int, "System.Int32"),
                (BuiltinTypes.String, "System.String"),
                (BuiltinTypes.Void, "System.Void"),
            };

            var assemblyName = new AssemblyNameDefinition( moduleName , new Version( 1 , 0 ) );
            this.AssemblyDefinition = AssemblyDefinition.CreateAssembly( assemblyName , moduleName , ModuleKind.Console );
            this.KnownTypes = new Dictionary<TypeSymbol , TypeReference>();

            foreach( var (type, metadataName) in builtInTypes )
            {
                var typeReference = ResolveType( type.Name , metadataName );

                this.KnownTypes.Add( type , typeReference );
            }

            this.RandomReference            = ResolveType( null , "System.Random" );

            TypeReference ResolveType( string? minskName , string metadataName )
            {
                var foundTypes = assemblies
                    .SelectMany( a => a.Modules )
                    .SelectMany( m => m.Types )
                    .Where( t => t.FullName == metadataName )
                    .ToArray();

                if( foundTypes.Length == 1 )
                {
                    var typeReference = this.AssemblyDefinition.MainModule.ImportReference( foundTypes[ 0 ] );

                    return typeReference;
                }
                else if( foundTypes.Length == 0 )
                {
                    this.Diagnostics.ReportRequiredTypeNotFound( minskName , metadataName );
                }
                else
                {
                    this.Diagnostics.ReportRequiredTypeAmbiguous( minskName , metadataName , foundTypes );
                }

                return null!;
            }

            MethodReference ResolveMethod( string typeName , string methodName , string[] parameterTypeNames )
            {
                var foundTypes = assemblies
                    .SelectMany( a => a.Modules )
                    .SelectMany( m => m.Types )
                    .Where( t => t.FullName == typeName )
                    .ToArray();

                if( foundTypes.Length == 1 )
                {
                    var foundType = foundTypes[ 0 ];
                    var methods = foundType.Methods.Where( m => m.Name == methodName );

                    foreach( var method in methods )
                    {
                        if( method.Parameters.Count != parameterTypeNames.Length )
                        {
                            continue;
                        }

                        var allParametersMatch = true;

                        for( var i = 0 ; i < parameterTypeNames.Length ; i++ )
                        {
                            if( method.Parameters[ i ].ParameterType.FullName != parameterTypeNames[ i ] )
                            {
                                allParametersMatch = false;

                                break;
                            }
                        }

                        if( !allParametersMatch )
                        {
                            continue;
                        }

                        return this.AssemblyDefinition.MainModule.ImportReference( method );
                    }

                    this.Diagnostics.ReportRequiredMethodNotFound( typeName , methodName , parameterTypeNames );

                    return null!;
                }
                else if( foundTypes.Length == 0 )
                {
                    this.Diagnostics.ReportRequiredTypeNotFound( null , typeName );
                }
                else
                {
                    this.Diagnostics.ReportRequiredTypeAmbiguous( null , typeName , foundTypes );
                }

                return null!;
            }

            this.DebuggableAttributeCtorReference = ResolveMethod( "System.Diagnostics.DebuggableAttribute" , ".ctor"     , new[] { "System.Boolean" , "System.Boolean" }                                   );
            this.ObjectEqualsReference            = ResolveMethod( "System.Object"                          , "Equals"    , new[] { "System.Object" , "System.Object" }                                     );
            this.ConsoleReadLineReference         = ResolveMethod( "System.Console"                         , "ReadLine"  , Array.Empty<string>()                                                           );
            this.ConsoleWriteLineReference        = ResolveMethod( "System.Console"                         , "WriteLine" , new[] { "System.Object" }                                                       );
            this.StringConcat2Reference           = ResolveMethod( "System.String"                          , "Concat"    , new[] { "System.String" , "System.String" }                                     );
            this.StringConcat3Reference           = ResolveMethod( "System.String"                          , "Concat"    , new[] { "System.String" , "System.String" , "System.String" }                   );
            this.StringConcat4Reference           = ResolveMethod( "System.String"                          , "Concat"    , new[] { "System.String" , "System.String" , "System.String" , "System.String" } );
            this.StringConcatArrayReference       = ResolveMethod( "System.String"                          , "Concat"    , new[] { "System.String[]" }                                                     );
            this.ConvertToBooleanReference        = ResolveMethod( "System.Convert"                         , "ToBoolean" , new[] { "System.Object" }                                                       );
            this.ConvertToInt32Reference          = ResolveMethod( "System.Convert"                         , "ToInt32"   , new[] { "System.Object" }                                                       );
            this.ConvertToStringReference         = ResolveMethod( "System.Convert"                         , "ToString"  , new[] { "System.Object" }                                                       );
            this.RandomCtorReference              = ResolveMethod( "System.Random"                          , ".ctor"     , Array.Empty<string>()                                                           );
            this.RandomNextReference              = ResolveMethod( "System.Random"                          , "Next"      , new[] { "System.Int32" }                                                        );

            var objectType = this.KnownTypes[ BuiltinTypes.Any ];

            if( objectType != null )
            {
                var attr = TypeAttributes.Abstract | TypeAttributes.Sealed;

                this.TypeDefinition = new TypeDefinition( "" , "Program" , attr , objectType );
                this.AssemblyDefinition.MainModule.Types.Add( this.TypeDefinition );
            }
            else
            {
                this.TypeDefinition = null!;
            }
        }

        public static ImmutableArray<Diagnostic> Emit( BoundProgram program , string moduleName , string[] references , string outputPath )
        {
            if( program.Diagnostics.Any() )
            {
                return program.Diagnostics;
            }

            var emitter = new CilEmitter( moduleName , references );

            return emitter.Emit( program , outputPath );
        }

        public ImmutableArray<Diagnostic> Emit( BoundProgram program , string outputPath )
        {
            if( this.Diagnostics.Any() )
            {
                return this.Diagnostics.ToImmutableArray();
            }

            foreach( var (symbol,boundBlock) in program.Functions )
            {
                this.EmitFunctionDeclaration( symbol );
            }

            foreach( var (symbol,boundBlock) in program.Functions )
            {
                var document = new Document( boundBlock.Syntax.Location.FileName );

                this.EmitFunctionBody( symbol , boundBlock );
            }

            if( program.MainFunction != null )
            {
                this.AssemblyDefinition.EntryPoint = this.Methods[ program.MainFunction ];
            }

            { // see: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debuggableattribute
                var debuggableAttribute    = new CustomAttribute( this.DebuggableAttributeCtorReference );
                var isJITTrackingEnabled   = new CustomAttributeArgument( this.KnownTypes[ BuiltinTypes.Bool ] , true );
                var isJITOptimizerDisabled = new CustomAttributeArgument( this.KnownTypes[ BuiltinTypes.Bool ] , true );

                debuggableAttribute.ConstructorArguments.Add( isJITTrackingEnabled );
                debuggableAttribute.ConstructorArguments.Add( isJITOptimizerDisabled );

                this.AssemblyDefinition.CustomAttributes.Add( debuggableAttribute );
            }

            var symbolPath = Path.ChangeExtension( outputPath , ".pdb" );

            using var symbolStream = File.Create( symbolPath );
            using var outputStream = File.Create( outputPath );

            var writerParameters = new WriterParameters {
                WriteSymbols = true,
                SymbolStream = symbolStream ,
                SymbolWriterProvider = new PortablePdbWriterProvider() ,
            };

            this.AssemblyDefinition.Write( outputStream , writerParameters );

            return this.Diagnostics.ToImmutableArray();
        }

        private void EmitFunctionDeclaration( FunctionSymbol function )
        {
            var functionType = this.KnownTypes[ function.ReturnType ];
            var attribs = MethodAttributes.Static | MethodAttributes.Private;
            var method = new MethodDefinition( function.Name , attribs , functionType );

            foreach( var parameter in function.Parameters )
            {
                var parameterType = this.KnownTypes[ parameter.Type ];
                var parameterAttributes = ParameterAttributes.None;
                var parameterDefinition = new ParameterDefinition( parameter.Name , parameterAttributes , parameterType );

                method.Parameters.Add( parameterDefinition );
            }

            this.TypeDefinition.Methods.Add( method );
            this.Methods.Add( function , method );
        }

        private void EmitFunctionBody( FunctionSymbol function , BoundBlockStatement body )
        {
            var method = this.Methods[ function ];

            this.Locals.Clear();
            this.Labels.Clear();
            this.Fixups.Clear();

            var ilProcessor = method.Body.GetILProcessor();

            foreach( var statement in body.Statements )
            {
                this.EmitStatement( ilProcessor , statement );
            }

            foreach( var fixup in this.Fixups )
            {
                var targetLabel = fixup.Target;
                var targetInstructionIndex = this.Labels[ targetLabel ];
                var targetInstruction = ilProcessor.Body.Instructions[ targetInstructionIndex ];
                var instructionToFixup = ilProcessor.Body.Instructions[ fixup.InstructionIndex ];

                instructionToFixup.Operand = targetInstruction;
            }

            method.Body.OptimizeMacros();
        }

        private void EmitStatement( ILProcessor ilProcessor , BoundStatement node )
        {
            switch( node.Kind )
            {
                case BoundNodeKind.NopStatement:
                    EmitNopStatement( ilProcessor , (BoundNopStatement)node );
                    break;
                case BoundNodeKind.VariableDeclaration:
                    EmitVariableDeclaration( ilProcessor , (BoundVariableDeclaration)node );
                    break;
                case BoundNodeKind.LabelStatement:
                    EmitLabelStatement( ilProcessor , (BoundLabelStatement)node );
                    break;
                case BoundNodeKind.GotoStatement:
                    EmitGotoStatement( ilProcessor , (BoundGotoStatement)node );
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    EmitConditionalGotoStatement( ilProcessor , (BoundConditionalGotoStatement)node );
                    break;
                case BoundNodeKind.ReturnStatement:
                    EmitReturnStatement( ilProcessor , (BoundReturnStatement)node );
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EmitExpressionStatement( ilProcessor , (BoundExpressionStatement)node );
                    break;
                case BoundNodeKind.SequencePointStatement:
                    EmitSequencePointStatement( ilProcessor , (BoundSequencePointStatement)node );
                    break;
                default:
                    throw new Exception( $"Unexpected node kind {node.Kind}" );
            }
        }

        private void EmitNopStatement( ILProcessor ilProcessor , BoundNopStatement node )
        {
            ilProcessor.Emit( OpCodes.Nop );
        }

        private void EmitVariableDeclaration( ILProcessor ilProcessor , BoundVariableDeclaration node )
        {
            var typeReference = this.KnownTypes[ node.Variable.Type ];
            var variableDefinition = new VariableDefinition( typeReference );

            this.Locals.Add( node.Variable , variableDefinition );

            ilProcessor.Body.Variables.Add( variableDefinition );

            this.EmitExpression( ilProcessor , node.Initializer );

            ilProcessor.Emit( OpCodes.Stloc , variableDefinition );
        }

        private void EmitLabelStatement( ILProcessor ilProcessor , BoundLabelStatement node )
        {
            this.Labels.Add( node.Label , ilProcessor.Body.Instructions.Count );
        }

        private void EmitGotoStatement( ILProcessor ilProcessor , BoundGotoStatement node )
        {
            this.Fixups.Add( (ilProcessor.Body.Instructions.Count, node.Label) );

            ilProcessor.Emit( OpCodes.Br , Instruction.Create( OpCodes.Nop ) );
        }

        private void EmitConditionalGotoStatement( ILProcessor ilProcessor , BoundConditionalGotoStatement node )
        {
            this.EmitExpression( ilProcessor , node.Condition );

            var opCode = node.JumpIfTrue ? OpCodes.Brtrue : OpCodes.Brfalse;

            this.Fixups.Add( (ilProcessor.Body.Instructions.Count, node.Label) );

            ilProcessor.Emit( opCode , Instruction.Create( OpCodes.Nop ) );
        }

        private void EmitReturnStatement( ILProcessor ilProcessor , BoundReturnStatement node )
        {
            if( node.Expression != null )
            {
                this.EmitExpression( ilProcessor , node.Expression );
            }

            ilProcessor.Emit( OpCodes.Ret );
        }

        private void EmitExpressionStatement( ILProcessor ilProcessor , BoundExpressionStatement node )
        {
            this.EmitExpression( ilProcessor , node.Expression );

            if( node.Expression.Type != BuiltinTypes.Void )
            {
                ilProcessor.Emit( OpCodes.Pop );
            }
        }

        private void EmitExpression( ILProcessor ilProcessor , BoundExpression node )
        {
            if( node.ConstantValue != null )
            {
                EmitConstantExpression( ilProcessor , node );

                return;
            }

            switch( node )
            {
                case BoundVariableExpression   boundExpr: this.EmitVariableExpression  ( ilProcessor , boundExpr ); break;
                case BoundAssignmentExpression boundExpr: this.EmitAssignmentExpression( ilProcessor , boundExpr ); break;
                case BoundUnaryExpression      boundExpr: this.EmitUnaryExpression     ( ilProcessor , boundExpr ); break;
                case BoundBinaryExpression     boundExpr: this.EmitBinaryExpression    ( ilProcessor , boundExpr ); break;
                case BoundCallExpression       boundExpr: this.EmitCallExpression      ( ilProcessor , boundExpr ); break;
                case BoundConversionExpression boundExpr: this.EmitConversionExpression( ilProcessor , boundExpr ); break;
                default:
                    throw new Exception( $"Unexpected node kind {node.Kind}" );
            }
        }

        private void EmitConstantExpression( ILProcessor ilProcessor , BoundExpression node )
        {
            Debug.Assert( node.ConstantValue != null );

            if( node.Type == BuiltinTypes.Bool )
            {
                var value = (bool)node.ConstantValue.Value;
                var instruction = value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;

                ilProcessor.Emit( instruction );
            }
            else if( node.Type == BuiltinTypes.Int )
            {
                var value = (int)node.ConstantValue.Value;

                ilProcessor.Emit( OpCodes.Ldc_I4 , value );
            }
            else if( node.Type == BuiltinTypes.String )
            {
                var value = (string)node.ConstantValue.Value;

                ilProcessor.Emit( OpCodes.Ldstr , value );
            }
            else
            {
                throw new Exception( $"Unexpected constant expression type: {node.Type}" );
            }
        }

        private void EmitVariableExpression( ILProcessor ilProcessor , BoundVariableExpression node )
        {
            if( node.Variable is ParameterSymbol parameter )
            {
                ilProcessor.Emit( OpCodes.Ldarg , parameter.Ordinal );
            }
            else
            {
                var variableDefinition = this.Locals[node.Variable];

                ilProcessor.Emit( OpCodes.Ldloc , variableDefinition );
            }
        }

        private void EmitAssignmentExpression( ILProcessor ilProcessor , BoundAssignmentExpression node )
        {
            var variableDefinition = this.Locals[ node.Variable ];

            this.EmitExpression( ilProcessor , node.Expression );

            ilProcessor.Emit( OpCodes.Dup );
            ilProcessor.Emit( OpCodes.Stloc , variableDefinition );
        }

        private void EmitUnaryExpression( ILProcessor ilProcessor , BoundUnaryExpression node )
        {
            this.EmitExpression( ilProcessor , node.Expression );

            if( node.Operator.Kind == BoundUnaryOperatorKind.Identity )
            {
                // Done
            }
            else if( node.Operator.Kind == BoundUnaryOperatorKind.LogicalNegation )
            {
                ilProcessor.Emit( OpCodes.Ldc_I4_0 );
                ilProcessor.Emit( OpCodes.Ceq );
            }
            else if( node.Operator.Kind == BoundUnaryOperatorKind.Negation )
            {
                ilProcessor.Emit( OpCodes.Neg );
            }
            else if( node.Operator.Kind == BoundUnaryOperatorKind.OnesComplement )
            {
                ilProcessor.Emit( OpCodes.Not );
            }
            else
            {
                throw new Exception( $"Unexpected unary operator {SyntaxInfo.GetText( node.Operator.TkType )}({node.Expression.Type})" );
            }
        }

        private void EmitBinaryExpression( ILProcessor ilProcessor , BoundBinaryExpression node )
        {
            // +(string, string)

            if( node.Operator.Kind == BoundBinaryOperatorKind.Addition )
            {
                if( node.Lhs.Type == BuiltinTypes.String && node.Rhs.Type == BuiltinTypes.String )
                {
                    this.EmitStringConcatExpression( ilProcessor , node );

                    return;
                }
            }

            this.EmitExpression( ilProcessor , node.Lhs );
            this.EmitExpression( ilProcessor , node.Rhs );

            // ==(any, any)
            // ==(string, string)

            if( node.Operator.Kind == BoundBinaryOperatorKind.Equals )
            {
                if( node.Lhs.Type == BuiltinTypes.Any    && node.Rhs.Type == BuiltinTypes.Any ||
                    node.Lhs.Type == BuiltinTypes.String && node.Rhs.Type == BuiltinTypes.String )
                {
                    ilProcessor.Emit( OpCodes.Call , this.ObjectEqualsReference );

                    return;
                }
            }

            // !=(any, any)
            // !=(string, string)

            if( node.Operator.Kind == BoundBinaryOperatorKind.NotEquals )
            {
                if( node.Lhs.Type == BuiltinTypes.Any    && node.Rhs.Type == BuiltinTypes.Any ||
                    node.Lhs.Type == BuiltinTypes.String && node.Rhs.Type == BuiltinTypes.String )
                {
                    ilProcessor.Emit( OpCodes.Call , this.ObjectEqualsReference );
                    ilProcessor.Emit( OpCodes.Ldc_I4_0 );
                    ilProcessor.Emit( OpCodes.Ceq );

                    return;
                }
            }

            switch( node.Operator.Kind )
            {
                case BoundBinaryOperatorKind.Addition:
                    ilProcessor.Emit( OpCodes.Add );
                    break;
                case BoundBinaryOperatorKind.Subtraction:
                    ilProcessor.Emit( OpCodes.Sub );
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    ilProcessor.Emit( OpCodes.Mul );
                    break;
                case BoundBinaryOperatorKind.Division:
                    ilProcessor.Emit( OpCodes.Div );
                    break;
                // TODO: Implement short-circuit evaluation
                case BoundBinaryOperatorKind.LogicalAnd:
                    ilProcessor.Emit( OpCodes.And );
                    break;
                // TODO: Implement short-circuit evaluation
                case BoundBinaryOperatorKind.LogicalOr:
                    ilProcessor.Emit( OpCodes.Or );
                    break;
                case BoundBinaryOperatorKind.Equals:
                    ilProcessor.Emit( OpCodes.Ceq );
                    break;
                case BoundBinaryOperatorKind.NotEquals:
                    ilProcessor.Emit( OpCodes.Ceq );
                    ilProcessor.Emit( OpCodes.Ldc_I4_0 );
                    ilProcessor.Emit( OpCodes.Ceq );
                    break;
                case BoundBinaryOperatorKind.Less:
                    ilProcessor.Emit( OpCodes.Clt );
                    break;
                case BoundBinaryOperatorKind.LessOrEquals:
                    ilProcessor.Emit( OpCodes.Cgt );
                    ilProcessor.Emit( OpCodes.Ldc_I4_0 );
                    ilProcessor.Emit( OpCodes.Ceq );
                    break;
                case BoundBinaryOperatorKind.Greater:
                    ilProcessor.Emit( OpCodes.Cgt );
                    break;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    ilProcessor.Emit( OpCodes.Clt );
                    ilProcessor.Emit( OpCodes.Ldc_I4_0 );
                    ilProcessor.Emit( OpCodes.Ceq );
                    break;
                default:
                    throw new Exception( $"Unexpected binary operator {SyntaxInfo.GetText( node.Operator.TkType )}({node.Lhs.Type}, {node.Rhs.Type})" );
            }
        }

        private void EmitStringConcatExpression( ILProcessor ilProcessor , BoundBinaryExpression node )
        {
            // Flatten the expression tree to a sequence of nodes to concatenate, then fold consecutive constants in that sequence.
            // This approach enables constant folding of non-sibling nodes, which cannot be done in the ConstantFolding class as it would require changing the tree.
            // Example: folding b and c in ((a + b) + c) if they are constant.

            var nodes = FoldConstants( node.Syntax , Flatten( node ) ).ToList();

            switch( nodes.Count )
            {
                case 0:
                    ilProcessor.Emit( OpCodes.Ldstr , string.Empty );
                    break;

                case 1:
                    this.EmitExpression( ilProcessor , nodes[ 0 ] );
                    break;

                case 2:
                    EmitExpression( ilProcessor , nodes[ 0 ] );
                    EmitExpression( ilProcessor , nodes[ 1 ] );
                    ilProcessor.Emit( OpCodes.Call , this.StringConcat2Reference );
                    break;

                case 3:
                    EmitExpression( ilProcessor , nodes[ 0 ] );
                    EmitExpression( ilProcessor , nodes[ 1 ] );
                    EmitExpression( ilProcessor , nodes[ 2 ] );
                    ilProcessor.Emit( OpCodes.Call , this.StringConcat3Reference );
                    break;

                case 4:
                    EmitExpression( ilProcessor , nodes[ 0 ] );
                    EmitExpression( ilProcessor , nodes[ 1 ] );
                    EmitExpression( ilProcessor , nodes[ 2 ] );
                    EmitExpression( ilProcessor , nodes[ 3 ] );
                    ilProcessor.Emit( OpCodes.Call , this.StringConcat4Reference );
                    break;

                default:
                    ilProcessor.Emit( OpCodes.Ldc_I4 , nodes.Count );
                    ilProcessor.Emit( OpCodes.Newarr , this.KnownTypes[ BuiltinTypes.String ] );

                    for( var i = 0 ; i < nodes.Count ; i++ )
                    {
                        ilProcessor.Emit( OpCodes.Dup );
                        ilProcessor.Emit( OpCodes.Ldc_I4 , i );
                        EmitExpression( ilProcessor , nodes[ i ] );
                        ilProcessor.Emit( OpCodes.Stelem_Ref );
                    }

                    ilProcessor.Emit( OpCodes.Call , this.StringConcatArrayReference );
                    break;
            }

            // (a + b) + (c + d) --> [a, b, c, d]
            static IEnumerable<BoundExpression> Flatten( BoundExpression node )
            {
                if( node is BoundBinaryExpression binaryExpression &&
                    binaryExpression.Operator.Kind == BoundBinaryOperatorKind.Addition &&
                    binaryExpression.Lhs.Type == BuiltinTypes.String &&
                    binaryExpression.Rhs.Type == BuiltinTypes.String )
                {
                    foreach( var result in Flatten( binaryExpression.Lhs ) )
                    {
                        yield return result;
                    }

                    foreach( var result in Flatten( binaryExpression.Rhs ) )
                    {
                        yield return result;
                    }
                }
                else
                {
                    if( node.Type != BuiltinTypes.String )
                    {
                        throw new Exception( $"Unexpected node type in string concatenation: {node.Type}" );
                    }

                    yield return node;
                }
            }

            // [a, "foo", "bar", b, ""] --> [a, "foobar", b]
            static IEnumerable<BoundExpression> FoldConstants( SyntaxNode syntax , IEnumerable<BoundExpression> nodes )
            {
                StringBuilder? sb = null;

                foreach( var node in nodes )
                {
                    if( node.ConstantValue != null )
                    {
                        var stringValue = (string)node.ConstantValue.Value;

                        if( string.IsNullOrEmpty( stringValue ) )
                        {
                            continue;
                        }

                        sb ??= new StringBuilder();
                        sb.Append( stringValue );
                    }
                    else
                    {
                        if( sb?.Length > 0 )
                        {
                            yield return new BoundLiteralExpression( syntax , sb.ToString() );

                            sb.Clear();
                        }

                        yield return node;
                    }
                }

                if( sb?.Length > 0 )
                {
                    yield return new BoundLiteralExpression( syntax , sb.ToString() );
                }
            }
        }

        private void EmitCallExpression( ILProcessor ilProcessor , BoundCallExpression node )
        {
            if( node.Function == BuiltinFunctions.Rnd )
            {
                if( this.RandomFieldDefinition == null )
                {
                    this.EmitRandomField();
                }

                ilProcessor.Emit( OpCodes.Ldsfld , this.RandomFieldDefinition );

                foreach( var argument in node.Arguments )
                {
                    this.EmitExpression( ilProcessor , argument );
                }

                ilProcessor.Emit( OpCodes.Callvirt , this.RandomNextReference );

                return;
            }

            foreach( var argument in node.Arguments )
            {
                EmitExpression( ilProcessor , argument );
            }

            if( node.Function == BuiltinFunctions.Input )
            {
                ilProcessor.Emit( OpCodes.Call , this.ConsoleReadLineReference );
            }
            else if( node.Function == BuiltinFunctions.Print )
            {
                ilProcessor.Emit( OpCodes.Call , this.ConsoleWriteLineReference );
            }
            else
            {
                var methodDefinition = this.Methods[ node.Function ];

                ilProcessor.Emit( OpCodes.Call , methodDefinition );
            }
        }

        private void EmitRandomField()
        {
            this.RandomFieldDefinition = new FieldDefinition(
                "$rnd" ,
                FieldAttributes.Static | FieldAttributes.Private ,
                this.RandomReference
            );
            this.TypeDefinition.Fields.Add( this.RandomFieldDefinition );

            var staticConstructor = new MethodDefinition(
                ".cctor" ,
                MethodAttributes.Static |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName ,
                this.KnownTypes[ BuiltinTypes.Void ]
            );
            this.TypeDefinition.Methods.Insert( 0 , staticConstructor );

            var ilProcessor = staticConstructor.Body.GetILProcessor();

            ilProcessor.Emit( OpCodes.Newobj , this.RandomCtorReference );
            ilProcessor.Emit( OpCodes.Stsfld , this.RandomFieldDefinition );
            ilProcessor.Emit( OpCodes.Ret );
        }

        private void EmitConversionExpression( ILProcessor ilProcessor , BoundConversionExpression node )
        {
            this.EmitExpression( ilProcessor , node.Expression );

            var needsBoxing = node.Expression.Type == BuiltinTypes.Bool ||
                              node.Expression.Type == BuiltinTypes.Int;

            if( needsBoxing )
            {
                ilProcessor.Emit( OpCodes.Box , this.KnownTypes[ node.Expression.Type ] );
            }

            if( node.Type == BuiltinTypes.Any )
            {
                // Done
            }
            else if( node.Type == BuiltinTypes.Bool )
            {
                ilProcessor.Emit( OpCodes.Call , this.ConvertToBooleanReference );
            }
            else if( node.Type == BuiltinTypes.Int )
            {
                ilProcessor.Emit( OpCodes.Call , this.ConvertToInt32Reference );
            }
            else if( node.Type == BuiltinTypes.String )
            {
                ilProcessor.Emit( OpCodes.Call , this.ConvertToStringReference );
            }
            else
            {
                throw new Exception( $"Unexpected convertion from {node.Expression.Type} to {node.Type}" );
            }
        }

        private SequencePoint CreateSequencePoint( TextLocation location , Instruction instruction , Document document )
        {
            return new SequencePoint( instruction , document ) {
                StartLine   = location.StartLine + 1 ,
                StartColumn = location.StartCharacter + 1 ,
                EndLine     = location.EndLine + 1 ,
                EndColumn   = location.EndCharacter + 1 ,
            };
        }

        private Dictionary<SourceText,Document> Documents = new Dictionary<SourceText, Document>();

        private void EmitSequencePointStatement( ILProcessor ilProcessor , BoundSequencePointStatement node )
        {
            var index = ilProcessor.Body.Instructions.Count();
            var location = node.Location;

            this.EmitStatement( ilProcessor , node.Statement );

            if( !this.Documents.TryGetValue( location.Text , out var document ) )
            {
                var fullPath = Path.GetFullPath( location.Text.FileName );

                document = new Document( fullPath );

                this.Documents.Add( location.Text , document );
            }

            var instruction = ilProcessor.Body.Instructions[ index ];
            var sequencePoint = CreateSequencePoint( location , instruction , document );

            ilProcessor.Body.Method.DebugInformation.SequencePoints.Add( sequencePoint );
        }
    }
}
