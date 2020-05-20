using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.Reflection;
using NonConTroll.CodeAnalysis.IO;

namespace NonConTroll
{
    internal abstract class Repl
    {
        private readonly List<MetaCommand> MetaCommands = new List<MetaCommand>();
        private readonly List<string> SubmissionHistory = new List<string>();
        private int SubmissionHistoryIndex;
        private bool IsDone;
        private string MetaCommandIdentifier { get; } = "§";

        protected Repl()
        {
            this.InitMetaCommands();
        }

        private void InitMetaCommands()
        {
            var flags = BindingFlags.NonPublic |
                        BindingFlags.Public |
                        BindingFlags.Static |
                        BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy;

            foreach( var method in GetType().GetMethods( flags ) )
            {
                var attribute = method.GetCustomAttribute<MetaCommandAttribute>();

                if( attribute == null )
                {
                    continue;
                }

                this.MetaCommands.Add( new MetaCommand( attribute.Name , attribute.Description , method ) );
            }
        }

        public void Run()
        {
            while( true )
            {
                var text = this.EditSubmission();

                if( string.IsNullOrEmpty( text ) )
                {
                    return;
                }

                if( !text.Contains( Environment.NewLine ) && text.StartsWith( this.MetaCommandIdentifier ) )
                {
                    this.EvaluateMetaCommand( text );
                }
                else
                {
                    this.EvaluateSubmission( text );
                }

                this.SubmissionHistory.Add( text );
                this.SubmissionHistoryIndex = 0;
            }
        }

        private string EditSubmission()
        {
            this.IsDone = false;
            var document = new ObservableCollection<string>() { "" };
            var view = new SubmissionView( this.RenderLine , document );

            while( !this.IsDone )
            {
                var key = Console.ReadKey( true );

                this.HandleKey( key , document , view );
            }

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[ view.CurrentLine ].Length;

            Console.WriteLine();

            return string.Join( Environment.NewLine , document );
        }

        private void HandleKey( ConsoleKeyInfo key , ObservableCollection<string> document , SubmissionView view )
        {
            if( key.Modifiers == default )
            {
                switch( key.Key )
                {
                    case ConsoleKey.Escape:     this.HandleEscape(     document , view ); break;
                    case ConsoleKey.Enter:      this.HandleEnter(      document , view ); break;
                    case ConsoleKey.LeftArrow:  this.HandleLeftArrow(  document , view ); break;
                    case ConsoleKey.RightArrow: this.HandleRightArrow( document , view ); break;
                    case ConsoleKey.UpArrow:    this.HandleUpArrow(    document , view ); break;
                    case ConsoleKey.DownArrow:  this.HandleDownArrow(  document , view ); break;
                    case ConsoleKey.Backspace:  this.HandleBackspace(  document , view ); break;
                    case ConsoleKey.Delete:     this.HandleDelete(     document , view ); break;
                    case ConsoleKey.Home:       this.HandleHome(       document , view ); break;
                    case ConsoleKey.End:        this.HandleEnd(        document , view ); break;
                    case ConsoleKey.Tab:        this.HandleTab(        document , view ); break;
                    case ConsoleKey.PageUp:     this.HandlePageUp(     document , view ); break;
                    case ConsoleKey.PageDown:   this.HandlePageDown(   document , view ); break;
                }
            }
            else if( key.Modifiers == ConsoleModifiers.Control )
            {
                switch( key.Key )
                {
                    case ConsoleKey.Enter:
                    {
                        this.HandleControlEnter( document , view );
                    }
                    break;
                }
            }

            if( key.KeyChar >= ' ' )
            {
                this.HandleTyping( document , view , key.KeyChar.ToString() );
            }
        }

        private void HandleEscape( ObservableCollection<string> document , SubmissionView view )
        {
            document.Clear();
            document.Add( string.Empty );

            view.CurrentLine = 0;
            view.CurrentCharacter = 0;
        }

        private void HandleEnter( ObservableCollection<string> document , SubmissionView view )
        {
            var submissionText = string.Join( Environment.NewLine , document );

            if( submissionText.StartsWith( this.MetaCommandIdentifier ) || this.IsCompleteSubmission( submissionText ) )
            {
                this.IsDone = true;

                return;
            }

            InsertLine( document , view );
        }

