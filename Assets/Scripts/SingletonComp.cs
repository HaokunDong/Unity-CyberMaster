using UnityEngine;

/// <summary>
/// 安全的 MonoBehaviour 单例基类，子类不要 override Awake，只实现 OnSingletonInit。
/// </summary>
public abstract class SingletonComp<T> : MonoBehaviour where T : SingletonComp<T>
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting;

    public static T Ins
    {
        get
        {
            if (_applicationIsQuitting) return null;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name + " (Singleton)");
                        _instance = go.AddComponent<T>();
                        DontDestroyOnLoad(go);
                    }
                    _instance.InitializeIfNeeded();
                }

                return _instance;
            }
        }
    }

    private bool _isInitialized = false;

    // 不能 sealed，不能 override，所以只能靠说明约束子类别写自己的 Awake
    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            InitializeIfNeeded();
        }
        else if (_instance != this)
        {
            DestroyImmediate(gameObject);
        }
    }

    private void InitializeIfNeeded()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            OnSingletonInit();
        }
    }

    protected virtual void OnSingletonInit() { }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            _applicationIsQuitting = true;
        }
    }
}
