using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Localization.Enum;
using Localization.ILocalizeAndMono;
using Localization.Reader;
using UnityEngine;
using Managers;
using Unity.Mathematics;
using GameBase.Log;
using TMPro;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using FileMode = System.IO.FileMode;
using UnityEditor.VersionControl;
using UnityEditor;
#endif

public class PlayerPrefsKey
{
    public const string ScreenMode = "ScreenMode";
    public const string MyVoice = "MyVoice";
    public const string AllMusic = "AllMusic";
    public const string Sound = "Sound";
    public const string BGM = "BGM";
    public const string IsAllMusicOn = "IsAllMusicOn";
    public const string IsSoundOn = "IsSoundOn";
    public const string IsBGMOn = "IsBGMOn";
    public const string Language = "Language";
}


namespace Localization
{
    public static class LocalizationManager
    {
        private static LocalizationSetting m_setting;

        public static LocalizationSetting setting {
            get
            {
                if (m_setting == null)
                {
                    Initialize();
                }
                return m_setting;
            }
        }
        private static Dictionary<string, Dictionary<string, string>> languageStrings = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> EmptyDict = new Dictionary<string, string>();
        private static List<LocalizationAsset> InputFiles = new List<LocalizationAsset>();
        private static List<ILocalize> localizeCash = new List<ILocalize>();
        private static Dictionary<int, TMP_FontAsset> currentLanFonts = new Dictionary<int, TMP_FontAsset>();

        private static string m_defaultKeyPath = "LocalizationSetting/DefaultLocalizationSetting";
        
        public const string ChineseType = "Chinese";
        public const string JapaneseType = "Japanese";
        public const string EnglishType = "English";
        public const string KoreanType= "Korean";
        public const string TchineseType = "Tchinese";

        public static string curLanguage = "";
        public static string Punctuation_Colon = string.Empty;

        public static void Initialize()
        {
            InitLoad(m_defaultKeyPath);
        }

        public static Language String2Lan(string language)
        {
            switch (language)
            {
                case JapaneseType:
                    return Language.Japanese;
                case EnglishType:
                    return Language.English;
                case KoreanType:
                    return Language.Korean;
                case TchineseType:
                    return Language.Tchinese;
                default:
                    return Language.Chinese;
            }
        }

        private static void _InitLanguage()
        {
            if (m_setting == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(curLanguage))
            {
                var language = PlayerPrefs.GetString(PlayerPrefsKey.Language, "Chinese");
                if (false == string.IsNullOrEmpty(language))
                {
                    switch (language)
                    {
                        case JapaneseType:
                            m_setting.selectedLanguage = Language.Japanese;
                            curLanguage = JapaneseType;
                            break;
                        case EnglishType:
                            m_setting.selectedLanguage = Language.English;
                            curLanguage = EnglishType;
                            break;
                        case KoreanType:
                            m_setting.selectedLanguage = Language.Korean;
                            curLanguage = KoreanType;
                            break;
                        case TchineseType:
                            m_setting.selectedLanguage = Language.Tchinese;
                            curLanguage = TchineseType;
                            break;
                        default:
                            m_setting.selectedLanguage = Language.Chinese;
                            curLanguage = ChineseType;
                            break;
                    }
                }
                else
                {
                    curLanguage = m_setting.selectedLanguage.ToString();
                    PlayerPrefs.SetString(PlayerPrefsKey.Language, curLanguage);
                }
            }
        }

        public static Language GetLanguage()
        {
            _InitLanguage();
            return m_setting.selectedLanguage;
        }

        public static string GetLanguageInfo()
        {
            _InitLanguage();
            return curLanguage;
        }

        public static void RestCurrentLanFonts(Language cl)
        {
            ChangeTask(cl).Forget();
        }

        private static async UniTaskVoid ChangeTask(Language cl)
        {
            //currentLanFonts.Clear();
            //var rtfm = await ResourceManager.LoadAssetAsync<TMPRunTimeFontMap>("TMPRunTimeFontMap", ResType.ScriptObject);
            //foreach (var kv in rtfm.localizationFontMap)
            //{
            //    if(kv.Value.TryGetValue(cl, out var str))
            //    {
            //        currentLanFonts[kv.Key] = await ResourceManager.LoadAssetAsync<TMP_FontAsset>($"ArtRes/UI/Fonts/{str}.asset", ResType.FullPath);
            //    }
            //}
            //await UniTask.DelayFrame(10);
            //GC.Collect();
        }

