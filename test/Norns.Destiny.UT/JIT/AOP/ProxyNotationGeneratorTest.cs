﻿using Norns.Destiny.JIT.AOP;
using System.Threading.Tasks;
using Xunit;

namespace Norns.Destiny.UT.JIT.AOP
{
    public class ProxyNotationGeneratorTest
    {
        [Fact]
        public void WhenInheritInterface()
        {
            var instance = JitTest.GenerateProxy<IJitD>();
            Assert.Equal(1, instance.GiveFive());
        }

        [Fact]
        public async Task WhenSimpleInterfaceAndSomeMethods()
        {
            var instance = JitTest.GenerateProxy<IJitC>();
            Assert.Equal(5, instance.AddOne(33));
            instance.AddVoid();
            await instance.AddTask(66);
            Assert.Equal(0, await instance.AddVTask(44));
            Assert.Equal(0, await instance.AddValueTask(11));
            Assert.Null(await instance.AddValueTask(this));
            Assert.Null(await instance.AddValueTask(new A(), instance));
            Assert.Equal(-5, instance.PA);
            instance.PD = 55;
            Assert.Null(instance[3, ""]);
            var c = instance;
            Assert.Null(instance.AddValue1(new A(), ref c));
            Assert.Null(instance.AddValue2(new A(), in c));
            Assert.Null(instance.AddValue3(new A(), out c));
            Assert.Equal(8, instance.A());
        }

        [Fact]
        public void WhenOutGenericInterfaceSyncMethod()
        {
            var instance = JitTest.GenerateProxy<IJitD<DataBase>>();
            Assert.Null(instance.A());
            instance = JitTest.GenerateProxy<IJitD<DataBase>>(typeof(IJitD<>));
            Assert.Null(instance.A());
        }

        [Fact]
        public void WhenOutGenericInterfaceInClassBSyncMethod()
        {
            var instance = JitTest.GenerateProxy<B.IJitDB<DataBase>>();
            Assert.Null(instance.A());

            var instance2 = JitTest.GenerateProxy<B.A.IJitDA<Data>>();
            Assert.Null(instance2.A());
        }

        [Fact]
        public void WhenInGenericInterfaceSyncMethod()
        {
            var instance = JitTest.GenerateProxy<IJitDIn<JitAopSourceGenerator, int, long?>>();
            Assert.Null(instance.A());
        }

        #region Abstract Class

        [Fact]
        public async Task WhenAbstractClassAndSomeMethods()
        {
            var instance = JitTest.GenerateProxy<JitCClass>();
            Assert.Equal(5, instance.AddOne(33));
            instance.AddVoid();
            await instance.AddTask(66);
            Assert.Equal(0, await instance.AddVTask(44));
            Assert.Equal(0, await instance.AddValueTask(11));
            Assert.Null(await instance.AddValueTask(this));
            Assert.Null(await instance.AddValueTask(new A(), instance));
            Assert.Equal(-5, instance.PA);
            instance.PD = 55;
            Assert.Null(instance[3, ""]);
            var c = instance;
            Assert.Null(instance.AddValue1(new A(), ref c));
            Assert.Null(instance.AddValue2(new A(), in c));
            Assert.Null(instance.AddValue3(new A(), out c));
            Assert.Equal(3, instance.A());
            Assert.Equal(8, instance.B());
        }

        [Fact]
        public void WhenAbstractClassSyncMethod()
        {
            var instance = JitTest.GenerateProxy<JitCClass<Data, int, long>>();
            var r = instance.A();
            Assert.Null(r.Item1);
            Assert.Equal(0, r.Item2);
            Assert.Equal(0, r.Item3);
        }

        [Fact]
        public void WhenNestedAbstractClassSyncMethod()
        {
            var instance = JitTest.GenerateProxy < B.JitCClassB<Data, long, short>>();
            var r = instance.A();
            Assert.Null(r.Item1);
            Assert.Equal(0L, r.Item2);
            Assert.Equal(0, r.Item3);

            var instance1 = JitTest.GenerateProxy < B.A.JitCClassA<DataBase, long, int>>();
            var r1 = instance1.A();
            Assert.Null(r1.Item1);
            Assert.Equal(0L, r1.Item2);
            Assert.Equal(0, r1.Item3);
        }

        #endregion Abstract Class
    }
}