#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Everlasting.Extend;
using GameBase.Log;
using Sirenix.OdinInspector;
//using Tools.ElrondTools.PSD2UGUI.Script;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Localization
{
    [Serializable]
    public class FontConfigEntry
    {
        [LabelText("字体用途")]public string desc;
        public string psdFontName;
        public Font chineseFont;
        public Font japaneseFont;
        public Font englishFont;
        public Font koreanFont;
    }
    
    public class LocalizationFontMap : ScriptableObject//, IPsdFontMappedProjectFontQueryProxy
    {       
        [InfoBox("以中文字体作为基准，建立与其他语言对应字体的映射")]
        public List<FontConfigEntry> configEntries;
        private Dictionary<string, FontConfigEntry> m_chineseFontName2ConfigEntryMap;
        private Dictionary<string, FontConfigEntry> m_psdFontName2ConfigEntryMap;
        
        private const string FONT_MAP_PATH = "Assets/Editor/UI/Data/LocalizationFontMap.asset";
        private static LocalizationFontMap s_instance;

        public static LocalizationFontMap Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = AssetDatabase.LoadAssetAtPath<LocalizationFontMap>(FONT_MAP_PATH);
                }

                return s_instance;
            }
        }

        //[InitializeOnLoadMethod]
        //private static void AddProxyForPsd2UGUIPlugin()
        //{
        //    Psd2UGUIProxyUtil.AddProxy((IPsdFontMappedProjectFontQueryProxy)Instance);
        //}

        public bool ReplacePrefabFontByTargetLanguage(GameObject prefab, Language targetLanguage)
        {
            
            var modified = false;
            var texts = prefab.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                if (text.font == null)
                {
                    LogUtils.Warning($"Font is null :{text.gameObject.GetPath()}");
                    continue;
                }

                if (TryQueryChineseMappedTargetLanguageFont(text.font.name, targetLanguage, out var font) && font != null)
                {
                    text.font = font;
                    modified = true;
                    LogUtils.Debug($"{text.font.name} {text.gameObject.GetPath()}");
                }
            }
            
            return modified;
        }

        public bool TryQueryChineseMappedTargetLanguageFont(string chineseFontName, Language targetLanguage, out Font font)
        {
            font = null;
            m_chineseFontName2ConfigEntryMap ??= BuildChineseFontName2ConfigEntryMap();
            if (!m_chineseFontName2ConfigEntryMap.TryGetValue(chineseFontName, out var configEntry))
            {
                return false;
            }

            font = targetLanguage switch
            {
                Language.Japanese => configEntry.japaneseFont,
                Language.English => configEntry.englishFont,
                Language.Korean => configEntry.koreanFont,
                _ => configEntry.chineseFont,
            };

            return true;
        }
        
        private Dictionary<string, FontConfigEntry> BuildChineseFontName2ConfigEntryMap()
        {
            return configEntries.ToDictionary(configEntry => configEntry.chineseFont.name);
        }
        
        public bool TryQueryPsdFontNameMappedProjectFont(string psdFontName, out Font font)
        {
            m_psdFontName2ConfigEntryMap ??= BuildPsdFontName2ConfigEntryMap();
            m_psdFontName2ConfigEntryMap.TryGetValue(psdFontName, out var configEntry);
            font = configEntry?.chineseFont;
            return font != null;
        }

        private Dictionary<string, FontConfigEntry> BuildPsdFontName2ConfigEntryMap()
        {
            return configEntries.ToDictionary(configEntry => configEntry.psdFontName);
        }
    }
}

#endif