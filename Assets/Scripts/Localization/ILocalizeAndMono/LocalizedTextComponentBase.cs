using UnityEngine;

namespace Localization.ILocalizeAndMono 
{
    public abstract class LocalizedTextComponentBase : MonoBehaviour, ILocalize
    {
        public abstract void OnLocalize();
    }
}