        private void HandleControlEnter( ObservableCollection<string> document , SubmissionView view )
        {
            InsertLine( document , view );
        }

        private static void InsertLine( ObservableCollection<string> document , SubmissionView view )
        {
            var remainder = document[ view.CurrentLine ].Substring( view.CurrentCharacter );

            document[ view.CurrentLine ] = document[ view.CurrentLine ].Substring( 0 , view.CurrentCharacter );

            var lineIndex = view.CurrentLine + 1;

            document.Insert( lineIndex , remainder );

            view.CurrentCharacter = 0;
            view.CurrentLine = lineIndex;
        }

        private void HandleLeftArrow( ObservableCollection<string> document , SubmissionView view )
        {
            if( view.CurrentCharacter > 0 )
            {
                view.CurrentCharacter--;
            }
        }

        private void HandleRightArrow( ObservableCollection<string> document , SubmissionView view )
        {
            var line = document[view.CurrentLine];

            if( view.CurrentCharacter <= line.Length - 1 )
            {
                view.CurrentCharacter++;
            }
        }

        private void HandleUpArrow( ObservableCollection<string> document , SubmissionView view )
        {
            if( view.CurrentLine > 0 )
            {
                view.CurrentLine--;
            }
        }

        private void HandleDownArrow( ObservableCollection<string> document , SubmissionView view )
        {
            if( view.CurrentLine < document.Count - 1 )
            {
                view.CurrentLine++;
            }
        }

        private void HandleBackspace( ObservableCollection<string> document , SubmissionView view )
        {
            var start = view.CurrentCharacter;

            if( start == 0 )
            {
                if( view.CurrentLine == 0 )
                {
                    return;
                }

                var currentLine = document[view.CurrentLine];
                var previousLine = document[view.CurrentLine - 1];

                document.RemoveAt( view.CurrentLine );
                view.CurrentLine--;
                document[ view.CurrentLine ] = previousLine + currentLine;
                view.CurrentCharacter = previousLine.Length;
            }
            else
            {
                var lineIndex = view.CurrentLine;
                var line = document[lineIndex];
                var before = line.Substring( 0 , start - 1 );
                var after = line.Substring( start );

                document[ lineIndex ] = before + after;

                view.CurrentCharacter--;
            }
        }

        private void HandleDelete( ObservableCollection<string> document , SubmissionView view )
        {
            var lineIndex = view.CurrentLine;
            var line = document[lineIndex];
            var start = view.CurrentCharacter;

            if( start >= line.Length )
            {
                if( view.CurrentLine == document.Count - 1 )
                {
                    return;
                }

                var nextLine = document[view.CurrentLine + 1];
                document[ view.CurrentLine ] += nextLine;
                document.RemoveAt( view.CurrentLine + 1 );

                return;
            }

            var before = line.Substring( 0 , start );
            var after = line.Substring( start + 1 );

            document[ lineIndex ] = before + after;
        }

        private void HandleHome( ObservableCollection<string> document , SubmissionView view )
        {
            view.CurrentCharacter = 0;
        }

        private void HandleEnd( ObservableCollection<string> document , SubmissionView view )
        {
            view.CurrentCharacter = document[ view.CurrentLine ].Length;
        }

        private void HandleTab( ObservableCollection<string> document , SubmissionView view )
        {
            const int TabWidth = 4;
            var start = view.CurrentCharacter;
            var remainingSpaces = TabWidth - start % TabWidth;
            var line = document[view.CurrentLine];

            document[ view.CurrentLine ] = line.Insert( start , new string( ' ' , remainingSpaces ) );

            view.CurrentCharacter += remainingSpaces;
        }

        private void HandlePageUp( ObservableCollection<string> document , SubmissionView view )
        {
            this.SubmissionHistoryIndex--;

            if( this.SubmissionHistoryIndex < 0 )
            {
                this.SubmissionHistoryIndex = this.SubmissionHistory.Count - 1;
            }

            this.UpdateDocumentFromHistory( document , view );
        }

        private void HandlePageDown( ObservableCollection<string> document , SubmissionView view )
        {
            this.SubmissionHistoryIndex++;

            if( this.SubmissionHistoryIndex > this.SubmissionHistory.Count -1 )
            {
                this.SubmissionHistoryIndex = 0;
            }

            this.UpdateDocumentFromHistory( document , view );
        }

