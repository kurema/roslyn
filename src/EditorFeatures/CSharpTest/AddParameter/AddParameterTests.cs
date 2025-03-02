﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.AddParameter;
using Microsoft.CodeAnalysis.CSharp.Shared.Extensions;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.AddParameter
{
    [Trait(Traits.Feature, Traits.Features.CodeActionsAddParameter)]
    public class AddParameterTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        public AddParameterTests(ITestOutputHelper logger)
           : base(logger)
        {
        }

        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (null, new CSharpAddParameterCodeFixProvider());

        protected override ImmutableArray<CodeAction> MassageActions(ImmutableArray<CodeAction> actions)
            => FlattenActions(actions);

        [Fact]
        public async Task TestMissingWithImplicitConstructor()
        {
            await TestMissingAsync(
@"
class C
{
}

class D
{
    void M()
    {
        new [|C|](1);
    }
}");
        }

        [Fact]
        public async Task TestOnEmptyConstructor()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C() { }
}

class D
{
    void M()
    {
        new [|C|](1);
    }
}",
@"
class C
{
    public C(int v) { }
}

class D
{
    void M()
    {
        new C(1);
    }
}");
        }

        [Fact]
        public async Task TestNamedArg()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C() { }
}

class D
{
    void M()
    {
        new C([|p|]: 1);
    }
}",
@"
class C
{
    public C(int p) { }
}

class D
{
    void M()
    {
        new C(p: 1);
    }
}");
        }

        [Fact]
        public async Task TestMissingWithConstructorWithSameNumberOfParams()
        {
            await TestMissingAsync(
@"
class C
{
    public C(bool b) { }
}

class D
{
    void M()
    {
        new [|C|](1);
    }
}");
        }

        [Fact]
        public async Task TestAddBeforeMatchingArg()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(int i) { }
}

class D
{
    void M()
    {
        new [|C|](true, 1);
    }
}",
@"
class C
{
    public C(bool v, int i) { }
}

class D
{
    void M()
    {
        new C(true, 1);
    }
}");
        }

        [Fact]
        public async Task TestAddAfterMatchingConstructorParam()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(int i) { }
}

class D
{
    void M()
    {
        new [|C|](1, true);
    }
}",
@"
class C
{
    public C(int i, bool v) { }
}

class D
{
    void M()
    {
        new C(1, true);
    }
}");
        }

        [Fact]
        public async Task TestParams1()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(params int[] i) { }
}

class D
{
    void M()
    {
        new C([|true|], 1);
    }
}",
@"
class C
{
    public C(bool v, params int[] i) { }
}

class D
{
    void M()
    {
        new C(true, 1);
    }
}");
        }

        [Fact]
        public async Task TestParams2()
        {
            await TestMissingAsync(
@"
class C
{
    public C(params int[] i) { }
}

class D
{
    void M()
    {
        new [|C|](1, true);
    }
}");
        }

        [Fact, WorkItem(20708, "https://github.com/dotnet/roslyn/issues/20708")]
        public async Task TestMultiLineParameters1()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(int i,
             /* goo */ int j)
    {

    }

    private void Goo()
    {
        new [|C|](true, 0, 0);
    }
}",
@"
class C
{
    public C(bool v,
             int i,
             /* goo */ int j)
    {

    }

    private void Goo()
    {
        new C(true, 0, 0);
    }
}");
        }

        [Fact, WorkItem(20708, "https://github.com/dotnet/roslyn/issues/20708")]
        public async Task TestMultiLineParameters2()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(int i,
             /* goo */ int j)
    {

    }

    private void Goo()
    {
        new [|C|](0, true, 0);
    }
}",
@"
class C
{
    public C(int i,
             bool v,
             /* goo */ int j)
    {

    }

    private void Goo()
    {
        new C(0, true, 0);
    }
}");
        }

        [Fact, WorkItem(20708, "https://github.com/dotnet/roslyn/issues/20708")]
        public async Task TestMultiLineParameters3()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(int i,
             /* goo */ int j)
    {

    }

    private void Goo()
    {
        new [|C|](0, 0, true);
    }
}",
@"
class C
{
    public C(int i,
             /* goo */ int j,
             bool v)
    {

    }

    private void Goo()
    {
        new C(0, 0, true);
    }
}");
        }

        [Fact, WorkItem(20708, "https://github.com/dotnet/roslyn/issues/20708")]
        public async Task TestMultiLineParameters4()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(
        int i,
        /* goo */ int j)
    {

    }

    private void Goo()
    {
        new [|C|](true, 0, 0);
    }
}",
@"
class C
{
    public C(
        bool v,
        int i,
        /* goo */ int j)
    {

    }

    private void Goo()
    {
        new C(true, 0, 0);
    }
}");
        }

        [Fact, WorkItem(20708, "https://github.com/dotnet/roslyn/issues/20708")]
        public async Task TestMultiLineParameters5()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(
        int i,
        /* goo */ int j)
    {

    }

    private void Goo()
    {
        new [|C|](0, true, 0);
    }
}",
@"
class C
{
    public C(
        int i,
        bool v,
        /* goo */ int j)
    {

    }

    private void Goo()
    {
        new C(0, true, 0);
    }
}");
        }

        [Fact, WorkItem(20708, "https://github.com/dotnet/roslyn/issues/20708")]
        public async Task TestMultiLineParameters6()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(
        int i,
        /* goo */ int j)
    {

    }

    private void Goo()
    {
        new [|C|](0, 0, true);
    }
}",
@"
class C
{
    public C(
        int i,
        /* goo */ int j,
        bool v)
    {

    }

    private void Goo()
    {
        new C(0, 0, true);
    }
}");
        }

        [Fact, WorkItem(20973, "https://github.com/dotnet/roslyn/issues/20973")]
        public async Task TestNullArg1()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(int i) { }
}

