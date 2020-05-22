using System;
using System.Collections.Generic;
using NonConTroll.CodeAnalysis.Syntax;
using Xunit;

namespace NonConTroll.Tests.CodeAnalysis.Syntax
{
    public class SyntaxInfoTests
    {
        [Theory]
        [MemberData( nameof( GetSyntaxKindData ) )]
        public void SyntaxFact_GetText_RoundTrips( SyntaxKind kind )
        {
            var text = SyntaxInfo.GetText( kind );

            if( text == null )
            {
                return;
            }

            var tokens = SyntaxTree.ParseTokens( text );
            var token = Assert.Single( tokens );

            Assert.Equal( kind , token.Kind );
            Assert.Equal( text , token.Text );
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            var kinds = (SyntaxKind[])Enum.GetValues( typeof( SyntaxKind ) );

            foreach( var kind in kinds )
            {
                yield return new object[] { kind };
            }
        }
    }
}
