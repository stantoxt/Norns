﻿using Norns.Destiny.AOP;
using Norns.Destiny.Attributes;
using Norns.Destiny.Structure;
using Norns.Destiny.Utils;
using Norns.Verthandi.AOP;
using Norns.Verthandi.Loom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NornsDependencyInjectionExtensions
    {
        public static bool TryCreateProxyDescriptor(Dictionary<Type, Type> defaultInterfaceImplementDict, Dictionary<Type, Type> proxyDict, ServiceDescriptor origin, out ServiceDescriptor proxy)
        {
            proxy = origin;
            var serviceType = proxy.ServiceType.IsGenericType ? proxy.ServiceType.GetGenericTypeDefinition() : proxy.ServiceType;
            if (proxy.ImplementationType == typeof(DefaultImplementAttribute)
                && defaultInterfaceImplementDict.TryGetValue(serviceType, out var implementType))
            {
                proxy = ServiceDescriptor.Describe(proxy.ServiceType, proxy.ServiceType.IsGenericType ? implementType.MakeGenericType(proxy.ServiceType.GetGenericArguments()) : implementType, proxy.Lifetime);
            }
            if (proxyDict.ContainsKey(serviceType))
            {
                proxy = ToImplementationServiceDescriptor(proxy, proxy.ServiceType.IsGenericType ? proxyDict[serviceType].MakeGenericType(proxy.ServiceType.GetGenericArguments()) : proxyDict[serviceType]);
            }

            return proxy != origin;
        }

        public static IServiceProvider BuildAopServiceProvider(this IServiceCollection sc, params Assembly[] assemblies)
        {
            var (defaultInterfaceImplementDict, proxyDict) = DestinyExtensions.FindProxyTypes(assemblies.Distinct().ToArray());

            foreach (var c in sc.ToArray())
            {
                if (TryCreateProxyDescriptor(defaultInterfaceImplementDict, proxyDict, c, out var proxy))
                {
                    sc.Remove(c);
                    sc.Add(proxy);
                }
            }
            return sc.BuildServiceProvider();
        }

        public static IServiceProvider BuildVerthandiAopServiceProvider(this IServiceCollection sc, IInterceptorGenerator[] interceptors, LoomOptions options = null)
        {
            var op = options ?? LoomOptions.CreateDefault();
            var userFilterProxy = op.FilterProxy ?? ((ITypeSymbolInfo i) => true);
            var userFilterForDefaultImplement = op.FilterForDefaultImplement ?? ((ITypeSymbolInfo i) => true);
            op.FilterProxy = i => i.Namespace != null && AopUtils.CanAopType(i) && userFilterProxy(i);
            op.FilterForDefaultImplement = i => i.Namespace != null && AopUtils.CanDoDefaultImplement(i) && userFilterForDefaultImplement(i);
            var generator = new AopSourceGenerator(op, interceptors ?? new IInterceptorGenerator[0]);
            var types = sc.Select(i => i.ServiceType).Select(j => j.IsGenericType ? j.GetGenericTypeDefinition() : j).Distinct().ToArray();
            var assembly = generator.Generate(new TypesSymbolSource(types));
            DestinyExtensions.CleanCache();
            GC.Collect();
            return sc.BuildAopServiceProvider(assembly);
        }

        public static IServiceCollection AddDestinyInterface<T>(this IServiceCollection sc, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return sc.AddDestinyInterface(typeof(T), lifetime);
        }

        public static IServiceCollection AddDestinyInterface(this IServiceCollection sc, Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            sc.Add(ServiceDescriptor.Describe(serviceType, typeof(DefaultImplementAttribute), lifetime));
            return sc;
        }

        public static ServiceDescriptor ToImplementationServiceDescriptor(ServiceDescriptor serviceDescriptor, Type implementationType)
        {
            switch (serviceDescriptor)
            {
                case ServiceDescriptor d when d.ImplementationType != null:
                    return ServiceDescriptor.Describe(serviceDescriptor.ServiceType, i =>
                    {
                        var p = ActivatorUtilities.CreateInstance(i, implementationType) as IInterceptProxy;
                        p?.SetProxy(ActivatorUtilities.CreateInstance(i, d.ImplementationType), i);
                        return p;
                    }, d.Lifetime);
                case ServiceDescriptor d when d.ImplementationFactory != null:
                    return ServiceDescriptor.Describe(serviceDescriptor.ServiceType, i =>
                    {
                        var p = ActivatorUtilities.CreateInstance(i, implementationType) as IInterceptProxy;
                        p?.SetProxy(d.ImplementationFactory(i), i);
                        return p;
                    }, d.Lifetime);
                default:
                    return ServiceDescriptor.Describe(serviceDescriptor.ServiceType, i =>
                    {
                        var p = ActivatorUtilities.CreateInstance(i, implementationType) as IInterceptProxy;
                        p?.SetProxy(serviceDescriptor.ImplementationInstance, i);
                        return p;
                    }, serviceDescriptor.Lifetime);
            }
        }
    }
}