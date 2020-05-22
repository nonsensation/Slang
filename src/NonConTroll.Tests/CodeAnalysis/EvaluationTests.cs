using System;
using System.Collections.Generic;
using NonConTroll.CodeAnalysis;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using Xunit;

namespace NonConTroll.Tests.CodeAnalysis
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData( "1" , 1 )]
        [InlineData( "+1" , 1 )]
        [InlineData( "-1" , -1 )]
        [InlineData( "(10)" , 10 )]

        [InlineData( "14 + 12" , 26 )]
        [InlineData( "12 - 3" , 9 )]
        [InlineData( "4 * 2" , 8 )]
        [InlineData( "9 / 3" , 3 )]

        [InlineData( "12 == 3" , false )]
        [InlineData( "3 == 3" , true )]
        [InlineData( "12 != 3" , true )]
        [InlineData( "3 != 3" , false )]

        [InlineData( "3 < 4" , true )]
        [InlineData( "5 < 4" , false )]
        [InlineData( "4 <= 4" , true )]
        [InlineData( "4 <= 5" , true )]
        [InlineData( "5 <= 4" , false )]
        [InlineData( "4 > 3" , true )]
        [InlineData( "4 > 5" , false )]
        [InlineData( "4 >= 4" , true )]
        [InlineData( "5 >= 4" , true )]
        [InlineData( "4 >= 5" , false )]

        [InlineData( "false == false" , true )]
        [InlineData( "true == false" , false )]
        [InlineData( "false != false" , false )]
        [InlineData( "true != false" , true )]
        [InlineData( "true && true" , true )]
        [InlineData( "false || false" , false )]

        [InlineData( "true" , true )]
        [InlineData( "false" , false )]
        [InlineData( "!true" , false )]
        [InlineData( "!false" , true )]

        [InlineData( "var a = 10 return a" , 10 )]

        // [InlineData( "\"test\"" , "test" )]
        // [InlineData( "\"te\"\"st\"" , "te\"st" )]

        [InlineData( "\"test\" == \"test\"" , true )]
        [InlineData( "\"test\" != \"test\"" , false )]
        [InlineData( "\"test\" == \"abc\"" , false )]
        [InlineData( "\"test\" != \"abc\"" , true )]
        [InlineData( "\"test\" + \"abc\"" , "testabc" )]

        // [InlineData( "{ var a : any = 0 var b : any = \"b\" return a == b }" , false )]
        // [InlineData( "{ var a : any = 0 var b : any = \"b\" return a != b }" , true )]
        // [InlineData( "{ var a : any = 0 var b : any = 0 return a == b }" , true )]
        // [InlineData( "{ var a : any = 0 var b : any = 0 return a != b }" , false )]

        [InlineData( "{ var a = 10 return a * a }" , 100 )]
        [InlineData( "{ var a = 0 return (a = 10) * a }" , 100 )]

        [InlineData( "{ var a = 0 if a == 0 a = 10 return a }" , 10 )]
        [InlineData( "{ var a = 0 if a == 4 a = 10 return a }" , 0 )]
        [InlineData( "{ var a = 0 if a == 0 a = 10 else a = 5 return a }" , 10 )]
        [InlineData( "{ var a = 0 if a == 4 a = 10 else a = 5 return a }" , 5 )]

        [InlineData( "{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1} return result }" , 55 )]
        [InlineData( "{ var result = 0 for i = 1 in 10 { result = result + i } return result }" , 55 )]
        [InlineData( "{ var a = 10 for i = 1 in (a = a - 1) { } return a }" , 9 )]
        [InlineData( "{ var i = 0 while i < 5 { i = i + 1 if i == 5 continue } return i }" , 5 )]

        // [InlineData( "{ var a = 0 do a = a + 1 while a < 10 return a}" , 10 )]
        // [InlineData( "{ var i = 0 do { i = i + 1 if i == 5 continue } while i < 5 return i }" , 5 )]
        public void Evaluator_Computes_CorrectValues( string text , object expectedValue )
        {
            AssertValue( text , expectedValue );
        }

        [Fact]
        public void Evaluator_VariableDeclaration_Reports_Redeclaration()
        {
            var text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
            ";

            var diagnostics = @"
                'x' is already declared.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_BlockStatement_NoInfiniteLoop()
        {
            var text = @"
                {
                [)][]
            ";

            var diagnostics = @"
                Unexpected token <CloseParenToken>, expected <Identifier>.
                Unexpected token <EndOfFile>, expected <CloseBraceToken>.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_Missing()
        {
            var text = @"
                print([)]
            ";

            var diagnostics = @"
                Function 'print' requires 1 arguments but was given 0.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_Exceeding()
        {
            var text = @"
                print(""Hello""[, "" "", "" world!""])
            ";

            var diagnostics = @"
                Function 'print' requires 1 arguments but was given 3.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop()
        {
            var text = @"
                print(""Hi""[[=]][)]
            ";

            var diagnostics = @"
                Unexpected token <EqToken>, expected <CloseParenToken>.
                Unexpected token <EqToken>, expected <Identifier>.
                Unexpected token <CloseParenToken>, expected <Identifier>.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_FunctionParameters_NoInfiniteLoop()
        {
            var text = @"
                func hi(name: string[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]
            ";

            var diagnostics = @"
                Unexpected token <EqToken>, expected <CloseParenToken>.
                Unexpected token <EqToken>, expected <OpenBraceToken>.
                Unexpected token <EqToken>, expected <Identifier>.
                Unexpected token <CloseParenToken>, expected <Identifier>.
                Unexpected token <EndOfFile>, expected <CloseBraceToken>.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_FunctionReturn_Missing()
        {
            var text = @"
                func [add](a: int, b: int): int
                {
                }
            ";

            var diagnostics = @"
                Not all code paths return a value.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_IfStatement_Reports_CannotConvert()
        {
            var text = @"
                {
                    var x = 0
                    if [10]
                        x = 10
                }
            ";

            var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_WhileStatement_Reports_CannotConvert()
        {
            var text = @"
                {
                    var x = 0
                    while [10]
                        x = 10
                }
            ";

            var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        // [Fact]
        // public void Evaluator_DoWhileStatement_Reports_CannotConvert()
        // {
        //     var text = @"
        //         {
        //             var x = 0
        //             do
        //                 x = 10
        //             while [10]
        //         }
        //     ";

        //     var diagnostics = @"
        //         Cannot convert type 'int' to 'bool'.
        //     ";

        //     AssertDiagnostics( text , diagnostics );
        // }

        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert_LowerBound()
        {
            var text = @"
                {
                    var result = 0
                    for i = [false] in 10
                        result = result + i
                }
            ";

            var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert_UpperBound()
        {
            var text = @"
                {
                    var result = 0
                    for i = 1 in [true]
                        result = result + i
                }
            ";

            var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_Undefined()
        {
            var text = @"[x] * 10";

            var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_NoErrorForInsertedToken()
        {
            var text = @"1 + []";

            var diagnostics = @"
                Unexpected token <EndOfFile>, expected <Identifier>.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_UnaryExpression_Reports_Undefined()
        {
            var text = @"[+]true";

            var diagnostics = @"
                Unary operator '+' is not defined for type 'bool'.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_BinaryExpression_Reports_Undefined()
        {
            var text = @"10 [*] false";

            var diagnostics = @"
                Binary operator '*' is not defined for types 'int' and 'bool'.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_Undefined()
        {
            var text = @"[x] = 10";

            var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_NotAVariable()
        {
            var text = @"[print] = 42";

            var diagnostics = @"
                'print' is not a variable.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_CannotAssign()
        {
            var text = @"
                {
                    let x = 10
                    x [=] 0
                }
            ";

            var diagnostics = @"
                Variable 'x' is read-only and cannot be assigned to.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_AssignmentExpression_Reports_CannotConvert()
        {
            var text = @"
                {
                    var x = 10
                    x = [true]
                }
            ";

            var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_Undefined()
        {
            var text = @"[foo](42)";

            var diagnostics = @"
                Function 'foo' doesn't exist.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_NotAFunction()
        {
            var text = @"
                {
                    let foo = 42
                    [foo](42)
                }
            ";

            var diagnostics = @"
                'foo' is not a function.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Variables_Can_Shadow_Functions()
        {
            var text = @"
                {
                    let print = 42
                    [print](""test"")
                }
            ";

            var diagnostics = @"
                'print' is not a function.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Void_Function_Should_Not_Return_Value()
        {
            var text = @"
                func test()
                {
                    return [1]
                }
            ";

            var diagnostics = @"
                Since the function 'test' does not return a value the 'return' keyword cannot be followed by an expression.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Function_With_ReturnValue_Should_Not_Return_Void()
        {
            var text = @"
                func test(): int
                {
                    [return]
                }
            ";

            var diagnostics = @"
                An expression of type 'int' is expected.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Not_All_Code_Paths_Return_Value()
        {
            var text = @"
                func [test](n: int): bool
                {
                    if (n > 10)
                       return true
                }
            ";

            var diagnostics = @"
                Not all code paths return a value.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Expression_Must_Have_Value()
        {
            var text = @"
                func test(n: int)
                {
                    return
                }

                let value = [test(100)]
            ";

            var diagnostics = @"
                Expression must have a value.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Theory]
        [InlineData( "[break]" , "break" )]
        [InlineData( "[continue]" , "continue" )]
        public void Evaluator_Invalid_Break_Or_Continue( string text , string keyword )
        {
            var diagnostics = $@"
                The keyword '{keyword}' can only be used inside of loops.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Script_Return()
        {
            var text = @"
                return
            ";

            AssertValue( text , "" );
        }

        [Fact]
        public void Evaluator_Parameter_Already_Declared()
        {
            var text = @"
                func sum(a: int, b: int, [a: int]): int
                {
                    return a + b + c
                }
            ";

            var diagnostics = @"
                A parameter with the name 'a' already exists.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Function_Must_Have_Name()
        {
            var text = @"
                func [(]a: int, b: int): int
                {
                    return a + b
                }
            ";

            var diagnostics = @"
                Unexpected token <OpenParenToken>, expected <Identifier>.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Wrong_Argument_Type()
        {
            var text = @"
                func test(n: int): bool
                {
                    return n > 10
                }
                let testValue = ""string""
                test([testValue])
            ";

            var diagnostics = @"
                Cannot convert type 'string' to 'int'. An explicit conversion exists (are you missing a cast?)
            ";

            AssertDiagnostics( text , diagnostics );
        }

        [Fact]
        public void Evaluator_Bad_Type()
        {
            var text = @"
                func test(n: [invalidtype])
                {
                }
            ";

            var diagnostics = @"
                Type 'invalidtype' doesn't exist.
            ";

            AssertDiagnostics( text , diagnostics );
        }

        private static void AssertValue( string text , object expectedValue )
        {
            var syntaxTree = SyntaxTree.Parse( text );
            var compilation = Compilation.CreateScript( null , syntaxTree );
            var variables = new Dictionary<VariableSymbol , object>();
            var result = compilation.Evaluate( variables );

            Assert.Empty( result.Diagnostics );
            Assert.Equal( expectedValue , result.Value );
        }

        private void AssertDiagnostics( string text , string diagnosticText )
        {
            var annotatedText = AnnotatedText.Parse( text );
            var syntaxTree = SyntaxTree.Parse( annotatedText.Text );
            var compilation = Compilation.CreateScript( null , syntaxTree );
            var result = compilation.Evaluate( new Dictionary<VariableSymbol , object>() );
            var expectedDiagnostics = AnnotatedText.UnindentLines( diagnosticText );

            if( annotatedText.Spans.Length != expectedDiagnostics.Length )
            {
                throw new Exception( "ERROR: Must mark as many spans as there are expected diagnostics" );
            }

            Assert.Equal( expectedDiagnostics.Length , result.Diagnostics.Length );

            for( var i = 0 ; i < expectedDiagnostics.Length ; i++ )
            {
                var expectedMessage = expectedDiagnostics[ i ];
                var expectedSpan = annotatedText.Spans[ i ];
                var actualDiag = result.Diagnostics[ i ];

                Assert.Equal( expectedMessage , actualDiag.Message );
                Assert.Equal( expectedSpan , actualDiag.Location.Span );
            }
        }
    }

}