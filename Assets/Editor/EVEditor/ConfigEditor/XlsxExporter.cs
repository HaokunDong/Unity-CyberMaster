using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using EverlastingEditor.Config.Export;
using GameBase.Log;
using Localization;
using Tools.Editor;
using UnityEditor;
using UnityEditor.VersionControl;

namespace EverlastingEditor
{
	class XlsxExporter
    {
        // class XlsxPostprocessor : AssetPostprocessor
        // {
        //  static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        //   string[] movedFromAssetPaths)
        //  {
        //   List<string> stringList = ListPool<string>.Fetch();
        //   foreach (var asset in importedAssets)
        //   {
        //    if (asset.StartsWith("Assets/Config/") && asset.EndsWith(".xlsx") && !asset.Contains("~"))
        //    {
        //     stringList.Add(asset);
        //    }
        //   }
        //
        //   if (stringList.Count != 0)
        //   {
        //    ExportOnes(stringList.ToArray());
        //    ExportLoader();
        //    AssetDatabase.Refresh();
        //   }
        //   ListPool<string>.Release(stringList);
        //  }
        // }

        private static List<string> errorLogs = new List<string>();
        private static object lockObj = new object();


        private static void ClearFolder(string folderPath)
	    {
		    var di = new DirectoryInfo(folderPath);
		    foreach (var file in di.GetFiles())
		    {
			    file.IsReadOnly = false;
			    file.Delete(); 
		    }
	    }

		[MenuItem("Tools/Language/导出Excel多语言数据源至CSV文件", false, 1)]
		private static void ExportLocalizationCsv()
		{
			errorLogs.Clear();
            var excelExportConfig = ExcelExportConfig.BuildConfig(Path.Combine(ExcelExportConfig.XlsxPath, "Config.txt"));
			if (excelExportConfig == null)
			{
				LogUtils.Error("缺少打表工具配置文件");
				return;
			}

			var assets = new AssetList { new Asset(ExcelExportConfig.CsvOutputPath), new Asset(LocalizationXlsmExporter.LocalizationSettingFullPath) };
			if (Provider.enabled)
			{
				Provider.Checkout(assets, CheckoutMode.Asset).Wait();
			}

			ClearFolder(ExcelExportConfig.CsvOutputPath);
			var exportResult = true;
			var (_, _, localizationTables) = excelExportConfig.GetFilesToBeProcessed();
			//var setting = AssetDatabase.LoadAssetAtPath<LocalizationSpeakerSetting>(LocalizationSpeakerSetting.SETTING_PATH);
			//setting.Clear();
			Parallel.ForEach(localizationTables, file =>
			{
				var name = Path.GetFileNameWithoutExtension(file);
				name = char.ToUpper(name[0]) + name.Substring(1);
				try
				{
					var relativePath = EditorFileUtils.GetRelativePath(file, excelExportConfig.localizationTableXlsmFilePath);
					var csvOutputPath = Path.Combine(ExcelExportConfig.CsvOutputPath, relativePath).Replace(".xlsm", ".csv");
					new LocalizationXlsmExporter(name, file, csvOutputPath/*, setting*/).Export();
				}
				catch (Exception e)
				{
					lock (lockObj)
					{
                        errorLogs.Add($"处理{name}表出现异常： {e}");
					}
					exportResult = false;
				}
			});
			//EditorUtility.SetDirty(setting);

			if (!exportResult)
			{
                foreach (var log in errorLogs)
                {
                    LogUtils.Error(log);
                }
                ShowExportFailedPrompt();
				return;
			}

			AssetDatabase.Refresh();
			if (Provider.enabled)
			{
				Provider.Revert(assets, RevertMode.Unchanged).Wait();
			}

			LocalizationXlsmExporter.MapLocalizationSetting();
			LocalizationManager.EditorInit();
            LogUtils.Debug("多语言表格导出结束");
		}

