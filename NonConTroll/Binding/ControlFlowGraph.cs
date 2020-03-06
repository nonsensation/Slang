using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class ControlFlowGraph
    {
        private ControlFlowGraph( BasicBlock start , BasicBlock end , List<BasicBlock> blocks , List<BasicBlockBranch> branches )
        {
            this.Start = start;
            this.End = end;
            this.Blocks = blocks;
            this.Branches = branches;
        }

        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        public class BasicBlock
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
                    return "<Start>";

                if( this.IsEnd )
                    return "<End>";

                using( var writer = new StringWriter() )
                {
                    foreach( var statement in this.Statements )
                        statement.WriteTo( writer );

                    return writer.ToString();
                }
            }
        }

        public class BasicBlockBranch
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
                    return string.Empty;

                return this.Condition.ToString();
            }
        }

        public class BasicBlockBuilder
        {
            private List<BoundStatement> _statements = new List<BoundStatement>();
            private List<BasicBlock> _blocks = new List<BasicBlock>();

            public List<BasicBlock> Build( BoundBlockStatement block )
            {
                foreach( var statement in block.Statements )
                {
                    switch( statement.Kind )
                    {
                        case BoundNodeKind.LabelStatement:
                            this.StartBlock();
                            this._statements.Add( statement );
                            break;
                        case BoundNodeKind.GotoStatement:
                        case BoundNodeKind.ConditionalGotoStatement:
                        case BoundNodeKind.ReturnStatement:
                            this._statements.Add( statement );
                            this.StartBlock();
                            break;
                        case BoundNodeKind.VariableDeclaration:
                        case BoundNodeKind.ExpressionStatement:
                            this._statements.Add( statement );
                            break;
                        default:
                            throw new Exception( $"Unexpected statement: {statement.Kind}" );
                    }
                }

                this.EndBlock();

                return this._blocks.ToList();
            }

            private void StartBlock()
            {
                this.EndBlock();
            }

            private void EndBlock()
            {
                if( this._statements.Count > 0 )
                {
                    var block = new BasicBlock();
                    block.Statements.AddRange( this._statements );
                    this._blocks.Add( block );
                    this._statements.Clear();
                }
            }
        }

        public class GraphBuilder
        {
            private Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
            private Dictionary<BoundLabel, BasicBlock> _blockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
            private List<BasicBlockBranch> _branches = new List<BasicBlockBranch>();
            private BasicBlock _start = new BasicBlock(isStart: true);
            private BasicBlock _end = new BasicBlock(isStart: false);

            public ControlFlowGraph Build( List<BasicBlock> blocks )
            {
                if( !blocks.Any() )
                    this.Connect( this._start , this._end );
                else
                    this.Connect( this._start , blocks.First() );

                foreach( var block in blocks )
                {
                    foreach( var statement in block.Statements )
                    {
                        this._blockFromStatement.Add( statement , block );

                        if( statement is BoundLabelStatement labelStatement )
                            this._blockFromLabel.Add( labelStatement.Label , block );
                    }
                }

                for( var i = 0 ; i < blocks.Count ; i++ )
                {
                    var current = blocks[i];
                    var next = i == blocks.Count - 1 ? this._end : blocks[i + 1];

                    foreach( var statement in current.Statements )
                    {
                        var isLastStatementInBlock = statement == current.Statements.Last();

                        switch( statement.Kind )
                        {
                            case BoundNodeKind.GotoStatement:
                                var gs = (BoundGotoStatement)statement;
                                var toBlock = this._blockFromLabel[gs.Label];
                                this.Connect( current , toBlock );
                                break;
                            case BoundNodeKind.ConditionalGotoStatement:
                                var cgs = (BoundConditionalGotoStatement)statement;
                                var thenBlock = this._blockFromLabel[cgs.Label];
                                var elseBlock = next;
                                var negatedCondition = this.Negate(cgs.Condition); ;
                                var thenCondition = cgs.JumpIfTrue ? cgs.Condition : negatedCondition;
                                var elseCondition = cgs.JumpIfTrue ? negatedCondition : cgs.Condition;
                                this.Connect( current , thenBlock , thenCondition );
                                this.Connect( current , elseBlock , elseCondition );
                                break;
                            case BoundNodeKind.ReturnStatement:
                                this.Connect( current , this._end );
                                break;
                            case BoundNodeKind.VariableDeclaration:
                            case BoundNodeKind.LabelStatement:
                            case BoundNodeKind.ExpressionStatement:
                                if( isLastStatementInBlock )
                                    this.Connect( current , next );
                                break;
                            default:
                                throw new Exception( $"Unexpected statement: {statement.Kind}" );
                        }
                    }
                }

            ScanAgain:

                foreach( var block in blocks )
                {
                    if( !block.Incoming.Any() )
                    {
                        this.RemoveBlock( blocks , block );

                        goto ScanAgain;
                    }
                }

                blocks.Insert( 0 , this._start );
                blocks.Add( this._end );

                return new ControlFlowGraph( this._start , this._end , blocks , this._branches );
            }

            private void Connect( BasicBlock from , BasicBlock to , BoundExpression? condition = null )
            {
                if( condition is BoundLiteralExpression l )
                {
                    var value = (bool)l.Value;

                    if( value )
                        condition = null;
                    else
                        return;
                }

                var branch = new BasicBlockBranch(from, to, condition);

                from.Outgoing.Add( branch );
                to.Incoming.Add( branch );
                this._branches.Add( branch );
            }

            private void RemoveBlock( List<BasicBlock> blocks , BasicBlock block )
            {
                foreach( var branch in block.Incoming )
                {
                    _ = branch.From.Outgoing.Remove( branch );
                    _ = this._branches.Remove( branch );
                }

                foreach( var branch in block.Outgoing )
                {
                    _ = branch.To.Incoming.Remove( branch );
                    _ = this._branches.Remove( branch );
                }

                _ = blocks.Remove( block );
            }

            private BoundExpression Negate( BoundExpression condition )
            {
                if( condition is BoundLiteralExpression literal )
                {
                    var value = (bool)literal.Value;

                    return new BoundLiteralExpression( !value );
                }

                var op = BoundUnaryOperator.Bind(TokenType.Exm, TypeSymbol.Bool)!;

                return new BoundUnaryExpression( op , condition );
            }
        }

        public void WriteTo( TextWriter writer )
        {
            string Quote( string text )
            {
                return "\"" + text.Replace( "\"" , "\\\"" ) + "\"";
            }

            writer.WriteLine( "digraph G {" );

            var blockIds = new Dictionary<BasicBlock, string>();

            for( var i = 0 ; i < this.Blocks.Count ; i++ )
            {
                var id = $"N{i}";
                blockIds.Add( this.Blocks[ i ] , id );
            }

            foreach( var block in this.Blocks )
            {
                var id = blockIds[block];
                var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
                writer.WriteLine( $"    {id} [label = {label} shape = box]" );
            }

            foreach( var branch in this.Branches )
            {
                var fromId = blockIds[branch.From];
                var toId = blockIds[branch.To];
                var label = Quote(branch.ToString());
                writer.WriteLine( $"    {fromId} -> {toId} [label = {label}]" );
            }

            writer.WriteLine( "}" );
        }

        public static ControlFlowGraph Create( BoundBlockStatement body )
        {
            var basicBlockBuilder = new BasicBlockBuilder();
            var blocks = basicBlockBuilder.Build(body);

            var graphBuilder = new GraphBuilder();
            return graphBuilder.Build( blocks );
        }

        public static bool AllPathsReturn( BoundBlockStatement body )
        {
            var graph = Create(body);

            foreach( var branch in graph.End.Incoming )
            {
                var lastStatement = branch.From.Statements.Last();
                if( lastStatement.Kind != BoundNodeKind.ReturnStatement )
                    return false;
            }

            return true;
        }
    }

}
