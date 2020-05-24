using System;
using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement( BoundExpression condition , BoundStatement thenStatement , BoundStatement? elseStatement )
        {
            this.Condition = condition;
            this.ThenStatement = thenStatement;
            this.ElseStatement = elseStatement;
        }

        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;

        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement? ElseStatement { get; }
    }

    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression( object value )
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

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        public override BoundConstant ConstantValue { get; }

        public object Value => this.ConstantValue.Value;
    }

    internal sealed partial class BoundMatchExpression : BoundExpression
    {
        public BoundMatchExpression( BoundExpression expression , ImmutableArray<BoundPatternSectionExpression> patternSections )
        {
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public override BoundNodeKind Kind => BoundNodeKind.MatchExpression;
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
        public BoundMatchStatement( BoundExpression expression , ImmutableArray<BoundPatternSectionStatement> patternSections )
        {
            this.Expression      = expression;
            this.PatternSections = patternSections;
        }

        public override BoundNodeKind Kind => BoundNodeKind.MatchExpression;

        public BoundPatternSectionStatement? DefaultPattern => this.PatternSections.SingleOrDefault( x => x.HasDefaultPattern );

        public BoundExpression Expression { get; }
        public ImmutableArray<BoundPatternSectionStatement> PatternSections { get; }
    }

    internal sealed partial class BoundPatternSectionExpression : BoundExpression
    {
        public BoundPatternSectionExpression( ImmutableArray<BoundPattern> patterns , BoundExpression expression )
        {
            this.Patterns   = patterns;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.PatternSectionExpression;
        public override TypeSymbol Type => this.Expression.Type;

        public ImmutableArray<BoundPattern> Patterns { get; }
        public BoundExpression Expression { get; }
        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == BoundNodeKind.MatchAnyPattern );
    }

    internal sealed partial class BoundPatternSectionStatement : BoundStatement
    {
        public BoundPatternSectionStatement( ImmutableArray<BoundPattern> patterns , BoundStatement statement )
        {
            this.Patterns  = patterns;
            this.Statement = statement;
        }

        public override BoundNodeKind Kind => BoundNodeKind.PatternSectionStatement;

        public ImmutableArray<BoundPattern> Patterns { get; }
        public BoundStatement Statement { get; }
        public bool HasDefaultPattern => this.Patterns.Any( x => x.Kind == BoundNodeKind.MatchAnyPattern );
    }

    internal abstract class BoundPattern : BoundNode
    {
    }

    internal sealed partial class BoundMatchAnyPattern : BoundPattern
    {
        public override BoundNodeKind Kind => BoundNodeKind.MatchAnyPattern;
    }

    internal sealed partial class BoundConstantPattern : BoundPattern
    {
        public BoundConstantPattern( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConstantPattern;

        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundInfixPattern : BoundPattern
    {
        public BoundInfixPattern( DeclaredFunctionSymbol infixFunction , BoundExpression expression )
        {
            this.InfixFunction = infixFunction;
            this.Expression    = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.InfixPattern;

        public DeclaredFunctionSymbol InfixFunction { get; }
        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression( VariableSymbol variable , BoundExpression expression )
        {
            this.Variable   = variable;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public override TypeSymbol Type => this.Expression.Type;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }

    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }

        public virtual BoundConstant? ConstantValue => null;
    }

    internal sealed partial class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression( TypeSymbol type , BoundExpression expression )
        {
            this.Type       = type;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundDeferStatement : BoundStatement
    {
        public BoundDeferStatement( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DeferStatement;

        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundErrorExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
        public override TypeSymbol Type => BuiltinTypes.Error;
    }

    internal sealed partial class BoundDoWhileStatement : BoundLoopStatement
    {
        public BoundDoWhileStatement( BoundStatement body , BoundExpression condition , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Body      = body;
            this.Condition = condition;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;

        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
    }

    internal sealed partial class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement( VariableSymbol variable , BoundExpression lowerBound , BoundExpression upperBound , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Variable   = variable;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.Body       = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }
    }

    internal sealed partial class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement( BoundExpression? expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;

        public BoundExpression? Expression { get; }
    }

    internal sealed partial class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement( BoundLabel label )
        {
            this.Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;

        public BoundLabel Label { get; }
    }

    internal sealed partial class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement( BoundLabel label )
        {
            this.Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

        public BoundLabel Label { get; }
    }

    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement( BoundLabel breakLabel , BoundLabel continueLabel )
        {
            this.BreakLabel    = breakLabel;
            this.ContinueLabel = continueLabel;
        }

        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }

    internal sealed partial class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement( BoundLabel label , BoundExpression condition , bool jumpIfTrue = true )
        {
            this.Label      = label;
            this.Condition  = condition;
            this.JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }
    }

    internal abstract class BoundStatement : BoundNode
    {
    }

    internal sealed partial class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression( BoundUnaryOperator @operator , BoundExpression expression )
        {
            this.Operator      = @operator;
            this.Expression    = expression;
            this.ConstantValue = ConstantFolding.Fold( @operator , expression );
        }

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override TypeSymbol Type => this.Operator.Type;
        public override BoundConstant? ConstantValue { get; }

        public BoundUnaryOperator Operator { get; }
        public BoundExpression Expression { get; }
    }

    internal sealed partial class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration( VariableSymbol variable , BoundExpression initializer )
        {
            this.Variable    = variable;
            this.Initializer = initializer;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;

        public VariableSymbol Variable { get; }
        public BoundExpression Initializer { get; }
    }

    internal sealed partial class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression( VariableSymbol variable )
        {
            this.Variable = variable;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override TypeSymbol Type => this.Variable.Type;
        public override BoundConstant? ConstantValue => this.Variable.Constant;

        public VariableSymbol Variable { get; }
    }

    internal sealed partial class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression( FunctionSymbol function , ImmutableArray<BoundExpression> arguments )
        {
            this.Function  = function;
            this.Arguments = arguments;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
        public override TypeSymbol Type => this.Function.ReturnType;
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }

    internal sealed partial class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression( BoundExpression lhs , BoundBinaryOperator op , BoundExpression rhs )
        {
            this.Lhs      = lhs;
            this.Operator = op;
            this.Rhs      = rhs;
            this.ConstantValue = ConstantFolding.Fold( lhs , op , rhs );
        }

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override TypeSymbol Type => this.Operator.Type;
        public override BoundConstant? ConstantValue { get; }

        public BoundExpression Lhs { get; }
        public BoundBinaryOperator Operator { get; }
        public BoundExpression Rhs { get; }
    }

    internal sealed partial class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement( BoundExpression condition , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Condition = condition;
            this.Body      = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }

    internal sealed partial class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement( ImmutableArray<BoundStatement> statements )
        {
            this.Statements = statements;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
        public ImmutableArray<BoundStatement> Statements { get; }
    }

}
