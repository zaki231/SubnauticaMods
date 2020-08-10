﻿using System;
using System.Linq;
using System.Reflection;

using Harmony;

namespace Common.Harmony
{
	using Reflection;

	// is class have methods that can be used as harmony patches (for more: void patch(Type typeWithPatchMethods))
	[AttributeUsage(AttributeTargets.Class)]
	public class PatchClassAttribute: Attribute {}

	static partial class HarmonyHelper
	{
		[Flags]
		public enum PatchOptions
		{
			None = 0,
			PatchOnce = 1, // need to check before patching if already patched by the same method
		}

		// for use with patch classes
		// attribute can use assembly-qualified name for target type
		// it can be used on methods with HarmonyPriority attribute
		// patch class can check for patched status of target method if 'patchOnce' is true
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
		public class PatchAttribute: Attribute
		{
			public Type type { get; private set; }
			public readonly string typeName; // assembly-qualified name
			public readonly string methodName;
			readonly Type[] methodParams;

			public PatchOptions options { get; private set; }

			PatchAttribute(Type type, string typeName, string methodName, Type[] methodParams, PatchOptions options = PatchOptions.None)
			{
				this.type = type;
				this.typeName = typeName;
				this.methodName = methodName;
				this.methodParams = methodParams;
				this.options = options;
			}

			public PatchAttribute(string typeName, string methodName):
				this(null, typeName, methodName, null) {}

			public PatchAttribute(string typeName, string methodName, params Type[] methodParams):
				this(null, typeName, methodName, methodParams) {}

			public PatchAttribute(Type type, string methodName):
				this(type, null, methodName, null) {}

			public PatchAttribute(PatchOptions options):
				this(null, null, null, null, options) {}

			// just merge options to the main attribute (with type and method)
			public static PatchAttribute merge(PatchAttribute[] attrs)
			{
				if (attrs.isNullOrEmpty()) return null;
				if (attrs.Length == 1) return attrs[0];

				var attrMain = attrs.FirstOrDefault(attr => attr.methodName != null);

				if (attrMain != null)
					attrs.forEach(attr => attrMain.options |= attr.options);

				return attrMain;
			}

			public MethodInfo targetMethod => (type ??= Type.GetType(typeName))?.method(methodName, methodParams);
		}

		// use methods from 'typeWithPatchMethods' class as harmony patches
		// valid method need to have HarmonyPatch and Harmony[Prefix/Postfix/Transpiler] attributes
		// if typeWithPatchMethods is null, we use type from which this method is called
		// patchStatus - null: patch without checking patched status (with call of 'prepare' method), true: patch, if not already patched, false: unpatch, if already patched
		public static void patch(Type typeWithPatchMethods = null, bool? patchStatus = null)
		{
			typeWithPatchMethods ??= ReflectionHelper.getCallingType();

			if (patchStatus == null && typeWithPatchMethods.method("prepare")?.wrap().invoke<bool>() == false)
				return; // if patchStatus != null, 'prepare' probably called in OptionalPatches already, but we also can call this method directly

			bool? patched = null; // we will check only first method with patch attribute, they should have same patched status anyway

			foreach (var method in typeWithPatchMethods.methods(ReflectionHelper.bfAll | BindingFlags.DeclaredOnly))
			{
				if (!(_getPatchTarget(method) is MethodBase targetMethod))
					continue;

				bool _isPatched() => patched ??= isPatchedBy(targetMethod, method);

				if (patchStatus == null || (patchStatus == true && !_isPatched()))
				{
					MethodInfo _method_if<H>() where H: Attribute => method.checkAttr<H>()? method: null;
					patch(targetMethod, _method_if<HarmonyPrefix>(), _method_if<HarmonyPostfix>(), _method_if<HarmonyTranspiler>());
				}
				else if (patchStatus == false && _isPatched())
				{
					unpatch(targetMethod, method);
				}
			}

			static MethodBase _getPatchTarget(MethodInfo method)
			{
				if (method.getAttr<HarmonyPatch>() is HarmonyPatch harmonyPatch)
					return harmonyPatch.info.getTargetMethod();

				if (PatchAttribute.merge(method.getAttrs<PatchAttribute>()) is PatchAttribute patchAttr)
				{
					var targetMethod = patchAttr.targetMethod;

					bool patchOnce = patchAttr.options.HasFlag(PatchOptions.PatchOnce);
					return patchOnce && isPatchedBy(targetMethod, method, true)? null: targetMethod;
				}

				return null;
			}
		}
	}
}