        private void UpdateDocumentFromHistory( ObservableCollection<string> document , SubmissionView view )
        {
            if( !this.SubmissionHistory.Any() )
            {
                return;
            }

            document.Clear();

            var historyItem = this.SubmissionHistory[ this.SubmissionHistoryIndex ];

            foreach( var line in historyItem.Split( Environment.NewLine ) )
            {
                document.Add( line );
            }

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[ view.CurrentLine ].Length;
        }

        private void HandleTyping( ObservableCollection<string> document , SubmissionView view , string text )
        {
            var lineIndex = view.CurrentLine;

            document[ lineIndex ] = document[ lineIndex ].Insert( view.CurrentCharacter , text );
            view.CurrentCharacter += text.Length;
        }

        protected void ClearHistory()
        {
            this.SubmissionHistory.Clear();
        }

        protected virtual object? RenderLine( IReadOnlyList<string> line , int lineIndex , object? state )
        {
            Console.Write( line );

            return state;
        }

        private void EvaluateMetaCommand( string input )
        {
            var pos = 1;
            var quotes = false;
            var sb = new StringBuilder();
            var args = new List<string>();

            while( pos < input.Length )
            {
                var currentChar = input[ pos ];
                var lookaheadChar = pos + 1 >= input.Length ? '\0' : input[ pos + 1 ];

                if( char.IsWhiteSpace( currentChar ) )
                {
                    if( !quotes )
                    {
                        CommitPendingArgument();
                    }
                    else
                    {
                        sb.Append( currentChar );
                    }
                }
                else if( currentChar == '\"' )
                {
                    if( !quotes )
                    {
                        quotes = true;
                    }
                    else if( lookaheadChar == '\"' )
                    {
                        sb.Append( currentChar );
                        pos++;
                    }
                    else
                    {
                        quotes = false;
                    }
                }
                else
                {
                    sb.Append( currentChar );
                }

                pos++;
            }

            CommitPendingArgument();

            void CommitPendingArgument()
            {
                var arg = sb.ToString();

                if( !string.IsNullOrWhiteSpace( arg ) )
                {
                    args.Add( arg );
                }

                sb.Clear();
            }

            var cmdName = args.FirstOrDefault();

            if( args.Count > 0 )
            {
                args.RemoveAt( 0 );
            }

            var cmd =  this.MetaCommands.SingleOrDefault( x => x.Name == cmdName );

            if( cmd == null )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"Invalid command {input}." );
                Console.ResetColor();

                return;
            }

            var parameters = cmd.MethodInfo.GetParameters();

            if( args.Count != parameters.Length )
            {
                var paramNames = string.Join( " " , parameters.Select( x => $"<{x.Name}>" ) );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"Invalid number of arguments (given {args.Count}, expected {paramNames.Length})" );
                Console.WriteLine( $"usage: {this.MetaCommandIdentifier}{cmd.Name} {paramNames}" );
                Console.ResetColor();

