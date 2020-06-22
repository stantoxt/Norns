﻿using Norns.Destiny.Abstraction.Structure;
using Norns.Destiny.JIT.Structure;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Norns.Destiny.UT.JIT.Structure
{
    public static class Sta
    {
        public static int A(this int d) => d;
    }

    public class MethodSymbolInfoTest
    {
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
        }

        [Fact]
        public void WhenMethods()
        {
            var ms = new TypeSymbolInfo(typeof(A)).GetMembers()
                 .Select(i => i as IMethodSymbolInfo)
                 .Where(i => i != null)
                 .ToDictionary(i => i.FullName, i => i);
            var m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.PrivateM"];
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
            Assert.Equal("System.Void", m.ReturnType.FullName);

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.InternalM"];
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
            Assert.Equal("System.Int32", m.ReturnType.FullName);

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.ProtectedM"];
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
            Assert.Equal("ValueTuple`2", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.PIS"];
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

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.PPS"];
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

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.ToString"];
            Assert.Equal(AccessibilityInfo.Public, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.True(m.IsOverride);
            Assert.True(m.IsVirtual);
            Assert.Equal("String", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.GetTask"];
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

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+A.GetT"];
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

            ms = new TypeSymbolInfo(typeof(B)).GetMembers()
                 .Select(i => i as IMethodSymbolInfo)
                 .Where(i => i != null)
                 .ToDictionary(i => i.FullName, i => i);
            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+B.ProtectedM"];
            Assert.Equal(AccessibilityInfo.Protected, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.True(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.True(m.IsOverride);
            Assert.True(m.IsVirtual);
            Assert.Equal("ValueTuple`2", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+B.PIS"];
            Assert.Equal(AccessibilityInfo.ProtectedOrInternal, m.Accessibility);
            Assert.Empty(m.Parameters);
            Assert.Empty(m.TypeParameters);
            Assert.False(m.IsGenericMethod);
            Assert.False(m.IsExtensionMethod);
            Assert.False(m.IsStatic);
            Assert.False(m.IsSealed);
            Assert.False(m.IsAbstract);
            Assert.True(m.IsOverride);
            Assert.True(m.IsVirtual);
            Assert.Equal("String", m.ReturnType.Name);

            m = ms["Norns.Destiny.UT.JIT.Structure.MethodSymbolInfoTest+B.GetTask"];
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

            ms = new TypeSymbolInfo(typeof(Sta)).GetMembers()
                 .Select(i => i as IMethodSymbolInfo)
                 .Where(i => i != null)
                 .ToDictionary(i => i.FullName, i => i);
            m = ms["Norns.Destiny.UT.JIT.Structure.Sta.A"];
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
    }
}