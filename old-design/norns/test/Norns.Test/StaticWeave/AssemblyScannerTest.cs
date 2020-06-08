﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using Norns.AOP.Interceptors;
using Norns.StaticWeave;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Norns.DependencyInjection;

namespace Norns.Test.StaticWeave
{
    public class AssemblyScannerTest
    {
        [Fact]
        public void FindNeedProxyClass()
        {
            var dllPath = Path.Combine(Directory.GetCurrentDirectory(), "debugdll", "TestFuncToDll.dll");
            var assembly = AssemblyDefinition.ReadAssembly(dllPath, new ReaderParameters() { ReadSymbols = false });
            //assembly.MainModule.ImportReference(typeof(System.Reflection.MethodInfo));
            var types = assembly.FindNeedInterceptTypes().ToArray();
            var typedReference = assembly.MainModule.ImportReference(typeof(Type));
            var contextReference = assembly.MainModule.ImportReference(typeof(InterceptContext));
            var additionsReference = assembly.MainModule.ImportReference(typeof(Additions));
            var getTypeFromHandle = assembly.MainModule.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"));
            var getMethodInfo = assembly.MainModule.ImportReference(typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string), typeof(Type[]) }));
            var builderReference = assembly.MainModule.ImportReference(typeof(IInterceptDelegateBuilder));
            var delegateReference = assembly.MainModule.ImportReference(typeof(InterceptDelegate));
            var methodInfoReference = assembly.MainModule.ImportReference(typeof(System.Reflection.MethodInfo));
            foreach (var t in types)
            {
                var builderF = new FieldDefinition($"interceptDelegateBuilder_{Guid.NewGuid()}", FieldAttributes.Private, builderReference);
                var builderP = new PropertyDefinition($"InterceptDelegateBuilder_{Guid.NewGuid()}", PropertyAttributes.None, builderReference);
                builderP.CustomAttributes.Add(new CustomAttribute(t.Module.ImportReference(typeof(FromDIAttribute).GetConstructors().First())));
                builderP.GetMethod = new MethodDefinition($"{builderF.Name}_get", MethodAttributes.Public, builderReference);
                var il = builderP.GetMethod.Body.GetILProcessor();
                il.Append(Instruction.Create(OpCodes.Ldarg_0));
                il.Append(Instruction.Create(OpCodes.Ldfld, builderF));
                il.Append(Instruction.Create(OpCodes.Ret));
                builderP.SetMethod = new MethodDefinition($"{builderF.Name}_set", MethodAttributes.Public, assembly.MainModule.TypeSystem.Void);
                il = builderP.SetMethod.Body.GetILProcessor();
                builderP.SetMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, builderReference));
                il.Append(Instruction.Create(OpCodes.Ldarg_0));
                il.Append(Instruction.Create(OpCodes.Ldarg_1));
                il.Append(Instruction.Create(OpCodes.Stfld, builderF));
                t.Fields.Add(builderF);
                t.Properties.Add(builderP);

                VariableDefinition typevariableDef = null;
                var staticCtor = t.FindStaticCtorMethod();
                if (staticCtor == null)
                {
                    staticCtor = new MethodDefinition(TypeReferenceExtensions.StaticCtorName, TypeReferenceExtensions.StaticCtorAttributes,
                        assembly.MainModule.TypeSystem.Void);
                    staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Ret));
                    t.Methods.Add(staticCtor);
                }
                var methods = t.FindNeedInterceptMethods().ToArray();

                foreach (var method in methods)
                {
                    var newMethodName = $"{method.Name}_{Guid.NewGuid()}";
                    var newfieldName = $"f_{newMethodName}";
                    var field = new FieldDefinition(newfieldName, FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly, methodInfoReference);
                    if (typevariableDef == null)
                    {
                        staticCtor.Body.InitLocals = true;
                        typevariableDef = new VariableDefinition(typedReference);
                        staticCtor.Body.Variables.Add(typevariableDef);
                        staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Ldtoken, t));
                        staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Call, getTypeFromHandle));
                        staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Stloc_S, typevariableDef));
                    }
                    staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Ldloc_S, typevariableDef));
                    staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Ldstr, method.Name));
                    staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)method.Parameters.Count));
                    staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Newarr, typedReference));
                    staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Call, getMethodInfo));
                    staticCtor.InsertBeforeLast(Instruction.Create(OpCodes.Stsfld, field));
                    t.Fields.Add(field);

                    var newMethod = new MethodDefinition(newMethodName, method.Attributes, method.ReturnType);
                    newMethod.Attributes = method.Attributes;
                    newMethod.IsReuseSlot = method.IsReuseSlot;
                    newMethod.IsNewSlot = false;
                    newMethod.IsCheckAccessOnOverride = method.IsCheckAccessOnOverride;
                    newMethod.IsAbstract = method.IsAbstract;
                    newMethod.IsSpecialName = method.IsSpecialName;
                    newMethod.IsUnmanagedExport = method.IsUnmanagedExport;
                    newMethod.IsRuntimeSpecialName = method.IsRuntimeSpecialName;
                    newMethod.HasSecurity = method.HasSecurity;
                    newMethod.IsIL = method.IsIL;
                    newMethod.IsNative = method.IsNative;
                    newMethod.IsRuntime = method.IsRuntime;
                    newMethod.IsUnmanaged = method.IsUnmanaged;
                    newMethod.IsHideBySig = method.IsHideBySig;
                    newMethod.IsManaged = method.IsManaged;
                    newMethod.IsPreserveSig = method.IsPreserveSig;
                    newMethod.IsInternalCall = method.IsInternalCall;
                    newMethod.IsSynchronized = method.IsSynchronized;
                    newMethod.NoInlining = method.NoInlining;
                    newMethod.NoOptimization = method.NoOptimization;
                    newMethod.AggressiveInlining = method.AggressiveInlining;
                    newMethod.IsSetter = method.IsSetter;
                    newMethod.IsGetter = method.IsGetter;
                    newMethod.IsOther = method.IsOther;
                    newMethod.IsAddOn = method.IsAddOn;
                    newMethod.IsRemoveOn = method.IsRemoveOn;
                    newMethod.IsFire = method.IsFire;
                    newMethod.DeclaringType = method.DeclaringType;
                    newMethod.IsForwardRef = method.IsForwardRef;
                    newMethod.IsVirtual = false;
                    newMethod.IsStatic = method.IsStatic;
                    newMethod.ImplAttributes = method.ImplAttributes;
                    newMethod.SemanticsAttributes = method.SemanticsAttributes;
                    newMethod.Body = method.Body;
                    newMethod.IsFinal = method.IsFinal;
                    newMethod.IsCompilerControlled = method.IsCompilerControlled;
                    newMethod.IsPrivate = method.IsPrivate;
                    newMethod.IsFamilyAndAssembly = method.IsFamilyAndAssembly;
                    newMethod.IsAssembly = method.IsAssembly;
                    newMethod.IsFamily = method.IsFamily;
                    newMethod.IsFamilyOrAssembly = method.IsFamilyOrAssembly;
                    newMethod.IsPublic = method.IsPublic;
                    newMethod.MethodReturnType = method.MethodReturnType;
                    newMethod.ReturnType = method.ReturnType;
                    newMethod.CallingConvention = method.CallingConvention;
                    newMethod.ExplicitThis = method.ExplicitThis;
                    newMethod.HasThis = method.HasThis;
                    newMethod.MetadataToken = method.MetadataToken;
                    t.Methods.Add(newMethod);

                    var ct = contextReference.Resolve();
                    var callMethod = new MethodDefinition($"Call_{newMethodName}", MethodAttributes.Private, assembly.MainModule.TypeSystem.Void);
                    var contextParameter = new ParameterDefinition("context", ParameterAttributes.None, contextReference);
                    callMethod.Parameters.Add(contextParameter);
                    t.Methods.Add(callMethod);
                    var ilp = callMethod.Body.GetILProcessor();
                    if (method.ReturnType != assembly.MainModule.TypeSystem.Void)
                    {
                        ilp.Append(Instruction.Create(OpCodes.Ldarg_1));
                    }
                    ilp.Append(Instruction.Create(OpCodes.Ldarg_0));
                    foreach (var parameter in method.Parameters)
                    {
                        ilp.Append(Instruction.Create(OpCodes.Ldarg_1));
                        ilp.Append(Instruction.Create(OpCodes.Ldfld, assembly.MainModule.ImportReference(ct.Fields.First(j => j.Name == "Parameters"))));
                        ilp.Append(Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)parameter.Index));
                        ilp.Append(Instruction.Create(OpCodes.Ldelem_Ref, parameter.Index));
                        if (parameter.ParameterType.IsValueType)
                        {
                            ilp.Append(Instruction.Create(OpCodes.Unbox_Any, parameter.ParameterType));
                        }
                    }
                    ilp.Append(Instruction.Create(OpCodes.Call, newMethod));
                    if (method.ReturnType != assembly.MainModule.TypeSystem.Void)
                    {
                        if (method.ReturnType.IsValueType)
                        {
                            ilp.Append(Instruction.Create(OpCodes.Box, method.ReturnType));
                        }
                        ilp.Append(Instruction.Create(OpCodes.Stsfld, ct.Fields.First(j => j.Name == "Result")));
                    }
                    ilp.Append(Instruction.Create(OpCodes.Ret));
                    var field1 = new FieldDefinition($"f_{callMethod.Name}", FieldAttributes.Private, delegateReference);
                    t.Fields.Add(field1);

                    method.Body = new MethodBody(method);
                    var context = new VariableDefinition(contextReference);
                    var parameters = new VariableDefinition(assembly.MainModule.ImportReference(typeof(object[])));
                    method.Body.Variables.Add(context);
                    //method.Body.Variables.Add(parameters);
                    ilp = method.Body.GetILProcessor();
                    ilp.Append(Instruction.Create(OpCodes.Ldloca_S, context));
                    ilp.Append(Instruction.Create(OpCodes.Initobj, contextReference));
                    ilp.Append(Instruction.Create(OpCodes.Ldloca_S, context));
                    ilp.Append(Instruction.Create(OpCodes.Ldsfld, assembly.MainModule.ImportReference(field)));
                    ilp.Append(Instruction.Create(OpCodes.Stfld, assembly.MainModule.ImportReference(ct.Fields.First(i => i.Name == "ServiceMethod"))));
                    ilp.Append(Instruction.Create(OpCodes.Ldloca_S, context));
                    var a = typeof(Additions).GetConstructors().First();
                    ilp.Append(Instruction.Create(OpCodes.Newobj, assembly.MainModule.ImportReference(a)));
                    ilp.Append(Instruction.Create(OpCodes.Stfld, assembly.MainModule.ImportReference(ct.Fields.First(i => i.Name == "Additions"))));
                    ilp.Append(Instruction.Create(OpCodes.Ldloca_S, context));
                    ilp.Append(Instruction.Create(OpCodes.Ldc_I4_0));
                    //ilp.Append(Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)method.Parameters.Count));
                    ilp.Append(Instruction.Create(OpCodes.Newarr, assembly.MainModule.TypeSystem.Object));
                    //ilp.Append(Instruction.Create(OpCodes.Stloc_S, parameters));
                    //ilp.Append(Instruction.Create(OpCodes.Ldloc_S, parameters));
                    ilp.Append(Instruction.Create(OpCodes.Stfld, assembly.MainModule.ImportReference(ct.Fields.First(i => i.Name == "Parameters"))));
                    foreach (var parameter in method.Parameters)
                    {
                        ilp.Append(Instruction.Create(OpCodes.Ldloc_S, parameters));
                        ilp.Append(Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)parameter.Index));
                        ilp.Append(Instruction.Create(OpCodes.Ldarga_S, parameter.Index));
                        if (parameter.ParameterType.IsValueType)
                        {
                            ilp.Append(Instruction.Create(OpCodes.Box, parameter.ParameterType));
                        }
                        ilp.Append(Instruction.Create(OpCodes.Stelem_Ref));
                    }

                    ilp.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilp.Append(Instruction.Create(OpCodes.Ldfld, assembly.MainModule.ImportReference(field1)));
                    ilp.Append(Instruction.Create(OpCodes.Ldloc_0));
                    ilp.Append(Instruction.Create(OpCodes.Callvirt, assembly.MainModule.ImportReference(delegateReference.Resolve().Methods.First(j => j.Name == "Invoke"))));
                    ilp.Append(Instruction.Create(OpCodes.Ret));


                    il.Append(Instruction.Create(OpCodes.Ldarg_0));
                    il.Append(Instruction.Create(OpCodes.Ldarg_1));
                    il.Append(Instruction.Create(OpCodes.Ldsfld, assembly.MainModule.ImportReference(field)));
                    il.Append(Instruction.Create(OpCodes.Ldarg_0));
                    il.Append(Instruction.Create(OpCodes.Ldftn, assembly.MainModule.ImportReference(callMethod)));
                    il.Append(Instruction.Create(OpCodes.Newobj, assembly.MainModule.ImportReference(delegateReference.Resolve().Methods.First(j => j.Name == TypeReferenceExtensions.CtorName))));
                    il.Append(Instruction.Create(OpCodes.Callvirt, assembly.MainModule.ImportReference(builderReference.Resolve().Methods.First(j => j.Name == "BuildInterceptDelegate"))));
                    il.Append(Instruction.Create(OpCodes.Stfld, assembly.MainModule.ImportReference(field1)));
                }

                il.Append(Instruction.Create(OpCodes.Ret));
                t.Methods.Add(builderP.GetMethod);
                t.Methods.Add(builderP.SetMethod);
                t.IsBeforeFieldInit = false;
            }
            assembly.Write(Path.Combine(Directory.GetCurrentDirectory(), "debugdll", "TestFuncToDll2.dll"));
        }
    }
}