                return;
            }

            var instance = cmd.MethodInfo.IsStatic ? null : this;

            _ = cmd.MethodInfo.Invoke( instance , args.ToArray() );
        }

        protected abstract bool IsCompleteSubmission( string text );

        protected abstract void EvaluateSubmission( string text );

        private delegate object? LineRenderHandler( IReadOnlyList<string> lines , int lineIndex , object? state );

        private sealed class SubmissionView
        {
            private readonly LineRenderHandler LineRenderer;
            private readonly ObservableCollection<string> SubmissionDocument;
            private int CursorTop;
            private int RenderedLineCount;
            private int currentLine;
            private int currentCharacter;

            public SubmissionView( LineRenderHandler lineRenderer , ObservableCollection<string> submissionDocument )
            {
                this.LineRenderer       = lineRenderer;
                this.SubmissionDocument = submissionDocument;
                this.CursorTop          = Console.CursorTop;
                this.SubmissionDocument.CollectionChanged  += this.SubmissionDocumentChanged!;

                this.Render();
            }

            private void SubmissionDocumentChanged( object sender , NotifyCollectionChangedEventArgs e )
            {
                this.Render();
            }

            private void Render()
            {
                Console.CursorVisible = false;

                var lineCount = 0;
                var state = (object?)null;

                foreach( var line in this.SubmissionDocument )
                {
                    if( this.CursorTop + lineCount >= Console.WindowHeight )
                    {
                        Console.SetCursorPosition( 0 , Console.WindowHeight - 1 );
                        Console.WriteLine();

                        if( this.CursorTop > 0 )
                        {
                            this.CursorTop--;
                        }
                    }

                    Console.SetCursorPosition( 0 , this.CursorTop + lineCount );
                    Console.ForegroundColor = ConsoleColor.Green;

                    // already printing 2 chars to the console
                    if( lineCount == 0 )
                    {
                        Console.Write( "» " );
                    }
                    else
                    {
                        Console.Write( "· " );
                    }

                    Console.ResetColor();

                    state = this.LineRenderer( this.SubmissionDocument , lineCount , state );

                    Console.Write( new string( ' ' , Console.WindowWidth - line.Length - 2 ) );

                    lineCount++;
                }

                var numberOfBlankLines = this.RenderedLineCount - lineCount;

                if( numberOfBlankLines > 0 )
                {
                    var blankLine = new string( ' ' , Console.WindowWidth );

                    for( var i = 0 ; i < numberOfBlankLines ; i++ )
                    {
                        Console.SetCursorPosition( 0 , this.CursorTop + lineCount + i );
                        Console.WriteLine( blankLine );
                    }
                }

                this.RenderedLineCount = lineCount;

                Console.CursorVisible = true;

                this.UpdateCursorPosition();
            }

            private void UpdateCursorPosition()
            {
                Console.CursorTop = this.CursorTop + this.currentLine;
                Console.CursorLeft = 2 + this.currentCharacter;
            }

            public int CurrentLine {
                get => this.currentLine;
                set {
                    if( this.currentLine != value )
                    {
                        this.currentLine = value;
                        this.currentCharacter = Math.Min( this.SubmissionDocument[ this.currentLine ].Length , this.currentCharacter );
                        this.UpdateCursorPosition();
                    }
                }
            }

            public int CurrentCharacter {
                get => this.currentCharacter;
                set {
                    if( this.currentCharacter != value )
                    {
                        this.currentCharacter = value;
                        this.UpdateCursorPosition();
                    }
                }
            }
        }

        #region Meta-commands

        [AttributeUsage( AttributeTargets.Method , AllowMultiple = false )]
        protected class MetaCommandAttribute : Attribute
        {
            public MetaCommandAttribute( string name , string description )
            {
                this.Name = name;
                this.Description = description;
            }

            public string Name { get; }
            public string Description { get; }
        }


        protected class MetaCommand
        {
            public MetaCommand( string name , string description , MethodInfo methodInfo )
            {
                this.Name = name;
                this.Description = description;
                this.MethodInfo = methodInfo;
            }

            public string Name { get; }
            public string Description { get; }
            public MethodInfo MethodInfo { get; }
        }

        [MetaCommand( "help" , "Shows help" )]
        protected void Evaluate_Help()
        {
            var maxNameLength = this.MetaCommands.Max( x => x.Name.Length );

            foreach( var metaCmd in this.MetaCommands.OrderBy( x => x.Name ) )
            {
                var metaParams = metaCmd.MethodInfo.GetParameters();

                if( !metaParams.Any() )
                {
                    var paddedName = metaCmd.Name.PadRight( maxNameLength );

                    Console.Out.WritePunctuation( this.MetaCommandIdentifier );
                    Console.Out.WriteIdentifier( paddedName );
                }
                else
                {
                    Console.Out.WritePunctuation( this.MetaCommandIdentifier );
                    Console.Out.WriteIdentifier( metaCmd.Name );

                    foreach( var paramInfo in metaParams )
                    {
                        Console.Out.WriteSpace();
                        Console.Out.WritePunctuation( "<" );
                        Console.Out.WriteIdentifier( paramInfo.Name! );
                        Console.Out.WritePunctuation( ">" );
                    }

                    Console.Out.WriteLine();
                    Console.Out.WriteSpace();

                    for( var _ = 0 ; _ < maxNameLength ; _++ )
                    {
                        Console.Out.WriteSpace();
                    }
                }

                Console.Out.WriteSpace();
                Console.Out.WritePunctuation( metaCmd.Description );
                Console.Out.WriteLine();
            }
        }

        #endregion
    }
}
