﻿using Norns.Destiny.Structure;
using System.Linq;
using Xunit;

namespace Norns.Destiny.UT.Skuld.Structure
{
    public class MethodSymbolInfoTest
    {
        [Fact]
        public void WhenMethods()
        {
            var code = @"
    public static class Sta
    {
        public static int A(this int d) => d;
    }
public abstract class A
        {
            private void PrivateM()
            {
            }

            internal int InternalM()
            {
                return default;
            }

            protected virtual (int v, long s) ProtectedM()
            {
                return default;
            }

            protected internal abstract string PIS();

            protected private string PPS()
            {
                return default;
            }

            public T GetT<T>() where T : class
            {
                return default;
            }

            public Task GetTask()
            {
                return default;
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }

        public class B : A
        {
            protected sealed override (int v, long s) ProtectedM()
            {
                return base.ProtectedM();
            }

            protected internal override string PIS()
            {
                return default;
            }

            public new Task GetTask()
            {
                return default;
            }
        }";
            var types = SkuldTest.SimpleGenerateTypeSymbolInfos(code);
            var ms = types["A"].Members
                 .Select(i => i as IMethodSymbolInfo)
                 .Where(i => i != null)
                 .ToDictionary(i => i.FullName, i => i);
            var m = ms["Norns.Destiny.UT.AOT.Generated.A.PrivateM()"];
            Assert.Equal(AccessibilityInfo.Private, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("void", m.ReturnType.FullName);

            m = ms["Norns.Destiny.UT.AOT.Generated.A.InternalM()"];
            Assert.Equal(AccessibilityInfo.Internal, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("int", m.ReturnType.FullName);

            m = ms["Norns.Destiny.UT.AOT.Generated.A.ProtectedM()"];
            Assert.Equal(AccessibilityInfo.Protected, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.True(m.IsVirtual);
            Assert.Equal("ValueTuple", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.AOT.Generated.A.PIS()"];
            Assert.Equal(AccessibilityInfo.ProtectedOrInternal, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.True(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("String", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.AOT.Generated.A.PPS()"];
            Assert.Equal(AccessibilityInfo.ProtectedAndInternal, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("String", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.AOT.Generated.A.ToString()"];
            Assert.Equal(AccessibilityInfo.Public, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.True(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("String", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.AOT.Generated.A.GetTask()"];
            Assert.Equal(AccessibilityInfo.Public, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("Task", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.AOT.Generated.A.GetT<T>()"];
            Assert.Equal(AccessibilityInfo.Public, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.True(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("T", m.ReturnType.Name);
            Assert.Single(m.TypeParameters);
            Assert.True(m.TypeParameters.First().HasReferenceTypeConstraint);

            ms = types["B"].Members
                 .Select(i => i as IMethodSymbolInfo)
                 .Where(i => i != null)
                 .ToDictionary(i => i.FullName, i => i);
            m = ms["Norns.Destiny.UT.AOT.Generated.B.ProtectedM()"];
            Assert.Equal(AccessibilityInfo.Protected, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.True(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.True(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("ValueTuple", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.AOT.Generated.B.PIS()"];
            Assert.Equal(AccessibilityInfo.ProtectedOrInternal, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.True(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("String", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.AOT.Generated.B.GetTask()"];
            Assert.Equal(AccessibilityInfo.Public, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("Task", m.ReturnType.Name);

            ms = types["Sta"].Members
                 .Select(i => i as IMethodSymbolInfo)
                 .Where(i => i != null)
                 .ToDictionary(i => i.FullName, i => i);
            m = ms["Norns.Destiny.UT.AOT.Generated.Sta.A(int)"];
            Assert.Equal(AccessibilityInfo.Public, m.Accessibility);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.True(m.IsExtensionMethod);
            Assert.True(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.False(m.IsOverride);
            Assert.False(m.IsVirtual);
            Assert.Equal("Int32", m.ReturnType.Name);
            Assert.Single(m.Parameters);
            Assert.Equal("d", m.Parameters.First().Name);
        }

        [Fact]
        public void MethodWhenHasReturnValue()
        {
            var code = @"
using System.Threading.Tasks;
    public interface IC
    {
        int AddOne(int v);
        void AddVoid(int v);
        Task AddTask(int v);
Task<int> AddVTask(int v);
ValueTask<int> AddValueTask(int v);
    }";
            var types = SkuldTest.SimpleGenerateTypeSymbolInfos(code);
            var ms = types["IC"].Members
                 .Select(i => i as IMethodSymbolInfo)
                 .Where(i => i != null)
                 .ToDictionary(i => i.FullName, i => i);
            var m = ms["Norns.Destiny.UT.AOT.Generated.IC.AddOne(int)"];
            Assert.True(m.HasReturnValue);
            Assert.False(m.IsAsync);

            m = ms["Norns.Destiny.UT.AOT.Generated.IC.AddVoid(int)"];
            Assert.False(m.HasReturnValue);
            Assert.False(m.IsAsync);

            m = ms["Norns.Destiny.UT.AOT.Generated.IC.AddTask(int)"];
            Assert.False(m.HasReturnValue);
            Assert.True(m.IsAsync);

            m = ms["Norns.Destiny.UT.AOT.Generated.IC.AddVTask(int)"];
            Assert.True(m.HasReturnValue);
            Assert.True(m.IsAsync);

            m = ms["Norns.Destiny.UT.AOT.Generated.IC.AddValueTask(int)"];
            Assert.True(m.HasReturnValue);
            Assert.True(m.IsAsync);
        }
    }
}