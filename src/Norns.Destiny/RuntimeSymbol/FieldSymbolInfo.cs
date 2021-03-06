﻿using Norns.Destiny.Immutable;
using Norns.Destiny.Structure;
using System.Linq;
using System.Reflection;

namespace Norns.Destiny.RuntimeSymbol
{
    public class FieldSymbolInfo : IFieldSymbolInfo
    {
        public FieldSymbolInfo(FieldInfo f)
        {
            ContainingType = f.DeclaringType.GetSymbolInfo();
            RealField = f;
            FieldType = f.FieldType.GetSymbolInfo();
            Accessibility = f.ConvertAccessibilityInfo();
            Attributes = EnumerableExtensions.CreateLazyImmutableArray(() => RealField.GetCustomAttributesData().Select(i => new AttributeSymbolInfo(i)));
        }

        public FieldInfo RealField { get; }
        public object Origin => RealField;
        public string Name => RealField.Name;
        public ITypeSymbolInfo FieldType { get; }
        public bool IsConst => RealField.IsLiteral;
        public bool IsReadOnly => RealField.IsInitOnly;
        public bool IsVolatile => RealField.GetRequiredCustomModifiers().Any(i => i == typeof(System.Runtime.CompilerServices.IsVolatile));
        public bool IsFixedSizeBuffer => RealField.IsDefined(typeof(System.Runtime.CompilerServices.FixedBufferAttribute));
        public bool HasConstantValue => (RealField.Attributes & FieldAttributes.HasDefault) == FieldAttributes.HasDefault;
        public object ConstantValue => RealField.GetRawConstantValue();
        public bool IsStatic => RealField.IsStatic;
        public AccessibilityInfo Accessibility { get; }
        public string FullName => $"{RealField.DeclaringType.FullName}.{RealField.Name}";
        public IImmutableArray<IAttributeSymbolInfo> Attributes { get; }
        public ITypeSymbolInfo ContainingType { get; }
    }
}