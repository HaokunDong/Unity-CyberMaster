using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Managers
{
    public class CoroutineMgr: MonoBehaviour
    {
        #region Coroutine Related
        public class WaitForSecondsX : CustomYieldInstruction
        {
            private float m_curTime;
            private bool m_ret = false;

            public override bool keepWaiting
            {
                get
                {
                    m_ret = m_curTime > Time.realtimeSinceStartup;
                    if (false == m_ret)
                    {
                        _Recycle();
                    }
                    return m_ret;
                }
            }

            private void _Recycle()
            {
                Instance.RecycleWaitForSeconds(this);
            }

            public void SetTime(float seconds)
            {
                m_curTime = Time.realtimeSinceStartup + seconds;
                m_ret = false;
            }
        }

        public static CoroutineMgr Instance
        {
            get
            {
#if UNITY_EDITOR
                Init();
#endif
                return s_instance;
            }
        }

        private static CoroutineMgr s_instance;
#if UNITY_EDITOR
        private static string s_name = "/CoroutineMgr";
#endif
        public static void Init()
        {
            if (s_instance == null)
            {
#if UNITY_EDITOR
                var existed = GameObject.Find(s_name);
                if (existed != null)
                {
                    _RefreshInfo(existed);
                }
                else
                {
                    GameObject obj = new GameObject("CoroutineMgr");
                    _RefreshInfo(obj);
                }
#else
                GameObject obj = new GameObject("CoroutineMgr");
                _RefreshInfo(obj);
#endif
            }

            if (Application.isPlaying)
            {
                GameObject.DontDestroyOnLoad(s_instance.gameObject);
            }
        }

        private static void _RefreshInfo(GameObject obj)
        {
            if (Application.isPlaying)
            {
                GameObject.DontDestroyOnLoad(obj);
            }
            s_instance = obj.GetComponent<CoroutineMgr>();
            if (s_instance == null)
            {
                s_instance = obj.AddComponent<CoroutineMgr>();
            }

            // SysUtils.StartUpdateTime();
        }

        //可用的对象池
        private Stack<WaitForSecondsX> _WaitForSecondStack = new Stack<WaitForSecondsX>();
        private Stack<WaitForEndOfFrame> _WaitForEndofFrameStack = new Stack<WaitForEndOfFrame>();


        //当前帧回收的EndofFrame
        private Stack<WaitForEndOfFrame> _DeletingEndofFrameStack = new Stack<WaitForEndOfFrame>();
        //下一帧需要释放的GC--由WaitForEndOfFrame的定义进行推断，当前帧结束以后，这个字段就不再起作用
        private Stack<WaitForEndOfFrame> _ReleaseForNextFrameStack = new Stack<WaitForEndOfFrame>();

        public WaitForSecondsX WaitForSeconds(float seconds)
        {
            WaitForSecondsX cur = null;
            if (_WaitForSecondStack.Count > 0)
            {
                cur =  _WaitForSecondStack.Pop();
            }
            else
            {
                cur = new WaitForSecondsX();
            }
            cur.SetTime(seconds);
            return cur;
        }

        /// <summary>
        /// 延迟time，调用特定的func
        /// </summary>
        public void DelayCallFunc(Action func, float time)
        {
            StartCorouX(_DelayFuncEmulator(func, time));
        }

        private IEnumerator _DelayFuncEmulator(Action func, float time)
        {
            yield return new WaitForSeconds(time);
            func?.Invoke();
        }

        internal void RecycleWaitForSeconds(WaitForSecondsX cur)
        {
            if (_WaitForSecondStack.Contains(cur))
            {
                return;
            }
            _WaitForSecondStack.Push(cur);
        }

        internal void RecycleWaitForEndOfFrame(WaitForEndOfFrame cur)
        {
            _WaitForEndofFrameStack.Push(cur);
        }

        public WaitForEndOfFrame WaitForEndOfFrame()
        {
            WaitForEndOfFrame cur = null;
            if (_WaitForEndofFrameStack.Count > 0)
            {
                cur =  _WaitForEndofFrameStack.Pop();
            }
            else
            {
                cur = new WaitForEndOfFrame();
            }

            _ReleaseForNextFrameStack.Push(cur);
            return cur;
        }

        private void UpdateCoroutine()
        {
            while (_DeletingEndofFrameStack.Count > 0)
            {
                RecycleWaitForEndOfFrame(_DeletingEndofFrameStack.Pop());
            }

            while (_ReleaseForNextFrameStack.Count > 0)
            {
                _DeletingEndofFrameStack.Push(_ReleaseForNextFrameStack.Pop());
            }
        }

        public Coroutine StartCorouX(IEnumerator ienumerator)
        {
            return StartCoroutine(ienumerator);
        }

        public void EndCorouX(Coroutine routine)
        {
            StopCoroutine(routine);
        }

        public Coroutine StartCorouX(string methodName)
        {
            return StartCoroutine(methodName);
        }

        public Coroutine StartCorouX(string methodName, object value = null)
        {
            return StartCoroutine(methodName, value);
        }

        #endregion

        #region Update Process
        //延迟一帧执行的函数
        private Stack<Action> _workingFrameFuncStack = new Stack<Action>();
        private Stack<Action> _NextFrameFuncStack = new Stack<Action>();

        //开启正常Update的功能,部分系统没有MonoBehaviror的基类
        private List<Action> _UpdatingFuncList = new List<Action>();
        /// <summary>
        ///在下一帧执行，只执行一次
        /// </summary>
        public void CallFuncInNextFrame(Action cur)
        {
            _NextFrameFuncStack.Push(cur);
        }

        /// <summary>
        /// 部分系统不需要MonoBehavior，那只需要开启这个接口即可
        /// </summary>
        public void StartUpdate(Action cur)
        {
            if (false == _UpdatingFuncList.Contains(cur))
            {
                _UpdatingFuncList.Add(cur);
            }
        }

        public void StopUpdate(Action cur)
        {
            _UpdatingFuncList.Remove(cur);
        }

        void Update()
        {
            while (_workingFrameFuncStack.Count > 0)
            {
                var actionFunc = _workingFrameFuncStack.Pop();
                actionFunc?.Invoke();
            }

            //由于两次栈的先入后出，保证了Action的顺序执行
            while (_NextFrameFuncStack.Count > 0)
            {
                _workingFrameFuncStack.Push(_NextFrameFuncStack.Pop());
            }

            for (int i = 0; i < _UpdatingFuncList.Count; i++)
            {
                if (_UpdatingFuncList[i] != null)
                {
                    _UpdatingFuncList[i]();
                }
            }

            UpdateCoroutine();
        }

        private void _UnInit()
        {
            _workingFrameFuncStack.Clear();
            _NextFrameFuncStack.Clear();
            _UpdatingFuncList.Clear();
            _WaitForSecondStack.Clear();
            _WaitForEndofFrameStack.Clear();
        }

        public static void Uninit()
        {
            if (s_instance != null)
            {
                // SysUtils.EndUpdateTime();
                s_instance._UnInit();
                GameObject.Destroy(s_instance.gameObject);
                s_instance = null;
            }
        }

        #endregion
    }
}

