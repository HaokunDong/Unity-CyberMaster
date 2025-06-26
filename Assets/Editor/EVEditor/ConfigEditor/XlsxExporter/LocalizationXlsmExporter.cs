using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Everlasting.Extend;
using EverlastingEditor.Utils;
using Localization;
using Localization.Enum;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using FileMode = System.IO.FileMode;
using LocalizationAsset = Localization.LocalizationAsset;

namespace EverlastingEditor.Config.Export
{
    public class LocalizationXlsmExporter : Exporter
    {
        private const string LOCALIZATION_SETTING_RELATIVE_PATH = "Assets/Res/ScriptableObjects/LocalizationSetting/DefaultLocalizationSetting.asset";
        public static readonly string LocalizationSettingFullPath = Application.dataPath + LOCALIZATION_SETTING_RELATIVE_PATH.Substring("Assets".Length);
        private const string TEMP_XLSX_FILE_SUFFIX = "_csvSrcXlsxTemp";
        private readonly string m_fileName;
        private readonly string m_filePath;
        private readonly string m_csvOutputPath;
        //private LocalizationSpeakerSetting m_setting = null;
        public bool generateIntermediateExcel = false;

        public override void Export()
        {
            //var startDateTime = DateTime.Now;
            using var rawExcel = new ExcelPackage(CopyFile(m_filePath));
            //Debug.Log($"{m_fileName} 打开excel完毕, 累计已经消耗{DateTime.Now.Subtract(startDateTime).TotalSeconds}s.");
            
            var (columns, rowCount) = ExtractValidRowsFromRawExcelFile(rawExcel, m_filePath);
            //Debug.Log( $"{m_fileName} 提取列数据完毕，累计已经消耗{DateTime.Now.Subtract(startDateTime).TotalSeconds}s.");
            
            var contents = BuildOutputContentByAggregateColumns(columns, rowCount, m_fileName, generateIntermediateExcel);
            //Debug.Log( $"{m_fileName} 生成临时excel完毕，累计已经消耗{DateTime.Now.Subtract(startDateTime).TotalSeconds}s.");
            
            WriteToCsvFile(m_fileName, contents, m_csvOutputPath);
            //Debug.Log( $"{m_fileName} 生成csv文件完毕，累计已经消耗{DateTime.Now.Subtract(startDateTime).TotalSeconds}s.");
        }

        protected override void WriteTemplate(List<IData[]> data, List<SettingEntry> settingEntries)
        {
        }

        public LocalizationXlsmExporter(string name, string filePath, string csvOutputPath/*, LocalizationSpeakerSetting setting*/)
        {
            (m_fileName, m_filePath, m_csvOutputPath/*, m_setting*/) = (name, filePath, csvOutputPath/*, setting*/);
        }
        
        private (List<SheetView.Column>, int) ExtractValidRowsFromRawExcelFile(ExcelPackage excelPackage, string filePath)
        {
            var rowCount = -1;
            var columns = new List<SheetView.Column>();
            var headerNames = new HashSet<string>();
            var workSheets = excelPackage.Workbook.Worksheets;
            foreach (var workSheet in workSheets)
            {
                if (workSheet.Name.StartsWith("~"))
                {
                    continue;
                }
                
                var sheetView = new ExcelSheetView(workSheet, filePath);
                rowCount = Math.Max(rowCount, sheetView.MaxRow);
                for (var i = 1; i <= sheetView.MaxColumn; i++)
                {
                    var column = sheetView.GetColumn(i);
                    if (!column.IsCommentColumn() && !headerNames.Contains(column[1]))
                    {
                        columns.Add(column);
                        headerNames.Add(column[1]);
                    }
                }

                //if (workSheet.Name.Equals("Chinese"))
                //{
                //    if (m_setting != null)
                //    {
                //        for (var i = 2; i <= sheetView.MaxRow; i++)
                //        {
                //            //Debug.LogError(sheetView[i, 3].IsNullOrEmpty());
                //            if (!sheetView[i, 3].IsNullOrEmpty())
                //            {
                //                m_setting.SaveSpeaker(sheetView[i, 1], sheetView[i, 3], sheetView[i, 2]);
                //            }
                //            //Debug.LogError(workSheet.Cells[i, 1].Style.Fill.BackgroundColor.Indexed);
                //            if (workSheet.Cells[i, 1].Style.Fill.BackgroundColor.Indexed > 0)
                //            {
                //                m_setting.SaveChange(sheetView[i, 1]);
                //            }
                //        }
                //    }
                //}
            }

            return (columns, rowCount);
        }

        private List<string[]> BuildOutputContentByAggregateColumns(List<SheetView.Column> columns, int rowCount, string fileName, bool genDebugFile = false)
        {
            var contents = new List<string[]>(rowCount);
            contents.Resize(rowCount);
            for (var i = 0; i < rowCount; i++)
            {
                contents[i] = new string[columns.Count];
            }

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                for (var row = 1; row <= rowCount; row++)
                {
                    contents[row - 1][i] = column[row];
                }
            }

            TrimContent(ref contents);
            if (genDebugFile)
            {      
                using var intermediateExcel = new ExcelPackage();
                var workSheet = intermediateExcel.Workbook.Worksheets.Add("data");
                workSheet.Cells.LoadFromArrays(contents);
                var tempPath = Path.Combine(Path.GetTempPath(), fileName + TEMP_XLSX_FILE_SUFFIX + ".xlsx");
                File.Delete(tempPath);
                intermediateExcel.SaveAs(new FileInfo(tempPath));
            }

            return contents;
        }

        private static void TrimContent(ref List<string[]> contents)
        {
            Func<string, bool> strNullOrEmpty = string.IsNullOrEmpty;
            for (var i = contents.Count - 1; i >= 0; i--)
            {
                var isEmptyLine = contents[i].All(strNullOrEmpty);
                if (isEmptyLine)
                {
                    contents.RemoveAt(i);
                }
            }
        }
        
        private static void WriteToCsvFile(string fileName, List<string[]> content, string outFilePath)
        {
            var directoryName = Path.GetDirectoryName(outFilePath);
            if (directoryName != null && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            using var file = new FileStream(outFilePath, FileMode.Create, FileAccess.Write);
            using var sw = new StreamWriter(file, Encoding.UTF8);
            using var csvWriter = new CsvWriter(sw, CultureInfo.InvariantCulture);
            foreach (var line in content)
            {
                foreach (var field in line)
                {
                    csvWriter.WriteField(field);
                }

                csvWriter.NextRecord();
            }
        }

        public static void MapLocalizationSetting()
        {
            var localizationSetting = AssetDatabase.LoadAssetAtPath<LocalizationSetting>(LOCALIZATION_SETTING_RELATIVE_PATH);
            localizationSetting.inputFiles.Clear();

            var csvFilePaths = EditorUtils.GetFiles(ExcelExportConfig.CsvOutputPath, "*.csv");
            foreach (var csvFilePath in csvFilePaths)
            {
                var csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets" + csvFilePath.Substring(Application.dataPath.Length));
                var localizationAsset = new LocalizationAsset { textAsset = csvFile, format = TextFileFormat.CSV };
                localizationSetting.inputFiles.Add(localizationAsset);
            }

            EditorUtility.SetDirty(localizationSetting);
            AssetDatabase.SaveAssets();
        }
    }
}