        //public static TMP_FontAsset GetTMP_FontAsset(int index)
        //{
        //    if (currentLanFonts.Count <= 0)
        //    {
        //        var rtfm = ResourceManager.LoadAsset<TMPRunTimeFontMap>("TMPRunTimeFontMap", ResType.ScriptObject);
        //        foreach (var kv in rtfm.localizationFontMap)
        //        {
        //            currentLanFonts[kv.Key] = ResourceManager.LoadAsset<TMP_FontAsset>($"ArtRes/UI/Fonts/{kv.Value[setting.selectedLanguage]}.asset", ResType.FullPath);
        //        }
        //    }
        //    return currentLanFonts[index];
        //}

#if UNITY_EDITOR
        public static void EditorInit()
        {
            m_setting = null;
            InitLoad(m_defaultKeyPath);
        }
        
        public static string GetNewKey(string kk, int ii)
        {
            var nk = kk + "_" + ii;
            if (setting.KeyExist(nk))
            {
                return GetNewKey(kk, ii + 1);
            }
            return nk;
        }
        
        public static void AddNewOne(string path, string key, string chineseStr)
        {
            var csvNewStr = $"{key},{chineseStr},";
            var pp = "Assets/Res/ScriptableObjects/LocalizationText/Generated/" + path + ".csv";
            Provider.Checkout(pp, CheckoutMode.Asset).Wait();
            FileStream fs = new FileStream(pp, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            var first = sr.ReadLine();
            int num = first.Split(',').Length - 3;
            sr.Close();
            for (int i = 1; i <= num; i++)
            {
                csvNewStr += ",";
            }
            fs.Close();
            fs = new FileStream(pp, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(csvNewStr);
            sw.Close();
            fs.Close();
            languageStrings[key] = new Dictionary<string, string>();
            languageStrings[key]["Chinese"] = chineseStr;
        }
#endif
        
        private static void InitLoad(string keyPath)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                m_setting = ResourceManager.LoadAsset<LocalizationSetting>(m_defaultKeyPath, ResType.ScriptObject);
            }
            else
            {
                m_setting = (LocalizationSetting)AssetDatabase.LoadAssetAtPath($"Assets/Res/ScriptableObjects/{keyPath}.asset", typeof(LocalizationSetting));
            }
#else
            m_setting = ResourceManager.LoadAsset<LocalizationSetting>(m_defaultKeyPath, ResType.ScriptObject);
#endif
            foreach (var il in localizeCash)
            {
                m_setting.AddOnLocalizeEvent(il);
            }
            localizeCash.Clear();
            languageStrings.Clear();
            currentLanFonts.Clear();
            _InitLanguage();
            ImportFromFiles(m_setting);
        }

        private static void ImportFromFiles(LocalizationSetting setting)
        {
            InputFiles.Clear();
            InputFiles.AddRange(setting.inputFiles);
            ImportInputFiles();
        }
        
        private static void ImportInputFiles()
        {
            for (var index = 0; index < InputFiles.Count; index++)
            {
                var inputAsset = InputFiles[index];

                if (inputAsset == null)
                {
                    LogUtils.Error("DefaultLocalizationSetting配置里第" + (index + 1) + "份文本源为空，请截图并联系文本老师。");
                    continue;
                }
                
                if (inputAsset.textAsset == null)
                {
                    LogUtils.Error("DefaultLocalizationSetting配置里第" + (index + 1) + "份文本源为空，请截图并联系文本老师。");
                    continue;
                }

                ImportTextFile(inputAsset.textAsset.text, inputAsset.format);
            }
            
            Punctuation_Colon = setting.Get("localization_Punctuation_Colon");
        }
        
        private static void ImportTextFile(string text, TextFileFormat format)
        {
            List<List<string>> rows;
            text = text.Replace("\r\n", "\n");
            if (text.EndsWith("\n"))
            {
                text = text.Remove(text.Length - 1, 1);
            }

            if (format == TextFileFormat.CSV)
            {
                rows = CsvReader.Ins.Parse(text);
            }
            else
            {
                rows = TxtReader.Ins.Parse(text);
            }
            //var canBegin = false;
            var lans = rows[0];
            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var key = row[0];
                
                if (string.IsNullOrEmpty(key) || IsLineBreak(key) || row.Count <= 1)
                {
                    continue;
                }
                // row.RemoveAt(0);
                //
                // if (languageStrings.ContainsKey(key))
                // {
                //     Debug.Log("The key '" + key + "' already exist, but is now overwritten");
                //     languageStrings[key] = row;
                //     continue;
                // }
                // languageStrings.Add(key, row);
                if (!languageStrings.ContainsKey(key))
                {
                    languageStrings[key] = new Dictionary<string, string>();
                }

                var dict = languageStrings[key];
                if (row.Count > lans.Count)
                {
                    LogUtils.Error($"检测到多语言文本配置数量大于语言数量，key ：{row[0]}");
                }
                
                var count = math.min(lans.Count, row.Count);
                for (int i = 1; i < count; i++)
                {
                    dict[lans[i]] = row[i];
                }
            }
        }

