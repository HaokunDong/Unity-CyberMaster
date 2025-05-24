using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Struct
{
    //用于统一销毁回收
    public class DisposableBag : IDisposable
    {
        public List<IDisposable> disposables;
        public bool IsInPool { get; private set; }
        
        public void Dispose()
        {
            if (IsInPool)
            {
                Debug.LogError("DisposableBag has disposed!");
                return;
            }

            foreach (var disposable in disposables) disposable.Dispose();
            IsInPool = true;
            disposables = null;
        }

        public DisposableBag(int initCap = 0)
        {
            disposables = new List<IDisposable>(initCap);
        }
        
        public static DisposableBag Create(params IDisposable[] disposables)
        {
            var bag = new DisposableBag(disposables.Length);
            foreach (var disposable in disposables)
            {
                bag.Add(disposable);
            }
            return bag;
        }

        public void Add(IDisposable disposable)
        {
            disposables.Add(disposable);
        }
        
        public void Remove(IDisposable disposable)
        {
            disposables.Remove(disposable);
        }
    }
    
    //泛型版本，struct用这个避免转接口装箱开销
    public class DisposableBag<T> : IDisposable where T : IDisposable
    {
        public List<T> disposables;
        public bool IsInPool { get; private set; }

        public void Dispose()
        {
            if (IsInPool)
            {
                Debug.LogError("DisposableBag has disposed!");
                return;
            }

            foreach (var disposable in disposables) disposable.Dispose();
            IsInPool = true;
            disposables = null;
        }
        
        public DisposableBag(int initCap = 0)
        {
            disposables = new List<T>(initCap);
        }

        public static DisposableBag<T> Create(params T[] disposables)
        {
            var bag = new DisposableBag<T>(disposables.Length);
            foreach (var disposable in disposables)
            {
                bag.Add(disposable);
            }
            return bag;
        }

        public void Add(T disposable)
        {
            disposables.Add(disposable);
        }
        
        public void Remove(T disposable)
        {
            disposables.Remove(disposable);
        }
    }
}