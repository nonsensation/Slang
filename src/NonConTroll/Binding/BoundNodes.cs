using System;
using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Binding
{

    internal sealed class BoundNopStatement : BoundStatement
    {
        public BoundNopStatement( SyntaxNode syntax )
            : base( BoundNodeKind.NopStatement , syntax )
        {
        }
    }

    internal sealed class BoundSequencePointStatement : BoundStatement
    {
        public BoundSequencePointStatement( SyntaxNode syntax , BoundStatement statement , TextLocation location )
            : base( BoundNodeKind.SequencePointStatement , syntax )
        {
            this.Statement = statement;
            this.Location  = location;
        }

        public BoundStatement Statement { get; }
        public TextLocation Location { get; }
    }

    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement( SyntaxNode syntax , BoundExpression condition , BoundStatement thenStatement , BoundStatement? elseStatement )
            : base( BoundNodeKind.IfStatement , syntax )
        {
            this.Condition = condition;
            this.ThenStatement = thenStatement;
            this.ElseStatement = elseStatement;
        }

        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement? ElseStatement { get; }
    }

    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression( SyntaxNode syntax , object value )
            : base( BoundNodeKind.LiteralExpression , syntax )
        {
            this.ConstantValue = new BoundConstant( value );

            switch( value )
            {
                case bool _:   this.Type = BuiltinTypes.Bool  ; break;
                case int _:    this.Type = BuiltinTypes.Int   ; break;
                case string _: this.Type = BuiltinTypes.String; break;
                default:
                    throw new Exception( $"Unexpected literal '{value}' of type {value.GetType()}" );
            }
        }

        public override TypeSymbol Type { get; }
        public override BoundConstant ConstantValue { get; }

        public object Value => this.ConstantValue.Value;
    }

    internal sealed partial class BoundMatchExpression : BoundExpression
    {
        public BoundMatchExpression( SyntaxNode syntax , BoundExpression expression , ImmutableArray<BoundPatternSectionExpression> patternSections )
            : base( BoundNodeKind.MatchExpression , syntax )
        {
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public override TypeSymbol Type =>
            // TODO: get common type, if this.IsStatement == false
            (this.PatternSections.FirstOrDefault()?.Expression as BoundExpression)?.Type
            ?? BuiltinTypes.Void;

        public BoundPatternSectionExpression? DefaultPattern => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public BoundExpression Expression { get; }
        public ImmutableArray<BoundPatternSectionExpression> PatternSections { get; }
    }

    internal sealed partial class BoundMatchStatement : BoundStatement
    {
        public BoundMatchStatement( SyntaxNode syntax , BoundExpression expression , ImmutableArray<BoundPatternSectionStatement> patternSections )
            : base( BoundNodeKind.MatchStatement , syntax )
        {
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public BoundPatternSectionStatement? DefaultPattern => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public BoundExpression Expression { get; }
        public ImmutableArray<BoundPatternSectionStatement> PatternSections { get; }
    }

    internal sealed partial class BoundPatternSectionExpression : BoundExpression
    {
        public BoundPatternSectionExpression( SyntaxNode syntax , ImmutableArray<BoundPattern> patterns , BoundExpression expression )
            : base( BoundNodeKind.PatternSectionExpression , syntax )
        {
            this.Patterns   = patterns;
            this.Expression = expression;
        }

        public override TypeSymbol Type => this.Expression.Type;

        public ImmutableArray<BoundPattern> Patterns { get; }
        public BoundExpression Expression { get; }
        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == BoundNodeKind.MatchAnyPattern );
    }

    internal sealed partial class BoundPatternSectionStatement : BoundStatement
    {
        public BoundPatternSectionStatement( SyntaxNode syntax , ImmutableArray<BoundPattern> patterns , BoundStatement statement )
            : base( BoundNodeKind.PatternSectionStatement , syntax )
        {
            this.Patterns  = patterns;
            this.Statement = statement;
        }

        public ImmutableArray<BoundPattern> Patterns { get; }
        public BoundStatement Statement { get; }
        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == BoundNodeKind.MatchAnyPattern );
    }

    internal abstract class BoundPattern : BoundNode
    {
        public BoundPattern( BoundNodeKind kind , SyntaxNode syntax )
            : base( kind , syntax )
        {

        }
    }

    internal sealed partial class BoundMatchAnyPattern : BoundPattern
    {
        public BoundMatchAnyPattern( SyntaxNode syntax )
            : base( BoundNodeKind.MatchAnyPattern , syntax )
        {

        }
    }

    internal sealed partial class BoundConstantPattern : BoundPattern
    {
        public BoundConstantPattern( SyntaxNode syntax , BoundExpression expression )
            : base( BoundNodeKind.ConstantPattern , syntax )
        {
            this.Expression = expression;
        }

        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundInfixPattern : BoundPattern
    {
        public BoundInfixPattern( SyntaxNode syntax , DeclaredFunctionSymbol infixFunction , BoundExpression expression )
            : base( BoundNodeKind.InfixPattern , syntax )
        {
            this.InfixFunction = infixFunction;
            this.Expression    = expression;
        }

        public DeclaredFunctionSymbol InfixFunction { get; }
        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression( SyntaxNode syntax , VariableSymbol variable , BoundExpression expression )
            : base( BoundNodeKind.AssignmentExpression , syntax )
        {
            this.Variable   = variable;
            this.Expression = expression;
        }

        public override TypeSymbol Type => this.Expression.Type;

        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }

    internal abstract class BoundExpression : BoundNode
    {
        public BoundExpression( BoundNodeKind kind , SyntaxNode syntax )
            : base( kind , syntax )
        {

        }

        public abstract TypeSymbol Type { get; }

        public virtual BoundConstant? ConstantValue => null;
    }

    internal sealed partial class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression( SyntaxNode syntax , TypeSymbol type , BoundExpression expression )
            : base( BoundNodeKind.ConversionExpression , syntax )
        {
            this.Type       = type;
            this.Expression = expression;
        }

        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundDeferStatement : BoundStatement
    {
        public BoundDeferStatement( SyntaxNode syntax , BoundExpression expression )
            : base( BoundNodeKind.DeferStatement , syntax )
        {
            this.Expression = expression;
        }

        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundErrorExpression : BoundExpression
    {
        public BoundErrorExpression( SyntaxNode syntax )
            : base( BoundNodeKind.ErrorExpression , syntax )
        {

        }

        public override TypeSymbol Type => BuiltinTypes.Error;
    }

    internal sealed partial class BoundDoWhileStatement : BoundLoopStatement
    {
        public BoundDoWhileStatement( SyntaxNode syntax , BoundStatement body , BoundExpression condition , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( BoundNodeKind.DoWhileStatement , syntax  , breakLabel , continueLabel )
        {
            this.Body      = body;
            this.Condition = condition;
        }

        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
    }

    internal sealed partial class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement( SyntaxNode syntax , VariableSymbol variable , BoundExpression lowerBound , BoundExpression upperBound , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( BoundNodeKind.ForStatement , syntax  , breakLabel , continueLabel )
        {
            this.Variable   = variable;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.Body       = body;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }
    }

    internal sealed partial class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement( SyntaxNode syntax , BoundExpression expression )
            : base( BoundNodeKind.ExpressionStatement , syntax )
        {
            this.Expression = expression;
        }

        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement( SyntaxNode syntax , BoundExpression? expression )
            : base( BoundNodeKind.ReturnStatement , syntax )
        {
            this.Expression = expression;
        }

        public BoundExpression? Expression { get; }
    }

    internal sealed partial class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement( SyntaxNode syntax , BoundLabel label )
            : base( BoundNodeKind.GotoStatement , syntax )
        {
            this.Label = label;
        }

        public BoundLabel Label { get; }
    }

    internal sealed partial class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement( SyntaxNode syntax , BoundLabel label )
            : base( BoundNodeKind.LabelStatement , syntax )
        {
            this.Label = label;
        }

        public BoundLabel Label { get; }
    }

    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement( BoundNodeKind kind , SyntaxNode syntax , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( kind , syntax )
        {
            this.BreakLabel    = breakLabel;
            this.ContinueLabel = continueLabel;
        }

        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }

    internal sealed partial class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement( SyntaxNode syntax , BoundLabel label , BoundExpression condition , bool jumpIfTrue = true )
            : base( BoundNodeKind.ConditionalGotoStatement , syntax )
        {
            this.Label      = label;
            this.Condition  = condition;
            this.JumpIfTrue = jumpIfTrue;
        }

        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }
    }

    internal abstract class BoundStatement : BoundNode
    {
        public BoundStatement( BoundNodeKind kind , SyntaxNode syntax )
            : base( kind , syntax )
        {

        }
    }

    internal sealed partial class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression( SyntaxNode syntax , BoundUnaryOperator @operator , BoundExpression expression )
            : base( BoundNodeKind.UnaryExpression , syntax )
        {
            this.Operator      = @operator;
            this.Expression    = expression;
            this.ConstantValue = ConstantFolding.Fold( @operator , expression );
        }

        public override TypeSymbol Type => this.Operator.Type;
        public override BoundConstant? ConstantValue { get; }

        public BoundUnaryOperator Operator { get; }
        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration( SyntaxNode syntax , VariableSymbol variable , BoundExpression initializer )
            : base( BoundNodeKind.VariableDeclaration , syntax )
        {
            this.Variable    = variable;
            this.Initializer = initializer;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Initializer { get; }
    }

    internal sealed partial class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression( SyntaxNode syntax , VariableSymbol variable )
            : base( BoundNodeKind.VariableExpression , syntax )
        {
            this.Variable = variable;
        }

        public override TypeSymbol Type => this.Variable.Type;
        public override BoundConstant? ConstantValue => this.Variable.Constant;

        public VariableSymbol Variable { get; }
    }

    internal sealed partial class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression( SyntaxNode syntax , FunctionSymbol function , ImmutableArray<BoundExpression> arguments )
            : base( BoundNodeKind.CallExpression , syntax )
        {
            this.Function  = function;
            this.Arguments = arguments;
        }

        public override TypeSymbol Type => this.Function.ReturnType;
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }

    internal sealed partial class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression( SyntaxNode syntax , BoundExpression lhs , BoundBinaryOperator op , BoundExpression rhs )
            : base( BoundNodeKind.BinaryExpression , syntax )
        {
            this.Lhs      = lhs;
            this.Operator = op;
            this.Rhs      = rhs;
            this.ConstantValue = ConstantFolding.Fold( lhs , op , rhs );
        }

        public override TypeSymbol Type => this.Operator.Type;
        public override BoundConstant? ConstantValue { get; }

        public BoundExpression Lhs { get; }
        public BoundBinaryOperator Operator { get; }
        public BoundExpression Rhs { get; }
    }

    internal sealed partial class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement( SyntaxNode syntax , BoundExpression condition , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( BoundNodeKind.WhileStatement , syntax  , breakLabel , continueLabel )
        {
            this.Condition = condition;
            this.Body      = body;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }

    internal sealed partial class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement( SyntaxNode syntax , ImmutableArray<BoundStatement> statements )
            : base( BoundNodeKind.BlockStatement , syntax )
        {
            this.Statements = statements;
        }

        public ImmutableArray<BoundStatement> Statements { get; }
    }

}