        public static void AddToSetting(ILocalize il)
        {
            localizeCash.Add(il);
        }

        public static Dictionary<string, string> GetLanguages(string key, List<Language> supportedLanguages = null)
        {
            if (languageStrings == null || languageStrings.Count == 0)
            {
                InitLoad(m_defaultKeyPath);
            }

            if (string.IsNullOrEmpty(key) || !languageStrings.ContainsKey(key))
            {
                return EmptyDict;
            }

            if (supportedLanguages == null || supportedLanguages.Count == 0)
            {
                return languageStrings[key];
            }

            // Filter the supported languages down to the supported ones
            var supportedLanguageStrings = new Dictionary<string, string>(supportedLanguages.Count);
            for (int index = 0; index < supportedLanguages.Count; index++)
            {
                var language = supportedLanguages[index].ToString();
                supportedLanguageStrings.Add(language,languageStrings[key][language]);
            }
            return supportedLanguageStrings;
        }

        public static List<string> GetKeys()
        {//////
            return languageStrings.Keys.ToList();
        }
        
        public static bool IsLineBreak(string currentString)
        {
            return currentString.Length == 1 && (currentString[0] == '\r' || currentString[0] == '\n')
                   || currentString.Length == 2 && currentString.Equals(Environment.NewLine);
        }
        
        public static Dictionary<string, Dictionary<string, string>> GetLanguagesStartsWith(string key)
        {
            if (languageStrings == null || languageStrings.Count == 0)
            {
                InitLoad(m_defaultKeyPath);
            }

            var multipleLanguageStrings = new Dictionary<string, Dictionary<string, string>>();
            foreach (var languageString in languageStrings)
            {
                if (languageString.Key.ToLower().StartsWith(key.ToLower()))
                {
                    multipleLanguageStrings.Add(languageString.Key, languageString.Value);
                }
            }

            return multipleLanguageStrings;
        }
        
        public static Dictionary<string, Dictionary<string, string>> GetLanguagesContains(string key)
        {
            if (languageStrings == null || languageStrings.Count == 0)
            {
                InitLoad(m_defaultKeyPath); 
            }

            var multipleLanguageStrings = new Dictionary<string, Dictionary<string, string>>();
            foreach (var languageString in languageStrings)
            {
                if (languageString.Key.ToLower().Contains(key.ToLower()))
                {
                    multipleLanguageStrings.Add(languageString.Key, languageString.Value);
                }
            }

            return multipleLanguageStrings;
        }

        public static void GetLanguagesContainsWorld(Dictionary<string, string> dict, string str, string language = "Chinese")
        {
            if (languageStrings == null || languageStrings.Count == 0)
            {
                InitLoad(m_defaultKeyPath); 
            }

            var ss = str.ToLower();
            dict.Clear();
            foreach (var kv in languageStrings)
            {
                if (kv.Value[language].ToLower().Contains(ss))
                {
                    dict[kv.Key] = kv.Value[language];
                }
            }
        }

        public static void GetLanguagesContains(string key, in Dictionary<string, Dictionary<string, string>> multipleLanguageStrings)
        {
            if (languageStrings == null || languageStrings.Count == 0)
            {
                InitLoad(m_defaultKeyPath);     
            }

            if (multipleLanguageStrings != null)
            {
                foreach (var languageString in languageStrings)
                {
                    if (languageString.Key.ToLower().Contains(key.ToLower()))
                    {
                        multipleLanguageStrings.Add(languageString.Key, languageString.Value);
                    }
                }
            }
        }
        
        public static void Refresh()
        {
            Initialize();
            if (m_setting != null)
            {
                m_setting.InvokeOnLocalize();
            }
        }

#if UNITY_EDITOR
        public static HashSet<char> GetAllCharacter(Language lan)
        {
            HashSet<char> all = new HashSet<char>();
            if (languageStrings == null || languageStrings.Count == 0)
            {
                InitLoad(m_defaultKeyPath);
            }
            var language = lan.ToString();
            foreach (var kv in languageStrings)
            {
                var str = kv.Value[language];
                foreach(var c in str)
                {
                    all.Add(c);
                }
            }
            return all;
        }

        public static void AddSubtitleCharacter(Language lan, HashSet<char> all)
        {
            var list = new List<DefaultAsset>();
            var path = "Assets/StreamingAssets/Subtitle";
            Tools.FileUtils.FindAssetInFolder(path, list);
            var lanStr = "_" + lan;
            foreach (var asset in list)
            {
                if(asset.name.EndsWith(lanStr))
                {
                    string[] strs = File.ReadAllLines(path + "/" + asset.name);
                    foreach (var str in strs)
                    {
                        foreach (var c in str)
                        {
                            all.Add(c);
                        }
                    }
                }
            }
        }
#endif
    }
}