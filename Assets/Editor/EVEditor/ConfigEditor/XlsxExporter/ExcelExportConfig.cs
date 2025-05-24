using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Everlasting.Extend;
using EverlastingEditor.Utils;
using UnityEngine;

namespace EverlastingEditor
{
    public class ExcelExportConfig
    {
        public static readonly string XlsxPath = Application.dataPath + "/../Other/Config/Excel/";
        public static readonly string ConfigOutputPath = Application.dataPath + "/Scripts/Table/Generated/";
        public static readonly string BinaryOutputPath = Application.streamingAssetsPath + "/Excel/";
        public static readonly string CsvOutputPath = Application.dataPath + "/Res/ScriptableObjects/LocalizationText/Generated/";
        public string localizationTableXlsmFilePath => m_localizationTableFullPath;
        
        private enum ParseType
        {
            None,
            CompositeTable,//合并表输出
            ExcludedTable,//不需要处理的表
            LocalizationTable,//本地化表
        }
        
        private const string COMPOSITE_TABLE_CONFIG_IDENTIFIER = "@composite";
        private const string EXCLUDED_TABLE_CONFIG_IDENTIFIER = "@excluded";
        private const string LOCALIZATION_TABLE_PATH_IDENTIFIER = "@LocalizationTablePath";
        private const string CompositeTableConfigSplitSign = "->";
        private const char CompositeTableMemberSplitSign = ',';
        private HashSet<string> m_singleTables = new HashSet<string>();
        private HashSet<string> m_localizationTables = new HashSet<string>();
        private Dictionary<string, List<string>> m_compositeTables = new Dictionary<string, List<string>>();
        private string m_localizationTableFullPath;
        
        private Dictionary<string, string> m_compositeTableRecord = new Dictionary<string, string>();
        private readonly HashSet<string> m_excludedTables = new HashSet<string>();

        public static ExcelExportConfig BuildConfig(string path)
        {
            var config = new ExcelExportConfig();
            if (!File.Exists(path))
            {
                Debug.LogError("打表配置文件不存在");
                return null;
            }

            config.ParseConfigFile(File.ReadAllLines(path));
            return config;
        }

        /// <summary>
        /// 获取待处理的表
        /// </summary>
        /// <returns>(单一表，复合表, 多语言表)</returns>
        public (HashSet<string>, Dictionary<string, List<string>>, HashSet<string>) GetFilesToBeProcessed()
        {
            var files = EditorUtils.GetFiles(XlsxPath, "*.xlsx").Where(a => !a.Contains("~$"));
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                if (m_excludedTables.Contains(fileName))
                {
                    continue;
                }
                
                if (m_compositeTableRecord.TryGetValue(fileName, out var compositeTableName))
                {
                    if (!m_compositeTables.TryGetValue(compositeTableName, out var memberList))
                    {
                        memberList = new List<string>();
                        m_compositeTables.Add(compositeTableName, memberList);
                    }
                    
                    memberList.Add(file);
                }
                else
                {
                    m_singleTables.Add(file);
                }
            }

            files = EditorUtils.GetFiles(m_localizationTableFullPath, "*.xlsm").Where(a => !a.Contains("~$"));
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                if (m_excludedTables.Contains(fileName))
                {
                    continue;
                }
                
                m_localizationTables.Add(file);
            }
            
            return (m_singleTables, m_compositeTables, m_localizationTables);
        }
        
        private void ParseConfigFile(string[] lines)
        {
            ParseType parseType = ParseType.None;
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//") || trimmedLine.StartsWith("##"))
                {
                    continue;
                }
        
                if (trimmedLine.StartsWith(COMPOSITE_TABLE_CONFIG_IDENTIFIER))
                {
                    parseType = ParseType.CompositeTable;
                }
                else if (trimmedLine.StartsWith(EXCLUDED_TABLE_CONFIG_IDENTIFIER))
                {
                    parseType = ParseType.ExcludedTable;
                }
                else if (trimmedLine.StartsWith(LOCALIZATION_TABLE_PATH_IDENTIFIER))
                {
                    parseType = ParseType.LocalizationTable;
                }
                else
                {
                    switch (parseType)
                    {
                        case ParseType.CompositeTable:
                        {
                            ParseCompositeTableConfig(line);
                            break;
                        }
                        case ParseType.ExcludedTable:
                        {
                            ParseExcludedTableConfig(line);
                            break;
                        }
                        case ParseType.LocalizationTable:
                        {
                            ParseLocalizationTableConfig(line);
                            break;
                        }
                        default:
                            Debug.LogError("配置格式错误，缺少解析类型");
                            break;
                    }
                }
            }
        }

        private void ParseCompositeTableConfig(string line)
        {
            var splitArray = line.Split(new []{CompositeTableConfigSplitSign}, StringSplitOptions.RemoveEmptyEntries);
            if (splitArray.Length != 2)
            {
                Debug.LogError($"配置格式错误：{line}");
                return;
            }

            var (members, compositeTableName) = (splitArray[0].Trim(), splitArray[1].Trim());
            var memberTableNames = members.Split(CompositeTableMemberSplitSign).Select(tableName => tableName.Trim().ToLower()).ToList();
            if (memberTableNames.Count == 0)
            {
                Debug.LogError($"配置格式错误：{line}");
                return;
            }

            foreach (var memberTableName in memberTableNames)
            {
                m_compositeTableRecord[memberTableName] = compositeTableName;
            }
        }

        private void ParseExcludedTableConfig(string line)
        {
            m_excludedTables.Add(line.Trim().ToLower());
        }

        private void ParseLocalizationTableConfig(string line)
        {
            m_localizationTableFullPath = Path.Combine(XlsxPath, line.Trim());
        }
    }
}