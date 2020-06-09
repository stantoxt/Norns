﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Norns.DestinyLoom;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Xunit;

namespace Norns.UT.DestinyLoom
{
    public class EmptyProxyGenerator : AbstractProxyGenerator
    {
        public override IEnumerable<IInterceptorGenerator> FindInterceptorGenerators()
        {
            yield return new EmptyInterceptorGenerator();
        }

        public override bool CanProxy(INamedTypeSymbol @type)
        {
            return @type.ToDisplayString().StartsWith("Norns");
        }

        public override IEnumerable<AbstractProxyClassGenerator> FindProxyClassGenerators(IInterceptorGenerator[] interceptors)
        {
            yield return new InterfaceProxyClassGenerator(interceptors);
        }
    }

    public class ProxyGeneratorTest
    {
        public static readonly CSharpParseOptions Regular = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        public static readonly CSharpCompilationOptions DebugDll = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug);

        public static string GetUniqueName()
        {
            return Guid.NewGuid().ToString("D");
        }

        public static Compilation GenerateSource(string source, ISourceGenerator sourceGenerator)
        {
            Compilation compilation = CSharpCompilation.Create(GetUniqueName(),
                new[] { SyntaxFactory.ParseSyntaxTree(SourceText.From(source, Encoding.UTF8), Regular, "") },
                AppDomain.CurrentDomain.GetAssemblies().Where(i => !i.IsDynamic).Select(i => AssemblyMetadata.CreateFromFile(i.Location).GetReference()),
                DebugDll);

            GeneratorDriver driver = new CSharpGeneratorDriver(Regular, ImmutableArray.Create<ISourceGenerator>(sourceGenerator), ImmutableArray<AdditionalText>.Empty);
            driver.RunFullGeneration(compilation, out var outputCompilation, out var diagnostics);
            return outputCompilation;
        }
    }

    public class EmptyInterceptorGenerator : IInterceptorGenerator
    {
        public IEnumerable<string> AfterMethod(ProxyMethodGeneratorContext context)
        {
            return new string[0];
        }

        public IEnumerable<string> BeforeMethod(ProxyMethodGeneratorContext context)
        {
            return new string[0];
        }

        public class InterfaceProxyGeneratorTest
        {
            private static Compilation GenerateSource(string source)
            {
                return ProxyGeneratorTest.GenerateSource(source, new EmptyProxyGenerator());
            }

            [Fact]
            public void GenerateProxyClassWhenInterfaceAndVoidMethod()
            {
                var source = @"
namespace Norns.ProxyGenerators.Test
{
    public interface IC
    {
        void AddOne();
    }
}
";
                Compilation outputCompilation = GenerateSource(source);
                var array = outputCompilation.SyntaxTrees.ToArray();
                Assert.Equal(2, array.Length);
                var str = array[1].ToString();
                Assert.Contains("ProxyIC", str);
                Assert.Contains(": Norns.ProxyGenerators.Test.IC", str);
                Assert.Contains("public  void AddOne() {", str);
                Assert.Contains(".AddOne();", str);
            }

            [Fact]
            public void GenerateProxyClassWhenInterfaceAndReturnIntMethod()
            {
                var source = @"
namespace Norns.ProxyGenerators.Test
{
    public interface IC
    {
        int AddOne(int v);
    }
}
";
                Compilation outputCompilation = GenerateSource(source);
                var array = outputCompilation.SyntaxTrees.ToArray();
                Assert.Equal(2, array.Length);
                var str = array[1].ToString();
                Assert.Contains("ProxyIC", str);
                Assert.Contains(": Norns.ProxyGenerators.Test.IC", str);
                Assert.Contains("public  int AddOne(int v)", str);
                Assert.Contains("= default(int)", str);
                Assert.Contains("return r", str);
            }

            [Fact]
            public void GenerateProxyClassWhenInterfaceHasDefaultMethodAndReturnIntMethod()
            {
                var source = @"
namespace Norns.ProxyGenerators.Test
{
    public interface IC
    {
        public int AddOne(int v)
        {
            return v + 1;
        }
    }
}
";
                Compilation outputCompilation = GenerateSource(source);
                var array = outputCompilation.SyntaxTrees.ToArray();
                Assert.Equal(2, array.Length);
                var str = array[1].ToString();
                Assert.Contains("ProxyIC", str);
                Assert.Contains(": Norns.ProxyGenerators.Test.IC", str);
                Assert.Contains("public  int AddOne(int v)", str);
                Assert.Contains("= default(int)", str);
                Assert.Contains("return r", str);
                Assert.Contains(".AddOne(v);", str);
            }

            [Fact]
            public void GenerateProxyClassWhenInterfaceAndReturnValueTupleMethod()
            {
                var source = @"
namespace Norns.ProxyGenerators.Test
{
    public interface IC
    {
        (int,int) AddOne(int v);
    }
}
";
                Compilation outputCompilation = GenerateSource(source);
                var array = outputCompilation.SyntaxTrees.ToArray();
                Assert.Equal(2, array.Length);
                var str = array[1].ToString();
                Assert.Contains("ProxyIC", str);
                Assert.Contains(": Norns.ProxyGenerators.Test.IC", str);
                Assert.Contains("public  (int, int) AddOne(int v)", str);
                Assert.Contains("= default((int, int))", str);
                Assert.Contains("return r", str);
            }

            [Fact]
            public void GenerateProxyClassWhenInterfaceAndReturnValueListIntMethod()
            {
                var source = @"
using System.Collections.Generic;
namespace Norns.ProxyGenerators.Test
{
    public interface IC
    {
        List<int> AddOne(int v);
    }
}
";
                Compilation outputCompilation = GenerateSource(source);
                var array = outputCompilation.SyntaxTrees.ToArray();
                Assert.Equal(2, array.Length);
                var str = array[1].ToString();
                Assert.Contains("ProxyIC", str);
                Assert.Contains(": Norns.ProxyGenerators.Test.IC", str);
                Assert.Contains("public  System.Collections.Generic.List<int> AddOne(int v)", str);
                Assert.Contains("= default(System.Collections.Generic.List<int>)", str);
                Assert.Contains("return r", str);
            }

            [Fact]
            public void GenerateProxyClass()
            {
                var source = @"
namespace Norns.ProxyGenerators.Test
{
    public interface IC
    {
        int AddOne(int v);
    }

    public class C : IC
    {
        public int AddOne(int v)
{
    return v + 1;
}
    }
}
";
                Compilation outputCompilation = GenerateSource(source);
                var array = outputCompilation.SyntaxTrees.ToArray();
                Assert.Equal(2, array.Length);
                var str = array[1].ToString();
                Assert.Contains("ProxyIC", str);
                Assert.Contains(": Norns.ProxyGenerators.Test.IC", str);
                Assert.Contains("public  int AddOne(int v)", str);
                Assert.Contains("[Norns.Fate.Abstraction.Proxy(typeof(Norns.ProxyGenerators.Test.IC))]", str);
            }

            [Fact]
            public void GenerateProxyClassWhenInterfaceWithHaAsyncMethod()
            {
                var source = @"
using System.Threading.Tasks;
namespace Norns.ProxyGenerators.Test
{
    public interface IC
    {
        async Task AddOne(int v) {}
    }
}
";
                Compilation outputCompilation = GenerateSource(source);
                var array = outputCompilation.SyntaxTrees.ToArray();
                Assert.Equal(2, array.Length);
                var str = array[1].ToString();
                Assert.Contains("ProxyIC", str);
                Assert.Contains(": Norns.ProxyGenerators.Test.IC", str);
                Assert.Contains("public async  System.Threading.Tasks.Task AddOne(int v)", str);
                Assert.Contains("await proxy", str);
                Assert.Contains(".AddOne(v);", str);
            }
        }
    }
}