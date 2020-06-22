﻿using Norns.Destiny.Abstraction.Structure;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Norns.Destiny.JIT.Structure
{
    public class PropertySymbolInfo : IPropertySymbolInfo
    {
        public PropertySymbolInfo(PropertyInfo p)
        {
            RealProperty = p;
            Type = new TypeSymbolInfo(p.PropertyType);
            Parameters = RealProperty.GetIndexParameters().Select(i => new ParameterSymbolInfo(i)).ToImmutableArray<IParameterSymbolInfo>();
            GetMethod = CanRead ? new MethodSymbolInfo(p.GetMethod) : null;
            SetMethod = CanWrite ? new MethodSymbolInfo(p.SetMethod) : null;
            var getAccessibility = (CanRead ? GetMethod.Accessibility : AccessibilityInfo.NotApplicable);
            var setAccessibility = (CanWrite ? SetMethod.Accessibility : AccessibilityInfo.NotApplicable);
            Accessibility = getAccessibility > setAccessibility ? getAccessibility : setAccessibility;
        }

        public PropertyInfo RealProperty { get; }
        public bool IsIndexer => !Parameters.IsEmpty;
        public bool CanWrite => RealProperty.CanWrite;
        public bool CanRead => RealProperty.CanRead;
        public ITypeSymbolInfo Type { get; }
        public ImmutableArray<IParameterSymbolInfo> Parameters { get; }
        public AccessibilityInfo Accessibility { get; }
        public bool IsStatic => CanRead ? GetMethod.IsStatic : SetMethod.IsStatic;
        public bool IsExtern => CanRead ? GetMethod.IsExtensionMethod : SetMethod.IsExtensionMethod;
        public bool IsSealed => CanRead ? GetMethod.IsSealed : SetMethod.IsSealed;
        public bool IsAbstract => CanRead ? GetMethod.IsAbstract : SetMethod.IsAbstract;
        public bool IsOverride => CanRead ? GetMethod.IsOverride : SetMethod.IsOverride;
        public bool IsVirtual => CanRead ? GetMethod.IsVirtual : SetMethod.IsVirtual;
        public object Origin => RealProperty;
        public string Name => RealProperty.Name;
        public IMethodSymbolInfo GetMethod { get; }
        public IMethodSymbolInfo SetMethod { get; }
        public string FullName => $"{RealProperty.DeclaringType.FullName}.{Name}";
    }
}