		[MenuItem("Tools/Excel配置导出")]
	    private static void Export()
	    {
            errorLogs.Clear();
            var outputPath = ExcelExportConfig.ConfigOutputPath;
		    var binaryOutputPath = ExcelExportConfig.BinaryOutputPath;
		    
		    var excelExportConfig = ExcelExportConfig.BuildConfig(Path.Combine(ExcelExportConfig.XlsxPath, "Config.txt"));
		    if (excelExportConfig == null)
		    {
                LogUtils.Error("缺少打表工具配置文件");
			    return;
		    }
		    
		    var assets = new AssetList {new Asset(outputPath), new Asset(binaryOutputPath)};
		    if (Provider.enabled)
		    {
			    Provider.GetLatest(assets).Wait();
			    Provider.Checkout(assets, CheckoutMode.Asset).Wait();
		    }
		    
		    ClearFolder(outputPath);
		    ClearFolder(binaryOutputPath);

            var exportResult = true;
		    var names = new ConcurrentBag<string>();
		    var (singleTables, compositeTables, _) = excelExportConfig.GetFilesToBeProcessed();
		    Parallel.ForEach(singleTables, file =>
		    {
			    var name = Path.GetFileNameWithoutExtension(file);
			    name = char.ToUpper(name[0]) + name.Substring(1);
                try
                {
                    new CsExporter(name, file, outputPath, binaryOutputPath).Export();
                    names.Add(name);
                }
                catch (Exception e)
                {
                    lock (lockObj)
                    {
                        errorLogs.Add($"处理{name}表出现异常： {e}");
                    }
                    exportResult = false;
                }
            });
		    
		    Parallel.ForEach(compositeTables, pair =>
		    {
			    var (name, memberFileList) = (pair.Key, pair.Value);
			    name = char.ToUpper(name[0]) + name.Substring(1);
			    try
			    {
				    new CompositedCsExporter(name, memberFileList, outputPath, binaryOutputPath).Export();
				    names.Add(name);
			    }
			    catch (Exception e)
			    {
                    lock (lockObj)
                    {
                        errorLogs.Add($"处理{name}表出现异常： {e}");
                    }
                    exportResult = false;
                }
		    });

            if (!exportResult)
            {
                foreach (var log in errorLogs)
                {
                    LogUtils.Error(log);
                }
                ShowExportFailedPrompt();
                return;
            }
		    
		    new Loader(names.ToArray(), outputPath).Export();
            LogUtils.Debug("表格导出结束");
		    
		    AssetDatabase.Refresh();
		    if (Provider.enabled)
		    {
			    Provider.Revert(assets, RevertMode.Unchanged).Wait();
		    }
	    }
        
        [Conditional("UNITY_EDITOR")]
        private static void ShowExportFailedPrompt()
        {
            EditorUtility.DisplayDialog("Error", "表格导出失败，详情请查看控制台报错", "确定");
        }

	    // static void ExportOne(string file)
	    // {
		   //  string outputPath = Define.ConfigOutputPath;
		   //  string binaryOutputPath = Define.BinaryOutputPath;
		   //  string name = Path.GetFileNameWithoutExtension(file);
		   //  new CsExporter(name, file, outputPath, binaryOutputPath)
			  //   .Export();
		   //  AssetDatabase.Refresh();
	    // }
	    //
	    // static void ExportOnes(string[] files)
	    // {
		   //  string outputPath = Define.ConfigOutputPath;
		   //  string binaryOutputPath = Define.BinaryOutputPath;
		   //  Parallel.ForEach(files, (file) =>
		   //  {
			  //   string name = Path.GetFileNameWithoutExtension(file);
			  //   new CsExporter(name, file, outputPath, binaryOutputPath)
				 //    .Export();
		   //  });
		   //  AssetDatabase.Refresh();
	    // }
	    //
	    // static void ExportLoader()
	    // {
		   //  string outputPath = Define.ConfigOutputPath;
		   //  var files = EditorUtils.GetFiles("Assets/Config", "*.xlsx").Where(a => !a.Contains("~$"));
		   //  List<string> names = new List<string>();
		   //  foreach (var file in files)
		   //  {
			  //   string name = Path.GetFileNameWithoutExtension(file);
			  //   names.Add(name);
		   //  }
		   //  new Loader(names.ToArray(), outputPath).Export();
		   //  AssetDatabase.Refresh();
	    // }
    }
}
