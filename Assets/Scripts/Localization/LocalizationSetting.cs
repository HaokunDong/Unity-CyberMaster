using System.Collections.Generic;
using System.Text.RegularExpressions;
using Everlasting.Extend;
//using Everlasting.UI.Base;
using GameBase.Log;
using Localization.Enum;
using Localization.ILocalizeAndMono;
using Sirenix.OdinInspector;
#if DEBUG_ASSIST_ENABLE
using Tools;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Localization
{
    [CreateAssetMenu(fileName = "DefaultLocalizationSetting", menuName = "本地化/LocalizationSetting", order = 1)]
    public class LocalizationSetting : ScriptableObject
    {
        public List<LocalizationAsset> inputFiles = null;
        public List<Language> supportedLanguages = null;
#if DEBUG_ASSIST_ENABLE
        private const string m_longPlaceHolder = "一二三四五六七八九十一二三十五六七八九廿一二三十五六七八九卅一二三十五六七八九卌一二三十五六七八九伍一二三十五六七八九陆一二三十五六七八九柒一二三十五六七八九捌一二三十五六七八九玖一二三十五六七八九百";
        private const string m_longPlaceHolderKorean = "일이삼사오육칠팔구십10십일십이십삼십사십오십육십칠십팔십구이십20이십일이십이이십삼이십사이십오이십육이십칠이십팔이십구삼십30삼십일삼십이삼십삼삼십사삼십오삼십육삼십칠삼십팔삼십구사십40사십일사십이사십삼사십사사십오사십육사십칠사십팔사십구오십50오십일오십이오십삼오십사오십오오십육오십칠오십팔오십구육십60육십일육십이육십삼육십사육십오육십육육십칠육십팔육십구칠십70칠십일칠십이칠십삼칠십사칠십오칠십육칠십칠칠십팔칠십구팔십80팔십일팔십이팔십삼팔십사팔십오팔십육팔십칠팔십팔팔십구구십90구십일구십이구십삼구십사구십오구십육구십칠구십팔구십구백100";
#endif
        
        [SerializeField]
        private Language m_selectedLanguage = Language.Chinese;
        public Language selectedLanguage
        {
            get => m_selectedLanguage;
            set
            {
                if (m_selectedLanguage != value)
                {
                    m_selectedLanguage = value;
                    
                    if(Application.isPlaying)
                    {
                        LocalizationManager.RestCurrentLanFonts(m_selectedLanguage);
                        //PanelManager.AfterChangeLanguageAll();
                        OnSelectedLanguageChanged();
                    }
                    else
                    {
                        OnSelectedLanguageChanged();
                    }
                }
            }
        }

        //public AudioLanguage selectedAudioLanguage = AudioLanguage.Chinese;

        [Tooltip("If we cant find the string for the selected language we fall back to this language.")]
        [ReadOnly]
        public Language fallbackLanguage = Language.Chinese;
        [Header("Event invoked when language is changed")]
        [Tooltip("This event is invoked every time the selected language is changed.")]
        public UnityEvent Localize = new UnityEvent();
        
        private const string KeyNotFound = "Key {0} Not Found!";
        
        private static Regex m_argPattern = new Regex(@"(?<=([^{]|^)){\d+}(?=([^}]|$))", RegexOptions.Compiled);
        private static Regex m_refPattern = new Regex(@"#\w+#", RegexOptions.Compiled);

        public LanguageDirection SelectedLanguageDirection
        {
            get { return GetLanguageDirection(selectedLanguage); }
        }
        
        private LanguageDirection GetLanguageDirection(Language language)
        {
            switch (language)
            {
                //case Language.Hebrew:
                    //return LanguageDirection.RightToLeft;
                //case Language.Arabic:
                    //return LanguageDirection.RightToLeft;
                default:
                    return LanguageDirection.LeftToRight;
            }
        }

        //public string GetWwiseEventName(string key)
        //{
        //    if (!key.IsNullOrEmpty() && key.StartsWithEx("&"))
        //    {
        //        key = key.Substring(1);
        //    }
            
        //    //var selected = ZStringTool.Concat(audioLanguage.ToString(), "Audio");
        //    var selected = "Audio";
        //    var languages = LocalizationManager.GetLanguages(key);
        //    if (languages.Count > 0 && languages.TryGetValue(selected, out string res))
        //    {
        //        if (string.IsNullOrEmpty(res) || LocalizationManager.IsLineBreak(res))
        //        {
        //            return string.Empty;
        //        }
        //        return res;
        //    }
        //    return string.Empty;
        //}

        public string Get(string key, bool checkTermReference = true)
        {
            return Get(key, selectedLanguage, checkTermReference);
        }

        public string Get(string key, Language language, bool checkTermReference = true)
        {
            if (!key.IsNullOrEmpty() && key.StartsWithEx("&"))
            {
                key = key.Substring(1);
            }

            var selected = language.ToString();
            var languages = LocalizationManager.GetLanguages(key);
            if (languages.Count > 0 && languages.TryGetValue(selected, out string res))
            {
                if (string.IsNullOrEmpty(res) || LocalizationManager.IsLineBreak(res))
                {
#if DEBUG_ASSIST_ENABLE
                    if (GmUtils.ShowLocalizeLongPlaceHolder)
                    {
                        if (selected.ToLower().Contains("korean"))
                        {
                            return m_longPlaceHolderKorean;
                        }
                        return m_longPlaceHolder;
                    }
#endif
                    LogUtils.Warning("Could not find key " + key + " for current language " + language + ". Falling back to " + fallbackLanguage + " with " + languages[fallbackLanguage.ToString()]);
                    selected = fallbackLanguage.ToString();
                    return res = languages[selected];
                }

#if ARABSUPPORT_ENABLED
                if (language == Language.Arabic)
                {
                    return ArabicSupport.ArabicFixer.Fix(currentString, instance.showTashkeel, instance.useHinduNumbers);
                }
#endif
                if (checkTermReference)
                {
                    var refMatches = m_refPattern.Matches(res);
                    foreach (Match refMatch in refMatches)
                    {
                        string refString = refMatch.ToString();
                        refString = refString.Substring(1, refString.Length - 2);
                        //int locId;
                        //if (int.TryParse(refString, out locId))
                        //{
                            //currentString = currentString.Replace(refMatch.ToString(), GetString(locId));
                        //}
                        //else
                        //{
                        res = res.Replace(refMatch.ToString(), Get(refString,false));
                        //}
                    }
                    // 处理转义
                    res = res.Replace("{{", "{");
                    res = res.Replace("}}", "}");
                    res = res.Replace("##", "#");
                }
                res = res.Replace("\\n", "\n");
                return res;
            }
            return string.Format(KeyNotFound, key);
        }

        public bool KeyExist(string key)
        {
            var languages = LocalizationManager.GetLanguages(key);
            var selected = (int)selectedLanguage;
            return languages.Count > 0 && selectedLanguage >= 0 && selected < languages.Count;
        }

        public List<string> GetKeys()
        {
            return LocalizationManager.GetKeys();
        }
        
        public string Get(string key, params object[] arguments)
        {
            if (string.IsNullOrEmpty(key) || arguments == null || arguments.Length == 0)
            {
                return Get(key);
            }

            return string.Format(Get(key), arguments);
        }   
        
        private bool IsLanguageSupported(Language language)
        {
            if(supportedLanguages == null || supportedLanguages.Count == 0)
            {
                return false;
            }
            return supportedLanguages.Contains(language);
        }

        public void InvokeOnLocalize()
        {
            if (Localize != null)
            {
                Localize.Invoke();
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var localized = UnityEditor.SceneManagement.StageUtility.GetCurrentStageHandle().FindComponentsOfType<LocalizedTextComponentBase>();
                foreach (var local in localized)
                {
                    local.OnLocalize();
                }
            }
#endif
        }
        
        public void AddOnLocalizeEvent(ILocalize localize)
        {
            Localize.RemoveListener(localize.OnLocalize);
            Localize.AddListener(localize.OnLocalize);
            localize.OnLocalize();
        }

        public void RemoveOnLocalizeEvent(ILocalize localize)
        {
            Localize.RemoveListener(localize.OnLocalize);
        }

        public void OnSelectedLanguageChanged()
        {
            if (IsLanguageSupported(m_selectedLanguage))
            {
                InvokeOnLocalize();
            }
            else
            {
                Debug.LogWarning(m_selectedLanguage + " is not a supported language.");
            }
            //switch (m_selectedLanguage)
            //{
            //    case Language.Chinese:
            //        selectedAudioLanguage = AudioLanguage.Chinese;
            //        //WwiseManager.Instance.SetLanguage(selectedAudioLanguage);
            //        break;
            //    case Language.Japanese:
            //        selectedAudioLanguage = AudioLanguage.Japanese;
            //        //WwiseManager.Instance.SetLanguage(selectedAudioLanguage);
            //        break;
            //}

            //if(Application.isPlaying)
            //{ 
            //}
        }

        //public void ChangeAudioLanguage(AudioLanguage al)
        //{
        //    selectedAudioLanguage = al;
        //    //WwiseManager.Instance.SetLanguage(selectedAudioLanguage);
        //}
    }
}