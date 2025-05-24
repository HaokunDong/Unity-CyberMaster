using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AssetLoad
{
    public interface IAssetLoader
    {
        UniTask Init();
        UniTask<Object> LoadAssetAsync<T>(string path, bool folder = false);
        Object LoadAsset<T>(string path);
        bool IsLoaded(string path);
        void AddAssetRef(string path);
        int UnLoadAsset(string path);
    }
}