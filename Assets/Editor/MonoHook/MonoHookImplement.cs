using System;
using System.Linq;
using System.Reflection;
using GameBase.Log;
using GameBase.Utils;
//using GameEditor.UnityInternalFriend;
//using GameSerialize.Save;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace GameEditor.MonoHook
{
    public static class MonoHookImplement
    {
        //对于Native函数，需要标记isNative为True，调用原函数方式也有区别
        // [MonoHookMethod("UnityEditor.CoreModule", "UnityEditor.VersionControl.Provider", "Internal_Checkout",
        //     BindingFlags.NonPublic | BindingFlags.Static, true)]
        // public static void Internal_CheckoutHook(Asset[] assets,
        //     CheckoutMode mode,
        //     ChangeSet changeset)
        // {
        //     foreach (var asset in assets)
        //     {
        //         Debug.LogError($"Internal_CheckoutHook asset:{asset}");
        //     }
        //     MonoHookManager.methodName2NativeDetour["Internal_CheckoutHook"].GenerateTrampoline<Action<Asset[],CheckoutMode,ChangeSet>>()(assets,mode,changeset);
        // }
        
        [MonoHookMethod("UnityEditor.CoreModule", "UnityEditor.AnnotationWindow", "GetTopSectionHeight")]
        public static float GetTopSectionHeightHook(Func<object, float> ori, object obj)
        {
            var height = ori(obj);
            return height + 36;
        }
        
        //[MonoHookMethod("UnityEditor.CoreModule", "UnityEditor.AnnotationWindow", "DrawTopSection")]
        //public static void DrawTopSectionHook(Action<object, float> ori, object obj, float topSectionHeight)
        //{
        //    ori(obj, topSectionHeight);
        //    var toggleRect = new Rect(11, 7 + 18 * 4, 200, 18);
        //    GUI.Label(toggleRect, "Gizmo组选择");
        //    toggleRect.y += 18;
        //    var group = (GizmoGroup)EditorGUI.EnumPopup(toggleRect, GizmosGroupSetting.SelectGroup);
        //    if (GizmosGroupSetting.SelectGroup != group)
        //    {
        //        GizmosGroupSetting.SelectGroup = group;
        //        GizmosGroupSetting.OnGizmoGroupChange();
        //        ReflectionUtils.SetField(obj, "m_SyncWithState", true, BindingFlags.Instance | BindingFlags.NonPublic);
        //        SceneView.RepaintAll();
        //    }
        //}

        // [MonoHookMethod("UnityEngine.CoreModule", "UnityEngine.RectTransform", "SendReapplyDrivenProperties")]
        // public static void SendReapplyDrivenPropertiesHook(Action<RectTransform> ori, RectTransform driven)
        // {
        //     LogUtils.Trace($"{driven.gameObject.GetPath()} {Time.frameCount}");
        //      ori(driven);
        // }

        // [MonoHookMethod("UnityEngine.CoreModule", "UnityEngine.Object", "Instantiate", 
        //     BindingFlags.Static | BindingFlags.Public, paramTypes:new Type[]{typeof(GameObject)})]
        // public static GameObject GOInstantiateHook(Func<GameObject, GameObject> ori, GameObject obj)
        // {
        //     Debug.LogError($"GameObject Instantiate {obj.name}");
        //     return ori(obj);
        // }

        // [MonoHookMethod("UnityEngine.CoreModule", "UnityEngine.Transform", "SetPositionAndRotation", 
        //     BindingFlags.Instance | BindingFlags.Public)]
        // public static void SetPositionAndRotationHook(Action<Transform, Vector3, Quaternion> ori, Transform obj, Vector3 position, Quaternion rotation)
        // {
        //     LogUtils.Error($"Transform SetPositionAndRotation {obj.gameObject.GetPath()} {position}");
        //     ori(obj, position, rotation);
        // }
        //
        // [MonoHookPropertyAttribute("UnityEngine.CoreModule", "UnityEngine.Transform", "localPosition", MonoHookPropertyAttribute.HookType.SetMethod, isNative:true)]
        // public static void TransformSetLocalPosition(Transform behaviour, Vector3 pos)
        // {
        //     LogUtils.Error($"TransformSetLocalPosition {behaviour.gameObject.GetPath()} {behaviour} {pos}");
        //     MonoHookManager.methodName2NativeDetour["TransformSetLocalPosition"].GenerateTrampoline<Action<Transform, Vector3>>()(behaviour, pos);
        // }
        //
        // [MonoHookPropertyAttribute("UnityEngine.CoreModule", "UnityEngine.Transform", "position", MonoHookPropertyAttribute.HookType.SetMethod, isNative:true)]
        // public static void TransformSetPosition(Transform behaviour, Vector3 pos)
        // {
        //     LogUtils.Error($"TransformSetPosition {behaviour.gameObject.GetPath()} {behaviour} {pos}");
        //     MonoHookManager.methodName2NativeDetour["TransformSetPosition"].GenerateTrampoline<Action<Transform, Vector3>>()(behaviour, pos);
        // }

        // [MonoHookPropertyAttribute("UnityEngine.CoreModule", "UnityEngine.Behaviour", "enabled", MonoHookPropertyAttribute.HookType.SetMethod, isNative:true)]
        // public static void BehaviourEnabledSetHook(Behaviour behaviour, bool value)
        // {
        //     // Debug.LogError($"BehaviourEnabledSetHook {value} {behaviour}");
        //     if (behaviour.gameObject)
        //     {
        //         Debug.LogError($"Set Behaviour Enable {behaviour.gameObject.GetPath()} {behaviour} {value}");
        //     }
        //     MonoHookManager.methodName2NativeDetour["BehaviourEnabledSetHook"].GenerateTrampoline<Action<Behaviour, bool>>()(behaviour, value);
        // }

        //private static LoggerSettingConnectionState s_loggerSettingConnectionState;
        //LogConsole改进
        [MonoHookMethod("UnityEditor.CoreModule", "UnityEditor.Networking.PlayerConnection.PlayerConnectionGUILayout", "ConnectionTargetSelectionDropdown")]
        public static void ConnectionTargetSelectionDropdownHook(Action<IConnectionState, GUIStyle, int> ori, IConnectionState state, GUIStyle style, int v)
        {
            ori(state, style, v);
            if (state != null && state.GetType().Name == "ConsoleAttachToPlayerState")
            {
                //需要下拉菜单的话用这个
                //s_loggerSettingConnectionState ??= new LoggerSettingConnectionState();
                //ori(s_loggerSettingConnectionState, EditorStyles.toolbarDropDown, v);
                // LogUtils.IsLogTraceOn = GUILayout.Toggle(LogUtils.IsLogTraceOn, "打印Trace",
                //     LoggerSettingConnectionState.ConsoleWindowMiniButton);
                if (LogUtils.IsLogTraceOn != GUILayout.Toggle(LogUtils.IsLogTraceOn, "Trace"))
                {
                    LogUtils.IsLogTraceOn = !LogUtils.IsLogTraceOn;
                }
                if (LogUtils.PrintTimeFrame != GUILayout.Toggle(LogUtils.PrintTimeFrame, "帧"))
                {
                    LogUtils.PrintTimeFrame = !LogUtils.PrintTimeFrame;
                }
                if (GUILayout.Button("存档"))
                {
                    //EditorUtility.RevealInFinder(SaveConfig.SaveDirPath + "/");
                    //GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("日志"))
                {
                    EditorUtility.RevealInFinder(RollingFileLogger.LOGFilePath);
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("编辑器日志"))
                {
                    EditorUtility.RevealInFinder(Application.consoleLogPath);
                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("频道"))
                {
                    OdinSelector<LogChannel> selector = new EnumSelector<LogChannel>();
                    selector.SetSelection(LogUtils.LogChannel);
                    selector.SelectionTree.Config.DrawSearchToolbar = false;
                    selector.SelectionConfirmed += selection =>
                    {
                        LogUtils.LogChannel = selection.FirstOrDefault();
                    };
                    var window = selector.ShowInPopup();
                    window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
                }
            }
        }

        //private static bool m_occurError;
        //private static string DragExitSavePath => DragAndDrop.paths != null ? DragAndDrop.paths.FirstOrDefault(path => path.EndsWithEx(".save")) : null;
        //[MonoHookMethod("UnityEditor.CoreModule", "UnityEditor.GameView", "OnGUI")]
        //public static void GameViewOnGUIHook(Action<object> ori, object self)
        //{
        //    SaveDragEndListener.OnGuiCheckSave();
        //    ori(self);
        //}

        // [MonoHookMethod("UnityEngine.CoreModule", "UnityEngine.Mesh", "Internal_Create", isNative = true)]
        // public static void MeshCreateHook(Mesh mono)
        // {
        //     Debug.LogError("hahahahah");
        //     MonoHookManager.methodName2NativeDetour["MeshCreateHook"].GenerateTrampoline<Action<Mesh>>()(mono);
        // }

        // [MonoHookPropertyAttribute("UnityEditor.CoreModule", "UnityEditor.PlayerSettings", "runInBackground",
        //     MonoHookPropertyAttribute.HookType.SetMethod, isNative = true)]
        // public static void RunInBackground_Set(PlayerSettings settings, bool value)
        // {
        //     Debug.LogError($"runInBackground_Set {value}");
        //     MonoHookManager.methodName2NativeDetour["RunInBackground_Set"].GenerateTrampoline<Action<PlayerSettings, bool>>()(settings, value);
        // }

        // [MonoHookMethod("UnityEngine.CoreModule", "UnityEngine.GameObject", "SetActive", isNative = true)]
        // public static void GoSetActiveHook(GameObject go, bool value)
        // {
        //     Debug.LogError($"GoSetActiveHook {go.GetPath()} {value}");
        //     MonoHookManager.methodName2NativeDetour["GoSetActiveHook"].GenerateTrampoline<Action<GameObject, bool>>()(go, value);
        // }

        // [MonoHookMethod("UnityEditor.CoreModule", "UnityEditor.AssetDatabase", "LoadAssetAtPath", 
        //     isNative = true, paramTypes = new []{typeof(string), typeof(Type)})]
        // public static Object AssetDatabaseLoadAssetAtPathHook(string assetPath, Type type)
        // {
        //     Debug.LogError($"AssetDatabaseLoadAssetAtPathHook {assetPath} {type}");
        //     return MonoHookManager.methodName2NativeDetour["AssetDatabaseLoadAssetAtPathHook"].GenerateTrampoline<Func<string, Type, Object>>()(assetPath, type);
        // }

        // [MonoHookPropertyAttribute("UnityEditor.CoreModule", "UnityEditor.Selection", "activeGameObject", MonoHookPropertyAttribute.HookType.SetMethod, isNative:true)]
        // public static void SelectionActiveGameObjectSet(GameObject go)
        // {
        //     LogUtils.Error($"SelectionActiveGameObjectSet {go.GetPath()}");
        //     MonoHookManager.methodName2NativeDetour["SelectionActiveGameObjectSet"].GenerateTrampoline<Action<GameObject>>()(go);
        // }
        //
        // [MonoHookPropertyAttribute("UnityEditor.CoreModule", "UnityEditor.Selection", "activeObject", MonoHookPropertyAttribute.HookType.SetMethod, isNative:true)]
        // public static void SelectionActiveObjectSet(Object activeObject)
        // {
        //     LogUtils.Error($"SelectionActiveObjectSet");
        //     MonoHookManager.methodName2NativeDetour["SelectionActiveObjectSet"].GenerateTrampoline<Action<Object>>()(activeObject);
        // }
        //
        // [MonoHookPropertyAttribute("UnityEditor.CoreModule", "UnityEditor.Selection", "objects", MonoHookPropertyAttribute.HookType.SetMethod, isNative:true)]
        // public static void SelectionObjectsSet(Object[] objs)
        // {
        //     LogUtils.Error($"SelectionObjectsSet");
        //     MonoHookManager.methodName2NativeDetour["SelectionObjectsSet"].GenerateTrampoline<Action<Object[]>>()(objs);
        // }

        // [MonoHookMethod("UnityEngine.CoreModule", "UnityEngine.Object", "Destroy", paramTypes:new Type[]{typeof(Object)})]
        // public static void GODestroyHook(Action<Object> ori, Object obj)
        // {
        //     Debug.LogError($"GameObject Destroy {obj.name}");
        //     ori(obj);
        // }
        //
        // [MonoHookMethod("UnityEngine.CoreModule", "UnityEngine.Object", "DestroyImmediate", paramTypes:new Type[]{typeof(Object)})]
        // public static void GODestroyImmediateHook(Action<Object> ori, Object obj)
        // {
        //     Debug.LogError($"GameObject DestroyImmediate {obj.name}");
        //     ori(obj);
        // }

        //[MonoHookProperty("UnityEngine.Physics2DModule", "UnityEngine.Rigidbody2D", "velocity", MonoHookPropertyAttribute.HookType.SetMethod, isNative: true)]
        //public static void SetVelocityHook(Rigidbody2D rb, Vector2 v)
        //{
        //    LogUtils.Error("SetVelocityHook " + v);
        //    MonoHookManager.methodName2NativeDetour["SetVelocityHook"].GenerateTrampoline<Action<Rigidbody2D, Vector2>>()(rb, v);
        //}

        // [MonoHookMethod("DOTween", "DG.Tweening.ShortcutExtensions", "DOMove")]
        // public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveHook(Func<Transform, Vector3,float, bool, TweenerCore<Vector3, Vector3, VectorOptions>> ori, 
        //     Transform target,
        //     Vector3 endValue,
        //     float duration,
        //     bool snapping = false)
        // {
        //     Debug.LogError($"DOMoveHook {target.gameObject.GetPath()}");
        //     return ori(target, endValue, duration, snapping);
        // }

        // [MonoHookMethod("UniTask", "Cysharp.Threading.Tasks.AutoResetUniTaskCompletionSource", "TryReturn")]
        // public static bool AutoResetUniTaskCompletionSourceTryReturnHook(Func<AutoResetUniTaskCompletionSource, bool> ori, AutoResetUniTaskCompletionSource obj)
        // {
        //     Debug.LogError($"ARSourceTryReturnHook TryReturn SourceId:{UniTaskUtils.ActionWithUniTaskCompletionSource.TryGetId(obj)}");
        //     return ori(obj);
        // }
        //
        // [MonoHookMethod("UniTask", "Cysharp.Threading.Tasks.AutoResetUniTaskCompletionSource", "Create")]
        // public static AutoResetUniTaskCompletionSource AutoResetUniTaskCompletionSourceCreateHook(Func<AutoResetUniTaskCompletionSource> ori)
        // {
        //     var result = ori();
        //     Debug.LogError($"ARSourceTryReturnHook Create SourceId:{UniTaskUtils.ActionWithUniTaskCompletionSource.TryGetId(result)}");
        //     return result;
        // }
    }
}