class D
{
    void M()
    {
        new [|C|](null, 1);
    }
}",
@"
class C
{
    public C(object value, int i) { }
}

class D
{
    void M()
    {
        new C(null, 1);
    }
}");
        }

        [Fact, WorkItem(20973, "https://github.com/dotnet/roslyn/issues/20973")]
        public async Task TestNullArg2()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(string s) { }
}

class D
{
    void M()
    {
        new [|C|](null, 1);
    }
}",
@"
class C
{
    public C(string s, int v) { }
}

class D
{
    void M()
    {
        new C(null, 1);
    }
}");
        }

        [Fact, WorkItem(20973, "https://github.com/dotnet/roslyn/issues/20973")]
        public async Task TestDefaultArg1()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(int i) { }
}

class D
{
    void M()
    {
        new [|C|](default, 1);
    }
}",
@"
class C
{
    public C(int i, int v) { }
}

class D
{
    void M()
    {
        new C(default, 1);
    }
}");
        }

        [Fact, WorkItem(20973, "https://github.com/dotnet/roslyn/issues/20973")]
        public async Task TestDefaultArg2()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    public C(string s) { }
}

class D
{
    void M()
    {
        new [|C|](default, 1);
    }
}",
@"
class C
{
    public C(string s, int v) { }
}

