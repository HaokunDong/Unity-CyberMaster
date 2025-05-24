using System;
using System.Collections.Generic;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using EverlastingEditor.Base;
using UnityEngine;
using Everlasting.Extend;
//using Zstandard.Net;

namespace EverlastingEditor.Config.Export
{
	public interface IExporter
	{
		void Export();
	}
	
    public abstract class Exporter : IExporter
    {
	    protected class ExtractedData
	    {
		    public List<IData[]> ConfigDataList;
		    public List<SettingEntry> SettingEntries;
	    }
	    
		public string Name { get; }
        protected string FilePath { get; }
	    protected string CsOutputPath { get; }
	    protected string BinaryOutputPath { get; }
	    protected ExcelPackage ExcelPackage { get; }
	    protected DataView DataView { get; }
	    protected ConfigBinary Binary { get; }
	    private ExtractedData m_extractedData;
        private readonly HashSet<uint> m_tableIdLookupTable = new HashSet<uint>();

        protected FileInfo CopyFile(string filePath)
	    {
		    var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath));
		    File.Copy(filePath, tempPath, true);
		    return new FileInfo(tempPath) {IsReadOnly = false};
	    }
	    
	    protected Exporter(string name, string filePath, string csOutputPath, string binaryOutputPath)
        {
            FilePath = filePath;
            CsOutputPath = csOutputPath;
	        BinaryOutputPath = binaryOutputPath;
	        Name = name;

			ExcelPackage = new ExcelPackage(CopyFile(filePath));
	        DataView = new DataView(name, filePath, ExcelPackage);
	        Binary = new ConfigBinary($"{binaryOutputPath}{Name.ToLowerInvariant()}Table.bytes");
		}

        protected Exporter()
        {
            
        }

        public virtual void Export()
	    {
		    var extractedData = GetExtractedData();
		    var (data, settingEntries) = (extractedData.ConfigDataList, extractedData.SettingEntries);
            AssertTableIdNoDuplicate(data);
            Binary.WriteBinary(data, settingEntries);
		    WriteTemplate(data, settingEntries);
	    }

        private void AssertTableIdNoDuplicate(List<IData[]> configDataList, string tag = "")
        {
            if (configDataList == null)
            {
                return;
            }
            
            foreach (var configData in configDataList)
            {
                foreach (var data in configData)
                {
                    if (data is IdData idData)
                    {
                        var id = (uint)idData.Object;
                        if (m_tableIdLookupTable.Contains(id))
                        {
                            throw new Exception($"{tag}存在重复的Id值: {id}");
                        }

                        m_tableIdLookupTable.Add(id);
                        break;
                    }
                }
            }
            
            m_tableIdLookupTable.Clear();
        }

	    public void MergeTo(Exporter targetExporter)
	    {
		    var (srcExtractedData, targetExtractedData) = (GetExtractedData(), targetExporter.GetExtractedData());
		    if (srcExtractedData == null || targetExtractedData == null)
		    {
			    throw new Exception($"合并表格 {Name} -> {targetExporter.Name} 失败，源表或目的表中存在空表");
		    }
		    
		    var (srcConfigDataList, srcSettingEntries) = (srcExtractedData.ConfigDataList, srcExtractedData.SettingEntries);
		    var (targetConfigDataList, targetSettingEntries) = (targetExtractedData.ConfigDataList, targetExtractedData.SettingEntries);
		    
		    //合并data sheet
		    if (DataView.ColumnHeaders.Count != targetExporter.DataView.ColumnHeaders.Count)
		    {
			    throw new Exception($"合并表格 {Name} -> {targetExporter.Name} 失败，配置列数不相等");
		    }

		    for (var i = 0; i < DataView.ColumnHeaders.Count; i++)
		    {
			    var (srcColumn, targetColumn) = (DataView.ColumnHeaders[i], targetExporter.DataView.ColumnHeaders[i]);
			    if (srcColumn.ColumnType.TypeName != targetColumn.ColumnType.TypeName || srcColumn.ColumnTypeString != targetColumn.ColumnTypeString)
			    {
				    throw new Exception($"合并表格 {Name} -> {targetExporter.Name} 失败，源表 {Name} 第{Utils.ColumnNum2Label(srcColumn.ColumnNum)}列类型{srcColumn.ColumnTypeString}与目的表类型{targetColumn.ColumnTypeString}不一致");
			    }
		    }
		    
		    targetConfigDataList.AddRange(srcConfigDataList);
            AssertTableIdNoDuplicate(targetConfigDataList, $"合并表格{Name}失败：");

		    //合并setting sheet
		    if (srcSettingEntries != null)
		    {
			    targetExtractedData.SettingEntries ??= new List<SettingEntry>();
			    targetExtractedData.SettingEntries.AddRange(srcSettingEntries);
		    }
	    }

	    protected ExtractedData GetExtractedData()
	    {
		    if (m_extractedData == null)
		    {
			    DataView.ReadMainHeader();//读取表格头
			    var data = DataView.ReadMainData();
			    var settingEntries = DataView.ReadSettingData();
			    m_extractedData = new ExtractedData { ConfigDataList = data, SettingEntries = settingEntries, };
		    }

		    return m_extractedData;
	    }

	    protected abstract class AConfigBinary : IConfigBinary
		{
			public AlignedUnsafeMemoryStreamWriter Stream { get; } = new AlignedUnsafeMemoryStreamWriter();
			public void Write(int i)
			{
				Stream.Write(i);
			}

			public void Write(uint i)
			{
				Stream.Write(i);
			}

			public void Write(long l)
			{
				Stream.Write(l);
			}

			public void Write(ulong l)
			{
				Stream.Write(l);
			}

			public void Write(bool b)
			{
				Stream.Write(b);
			}

			public void Write(float f)
			{
				Stream.Write(f);
			}

			public void Write(string s)
			{
				Stream.Write(s);
			}

			public abstract Pool GetPool(IPooledColumnType columnType);
		}

	    protected class SubConfigBinary : AConfigBinary
	    {
		    private AConfigBinary Parent { get; }

		    public SubConfigBinary(AConfigBinary parent)
		    {
			    Parent = parent;
		    }

		    public override Pool GetPool(IPooledColumnType columnType)
		    {
			    return Parent.GetPool(columnType);
		    }
	    }

	    protected static bool IsPrimitivePooledColumnType(IPooledColumnType columnType)
	    {
		    return columnType is IPrimitiveColumnType ||
		            (columnType as IArrayColumnType)?.BaseType is IPrimitiveColumnType;
	    }

	    protected class ConfigBinary : AConfigBinary
	    {
		    private const int MAGIC = (byte) 'E' | ((byte) 'V' << 8) | ((byte) 'L' << 16) | ('C' << 24);
		    private readonly string outFile;
			private List<KeyValuePair<IPooledColumnType, Pool>> pools = new List<KeyValuePair<IPooledColumnType, Pool>>();
			private readonly Dictionary<string, Pool> poolsDic = new Dictionary<string, Pool>();

		    public ConfigBinary(string outFile)
		    {
			    this.outFile = outFile;
		    }

		    public override Pool GetPool(IPooledColumnType columnType)
		    {
			    Pool pool;
			    if (!poolsDic.TryGetValue(columnType.TypeName, out pool))
			    {
					pool = new Pool();
					pools.Add(new KeyValuePair<IPooledColumnType, Pool>(columnType, pool));
					poolsDic.Add(columnType.TypeName, pool);
			    }

			    return pool;
		    }

		    public List<KeyValuePair<IPooledColumnType, Pool>> Pools => pools;

			public void WriteBinary(List<IData[]> data, List<SettingEntry> settingEntries)
		    {
				//将数据写入mainchunk，确保各种Pool构造完毕
				if (data != null)
				{
					this.Write(data.Count);
					foreach (var datase in data)
					{
						foreach (var data1 in datase)
						{
							data1.WriteToBinary(this);
						}
					}
				}
				if (settingEntries != null)
				{
					foreach (var entry in settingEntries)
					{
						entry.Data.WriteToBinary(this);
					}
				}
				var tmpBinary = new SubConfigBinary(this);
				FileInfo fi = new FileInfo(outFile);
				fi.Directory.Create();
				using (var fs = fi.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			    {
					fs.Write(MAGIC);
					byte[] bytes;
					using (var ms = new MemoryStream())
					{
						//根据嵌套等级升序排列
						pools = pools.OrderBy(pair => pair.Key.ArrayLevel).ToList();

						//先写入Primitive Pool数据
						foreach (var keyValuePair in pools)
						{
							if (IsPrimitivePooledColumnType(keyValuePair.Key))
							{
								var pool = keyValuePair.Value;
								ms.Write(pool.Count);
								foreach (var data2 in pool)
								{
									data2.WriteToPool(tmpBinary);
									tmpBinary.Stream.WriteToStream(ms);
									tmpBinary.Stream.Clear();
								}
							}
						}

						//再先写入非Primitive Pool数据
						foreach (var keyValuePair in pools)
						{
							if (!IsPrimitivePooledColumnType(keyValuePair.Key))
							{
								var pool = keyValuePair.Value;
								ms.Write(pool.Count);
								foreach (var data2 in pool)
								{
									data2.WriteToPool(tmpBinary);
									tmpBinary.Stream.WriteToStream(ms);
									tmpBinary.Stream.Clear();
								}
							}
						}

						//再写入mainchunk
						Stream.WriteToStream(ms);
						bytes = ms.ToArray();
					}

					// byte[] compressedBytes;
					// using (var ms = new MemoryStream())
					// {
					// 	using (var zstd = new ZstandardStream(ms, ZstandardStream.MaxCompressionLevel))
					// 	{
					// 		zstd.Write(bytes);
					// 	}
					//
					// 	compressedBytes = ms.ToArray();
					// }

					int length = bytes.Length;
					int key = length ^ 0x6BF08C13;
					byte[] keyBin = BitConverter.GetBytes(key);
					for (int i = 0; i < bytes.Length; i++)
					{
						byte k = keyBin[i & 3];
						bytes[i] ^= k;
					}

					fs.Write(length);
					fs.Write(bytes);
					fs.Close();
			    }
		    }
	    }

	    protected abstract void WriteTemplate(List<IData[]> data, List<SettingEntry> settingEntries);
	}
}
