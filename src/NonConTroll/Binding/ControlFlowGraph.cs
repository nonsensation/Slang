using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal sealed class ControlFlowGraph
    {
        private ControlFlowGraph( BasicBlock start , BasicBlock end , List<BasicBlock> blocks , List<BasicBlockBranch> branches )
        {
            this.Start    = start;
            this.End      = end;
            this.Blocks   = blocks;
            this.Branches = branches;
        }

        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        public sealed class BasicBlock
        {
            public BasicBlock()
            {
            }

            public BasicBlock( bool isStart )
            {
                this.IsStart = isStart;
                this.IsEnd = !isStart;
            }

            public bool IsStart { get; }
            public bool IsEnd { get; }
            public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
            public List<BasicBlockBranch> Incoming { get; } = new List<BasicBlockBranch>();
            public List<BasicBlockBranch> Outgoing { get; } = new List<BasicBlockBranch>();

            public override string ToString()
            {
                if( this.IsStart )
                {
                    return "<Start>";
                }

                if( this.IsEnd )
                {
                    return "<End>";
                }

                using var writer = new StringWriter();
                using var indentedWriter = new IndentedTextWriter( writer );

                foreach( var statement in this.Statements )
                {
                    statement.WriteTo( indentedWriter );
                }

                return writer.ToString();
            }
        }

        public sealed class BasicBlockBranch
        {
            public BasicBlockBranch( BasicBlock from , BasicBlock to , BoundExpression? condition )
            {
                this.From = from;
                this.To = to;
                this.Condition = condition;
            }

            public BasicBlock From { get; }
            public BasicBlock To { get; }
            public BoundExpression? Condition { get; }

            public override string ToString()
            {
                if( this.Condition == null )
                {
                    return string.Empty;
                }

                return this.Condition.ToString();
            }
        }

        public class BasicBlockBuilder
        {
            private readonly List<BoundStatement> Statements = new List<BoundStatement>();
            private readonly List<BasicBlock> Blocks = new List<BasicBlock>();

            public List<BasicBlock> Build( BoundBlockStatement block )
            {
                foreach( var statement in block.Statements )
                {
                    switch( statement.Kind )
                    {
                        case BoundNodeKind.LabelStatement:
                            this.StartBlock();
                            this.Statements.Add( statement );
                            break;
                        case BoundNodeKind.GotoStatement:
                        case BoundNodeKind.ConditionalGotoStatement:
                        case BoundNodeKind.ReturnStatement:
                            this.Statements.Add( statement );
                            this.StartBlock();
                            break;
                        case BoundNodeKind.DeferStatement:
                        case BoundNodeKind.ExpressionStatement:
                        case BoundNodeKind.SequencePointStatement:
                        case BoundNodeKind.VariableDeclaration:
                            this.Statements.Add( statement );
                            break;
                        default:
                            throw new Exception( $"Unexpected statement: {statement.Kind}" );
                    }
                }

                this.EndBlock();

                return this.Blocks.ToList();
            }

            private void StartBlock()
            {
                this.EndBlock();
            }

            private void EndBlock()
            {
                if( this.Statements.Any() )
                {
                    var block = new BasicBlock();

                    block.Statements.AddRange( this.Statements );

                    this.Blocks.Add( block );
                    this.Statements.Clear();
                }
            }
        }

        public class GraphBuilder
        {
            private readonly Dictionary<BoundStatement, BasicBlock> BlockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
            private readonly Dictionary<BoundLabel, BasicBlock> BlockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
            private readonly List<BasicBlockBranch> Branches = new List<BasicBlockBranch>();
            private readonly BasicBlock Start = new BasicBlock(isStart: true);
            private readonly BasicBlock End = new BasicBlock(isStart: false);

            public ControlFlowGraph Build( List<BasicBlock> blocks )
            {
                if( !blocks.Any() )
                {
                    this.Connect( this.Start , this.End );
                }
                else
                {
                    this.Connect( this.Start , blocks.First() );
                }

                foreach( var block in blocks )
                {
                    foreach( var statement in block.Statements )
                    {
                        this.BlockFromStatement.Add( statement , block );

                        if( statement is BoundLabelStatement labelStatement )
                        {
                            this.BlockFromLabel.Add( labelStatement.Label , block );
                        }
                    }
                }

                for( var i = 0 ; i < blocks.Count ; i++ )
                {
                    var current = blocks[ i ];
                    var next = i == blocks.Count - 1
                        ? this.End
                        : blocks[ i + 1 ];

                    foreach( var statement in current.Statements )
                    {
                        var isLastStatementInBlock = statement == current.Statements.Last();

                        switch( statement.Kind )
                        {
                            case BoundNodeKind.GotoStatement:
                            {
                                var gs = (BoundGotoStatement)statement;
                                var toBlock = this.BlockFromLabel[ gs.Label ];

                                this.Connect( current , toBlock );
                            }
                            break;
                            case BoundNodeKind.ConditionalGotoStatement:
                            {
                                var cgs = (BoundConditionalGotoStatement)statement;
                                var thenBlock = this.BlockFromLabel[ cgs.Label ];
                                var elseBlock = next;
                                var negatedCondition = this.Negate( cgs.Condition );
                                var thenCondition = cgs.JumpIfTrue ? cgs.Condition    : negatedCondition;
                                var elseCondition = cgs.JumpIfTrue ? negatedCondition : cgs.Condition;

                                this.Connect( current , thenBlock , thenCondition );
                                this.Connect( current , elseBlock , elseCondition );
                            }
                            break;
                            case BoundNodeKind.ReturnStatement:
                            {
                                this.Connect( current , this.End );
                            }
                            break;
                            case BoundNodeKind.VariableDeclaration:
                            case BoundNodeKind.LabelStatement:
                            case BoundNodeKind.ExpressionStatement:
                            case BoundNodeKind.SequencePointStatement:
                            case BoundNodeKind.DeferStatement:
                            {
                                if( isLastStatementInBlock )
                                    this.Connect( current , next );
                            }
                            break;
                            default:
                            {
                                throw new Exception( $"Unexpected statement: {statement.Kind}" );
                            }
                        }
                    }
                }

                while( true )
                {
                    var done = true;

                    foreach( var block in blocks )
                    {
                        if( !block.Incoming.Any() )
                        {
                            this.RemoveBlock( blocks , block );

                            done = false;

                            break;
                        }
                    }

                    if( done )
                    {
                        break;
                    }
                }

                blocks.Insert( 0 , this.Start );
                blocks.Add( this.End );

                return new ControlFlowGraph( this.Start , this.End , blocks , this.Branches );
            }

            private void Connect( BasicBlock from , BasicBlock to , BoundExpression? condition = null )
            {
                if( condition is BoundLiteralExpression l )
                {
                    var value = (bool)l.Value;

                    if( value )
                    {
                        condition = null;
                    }
                    else
                    {
                        return;
                    }
                }

                var branch = new BasicBlockBranch(from, to, condition);

                from.Outgoing.Add( branch );
                to.Incoming.Add( branch );
                this.Branches.Add( branch );
            }

            private void RemoveBlock( List<BasicBlock> blocks , BasicBlock block )
            {
                foreach( var branch in block.Incoming )
                {
                    _ = branch.From.Outgoing.Remove( branch );
                    _ = this.Branches.Remove( branch );
                }

                foreach( var branch in block.Outgoing )
                {
                    _ = branch.To.Incoming.Remove( branch );
                    _ = this.Branches.Remove( branch );
                }

                _ = blocks.Remove( block );
            }

            private BoundExpression Negate( BoundExpression condition )
            {
                if( condition is BoundLiteralExpression literal )
                {
                    var value = (bool)literal.Value;

                    return new BoundLiteralExpression( condition.Syntax , !value );
                }

                var op = BoundUnaryOperator.Bind( SyntaxKind.ExmToken , BuiltinTypes.Bool )!;

                return new BoundUnaryExpression( condition.Syntax , op , condition );
            }
        }

        public void WriteTo( TextWriter writer )
        {
            static string Quote( string text )
            {
                return "\"" + text
                    .TrimEnd()
                    .Replace( "\\" , "\\\\" )
                    .Replace( "\"" , "\\\"" )
                    .Replace( Environment.NewLine , "\\l" )
                    + "\"";
            }

            writer.WriteLine( "digraph G {" );

            var blockIds = new Dictionary<BasicBlock, string>();

            for( var i = 0 ; i < this.Blocks.Count ; i++ )
            {
                blockIds.Add( this.Blocks[ i ] , $"N{i}" );
            }

            foreach( var block in this.Blocks )
            {
                var id = blockIds[ block ];
                var label = Quote( block.ToString() );

                writer.WriteLine( $"    {id} [label = {label}, shape = box]" );
            }

            foreach( var branch in this.Branches )
            {
                var fromId = blockIds[ branch.From ];
                var toId = blockIds [branch.To ];
                var label = Quote( branch.ToString() );

                writer.WriteLine( $"    {fromId} -> {toId} [label = {label}]" );
            }

            writer.WriteLine( "}" );
        }

        public static ControlFlowGraph Create( BoundBlockStatement body )
        {
            return new GraphBuilder().Build( new BasicBlockBuilder().Build( body ) );
        }

        public static bool AllPathsReturn( BoundBlockStatement body )
        {
            var graph = Create( body );

            foreach( var branch in graph.End.Incoming )
            {
                var lastStatement = branch.From.Statements.LastOrDefault();

                if( lastStatement == null ||
                    lastStatement.Kind != BoundNodeKind.ReturnStatement )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
