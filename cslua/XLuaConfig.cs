using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// using NPOI.SS.Formula.Functions;
using XLua;

namespace CSLua
{
    public static class XLuaConfig
    {
        // /***************如果你全lua编程，可以参考这份自动化配置***************/
        // --------------begin 纯lua编程配置参考----------------------------
        static List<string> exclude = new List<string>
        {
            "HideInInspector", "ExecuteInEditMode",
            "AddComponentMenu", "ContextMenu",
            "RequireComponent", "DisallowMultipleComponent",
            "SerializeField", "AssemblyIsEditorAssembly",
            "Attribute", "Types",
            "UnitySurrogateSelector", "TrackedReference",
            "TypeInferenceRules", "FFTWindow",
            "RPC", "Network", "MasterServer",
            "BitStream", "HostData",
            "ConnectionTesterStatus", "GUI", "EventType",
            "EventModifiers", "FontStyle", "TextAlignment",
            "TextEditor", "TextEditorDblClickSnapping",
            "TextGenerator", "TextClipping", "Gizmos",
            "ADBannerView", "ADInterstitialAd",
            "Android", "Tizen", "jvalue",
            "iPhone", "iOS", "Windows", "CalendarIdentifier",
            "CalendarUnit", "CalendarUnit",
            "ClusterInput", "FullScreenMovieControlMode",
            "FullScreenMovieScalingMode", "Handheld",
            "LocalNotification", "NotificationServices",
            "RemoteNotificationType", "RemoteNotification",
            "SamsungTV", "TextureCompressionQuality",
            "TouchScreenKeyboardType", "TouchScreenKeyboard",
            "MovieTexture", "UnityEngineInternal",
            "Terrain", "Tree", "SplatPrototype",
            "DetailPrototype", "DetailRenderMode",
            "MeshSubsetCombineUtility", "AOT", "Social", "Enumerator",
            "SendMouseEvents", "Cursor", "Flash", "ActionScript",
            "OnRequestRebuild", "Ping",
            "ShaderVariantCollection", "SimpleJson.Reflection",
            "CoroutineTween", "GraphicRebuildTracker",
            "Advertisements", "UnityEditor", "WSA",
            "EventProvider", "Apple",
            "ClusterInput", "Motion",
            "UnityEngine.UI.ReflectionMethodsCache", "NativeLeakDetection",
            "NativeLeakDetectionMode", "WWWAudioExtensions", "UnityEngine.Experimental",
        };

        static bool isExcluded(Type type)
        {
            var fullName = type.FullName;
            for (int i = 0; i < exclude.Count; i++)
            {
                if (fullName.Contains(exclude[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Type> mLuaCallCSharp = null;

        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp
        {
            get
            {
                if (mLuaCallCSharp == null)
                {
                    List<string> black_namespaces = new List<string>() // 在这里添加名字空间
                    {
                        "ICSharpCode.",
                        "Microsoft.",
                        "Mono.Math.",
                        "Mono.Security.",
                        "Mono.Xml.",
                        "System.",
                        "XLua.",
                    };
                    var customAssemblys = new List<string>()
                    {
                        // "Mono.Data.Sqlite",
                        // "NPOI",
                        // "NPOI.OOXML",
                        // "NPOI.OpenXml4Net",
                        // "NPOI.OpenXmlFormats",
                    };
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Concat(customAssemblys.Select(s => Assembly.Load(s)));
                    // foreach (var i in assemblies)
                    // {
                    //     Debug.WriteLine($"Domain assemblies: {i}");
                    // }

                    var enginetypes = assemblies.Where(
                            assembly => !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder))
                        .SelectMany(s => s
                            .GetExportedTypes()
                            .Where(type => (type.Namespace != null 
                                    && !type.IsInterface 
                                    && !type.IsEnum 
                                    && !isExcluded(type) 
                                    && type.BaseType != typeof(MulticastDelegate) 
                                    && !black_namespaces.Any(i => type.FullName.StartsWith(i))
                                )
                            )
                        );
                    mLuaCallCSharp = enginetypes.Distinct()
                        .OrderBy(t => t.FullName).ToList();
                }

                return mLuaCallCSharp;
            }
        }

        //自动把LuaCallCSharp涉及到的delegate加到CSharpCallLua列表，后续可以直接用lua函数做callback
        [CSharpCallLua]
        public static List<Type> CSharpCallLua
        {
            get
            {
                var lua_call_csharp = LuaCallCSharp;
                var delegate_types = new List<Type>();
                var flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly;
                foreach (var field in (from type in lua_call_csharp select type).SelectMany(type => type.GetFields(flag)))
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        delegate_types.Add(field.FieldType);
                    }
                }

                foreach (var method in (from type in lua_call_csharp select type).SelectMany(type => type.GetMethods(flag)))
                {
                    if (typeof(Delegate).IsAssignableFrom(method.ReturnType))
                    {
                        delegate_types.Add(method.ReturnType);
                    }

                    foreach (var param in method.GetParameters())
                    {
                        var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                        if (typeof(Delegate).IsAssignableFrom(paramType))
                        {
                            delegate_types.Add(paramType);
                        }
                    }
                }

                return delegate_types.Where(t => t.BaseType == typeof(MulticastDelegate) && !hasGenericParameter(t) && !delegateHasEditorRef(t))
                    .Distinct()
                    .ToList();
            }
        }
        //--------------end 纯lua编程配置参考----------------------------


        static bool hasGenericParameter(Type type)
        {
            if (type.IsGenericTypeDefinition) return true;
            if (type.IsGenericParameter) return true;
            if (type.IsByRef || type.IsArray)
            {
                return hasGenericParameter(type.GetElementType());
            }

            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (hasGenericParameter(typeArg))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool typeHasEditorRef(Type type)
        {
            if (type.Namespace != null && (type.Namespace == "UnityEditor" || type.Namespace.StartsWith("UnityEditor.")))
            {
                return true;
            }

            if (type.IsNested)
            {
                return typeHasEditorRef(type.DeclaringType);
            }

            if (type.IsByRef || type.IsArray)
            {
                return typeHasEditorRef(type.GetElementType());
            }

            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (typeHasEditorRef(typeArg))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool delegateHasEditorRef(Type delegateType)
        {
            if (typeHasEditorRef(delegateType)) return true;
            var method = delegateType.GetMethod("Invoke");
            if (method == null)
            {
                return false;
            }

            if (typeHasEditorRef(method.ReturnType)) return true;
            return method.GetParameters()
                .Any(pinfo => typeHasEditorRef(pinfo.ParameterType));
        }
    }
}