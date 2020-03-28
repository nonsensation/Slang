using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Linq;


namespace NonConTroll
{
    internal abstract class Repl
    {
        private readonly List<string> SubmissionHistory = new List<string>();
        private int SubmissionHistoryIndex;

        private bool Done;

        public void Run()
        {
            while( true )
            {
                var text = this.EditSubmission();

                if( string.IsNullOrEmpty( text ) )
                    return;

                if( !text.Contains( Environment.NewLine ) && text.StartsWith( "#" ) )
					this.EvaluateMetaCommand( text );
                else
					this.EvaluateSubmission( text );

                this.SubmissionHistory.Add( text );
                this.SubmissionHistoryIndex = 0;
            }
        }

        private string EditSubmission()
        {
            this.Done = false;
            var document = new ObservableCollection<string>() { "" };
            var view = new SubmissionView( this.RenderLine , document );

            while( !this.Done )
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
                    case ConsoleKey.Escape:     this.HandleEscape    ( document , view ); break;
                    case ConsoleKey.Enter:      this.HandleEnter     ( document , view ); break;
                    case ConsoleKey.LeftArrow:  this.HandleLeftArrow ( document , view ); break;
                    case ConsoleKey.RightArrow: this.HandleRightArrow( document , view ); break;
                    case ConsoleKey.UpArrow:    this.HandleUpArrow   ( document , view ); break;
                    case ConsoleKey.DownArrow:  this.HandleDownArrow ( document , view ); break;
                    case ConsoleKey.Backspace:  this.HandleBackspace ( document , view ); break;
                    case ConsoleKey.Delete:     this.HandleDelete    ( document , view ); break;
                    case ConsoleKey.Home:       this.HandleHome      ( document , view ); break;
                    case ConsoleKey.End:        this.HandleEnd       ( document , view ); break;
                    case ConsoleKey.Tab:        this.HandleTab       ( document , view ); break;
                    case ConsoleKey.PageUp:     this.HandlePageUp    ( document , view ); break;
                    case ConsoleKey.PageDown:   this.HandlePageDown  ( document , view ); break;
                }
            }
            else if( key.Modifiers == ConsoleModifiers.Control )
            {
                switch( key.Key )
                {
                    case ConsoleKey.Enter:
                        this.HandleControlEnter( document , view );
                        break;
                }
            }

            if( key.KeyChar >= ' ' )
                this.HandleTyping( document , view , key.KeyChar.ToString() );
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

            if( submissionText.StartsWith( "#" ) || this.IsCompleteSubmission( submissionText ) )
            {
                this.Done = true;

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
                view.CurrentCharacter--;
        }

        private void HandleRightArrow( ObservableCollection<string> document , SubmissionView view )
        {
            var line = document[view.CurrentLine];

            if( view.CurrentCharacter <= line.Length - 1 )
                view.CurrentCharacter++;
        }

        private void HandleUpArrow( ObservableCollection<string> document , SubmissionView view )
        {
            if( view.CurrentLine > 0 )
                view.CurrentLine--;
        }

        private void HandleDownArrow( ObservableCollection<string> document , SubmissionView view )
        {
            if( view.CurrentLine < document.Count - 1 )
                view.CurrentLine++;
        }

        private void HandleBackspace( ObservableCollection<string> document , SubmissionView view )
        {
            var start = view.CurrentCharacter;

            if( start == 0 )
            {
                if( view.CurrentLine == 0 )
                    return;

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
                    return;

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
                this.SubmissionHistoryIndex = this.SubmissionHistory.Count - 1;

            this.UpdateDocumentFromHistory( document , view );
        }

        private void HandlePageDown( ObservableCollection<string> document , SubmissionView view )
        {
            this.SubmissionHistoryIndex++;

            if( this.SubmissionHistoryIndex > this.SubmissionHistory.Count -1 )
                this.SubmissionHistoryIndex = 0;

            this.UpdateDocumentFromHistory( document , view );
        }

        private void UpdateDocumentFromHistory( ObservableCollection<string> document , SubmissionView view )
        {
            if( this.SubmissionHistory.Count == 0 )
                return;

            document.Clear();

            var historyItem = this.SubmissionHistory[this.SubmissionHistoryIndex];
            var lines = historyItem.Split( Environment.NewLine );

            foreach( var line in lines )
                document.Add( line );

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[ view.CurrentLine ].Length;
        }

        private void HandleTyping( ObservableCollection<string> document , SubmissionView view , string text )
        {
            var lineIndex = view.CurrentLine;
            var start = view.CurrentCharacter;

            document[ lineIndex ] = document[ lineIndex ].Insert( start , text );

            view.CurrentCharacter += text.Length;
        }

        protected void ClearHistory()
        {
            this.SubmissionHistory.Clear();
        }

        protected virtual void RenderLine( string line )
        {
            Console.Write( line );
        }

        protected virtual void EvaluateMetaCommand( string input )
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine( $"Invalid command {input}." );
            Console.ResetColor();
        }

        protected abstract bool IsCompleteSubmission( string text );

        protected abstract void EvaluateSubmission( string text );


        private sealed class SubmissionView
        {
            private readonly Action<string> LineRenderer;
            private readonly ObservableCollection<string> SubmissionDocument;
            private readonly int CursorTop;
            private int RenderedLineCount;
            private int currentLine;
            private int currentCharacter;

            public SubmissionView( Action<string> lineRenderer , ObservableCollection<string> submissionDocument )
            {
                this.LineRenderer       = lineRenderer;
                this.SubmissionDocument = submissionDocument;
                this.CursorTop          = Console.CursorTop;
                this.SubmissionDocument.CollectionChanged  += this.SubmissionDocumentChanged;

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

                foreach( var line in this.SubmissionDocument )
                {
                    Console.SetCursorPosition( 0 , this.CursorTop + lineCount );
                    Console.ForegroundColor = ConsoleColor.Green;

                    if( lineCount == 0 )
                        Console.Write( "» " );
                    else
                        Console.Write( "· " );

                    Console.ResetColor();

					this.LineRenderer( line );

                    Console.WriteLine( new string( ' ' , Console.WindowWidth - line.Length ) );

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
    }
}
