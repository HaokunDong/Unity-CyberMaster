using System.Collections.Generic;
using System.IO;
using AssetLoad;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using GameBase.Log;
using Tools;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
    public enum ResType
    {
        Null = 0,
        Prefab,
        RolePrefab,
        EffectPrefab,
        Camera,
        Terrain,
        UI,
        UIPicture,
        Texture,
        ScriptObject,
        Dialogue,
        AIGraph,
        QuestChain,
        Shader,
        ShaderVariant,
        /// <summary>
        /// 文件夹作为ab，加载完成，需要再调用同步接口来获取资源
        /// </summary>
        Folder,
        CutScenePrefab,
        FullPath,
    }

    public class ResourceManager
    {
        private static bool s_isInit = false;
        private static IAssetLoader s_assetLoader;
        private static Dictionary<int, string> s_instanceIDToPathDict = new Dictionary<int, string>(200);
        private static GameObject s_objPoolParent;
        private static GameObject s_objUsedParent;
        private static Dictionary<string, Queue<Object>> s_resObjectPool = new Dictionary<string, Queue<Object>>(100);
        private static HashSet<string> s_preloadingPath = new HashSet<string>(); //TODO ab加载层检测加载中
        private static HashSet<string> s_reqPreloadPath = new HashSet<string>(); //非自动卸载的预加载列表

        private static Dictionary<ResType, string> _KeyPathDic = new Dictionary<ResType, string>()
        {
            {ResType.Null, ""},
            {ResType.Prefab, "Res/Prefabs/"},
            {ResType.RolePrefab, "Res/Prefabs/Characters/"},
            {ResType.EffectPrefab, "Res/Prefabs/Effect/"},
            {ResType.Camera, "Res/Prefabs/Camera/"},
            {ResType.Terrain, "Res/Maps/"},
            {ResType.UI, "Res/UI/"},
            {ResType.UIPicture, "ArtRes/UI/Pictures/"},
            {ResType.Texture, "Res/Textures/"},
            {ResType.ScriptObject, "Res/ScriptableObjects/"},
            {ResType.Dialogue, "Res/ScriptableObjects/Dialogue/"},
            {ResType.AIGraph, "Res/ScriptableObjects/AI/"},
            {ResType.QuestChain, "Res/ScriptableObjects/QuestChains/"},
            {ResType.Shader, "Shaders/"},
            {ResType.ShaderVariant, "Shaders/ShaderVariants/"},
            {ResType.Folder, ""},
            {ResType.CutScenePrefab, "Res/Prefabs/CutScene/"},
            {ResType.FullPath, ""},
        };
        private static Dictionary<ResType, string> _KeySuffixDic = new Dictionary<ResType,string>()
        {
            {ResType.Null, ""},
            {ResType.Prefab, ".prefab"},
            {ResType.RolePrefab, ".prefab"},
            {ResType.EffectPrefab, ".prefab"},
            {ResType.Camera, ".prefab"},
            {ResType.Terrain, ".prefab"},
            {ResType.UI, ".prefab"},
            {ResType.UIPicture, ".png"},
            {ResType.Texture, ".png"},
            {ResType.ScriptObject, ".asset"},
            {ResType.Dialogue, ".asset"},
            {ResType.AIGraph, ".asset"},
            {ResType.QuestChain, ".asset"},
            {ResType.Shader, ".shader"},
            {ResType.ShaderVariant, ".shadervariants"},
            {ResType.Folder, ""},
            {ResType.CutScenePrefab, ".prefab"},
            {ResType.FullPath, ""},
        };

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void EnitorInit()
        {
            Init().Forget();
        }
#endif
        public static async UniTask Init()
        {
            if (s_isInit)
            {
                return;
            }

            s_isInit = true;
#if UNITY_EDITOR
            AssetBundleData.s_assetBundleLoad = EditorPrefs.GetBool("AssetBundleModeEditor");
            s_assetLoader = AssetBundleData.s_assetBundleLoad
                ? (IAssetLoader)new AssetBundleLoader()
                : new AssetDataBaseLoader();
#else
            AssetBundleData.s_assetBundleLoad = true;
            s_assetLoader = new AssetBundleLoader();
#endif
            await s_assetLoader.Init();
            s_objPoolParent = new GameObject("ResObjectPool");
            s_objUsedParent = new GameObject("ResObjectUsed");
            Application.lowMemory += delegate()
            {
                LogUtils.Error("内存低警告");
                UnloadUnusedAssets();
            };
        }

        /// <summary>
        /// 提前加载资源，可以instance的资源会加载后放入对象池
        /// </summary>
        /// <param name="path">Assets下路径 Res/xxx.prefab</param>
        /// <param name="isAutoUnload">是否场景切换时自动卸载</param>
        public static UniTask PreloadAsset<T>(string path, ResType abType, bool isAutoUnload = true) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return UniTask.CompletedTask;
            }
            string relativePath = GetRelativePath(path, abType);
            return _PreloadAsset<T>(relativePath, isAutoUnload);
        }
        
        /// <summary>
        /// 提前加载资源，可以instance的资源会加载后放入对象池
        /// </summary>
        /// <param name="path">全路径 Assets/Res/xxx.prefab</param>
        /// <param name="isAutoUnload">是否场景切换时自动卸载</param>
        public static UniTask PreloadAsset<T>(string path, bool isAutoUnload = true) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return UniTask.CompletedTask;
            }
            return _PreloadAsset<T>(path, isAutoUnload);
        }

        private static async UniTask _PreloadAsset<T>(string path, bool isAutoUnload) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return;
            }
            if (!isAutoUnload && !s_reqPreloadPath.Contains(path))
            {
                s_reqPreloadPath.Add(path);
            }
            if (s_resObjectPool.ContainsKey(path) || s_assetLoader.IsLoaded(path) || s_preloadingPath.Contains(path))
            {
                return;
            }
            LogUtils.Trace($"预加载 : {path}", LogChannel.Load);
            s_preloadingPath.Add(path);
            var obj = await s_assetLoader.LoadAssetAsync<T>(path);
            s_preloadingPath.Remove(path);
            if (obj == null)
            {
                LogUtils.Error($"预加载失败 : {path}", LogChannel.Load);
            }
            else
            {
                LogUtils.Trace($"预加载成功 : {path}", LogChannel.Load);
                if (IsInstanceAsset(obj))
                {
                    GameObject instanceObj = InstanceObj(obj, path);
                    Recycle(instanceObj);
                }
                else
                {
                    s_instanceIDToPathDict[obj.GetInstanceID()] = path;
                }
            }
        }

        private static string GetRelativePath(string path, ResType abType)
        {
            if (path.StartsWithEx("Assets"))
            {
                return ZString.Concat(_KeyPathDic[abType], path, _KeySuffixDic[abType]);
            }
            else
            {
                return ZString.Concat("Assets/", _KeyPathDic[abType], path, _KeySuffixDic[abType]);
            }
        }
        
        /// <summary>
        /// 同步加载资源，优先使用预加载避免卡顿
        /// </summary>
        /// <param name="path">Assets下路径 Res/xxx.prefab</param>
        public static T LoadAsset<T>(string path, ResType abType) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return null;
            }

            string relativePath = GetRelativePath(path, abType);
            return _LoadAsset<T>(relativePath);
        }
        
        /// <summary>
        /// 同步加载资源，优先使用预加载避免卡顿
        /// </summary>
        /// <param name="path">全路径 Assets/Res/xxx.prefab</param>
        public static T LoadAsset<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return null;
            }
            return _LoadAsset<T>(path);
        }

        private static T _LoadAsset<T>(string path) where T : Object
        {
            Object obj = GetGameObjectFormPool(path);
            if (obj != null) return obj as T;
            obj = s_assetLoader.LoadAsset<T>(path);
            if (obj == null)
            {
                LogUtils.Error(ZString.Concat("同步加载资源失败, path: ", path));
                return null;
            }

            if (IsInstanceAsset(obj))
            {
                GameObject newInstance = InstanceObj(obj, path);
                newInstance.SetActive(true);
                return newInstance as T;
            }
            else
            {
                s_instanceIDToPathDict[obj.GetInstanceID()] = path;
            }

            return obj as T;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">Assets下路径 Res/xxx.prefab</param>
        public static async UniTask<T> LoadAssetAsync<T>(string path, ResType abType) where T : Object
        {
            UniTask<Object> objUniTask = default;
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return await objUniTask as T;
            }
            string relativePath = GetRelativePath(path, abType);
            objUniTask = _LoadAssetAsync<T>(relativePath);
            return await objUniTask as T;
        }

        public static async UniTask<T> LoadAssetAsyncButNotInstance<T>(string path, ResType abType) where T : Object
        {
            UniTask<Object> objUniTask = default;
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return await objUniTask as T;
            }
            string relativePath = GetRelativePath(path, abType);
            objUniTask = _LoadAssetAsync<T>(relativePath, false, false);
            return await objUniTask as T;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">全路径 Assets/Res/xxx.prefab</param>
        public static async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        {
            UniTask<Object> objUniTask = default;
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return await objUniTask as T;
            }
            
            objUniTask = _LoadAssetAsync<T>(path);
            return await objUniTask as T;
        }

        /// <summary>
        /// 异步加载资源并且在实例化时设置位置旋转
        /// </summary>
        /// <param name="path">全路径 Assets/Res/xxx.prefab</param>
        public static async UniTask<T> LoadAssetAsync<T>(string path, Vector3 pos, Quaternion rot) where T : Object
        {
            Object obj = GetGameObjectFormPool(path);
            if (obj != null)
            {
                GameObject gameObj = (GameObject)obj;
                gameObj.transform.position = pos;
                gameObj.transform.rotation = rot;
            }
            else
            {
                obj = await s_assetLoader.LoadAssetAsync<T>(path);
                if (obj == null)
                {
                    return null;
                }
                if (IsInstanceAsset(obj))
                {
                    GameObject newInstance = InstanceObj(obj, path, pos, rot);
                    newInstance.SetActive(true);
                    return newInstance as T;
                }
                else
                {
                    s_instanceIDToPathDict[obj.GetInstanceID()] = path;
                }
            }
            return obj as T;
        }

        /// <summary>
        /// 对象加载
        /// </summary>
        private static async UniTask<Object> _LoadAssetAsync<T>(string path, bool folder = false, bool forceInstanceObj = true)
        {
            Object obj = GetGameObjectFormPool(path);
            if (obj != null) return obj;
            obj = await s_assetLoader.LoadAssetAsync<T>(path, folder);
            if (obj == null)
            {
                if (folder == false)
                {
                    LogUtils.Error(ZString.Concat("异步加载资源失败, path: ", path));
                }
                return null;
            }

            if (folder == false)
            {
                if (forceInstanceObj && IsInstanceAsset(obj))
                {
                    GameObject newInstance = InstanceObj(obj, path);
                    newInstance.SetActive(true);
                    return newInstance;
                }
                else
                {
                    s_instanceIDToPathDict[obj.GetInstanceID()] = path;
                }
            }
            return obj;
        }

        private static GameObject GetGameObjectFormPool(string path)
        {
            GameObject gameObj = null;
            if (s_resObjectPool.TryGetValue(path, out var objQueue) && objQueue.Count > 0)
            {
                Object obj = objQueue.Dequeue();
                gameObj = obj as GameObject;
                gameObj.transform.parent = s_objUsedParent.transform;
                gameObj.SetActive(true);
                LogUtils.Trace($"对象池获取资源成功, path: {path}", LogChannel.Load);
            }
            return gameObj;
        }

        /// <summary>
        /// 统一实例化GameObject方法
        /// </summary>
        private static GameObject InstanceObj(Object obj, string path)
        {
            if (obj == null)
            {
                LogUtils.Error($"InstanceObj is null: {path}");
                return null;
            }
            GameObject newInstance = Object.Instantiate(obj as GameObject);
            RecordObj(newInstance, path);
            return newInstance;
        }
        
        private static GameObject InstanceObj(Object obj, string path, Vector3 pos, Quaternion rot)
        {
            if (obj == null)
            {
                LogUtils.Error($"InstanceObj is null: {path}");
                return null;
            }
            GameObject newInstance = Object.Instantiate(obj as GameObject, pos, rot);
            RecordObj(newInstance, path);
            return newInstance;
        }

        private static void RecordObj(GameObject gameObj, string path)
        {
            gameObj.name = gameObj.name.Replace("(Clone)", "");
            var instantiateObj = gameObj.AddComponent<InstantiateObj>();
            int instanceID = gameObj.GetInstanceID();
            instantiateObj.originalInstanceID = instanceID;
            s_instanceIDToPathDict[instanceID] = path;
        }

        private static bool IsInstanceAsset(Object obj)
        {
            var isInstanceAsset = (obj is GameObject);
            return isInstanceAsset;
        }

        /// <summary>
        /// 加载文件夹的ab
        /// </summary>
        public static UniTask<Object> LoadFolderAb(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                LogUtils.Error("path is empty, you must fix it !");
                return UniTask.FromResult<Object>(null);
            }

            string relativePath = GetRelativePath(path, ResType.Folder);
            return _LoadAssetAsync<Object>(relativePath, true);
        }

        public static UniTask<string> LoadString(string path)
        {
            if (Application.isPlaying)
            {
                path = LoadUtils.GetLoadPath(path);
                return GetStringFile(path);
            }
            else
            {
                if (File.Exists(path))
                {
                    FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string str = sr.ReadToEnd();
                    sr.Close();
                    file.Close();
                    return UniTask.FromResult(str);
                }
            }
            return UniTask.FromResult<string>(null);
        }

        private static async UniTask<string> GetStringFile(string path)
        {
            System.Uri uri = new System.Uri(path);
            UnityWebRequest uwr = new UnityWebRequest(uri);
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            uwr.downloadHandler = dH;
            try
            {
                await uwr.SendWebRequest();
            }
            catch (System.Exception)
            {
                LogUtils.Error("未找到Json:" + path);
                return null;
            }
            if (uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                return null;
            }
            else
            {
                return System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data, 3, uwr.downloadHandler.data.Length - 3);
            }
        }

        public static async UniTask<byte[]> LoadBytes(string fullPath)
        {
            if (Application.isPlaying)
            {
                fullPath = LoadUtils.GetLoadPath(fullPath);
                return await GetFileBytes(fullPath);
            }
            else
            {
                return FileUtils.ReadAllBytesNullable(fullPath);
            }
        }
        
        private static async UniTask<byte[]> GetFileBytes(string path)
        {
            UnityWebRequest uwr = new UnityWebRequest(new System.Uri(path));
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            uwr.downloadHandler = dH;
            try
            {
                await uwr.SendWebRequest();
            }
            catch (System.Exception)
            {
                LogUtils.Error("未找到File:" + path);
                return null;
            }
            if (uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                LogUtils.Error($"GetFileBytes Fail!! path={path} error={uwr.error}");
                return null;
            }
            return uwr.downloadHandler.data;
        }

        /// <summary>
        /// 回收，并不是卸载
        /// </summary>
        public static void Recycle(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }
            if (s_instanceIDToPathDict.TryGetValue(instance.GetInstanceID(), out string path))
            {
                if (!s_resObjectPool.TryGetValue(path, out var objectQueue))
                {
                    objectQueue = new Queue<Object>();
                    s_resObjectPool.Add(path, objectQueue);
                }

                if (objectQueue.Contains(instance))
                {
                    LogUtils.Warning($"回收已经在对象池的资源, path: {path}", LogChannel.Load);
                }
                else
                {
                    LogUtils.Trace($"回收资源成功, path: {path}", LogChannel.Load);
                    objectQueue.Enqueue(instance);
                    instance.transform.parent = s_objPoolParent.transform;
                    instance.SetActive(false);
                }
            }
            else
            {
                LogUtils.Error($"回收资源失败, instance: {instance.name}", LogChannel.Load);
            }
        }

        /// <summary>
        /// 彻底删除这个对象，ab依赖资源如果没有被其他资源引用，也会一起卸载
        /// </summary>
        /// <param name="obj"></param>
        public static void Destroy(Object obj)
        {
            if (obj == null)
            {
                return;
            }
            if (s_instanceIDToPathDict.TryGetValue(obj.GetInstanceID(), out string path))
            {
                LogUtils.Trace($"Destroy资源成功, path: {path}", LogChannel.Load);
                int leftRefCount = s_assetLoader.UnLoadAsset(path);
                if (IsInstanceAsset(obj))
                {
                    s_instanceIDToPathDict.Remove(obj.GetInstanceID());
                    Object.Destroy(obj);
                }
                else
                {
                    if (leftRefCount <= 0) s_instanceIDToPathDict.Remove(obj.GetInstanceID());
                }
            }
            else
            {
                LogUtils.Warning(ZString.Concat("Destroy失败，资源没有从ResourceManager生成: " ,obj.name));
            }
        }
        
        public static void AddAssetRef(int originalInstanceID, int instanceID)
        {
            if (instanceID == 0 || s_instanceIDToPathDict.ContainsKey(instanceID)) return;
            if (s_instanceIDToPathDict.TryGetValue(originalInstanceID, out string path))
            {
                LogUtils.Trace($"AddAssetRef: {path} {originalInstanceID} {instanceID}", LogChannel.Load);
                s_assetLoader.AddAssetRef(path);
                s_instanceIDToPathDict[instanceID] = path;
            }
        }
        
        public static void DelAssetRef(int instanceID)
        {
            if (instanceID == 0) return;
            if (s_instanceIDToPathDict.TryGetValue(instanceID, out string path))
            {
                LogUtils.Trace($"DelAssetRef: {path} {instanceID}", LogChannel.Load);
                s_assetLoader.UnLoadAsset(path);
                s_instanceIDToPathDict.Remove(instanceID);
            }
        }

        public static void UnloadPreloadAsset(string path, ResType abType)
        {
            string relativePath = GetRelativePath(path, abType);
            s_reqPreloadPath.Remove(relativePath);
            UnloadImmediate(relativePath);
        }

        /// <summary>
        /// 根据路径进行卸载,只会卸载对象池内对应资源，并根据引用卸载ab
        /// </summary>
        private static void UnloadImmediate(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            LogUtils.Trace("DestroyByPath: " + path, LogChannel.Load);
            if (s_resObjectPool.TryGetValue(path, out var objectQueue))
            {
                while (objectQueue.Count > 0)
                {
                    Object obj = objectQueue.Dequeue();
                    Destroy(obj);
                }
                s_resObjectPool.Remove(path);
            }
        }

        private static void DestroyResObjectPoolWithoutPreload()
        {
            foreach (var resObjectKeyValue in s_resObjectPool)
            {
                if (!s_reqPreloadPath.Contains(resObjectKeyValue.Key))
                {
                    while (resObjectKeyValue.Value.Count > 0)
                    {
                        Object obj = resObjectKeyValue.Value.Dequeue();
                        LogUtils.Trace($"卸载对象池内资源：{resObjectKeyValue.Key}", LogChannel.Load);
                        Destroy(obj);
                    }
                }
            }
        }

        /// <summary>
        /// 卸载unused资源
        /// </summary>
        public static void UnloadUnusedAssets()
        {
            DestroyResObjectPoolWithoutPreload();
            Resources.UnloadUnusedAssets();
        }

        public static void Dispose()
        {
            
        }
    }
}

