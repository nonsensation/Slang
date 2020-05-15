using System;
using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundIfStatement : BoundStatement
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

    public class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression( object value )
        {
            this.Value = value;

            if( value is bool b )
            {
                this.Type = BuiltinTypes.Bool;
            }
            else if( value is int i )
            {
                this.Type = BuiltinTypes.Int;
            }
            else if( value is string str )
            {
                this.Type = BuiltinTypes.String;
                this.Value = str.Substring( 1 , str.Length - 2 );
            }
            else
            {
                throw new Exception( $"Unexpected literal '{value}' of type {value.GetType()}" );
            }
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        public object Value { get; }
    }

    public sealed partial class BoundMatchExpression : BoundExpression
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

    public sealed partial class BoundMatchStatement : BoundStatement
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

    public sealed partial class BoundPatternSectionExpression : BoundExpression
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

    public sealed partial class BoundPatternSectionStatement : BoundStatement
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

    public abstract class BoundPattern : BoundNode
    {
    }

    public sealed partial class BoundMatchAnyPattern : BoundPattern
    {
        public override BoundNodeKind Kind => BoundNodeKind.MatchAnyPattern;
    }

    public sealed partial class BoundConstantPattern : BoundPattern
    {
        public BoundConstantPattern( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConstantPattern;

        public BoundExpression Expression { get; }
    }

    public sealed partial class BoundInfixPattern : BoundPattern
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

    public sealed partial class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression( VariableSymbol variable , BoundExpression expression )
        {
            this.Variable = variable;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public override TypeSymbol Type => this.Expression.Type;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }

    public abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }
    }

    public sealed partial class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression( TypeSymbol type , BoundExpression expression )
        {
            this.Type = type;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }

    public sealed partial class BoundDeferStatement : BoundStatement
    {
        public BoundDeferStatement( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DeferStatement;

        public BoundExpression Expression { get; }
    }

    public sealed partial class BoundErrorExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
        public override TypeSymbol Type => BuiltinTypes.Error;
    }

    public sealed partial class BoundDoWhileStatement : BoundLoopStatement
    {
        public BoundDoWhileStatement( BoundStatement body , BoundExpression condition , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Body = body;
            this.Condition = condition;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;

        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
    }

    public sealed partial class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement( VariableSymbol variable , BoundExpression lowerBound , BoundExpression upperBound , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Variable = variable;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }
    }

    public sealed partial class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

        public BoundExpression Expression { get; }
    }

    public sealed partial class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement( BoundExpression? expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;

        public BoundExpression? Expression { get; }
    }

    public sealed partial class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement( BoundLabel label )
        {
            this.Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;

        public BoundLabel Label { get; }
    }

    public sealed partial class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement( BoundLabel label )
        {
            this.Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

        public BoundLabel Label { get; }
    }

    public abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement( BoundLabel breakLabel , BoundLabel continueLabel )
        {
            this.BreakLabel = breakLabel;
            this.ContinueLabel = continueLabel;
        }

        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }

    public sealed partial class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement( BoundLabel label , BoundExpression condition , bool jumpIfTrue = true )
        {
            this.Label = label;
            this.Condition = condition;
            this.JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }
    }

    public abstract class BoundStatement : BoundNode
    {
    }

    public sealed partial class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression( BoundUnaryOperator op , BoundExpression operand )
        {
            this.Op = op;
            this.Operand = operand;
        }

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override TypeSymbol Type => this.Op.Type;
        public BoundUnaryOperator Op { get; }
        public BoundExpression Operand { get; }
    }

    public sealed partial class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration( VariableSymbol variable , BoundExpression initializer )
        {
            this.Variable = variable;
            this.Initializer = initializer;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;

        public VariableSymbol Variable { get; }
        public BoundExpression Initializer { get; }
    }

    public sealed partial class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression( VariableSymbol variable )
        {
            this.Variable = variable;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override TypeSymbol Type => this.Variable.Type;
        public VariableSymbol Variable { get; }
    }

    public sealed partial class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression( FunctionSymbol function , ImmutableArray<BoundExpression> arguments )
        {
            this.Function = function;
            this.Arguments = arguments;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
        public override TypeSymbol Type => this.Function.ReturnType;
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }

    public sealed partial class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression( BoundExpression lhs , BoundBinaryOperator op , BoundExpression rhs )
        {
            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override TypeSymbol Type => this.Operator.Type;
        public BoundExpression Lhs { get; }
        public BoundBinaryOperator Operator { get; }
        public BoundExpression Rhs { get; }
    }

    public sealed partial class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement( BoundExpression condition , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Condition = condition;
            this.Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }

    public sealed partial class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement( ImmutableArray<BoundStatement> statements )
        {
            this.Statements = statements;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
        public ImmutableArray<BoundStatement> Statements { get; }
    }

}