class D
{
    void M()
    {
        new C(default, 1);
    }
}");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationInstanceMethod1()
        {
            await TestInRegularAndScriptAsync(
@"
class C
{
    void M1()
    {
    }
    void M2()
    {
        int i=0;
        [|M1|](i);
    }
}
",
@"
class C
{
    void M1(int i)
    {
    }
    void M2()
    {
        int i=0;
        M1(i);
    }
}
");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationInheritedMethodGetFixed()
        {
            await TestInRegularAndScriptAsync(
@"
class Base
{
    protected void M1()
    {
    }
}
class C1 : Base
{
    void M2()
    {
        int i = 0;
        [|M1|](i);
    }
}",
@"
class Base
{
    protected void M1(int i)
    {
    }
}
class C1 : Base
{
    void M2()
    {
        int i = 0;
        M1(i);
    }
}");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationInheritedMethodInMetadatGetsNotFixed()
        {
            await TestMissingAsync(
    @"
class C1
{
    void M2()
    {
        int i = 0;
        [|GetHashCode|](i);
    }
}");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationLocalFunction()
        {
            await TestInRegularAndScriptAsync(
@"
class C1
{
    void M1()
    {
        int Local() => 1;
        [|Local|](2);
    }
}",
@"
class C1
{
    void M1()
    {
        int Local(int v) => 1;
        Local(2);
    }
}");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        [Trait("TODO", "Fix broken")]
        public async Task TestInvocationLambda1()
        {
            await TestMissingInRegularAndScriptAsync(
@"
using System;
class C1
{
    void M1()
    {
        Action a = () => { };
        [|a|](2);
    }
}");
            //Should be Action<int> a = (int v) => { };
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationStaticMethod()
        {
            await TestInRegularAndScriptAsync(
@"
class C1
{
    static void M1()
    {
    }
    void M2()
    {
        [|M1|](1);
    }
}",
@"
class C1
{
    static void M1(int v)
    {
    }
    void M2()
    {
        M1(1);
    }
}");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationExtensionMethod()
        {
            var code =
@"
namespace N {
static class Extensions
{
    public static void ExtensionM1(this object o)
    {
    }
}
class C1
{
    void M1()
    {
        new object().[|ExtensionM1|](1);
    }
}}";
            var fix =
@"
namespace N {
static class Extensions
{
    public static void ExtensionM1(this object o, int v)
    {
    }
}
class C1
{
    void M1()
    {
        new object().ExtensionM1(1);
    }
}}";
            await TestInRegularAndScriptAsync(code, fix);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationExtensionMethod_StaticInvocationStyle()
        {
            // error CS1501: No overload for method 'ExtensionM1' takes 2 arguments
            var code =
@"
namespace N {
static class Extensions
{
    public static void ExtensionM1(this object o)
    {
    }
}
class C1
{
    void M1()
    {
        Extensions.[|ExtensionM1|](new object(), 1);
    }
}}";
            var fix =
@"
namespace N {
static class Extensions
{
    public static void ExtensionM1(this object o, int v)
    {
    }
}
class C1
{
    void M1()
    {
        Extensions.ExtensionM1(new object(), 1);
    }
}}";
            await TestInRegularAndScriptAsync(code, fix);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationOverride()
        {
            var code = @"
class Base
{
    protected virtual void M1() { }
}
class C1 : Base
{
    protected override void M1() { }
    void M2()
    {
        [|M1|](1);
    }
}";
            var fix_DeclarationOnly = @"
class Base
{
    protected virtual void M1() { }
}
class C1 : Base
{
    protected override void M1(int v) { }
    void M2()
    {
        M1(1);
    }
}";
            var fix_All = @"
class Base
{
    protected virtual void M1(int v) { }
}
class C1 : Base
{
    protected override void M1(int v) { }
    void M2()
    {
        M1(1);
    }
}";
            await TestInRegularAndScriptAsync(code, fix_DeclarationOnly, index: 0);
            await TestInRegularAndScriptAsync(code, fix_All, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationExplicitInterface()
        {
            var code = @"
interface I1
{
    void M1();
}
class C1 : I1
{
    void I1.M1() { }
    void M2()
    {
        ((I1)this).[|M1|](1);
    }
}";
            var fix_DeclarationOnly = @"
interface I1
{
    void M1(int v);
}
class C1 : I1
{
    void I1.M1() { }
    void M2()
    {
        ((I1)this).M1(1);
    }
}";
            var fix_All = @"
interface I1
{
    void M1(int v);
}
class C1 : I1
{
    void I1.M1(int v) { }
    void M2()
    {
        ((I1)this).M1(1);
    }
}";
            await TestInRegularAndScriptAsync(code, fix_DeclarationOnly, index: 0);
            await TestInRegularAndScriptAsync(code, fix_All, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationImplicitInterface()
        {
            var code =
@"
interface I1
{
    void M1();
}
class C1 : I1
{
    public void M1() { }
    void M2()
    {
        [|M1|](1);
    }
}";
            var fix_DeclarationOnly = @"
interface I1
{
    void M1();
}
class C1 : I1
{
    public void M1(int v) { }
    void M2()
    {
        M1(1);
    }
}";
            var fix_All = @"
interface I1
{
    void M1(int v);
}
class C1 : I1
{
    public void M1(int v) { }
    void M2()
    {
        M1(1);
    }
}";
            await TestInRegularAndScriptAsync(code, fix_DeclarationOnly, index: 0);
            await TestInRegularAndScriptAsync(code, fix_All, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationImplicitInterfaces()
        {
            var code =
@"
interface I1
{
    void M1();
}
interface I2
{
    void M1();
}
class C1 : I1, I2
{
    public void M1() { }
    void M2()
    {
        [|M1|](1);
    }
}";
            var fix_DeclarationOnly = @"
interface I1
{
    void M1();
}
interface I2
{
    void M1();
}
class C1 : I1, I2
{
    public void M1(int v) { }
    void M2()
    {
        M1(1);
    }
}";
            var fix_All = @"
interface I1
{
    void M1(int v);
}
interface I2
{
    void M1(int v);
}
class C1 : I1, I2
{
    public void M1(int v) { }
    void M2()
    {
        M1(1);
    }
}";
            await TestInRegularAndScriptAsync(code, fix_DeclarationOnly, index: 0);
            await TestInRegularAndScriptAsync(code, fix_All, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        [Trait("TODO", "Fix broken")]
        public async Task TestInvocationGenericMethod()
        {
            await TestInRegularAndScriptAsync(
@"
class C1
{
    void M1<T>(T arg) { }
    void M2()
    {
        [|M1|](1, 2);
    }
}",
@"
class C1
{
    void M1<T>(T arg, int v) { }
    void M2()
    {
        M1(1, 2);
    }
}");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationRecursion()
        {
            await TestInRegularAndScriptAsync(
@"
class C1
{
    void M1()
    {
        [|M1|](1);
    }
}",
@"
class C1
{
    void M1(int v)
    {
        M1(1);
    }
}");
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationOverloads1()
        {
            var code =
@"
class C1
{
    void M1(string s) { }
    void M1(int i) { }
    void M2()
    {
        [|M1|](1, 2);
    }
}";
            var fix0 =
@"
class C1
{
    void M1(string s) { }
    void M1(int i, int v) { }
    void M2()
    {
        M1(1, 2);
    }
}";
            var fix1 =
@"
class C1
{
    void M1(int v, string s) { }
    void M1(int i) { }
    void M2()
    {
        M1(1, 2);
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, 0);
            await TestInRegularAndScriptAsync(code, fix1, 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationOverloads2()
        {
            var code =
@"
class C1
{
    void M1(string s1, string s2) { }
    void M1(string s) { }
    void M1(int i) { }
    void M2()
    {
        M1(1, [|2|]);
    }
}";
            var fix0 =
@"
class C1
{
    void M1(string s1, string s2) { }
    void M1(string s) { }
    void M1(int i, int v) { }
    void M2()
    {
        M1(1, 2);
    }
}";
            var fix1 =
@"
class C1
{
    void M1(string s1, string s2) { }
    void M1(int v, string s) { }
    void M1(int i) { }
    void M2()
    {
        M1(1, 2);
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, 0);
            await TestInRegularAndScriptAsync(code, fix1, 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationTuple1()
        {
            var code =
@"
class C1
{
    void M1((int, int) t1)
    {
    }
    void M2()
    {
        [|M1|]((0, 0), (1, ""1""));
    }
}";
            var fix0 =
    @"
class C1
{
    void M1((int, int) t1, (int, string) value)
    {
    }
    void M2()
    {
        M1((0, 0), (1, ""1""));
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationTuple2()
        {
            var code =
@"
class C1
{
    void M1((int, int) t1)
    {
    }
    void M2()
    {
        var tup = (1, ""1"");
        [|M1|]((0, 0), tup);
    }
}";
            var fix0 =
    @"
class C1
{
    void M1((int, int) t1, (int, string) tup)
    {
    }
    void M2()
    {
        var tup = (1, ""1"");
        M1((0, 0), tup);
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationTuple3()
        {
            var code =
@"
class C1
{
    void M1((int, int) t1)
    {
    }
    void M2()
    {
        var tup = (i: 1, s: ""1"");
        [|M1|]((0, 0), tup);
    }
}";
            var fix0 =
    @"
class C1
{
    void M1((int, int) t1, (int i, string s) tup)
    {
    }
    void M2()
    {
        var tup = (i: 1, s: ""1"");
        M1((0, 0), tup);
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Missing_TypeArguments_AddingTypeArgumentAndParameter()
        {
            // error CS0305: Using the generic method 'C1.M1<T>(T)' requires 1 type arguments
            var code =
@"
class C1
{
    void M1<T>(T i) { }
    void M2()
    {
        [|M1|]<int, bool>(1, true);
    }
}";
            // Could be fixed as void M1<T, T1>(T i, T1 v) { }
            await TestMissingInRegularAndScriptAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Missing_TypeArguments_AddingTypeArgument()
        {
            // error CS0308: The non-generic method 'C1.M1(int)' cannot be used with type arguments
            var code =
@"
class C1
{
    void M1(int i) { }
    void M2()
    {
        [|M1<bool>|](1, true);
    }
}";
            // Could be fixed as void M1<T>(int i, T v) { }
            await TestMissingInRegularAndScriptAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        [Trait("TODO", "Fix missing")]
        public async Task TestInvocation_Missing_ExplicitInterfaceImplementation()
        {
            // error CS0539: 'C1.M1(int)' in explicit interface declaration is not a member of interface
            var code =
@"
interface I1
{
    void M1();
}
class C1 : I1
{
        void I1.M1() { }
        void I1.[|M1|](int i) { }
}";
            // Could apply argument to interface method: void M1(int i);
            await TestMissingAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_OverloadResolutionFailure()
        {
            // error CS1503: Argument 1: cannot convert from 'double' to 'int'
            var code =
@"
    class C1
    {
        void M1(int i1, int i2) { }
        void M1(double d) { }
        void M2()
        {
            M1([|1.0|], 1);
        }
    }
";
            var fix0 =
@"
    class C1
    {
        void M1(int i1, int i2) { }
        void M1(double d, int v) { }
        void M2()
        {
            M1(1.0, 1);
        }
    }
";
            await TestInRegularAndScriptAsync(code, fix0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_LambdaExpressionParameter()
        {
            // error CS1660: Cannot convert lambda expression to type 'int' because it is not a delegate type
            var code =
@"
    class C1
    {
        void M1(int i1, int i2) { }
        void M1(System.Action a) { }
        void M2()
        {
            M1([|()=> { }|], 1);
        }
    }
";
            var fix =
@"
    class C1
    {
        void M1(int i1, int i2) { }
        void M1(System.Action a, int v) { }
        void M2()
        {
            M1(()=> { }, 1);
        }
    }
";
            await TestInRegularAndScriptAsync(code, fix);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_NamedParameter()
        {
            // error CS1739: The best overload for 'M1' does not have a parameter named 'i2'
            var code =
@"
    class C1
    {
        void M1(int i1) { }
        void M2()
        {
            M1([|i2|]: 1);
        }
    }
";
            var fix =
@"
    class C1
    {
        void M1(int i1, int i2) { }
        void M2()
        {
            M1(i2: 1);
        }
    }
";
            await TestInRegularAndScriptAsync(code, fix);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationAddTypeParameter_AddTypeParameterIfUserSpecifiesOne_OnlyTypeArgument()
        {
            var code =
@"
    class C1
    {
        void M1() { }
        void M2()
        {
            [|M1|]<bool>();
        }
    }
";
            // Could be fixed as void M1<T>() { }
            await TestMissingInRegularAndScriptAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocationAddTypeParameter_AddTypeParameterIfUserSpecifiesOne_TypeArgumentAndParameterArgument()
        {
            var code =
@"
    class C1
    {
        void M1() { }
        void M2()
        {
            [|M1|]<bool>(true);
        }
    }
";
            // Could be fixed to void M1<T>(T v) { }
            await TestMissingInRegularAndScriptAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_ExisitingTypeArgumentIsNotGeneralized()
        {
            var code =
@"
    class C1
    {
        void M1<T>(T v) { }
        void M2()
        {
            [|M1|](true, true);
        }
    }
";
            var fix0 =
@"
    class C1
    {
        void M1<T>(T v, bool v1) { }
        void M2()
        {
            M1(true, true);
        }
    }
";
            await TestInRegularAndScriptAsync(code, fix0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_AddParameterToMethodWithParams()
        {
            // error CS1503: Argument 1: cannot convert from 'bool' to 'int'
            var code =
@"
    class C1
    {
        static void M1(params int[] nums) { }
        static void M2()
        {
            M1([|true|], 4);
        }
    }
";
            var fix0 =
@"
    class C1
    {
        static void M1(bool v, params int[] nums) { }
        static void M2()
        {
            M1(true, 4);
        }
    }
";
            await TestInRegularAndScriptAsync(code, fix0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_FixingVirtualFixesOverrideToo()
        {
            // error CS1501: No overload for method 'M1' takes 1 arguments
            var code =
@"
    class BaseClass
    {
        protected virtual void M1() { }
    }
    class Derived1: BaseClass
    {
        protected override void M1() { }
    }
    class Test: BaseClass
    {
        void M2() 
        {
            [|M1|](1);
        }
    }
";
            var fix_DeclarationOnly =
@"
    class BaseClass
    {
        protected virtual void M1(int v) { }
    }
    class Derived1: BaseClass
    {
        protected override void M1() { }
    }
    class Test: BaseClass
    {
        void M2() 
        {
            M1(1);
        }
    }
";
            var fix_All =
@"
    class BaseClass
    {
        protected virtual void M1(int v) { }
    }
    class Derived1: BaseClass
    {
        protected override void M1(int v) { }
    }
    class Test: BaseClass
    {
        void M2() 
        {
            M1(1);
        }
    }
";
            await TestInRegularAndScriptAsync(code, fix_DeclarationOnly, index: 0);
            await TestInRegularAndScriptAsync(code, fix_All, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_PartialMethods()
        {
            var code =
@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" CommonReferences=""true"">
        <Document>
namespace N1
{
    partial class C1
    {
        partial void PartialM();
    }
}
        </Document>
        <Document>
namespace N1
{
    partial class C1
    {
        partial void PartialM() { }
        void M1()
        {
            [|PartialM|](1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            var fix0 =
@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" CommonReferences=""true"">
        <Document>
namespace N1
{
    partial class C1
    {
        partial void PartialM(int v);
    }
}
        </Document>
        <Document>
namespace N1
{
    partial class C1
    {
        partial void PartialM(int v) { }
        void M1()
        {
            PartialM(1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            await TestInRegularAndScriptAsync(code, fix0);
        }

        [Fact]
        public async Task TestInvocation_Cascading_ExtendedPartialMethods()
        {
            var code =
@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" CommonReferences=""true"">
        <Document>
namespace N1
{
    partial class C1
    {
        public partial void PartialM();
    }
}
        </Document>
        <Document>
namespace N1
{
    partial class C1
    {
        public partial void PartialM() { }
        void M1()
        {
            [|PartialM|](1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            var fix0 =
@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" CommonReferences=""true"">
        <Document>
namespace N1
{
    partial class C1
    {
        public partial void PartialM(int v);
    }
}
        </Document>
        <Document>
namespace N1
{
    partial class C1
    {
        public partial void PartialM(int v) { }
        void M1()
        {
            PartialM(1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            await TestInRegularAndScriptAsync(code, fix0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_PartialMethodsInSameDocument()
        {
            var code =
@"
namespace N1
{
    partial class C1
    {
        partial void PartialM();
    }
    partial class C1
    {
        partial void PartialM() { }
        void M1()
        {
            [|PartialM|](1);
        }
    }
}";
            var fix0 =
@"
namespace N1
{
    partial class C1
    {
        partial void PartialM(int v);
    }
    partial class C1
    {
        partial void PartialM(int v) { }
        void M1()
        {
            PartialM(1);
        }
    }
}";
            await TestInRegularAndScriptAsync(code, fix0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_BaseNotInSource()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments
            var code =
@"
<Workspace>
    <Project Language=""C#"" CommonReferences=""true"">
        <MetadataReferenceFromSource Language=""C#"" CommonReferences=""true"">
            <Document FilePath=""ReferencedDocument"">
namespace N
{
    public class BaseClass
    {
        public virtual void M() { }
    }
}
            </Document>
        </MetadataReferenceFromSource>
        <Document FilePath=""TestDocument"">
namespace N
{
    public class Derived: BaseClass
    {
        public void M2()
        {
            [|M|](1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            await TestMissingAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_RootNotInSource()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments
            var code =
@"
<Workspace>
    <Project Language=""C#"" CommonReferences=""true"">
        <MetadataReferenceFromSource Language=""C#"" CommonReferences=""true"">
            <Document FilePath=""ReferencedDocument"">
namespace N
{
    public class BaseClass
    {
        public virtual void M() { }
    }
}
            </Document>
        </MetadataReferenceFromSource>
        <Document FilePath=""TestDocument"">
namespace N
{
    public class Derived: BaseClass
    {
        public override void M() { }
    }
    public class DerivedDerived: Derived
    {
        public void M2()
        {
            [|M|](1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            var fixedDocumentWithoutConflictAnnotation = @"
namespace N
{
    public class Derived: BaseClass
    {
        public override void M(int v) { }
    }
    public class DerivedDerived: Derived
    {
        public void M2()
        {
            M(1);
        }
    }
}
        ";
            var fixedDocumentWithConflictAnnotation = @"
namespace N
{
    public class Derived: BaseClass
    {
        public override void M({|Conflict:int v|}) { }
    }
    public class DerivedDerived: Derived
    {
        public void M2()
        {
            M(1);
        }
    }
}
        ";
            await TestInRegularAndScriptAsync(code, fixedDocumentWithoutConflictAnnotation, index: 0);
            await TestInRegularAndScriptAsync(code, fixedDocumentWithConflictAnnotation, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_ManyReferencesInManyProjects()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments
            var code =
@"
<Workspace>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""A1"">
        <Document FilePath=""ReferencedDocument"">
namespace N
{
    public class BaseClass
    {
        public virtual void M() { }
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A2"">
        <ProjectReference>A1</ProjectReference>
        <Document>
namespace N
{
    public class Derived1: BaseClass
    {
        public override void M() { }
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A3"">
        <ProjectReference>A1</ProjectReference>
        <Document>
namespace N
{
    public class Derived2: BaseClass
    {
        public override void M() { }
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A4"">
        <ProjectReference>A3</ProjectReference>
        <Document>
namespace N
{
    public class T
    {
        public void Test() { 
            new Derived2().[|M|](1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            var fix_All =
@"
<Workspace>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""A1"">
        <Document FilePath=""ReferencedDocument"">
namespace N
{
    public class BaseClass
    {
        public virtual void M(int v) { }
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A2"">
        <ProjectReference>A1</ProjectReference>
        <Document>
namespace N
{
    public class Derived1: BaseClass
    {
        public override void M(int v) { }
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A3"">
        <ProjectReference>A1</ProjectReference>
        <Document>
namespace N
{
    public class Derived2: BaseClass
    {
        public override void M(int v) { }
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A4"">
        <ProjectReference>A3</ProjectReference>
        <Document>
namespace N
{
    public class T
    {
        public void Test() { 
            new Derived2().M(1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            await TestInRegularAndScriptAsync(code, fix_All, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_OfferFixCascadingForImplicitInterface()
        {
            // error CS1501: No overload for method 'M1' takes 1 arguments
            var code =
@"
    interface I1
    {
        void M1();
    }
    class C: I1
    {
        public void M1() { }
        void MTest() 
        {
            [|M1|](1);
        }
    }
";
            var fix_DeclarationOnly =
@"
    interface I1
    {
        void M1();
    }
    class C: I1
    {
        public void M1(int v) { }
        void MTest() 
        {
            M1(1);
        }
    }
";
            var fix_All =
@"
    interface I1
    {
        void M1(int v);
    }
    class C: I1
    {
        public void M1(int v) { }
        void MTest() 
        {
            M1(1);
        }
    }
";
            await TestInRegularAndScriptAsync(code, fix_DeclarationOnly, index: 0);
            await TestInRegularAndScriptAsync(code, fix_All, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Cascading_CrossLanguage()
        {
            var code =
@"
<Workspace>
    <Project Language=""Visual Basic"" CommonReferences=""true"" AssemblyName=""VB1"">
        <Document FilePath=""ReferencedDocument"">
Namespace N
    Public Class BaseClass
        Public Overridable Sub M()
        End Sub
    End Class
End Namespace
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A2"">
        <ProjectReference>VB1</ProjectReference>
        <Document>
namespace N
{
    public class Derived: BaseClass
    {
        public override void M() { }
    }
    public class T
    {
        public void Test() { 
            new Derived().[|M|](1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            var fix =
@"
<Workspace>
    <Project Language=""Visual Basic"" CommonReferences=""true"" AssemblyName=""VB1"">
        <Document FilePath=""ReferencedDocument"">
Namespace N
    Public Class BaseClass
        Public Overridable Sub M(v As Integer)
        End Sub
    End Class
End Namespace
        </Document>
    </Project>
    <Project Language=""C#"" CommonReferences=""true""  AssemblyName=""A2"">
        <ProjectReference>VB1</ProjectReference>
        <Document>
namespace N
{
    public class Derived: BaseClass
    {
        public override void M(int v) { }
    }
    public class T
    {
        public void Test() { 
            new Derived().M(1);
        }
    }
}
        </Document>
    </Project>
</Workspace>";
            await TestInRegularAndScriptAsync(code, fix, index: 1);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_Positional_MoreThanOneArgumentToMuch()
        {
            var code =
@"
class C
{
    void M() { }
    void Test()
    {
        [|M|](1, 2, 3, 4);
    }
}";
            var fix0 =
@"
class C
{
    void M(int v) { }
    void Test()
    {
        M(1, 2, 3, 4);
    }
}";
            await TestActionCountAsync(code, 1);
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_Positional_WithOptionalParam()
        {
            // error CS1501: No overload for method 'M' takes 2 arguments
            var code =
@"
class C
{
    void M(int i = 1) { }
    void Test()
    {
        [|M|](1, 2);
    }
}";
            var fix0 =
@"
class C
{
    void M(int i = 1, int v = 0) { }
    void Test()
    {
        M(1, 2);
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_Named_WithOptionalParam()
        {
            // error CS1739: The best overload for 'M' does not have a parameter named 'i3'
            var code =
@"
class C
{
    void M(int i1, int i2 = 1) { }
    void Test()
    {
        M(1, i2: 2, [|i3|]: 3);
    }
}";
            var fix0 =
@"
class C
{
    void M(int i1, int i2 = 1, int i3 = 0) { }
    void Test()
    {
        M(1, i2: 2, i3: 3);
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_Positional_WithParams()
        {
            // error CS1503: Argument 1: cannot convert from 'string' to 'int'
            var code =
@"
class C
{
    void M(params int[] ints) { }
    void Test()
    {
        M([|""text""|]);
    }
}";
            var fix0 =
@"
class C
{
    void M(string v, params int[] ints) { }
    void Test()
    {
        M(""text"");
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_Named_WithTypemissmatch()
        {
            // error CS1503: Argument 1: cannot convert from 'string' to 'int'
            var code =
@"
class C
{
    void M(int i) { }
    void Test()
    {
        M(i: [|""text""|]);
    }
}";
            await TestMissingInRegularAndScriptAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_NamedAndPositional1()
        {
            // error CS1739: The best overload for 'M' does not have a parameter named 'i2'
            var code =
@"
class C
{
    void M(int i1, string s) { }
    void Test()
    {
        M(1, s: ""text"", [|i2|]: 0);
    }
}";
            var fix0 =
@"
class C
{
    void M(int i1, string s, int i2) { }
    void Test()
    {
        M(1, s: ""text"", i2: 0);
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_NamedAndPositional2()
        {
            // CS1744 is not yet a supported diagnostic (just declaring the diagnostic as supported does not work)
            // error CS1744: Named argument 's' specifies a parameter for which a positional argument has already been given
            var code =
@"
class C
{
    void M(string s) { }
    void Test()
    {
        M(1, [|s|]: ""text"");
    }
}";
            await TestMissingInRegularAndScriptAsync(code);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_Incomplete_1()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments
            var code =
@"
class C
{
    void M() { }
    void Test()
    {
        [|M|](1
    }
}";
            var fix0 =
@"
class C
{
    void M(int v) { }
    void Test()
    {
        M(1
    }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_Incomplete_2()
        {
            // error CS1503: Argument 1: cannot convert from 'string' to 'int'
            var code =
@"
class C
{
    void M(int v) { }
    void Test()
    {
        [|M|](""text"", 1
";
            var fix0 =
@"
class C
{
    void M(string v1, int v) { }
    void Test()
    {
        M(""text"", 1
";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_RefParameter()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments            
            var code =
@"
class C
{
    void M() { }
    void Test()
    {
        int i = 0;
        [|M|](ref i);
    }
}
";
            var fix0 =
@"
class C
{
    void M(ref int i) { }
    void Test()
    {
        int i = 0;
        M(ref i);
    }
}
";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_OutParameter_WithTypeDeclarationOutsideArgument()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments            
            var code =
@"
class C
{
    void M() { }
    void Test()
    {
        int i = 0;
        [|M|](out i);
    }
}
";
            var fix0 =
@"
class C
{
    void M(out int i) { }
    void Test()
    {
        int i = 0;
        M(out i);
    }
}
";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_OutParameter_WithTypeDeclarationInArgument()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments            
            var code =
@"
class C
{
    void M() { }
    void Test()
    {
        [|M|](out int i);
    }
}
";
            var fix0 =
@"
class C
{
    void M(out int i) { }
    void Test()
    {
        M(out int i);
    }
}
";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_InvocationStyles_OutParameter_WithVarTypeDeclarationInArgument()
        {
            // error CS1501: No overload for method 'M' takes 1 arguments            
            var code =
@"
class C
{
    void M() { }
    void Test()
    {
        [|M|](out var i);
    }
}
";
            var fix0 =
@"
class C
{
    void M(out object i) { }
    void Test()
    {
        M(out var i);
    }
}
";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(21446, "https://github.com/dotnet/roslyn/issues/21446")]
        public async Task TestInvocation_Indexer_NotSupported()
        {
            // Could be fixed by allowing ElementAccessExpression next to InvocationExpression
            // in AbstractAddParameterCodeFixProvider.RegisterCodeFixesAsync.
            // error CS1501: No overload for method 'this' takes 2 arguments
            var code =
@"
public class C {
    public int this[int i] 
    { 
        get => 1; 
        set {} 
    }
    
    public void Test() {
        var i = [|this[0,0]|];
    }
}";
            await TestMissingAsync(code);
        }

        [Fact, WorkItem(29061, "https://github.com/dotnet/roslyn/issues/29061")]
        public async Task TestThis_DontOfferToFixTheConstructorWithTheDiagnosticOnIt()
        {
            // error CS1729: 'C' does not contain a constructor that takes 1 arguments
            var code =
@"
public class C {
    
    public C(): [|this|](1)
    { }
}";
            await TestMissingAsync(code);
        }

        [Fact, WorkItem(29061, "https://github.com/dotnet/roslyn/issues/29061")]
        public async Task TestThis_Fix_IfACandidateIsAvailable()
        {
            // error CS1729: 'C' does not contain a constructor that takes 2 arguments
            var code =
@"
class C 
{
    public C(int i) { }
    
    public C(): [|this|](1, 1)
    { }
}";
            var fix0 =
@"
class C 
{
    public C(int i, int v) { }
    
    public C(): this(1, 1)
    { }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
            await TestActionCountAsync(code, 1);
        }

        [Fact, WorkItem(29061, "https://github.com/dotnet/roslyn/issues/29061")]
        public async Task TestBase_Fix_IfACandidateIsAvailable()
        {
            // error CS1729: 'B' does not contain a constructor that takes 1 arguments
            var code =
@"
public class B
{
    B() { }
}
public class C : B
{
    public C(int i) : [|base|](i) { }
}";
            var fix0 =
@"
public class B
{
    B(int i) { }
}
public class C : B
{
    public C(int i) : base(i) { }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
            await TestActionCountAsync(code, 1);
        }

        [Fact, WorkItem(29753, "https://github.com/dotnet/roslyn/issues/29753")]
        public async Task LocalFunction_AddParameterToLocalFunctionWithOneParameter()
        {
            // CS1501 No overload for method takes 2 arguments
            var code =
@"
class Rsrp
{
  public void M()
  {
    [|Local|](""ignore this"", true);
    void Local(string whatever)
    {

    }
  }
}";
            var fix0 =
@"
class Rsrp
{
  public void M()
  {
    Local(""ignore this"", true);
    void Local(string whatever, bool v)
    {

    }
  }
}";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(29752, "https://github.com/dotnet/roslyn/issues/29752")]
        public async Task LocalFunction_AddNamedParameterToLocalFunctionWithOneParameter()
        {
            // CS1739: The best overload for 'Local' does not have a parameter named 'mynewparameter'
            var code =
@"
class Rsrp
{
    public void M()
    {
        Local(""ignore this"", [|mynewparameter|]: true);
        void Local(string whatever)
        {

        }
    }
}
";
            var fix0 =
@"
class Rsrp
{
    public void M()
    {
        Local(""ignore this"", mynewparameter: true);
        void Local(string whatever, bool mynewparameter)
        {

        }
    }
}
";
            await TestInRegularAndScriptAsync(code, fix0, index: 0);
        }

        [Fact, WorkItem(39270, "https://github.com/dotnet/roslyn/issues/39270")]
        public async Task TestWithArgThatHasImplicitConversionToParamType1()
        {
            await TestInRegularAndScriptAsync(
@"
class BaseClass { }

class MyClass : BaseClass
{
    void TestFunc()
    {
        MyClass param1 = new MyClass();
        int newparam = 1;

        [|MyFunc|](param1, newparam);
    }

    void MyFunc(BaseClass param1) { }
}",
@"
class BaseClass { }

class MyClass : BaseClass
{
    void TestFunc()
    {
        MyClass param1 = new MyClass();
        int newparam = 1;

        MyFunc(param1, newparam);
    }

    void MyFunc(BaseClass param1, int newparam) { }
}");
        }

        [Fact]
        public async Task TestOnExtensionGetEnumerator()
        {
            var code =
@"
using System.Collections.Generic;
namespace N {
static class Extensions
{
    public static IEnumerator<int> GetEnumerator(this object o)
    {
    }
}
class C1
{
    void M1()
    {
        new object().[|GetEnumerator|](1);
        foreach (var a in new object());
    }
}}";
            var fix =
@"
using System.Collections.Generic;
namespace N {
static class Extensions
{
    public static IEnumerator<int> GetEnumerator(this object o, int v)
    {
    }
}
class C1
{
    void M1()
    {
        new object().GetEnumerator(1);
        foreach (var a in new object());
    }
}}";
            await TestInRegularAndScriptAsync(code, fix);
        }

        [Fact]
        public async Task TestOnExtensionGetAsyncEnumerator()
        {
            var code =
@"
using System.Collections.Generic;
using System.Threading.Tasks;
namespace N {
static class Extensions
{
    public static IAsyncEnumerator<int> GetAsyncEnumerator(this object o)
    {
    }
}
class C1
{
    async Task M1()
    {
        new object().[|GetAsyncEnumerator|](1);
        await foreach (var a in new object());
    }
}}" + IAsyncEnumerable;
            var fix =
@"
using System.Collections.Generic;
using System.Threading.Tasks;
namespace N {
static class Extensions
{
    public static IAsyncEnumerator<int> GetAsyncEnumerator(this object o, int v)
    {
    }
}
class C1
{
    async Task M1()
    {
        new object().GetAsyncEnumerator(1);
        await foreach (var a in new object());
    }
}}" + IAsyncEnumerable;
            await TestInRegularAndScriptAsync(code, fix);
        }

        [Fact, WorkItem(44271, "https://github.com/dotnet/roslyn/issues/44271")]
        public async Task TopLevelStatement()
        {
            await TestInRegularAndScriptAsync(@"
[|local|](1, 2, 3);

void local(int x, int y)
{
}
",
@"
[|local|](1, 2, 3);

void local(int x, int y, int v)
{
}
", parseOptions: TestOptions.Regular.WithLanguageVersion(LanguageVersion.CSharp9));
        }

        [Fact, WorkItem(44271, "https://github.com/dotnet/roslyn/issues/44271")]
        public async Task TopLevelStatement_Nested()
        {
            await TestInRegularAndScriptAsync(@"
void outer()
{
    [|local|](1, 2, 3);

    void local(int x, int y)
    {
    }
}
",
@"
void outer()
{
    local(1, 2, 3);

    void local(int x, int y, int v)
    {
    }
}
");
        }

        [Fact, WorkItem(42559, "https://github.com/dotnet/roslyn/issues/42559")]
        public async Task TestAddParameter_ImplicitObjectCreation()
        {
            await TestInRegularAndScriptAsync(@"
class C
{
    C(int i) { }

    void M()
    {
       C c = [||]new(1, 2);
    }
}",
@"
class C
{
    C(int i, int v) { }

    void M()
    {
       C c = new(1, 2);
    }
}");
        }

        [Fact, WorkItem(48042, "https://github.com/dotnet/roslyn/issues/48042")]
        public async Task TestNamedArgOnExtensionMethod()
        {
            await TestInRegularAndScriptAsync(
@"
namespace r
{
    static class AbcExtensions
    {
        public static Abc Act(this Abc state, bool p = true) => state;
    }
    class Abc {
        void Test()
            => new Abc().Act([|param3|]: 123);
    }
}",
@"
namespace r
{
    static class AbcExtensions
    {
        public static Abc Act(this Abc state, bool p = true, int param3 = 0) => state;
    }
    class Abc {
        void Test()
            => new Abc().Act(param3: 123);
    }
}");
        }

        [Fact, WorkItem(54408, "https://github.com/dotnet/roslyn/issues/54408")]
        public async Task TestPositionalRecord()
        {
            await TestInRegularAndScriptAsync(@"
var b = ""B"";
var r = [|new R(1, b)|];

record R(int A);

namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit { }
}
", @"
var b = ""B"";
var r = new R(1, b);

record R(int A, string b);

namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit { }
}
", parseOptions: TestOptions.Regular.WithLanguageVersion(LanguageVersion.CSharp9));
        }

        [Fact, WorkItem(54408, "https://github.com/dotnet/roslyn/issues/54408")]
        public async Task TestPositionalRecordStruct()
        {
            await TestInRegularAndScriptAsync(@"
var b = ""B"";
var r = [|new R(1, b)|];

record struct R(int A);

namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit { }
}
", @"
var b = ""B"";
var r = new R(1, b);

record struct R(int A, string b);

namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit { }
}
", parseOptions: TestOptions.Regular.WithLanguageVersion(LanguageVersion.CSharp9));
        }

        [Fact, WorkItem(56952, "https://github.com/dotnet/roslyn/issues/56952")]
        public async Task TestRecordsNamingConventions()
        {
            await TestInRegularAndScript1Async(@"[|new Test(""repro"")|];

record Test();
", @"new Test(""repro"");

record Test(string V);
");
        }

        [Fact, WorkItem(56952, "https://github.com/dotnet/roslyn/issues/56952")]
        public async Task TestRecordsNamingConventions_RecordStruct()
        {
            await TestInRegularAndScript1Async(@"[|new Test(""repro"")|];

record struct Test();
", @"new Test(""repro"");

record struct Test(string V);
");
        }

        [Fact, WorkItem(61715, "https://github.com/dotnet/roslyn/issues/61715")]
        public async Task TestMethodGroup1()
        {
            await TestInRegularAndScript1Async(@"public class Example
{
    public void Add(int x)
    {
    }

    public void DoSomething()
    {
    }

    public void Main()
    {
        [|DoSomething|](Add);
    }
}", @"public class Example
{
    public void Add(int x)
    {
    }

    public void DoSomething(System.Action<int> add)
    {
    }

    public void Main()
    {
        DoSomething(Add);
    }
}");
        }

        [Fact, WorkItem(61715, "https://github.com/dotnet/roslyn/issues/61715")]
        public async Task TestMethodGroup2()
        {
            await TestInRegularAndScript1Async(@"public class Example
{
    public void Add(int x, string y)
    {
    }

    public void DoSomething()
    {
    }

    public void Main()
    {
        [|DoSomething|](Add);
    }
}", @"public class Example
{
    public void Add(int x, string y)
    {
    }

    public void DoSomething(System.Action<int, string> add)
    {
    }

    public void Main()
    {
        DoSomething(Add);
    }
}");
        }

        [Fact, WorkItem(61715, "https://github.com/dotnet/roslyn/issues/61715")]
        public async Task TestMethodGroup3()
        {
            await TestInRegularAndScript1Async(@"public class Example
{
    public int Add(int x, string y)
    {
        return 0;
    }

    public void DoSomething()
    {
    }

    public void Main()
    {
        [|DoSomething|](Add);
    }
}", @"public class Example
{
    public int Add(int x, string y)
    {
        return 0;
    }

    public void DoSomething(System.Func<int, string, int> add)
    {
    }

    public void Main()
    {
        DoSomething(Add);
    }
}");
        }
    }
}
