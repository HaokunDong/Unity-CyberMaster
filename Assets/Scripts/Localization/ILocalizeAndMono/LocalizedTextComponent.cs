using System;
using System.Collections.Generic;
using Localization.Enum;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Localization.ILocalizeAndMono
{
    public abstract class LocalizedTextComponent<T> : LocalizedTextComponentBase where T : Component
    {
        public T text = null;
        [OnValueChanged("OnKeyChanged")]
        public string key;
        [OnValueChanged("OnKeyChanged")]
        public List<object> parameters = null;

        [Tooltip("Maintain original text alignment. If set to false, localization will determine whether text is left or right aligned")]
        [OnValueChanged("OnKeyChanged")]
        public bool maintainTextAlignment;

#if UNITY_EDITOR
        [ShowInInspector, ReadOnly]
        private string m_PreviewStr;
#endif
        protected abstract void SetText(T component, string value);
        protected abstract void UpdateAlignment(T component, LanguageDirection direction);

        public void Reset()
        {
            if (text == null)
            {
                text = GetComponent<T>();
            }
        }

        private void Start()
        {
            if (text == null)
            {
                text = GetComponent<T>();
            }
        }

        public void OnEnable()
        {
            if (LocalizationManager.setting != null)
            {
                LocalizationManager.setting.AddOnLocalizeEvent(this);
            }
            else
            {
                LocalizationManager.AddToSetting(this);
            }
        }


        public override void OnLocalize()
        {
#if UNITY_EDITOR
            var flags = text != null ? text.hideFlags : HideFlags.None;
            if (text != null) text.hideFlags = HideFlags.DontSave;
#endif
            if (parameters != null && parameters.Count > 0)
            {
                SetText(text, LocalizationManager.setting.Get(key, parameters.ToArray()));
#if UNITY_EDITOR
                m_PreviewStr = LocalizationManager.setting.Get(key, parameters.ToArray());
#endif
            }
            else
            {
                SetText(text, LocalizationManager.setting.Get(key));
#if UNITY_EDITOR
                m_PreviewStr = LocalizationManager.setting.Get(key);
#endif
            }


            // var direction = LocalizationManager.setting.SelectedLanguageDirection;

            // if (text != null && !maintainTextAlignment) UpdateAlignment(text, direction);

#if UNITY_EDITOR
            if (text != null) text.hideFlags = flags;
#endif
        }

        public void ClearParameters()
        {
            if (parameters == null)
            {
                parameters = new List<object>();
            }
            else
            {
                parameters.Clear();
            }
        }

        public void AddParameter(object parameter)
        {
            if (parameters == null)
            {
                parameters = new List<object>();
            }
            parameters.Add(parameter);
            OnLocalize();
        }

        public void SetParameters(params object[] parameters)
        {
            ClearParameters();
            this.parameters.AddRange(parameters);
            OnLocalize();
        }

#if UNITY_EDITOR
        private void OnKeyChanged()
        {
            CheckAutoKeys();
            Invoke(nameof(OnLocalize), 0.1f);
        }

        [Button]
        private void Refresh()
        {
            OnLocalize();
        }

        private void CheckAutoKeys()
        {
            aks.Clear();
            if (!string.IsNullOrEmpty(key))
            {
                var dict = LocalizationManager.GetLanguagesContains(key);
                if (dict != null && dict.Keys.Count > 0)
                {
                    foreach (var dk in dict.Keys)
                    {
                        aks.Add(new AutoKey(this, dk));
                    }
                }
            }
        }

        [Serializable]
        public class AutoKey
        {
            [ReadOnly]
            public string autoKey = string.Empty;
            [ShowInInspector, ReadOnly]
            private string keyPreview = string.Empty;

            private LocalizedTextComponent<T> component = null;

            public AutoKey(LocalizedTextComponent<T> c, string k)
            {
                autoKey = k;
                component = c;
                LocalizationSetting setting = LocalizationManager.setting;
                if (setting != null)
                {
                    keyPreview = autoKey;
                    keyPreview = setting.KeyExist(keyPreview) ? setting.Get(keyPreview) : $"key ${autoKey} has no localized text";
                }
            }

            [Button]
            private void UseKey()
            {
                if (component == null)
                {
                    return;
                }
                component.key = autoKey;
                component.OnLocalize();
                component.aks.Clear();
            }
        }

        [ListDrawerSettings(/*ShowFoldout = true*/)]
        public List<AutoKey> aks = new List<AutoKey>();
#endif
    }
}