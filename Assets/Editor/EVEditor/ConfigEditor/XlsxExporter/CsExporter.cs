using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EverlastingEditor.Base;
using JetBrains.Annotations;
using UnityEngine;

namespace EverlastingEditor.Config.Export
{
	public class CsExporter : Exporter
	{
		public CsExporter(string name, string filePath, string csOutputPath, string binaryOutputPath)
			: base(name, filePath, csOutputPath, binaryOutputPath)
		{
		}

		protected override void WriteTemplate(List<IData[]> data, List<SettingEntry> settingEntries)
		{
			WriteCsTemplate(data, settingEntries);
		}
		
		private void WriteCsTemplate(List<IData[]> data, List<SettingEntry> settingEntries)
		{
			using (var sb = new AutoIndentedTextWriter())
			{
				WriteCsTemplate(sb, data, settingEntries);
				FileInfo fi = new FileInfo($"{CsOutputPath}{Name}Table.cs");
				fi.Directory.Create();
				File.WriteAllText(fi.FullName, sb.ToString(), Encoding.UTF8);
			}
		}

		private static readonly Dictionary<Type, string> primitiveTypeStrings = new Dictionary<Type, string>()
		{
			{ typeof(int), "binary.ReadInt()"},
			{ typeof(uint), "binary.ReadUInt()"},
			{ typeof(long), "binary.ReadLong()"},
			{ typeof(ulong), "binary.ReadULong()"},
			{ typeof(bool), "binary.ReadBool()"},
			{ typeof(float), "binary.ReadFloat()"},
			{ typeof(string), "binary.ReadString()"},
			{ typeof(Vector3), "new Vector3(binary.ReadFloat(),binary.ReadFloat(),binary.ReadFloat())"},
			{ typeof(Vector2), "new Vector2(binary.ReadFloat(),binary.ReadFloat())"},
			{ typeof(Color), "ColorUtils.IntToColor(binary.ReadInt())"},
			{ typeof(DateTime), "DateTimeOffset.FromUnixTimeSeconds(binary.ReadInt()).DateTime"},

		};

		private string GetReadFromPrimitiveTypeString(Type primitiveType)
		{
			string str;
			return primitiveTypeStrings.TryGetValue(primitiveType, out str) ? str : null;
		}

		private string GetReadFromPrimitiveTypeString(IPrimitiveColumnType type)
		{
			string str = GetReadFromPrimitiveTypeString(type.PrimitiveType);
			if (str != null)
				return str;
			if (type.PrimitiveType == typeof(Enum))
			{
				return $"({type.TypeName})binary.ReadInt()";
			}

			throw new Exception($"找不到Primitive {type.PrimitiveType.Name} 的Reader");
		}

		private string GetReadFromPoolString(IPooledColumnType type)
		{
			var primitiveColumnType = type as IPrimitiveColumnType;
			if (primitiveColumnType != null)
			{
				return GetReadFromPrimitiveTypeString(primitiveColumnType);
			}
			var arrayColumnType = type as IArrayColumnType;
			if (arrayColumnType != null)
			{
				return $"Load{NormalizeName(arrayColumnType.TypeName.Replace("[]", "Array"))}(binary)";
			}
			var customColumnType = type as ICustomColumnType;
			if (customColumnType != null)
			{
				return $"Load{NormalizeName(customColumnType.TypeName)}(binary)";
			}
			throw new Exception();
		}

		private string NormalizeName(string name)
		{
			char head = name[0];
			if (char.IsUpper(head))
			{
				return name;
			}

			return char.ToUpper(head) + name.Substring(1, name.Length - 1);
		}

		private string NewArray(string typeName)
		{
			var sig = typeName.IndexOf('[');
			if (sig >= 0)
			{
				return $"{typeName.Substring(0, sig)}[count]{typeName.Substring(sig)}";
			}
			else
			{
				return $"{typeName}[count]";
			}
		}

		private string GetPoolName(IPooledColumnType type)
		{
			var arrayColumnType = type as IArrayColumnType;
			if (arrayColumnType != null)
			{
				return $"pool{NormalizeName(arrayColumnType.TypeName.Replace("[]", "Array"))}";
			}

			return $"pool{NormalizeName(type.TypeName)}";
		}

		private string GetReadFromBinaryString(IColumnType type)
		{
			var pooledColumnType = type as IPooledColumnType;
			if (pooledColumnType != null)
			{
				return $"{GetPoolName(pooledColumnType)}[binary.ReadInt()]";
			}
			var primitiveColumnType = type as IPrimitiveColumnType;
			if (primitiveColumnType != null)
			{
				return GetReadFromPrimitiveTypeString(primitiveColumnType);
			}
			throw new Exception();
		}

		private void WriteCsTemplate(IndentedTextWriter sw, List<IData[]> dataList, List<SettingEntry> settingEntries)
		{
			// 先找出所有自定义class定义
			var classes =
				DataView.ColumnTypeParser.CustomTypes
					.OfType<ICustomColumnType>().Select(@class =>
					new KeyValuePair<string, List<KeyValuePair<IColumnType, string>>>(@class.TypeName, @class.Fields)).ToList();
			var enums =
				DataView.ColumnTypeParser.CustomTypes
					.OfType<IEnumColumnType>().Select(@enum =>
					new KeyValuePair<string, List<KeyValuePair<string, int>>>(@enum.TypeName, @enum.Fields)).ToList();
			
			//检测是否含有组合key的情况
			var combinedKeyFieldTuples = DataView.ColumnTypeParser.CustomTypes
				.OfType<CombinedIdColumnType>().FirstOrDefault()?.CombinedColumnIndices.Select(index =>
					DataView.ColumnHeaders.Find(header => header.ColumnNum == index)).Select(column => 
					(column.ColumnType.TypeName, column.Name)).ToList();
	
			var combinedKeyExists = combinedKeyFieldTuples != null && combinedKeyFieldTuples.Count > 0;
			if (combinedKeyExists)
			{
				foreach (var (typeName, name) in combinedKeyFieldTuples)
				{
					if (typeName.Contains("string") || typeName.Contains("struct") || typeName.Contains("bool"))
					{
						throw new Exception("组合key仅支持数值或枚举类型");
					}
				}
			}
			
			sw.WriteLine("using System;");
			sw.WriteLine("using System.Collections;");
			sw.WriteLine("using System.Collections.Generic;");
			sw.WriteLine("using UnityEngine;");
			sw.WriteLine("using Everlasting.Config.Loader;");
			sw.WriteLine("using Cysharp.Threading.Tasks;");
			sw.WriteLine();
			sw.WriteLine($"using {Name}TableLoader = Everlasting.Config.Internal.{Name}TableLoader;");
			foreach (var pair in classes)
			{
				sw.WriteLine($"using {pair.Key} = Everlasting.Config.{Name}Table.{pair.Key};");
			}
			foreach (var pair in enums)
			{
				sw.WriteLine($"using {pair.Key} = Everlasting.Config.{Name}Table.{pair.Key};");
			}
			if (combinedKeyExists)
			{
				sw.WriteLine($"using {Name}TableCombinedKey = Everlasting.Config.{Name}Table.{Name}TableCombinedKey;");
			}

			sw.WriteLine();
			sw.WriteLine("namespace Everlasting.Config");
			sw.WriteLine("{");

			sw.WriteLine($"public class {Name}Table");
			sw.WriteLine("{");

			if(dataList != null)
			{
				// 写入字段
				foreach (var dataViewColumnHeader in DataView.ColumnHeaders)
				{
					sw.WriteLine($"public readonly {dataViewColumnHeader.ColumnType.TypeName} {dataViewColumnHeader.Name};");
				}
				sw.WriteLine();
			}

			if (dataList != null)
			{
				// 写入构造函数
				var ctorParams =
					from p in DataView.ColumnHeaders
					select $"{p.ColumnType.TypeName} {p.Name}";
				sw.WriteLine($"public {Name}Table({string.Join(", ", ctorParams.ToArray())})");
				sw.WriteLine("{");
				var ctorInits =
					from p in DataView.ColumnHeaders
					select $"this.{p.Name} = {p.Name};";
				foreach (var ctorInit in ctorInits)
				{
					sw.WriteLine(ctorInit);
				}
				sw.WriteLine("}");
			}

			if (dataList != null)
			{
				sw.WriteLine($"public static int Count => {Name}TableLoader.ConfigCount;");
				sw.WriteLine($"public static {Name}Table GetTableData(uint id) => {Name}TableLoader.ConfigDic.TryGetValue(id, out var cfg) ? cfg : null;");

				if (combinedKeyExists)
				{
					var fieldDeclareExpr = from f in combinedKeyFieldTuples select $"{f.TypeName} {f.Name}";
					var fieldNames = from f in combinedKeyFieldTuples select $"{f.Name}";
					sw.WriteLine($"public static {Name}Table GetTableData({string.Join(", ", fieldDeclareExpr.ToArray())}) => " +
					             $"{Name}TableLoader.ConfigDic2.TryGetValue(new {Name}TableCombinedKey({string.Join(", ", fieldNames.ToArray())}), out var cfg) ? cfg : null;");
				}
				
				sw.WriteLine($"public static IEnumerable<{Name}Table> All => {Name}TableLoader.ConfigList;");
				sw.WriteLine();
			}

			if (settingEntries != null && settingEntries.Count > 0)
			{
				foreach (var entry in settingEntries)
				{
					sw.WriteLine($"public static {entry.ColumnType.TypeName} {entry.Name} => {Name}TableLoader.{entry.Name};");
				}
				sw.WriteLine();
			}

			if (dataList != null)
			{
				// 写入静态atlas
				var atlases = (from data in dataList select data[0] as IIdData).Where(id => id.Alias != null)
					.Select(id => new KeyValuePair<uint, string>((uint) id.Object, id.Alias)).ToList();
				if (atlases.Any())
				{
					sw.WriteLine($"public static {Name}Table GetTableData(Alias idAlias) => GetTableData((uint)idAlias);");
					sw.WriteLine();
					sw.WriteLine($"public enum Alias : uint");
					sw.WriteLine("{");
					foreach (var atlas in atlases)
					{
						sw.WriteLine($"{atlas.Value} = {atlas.Key},");
					}
					sw.WriteLine("}");
					foreach (var atlas in atlases)
					{
						sw.WriteLine($"public const Alias {atlas.Value} = Alias.{atlas.Value};");
					}
				}
			}

			// 写入自定义class定义
			foreach (var keyValuePair in classes)
			{
				sw.WriteLine();
				var typeName = keyValuePair.Key;
				sw.WriteLine($"public class {typeName}");
				sw.WriteLine("{");

				foreach (var field in keyValuePair.Value)
				{
					sw.WriteLine($"public readonly {field.Key.TypeName} {field.Value};");
				}
				sw.WriteLine();
				var ctorParams =
					from p in keyValuePair.Value
					select $"{p.Key.TypeName} {p.Value}";
				sw.WriteLine($"public {typeName}({string.Join(", ", ctorParams.ToArray())})");
				sw.WriteLine("{");
				var ctorInits =
					from p in keyValuePair.Value
					select $"this.{p.Value} = {p.Value};";
				foreach (var ctorInit in ctorInits)
				{
					sw.WriteLine(ctorInit);
				}
				sw.WriteLine("}");

				sw.WriteLine("}");
			}

			//写入自定义enum
			foreach (var keyValuePair in enums)
			{
				sw.WriteLine();
				var typeName = keyValuePair.Key;
				sw.WriteLine($"public enum {typeName}");
				sw.WriteLine("{");

				foreach (var field in keyValuePair.Value)
				{
					sw.WriteLine($"{field.Key} = {field.Value},");
				}
				sw.WriteLine("}");
			}
			
			//写入组合key定义
			if (combinedKeyExists)
			{
				sw.WriteLine();
				sw.WriteLine($"internal struct {Name}TableCombinedKey : IEquatable<{Name}TableCombinedKey>");
				sw.WriteLine("{");
				foreach (var (typeName, name) in combinedKeyFieldTuples)
				{
					sw.WriteLine($"private readonly {typeName} {name};");
				}

				var ctorParams =
					from p in combinedKeyFieldTuples
					select $"{p.TypeName} {p.Name}";
				sw.WriteLine($"public {Name}TableCombinedKey({string.Join(", ", ctorParams.ToArray())})");
				sw.WriteLine("{");
				var ctorInits =
					from p in combinedKeyFieldTuples
					select $"this.{p.Name} = {p.Name};";
				foreach (var ctorInit in ctorInits)
				{ 
					sw.WriteLine(ctorInit);
				}

				sw.WriteLine("}");

				sw.WriteLine();
				sw.WriteLine($"public bool Equals({Name}TableCombinedKey other)");
				sw.WriteLine("{");
				var equalExpr =
					from p in combinedKeyFieldTuples
					select $"{p.Name} == other.{p.Name}";
				sw.WriteLine($"return {string.Join(" && ", equalExpr.ToArray())};");
				sw.WriteLine("}");
				
				sw.WriteLine();
				sw.WriteLine("public override bool Equals(object obj)");
				sw.WriteLine("{");
				sw.WriteLine($"return obj is {Name}TableCombinedKey other && Equals(other);");
				sw.WriteLine("}");
				
				sw.WriteLine();
				sw.WriteLine("public override int GetHashCode()");
				sw.WriteLine("{");
				sw.WriteLine("unchecked");
				sw.WriteLine("{");
				var hashExpr = combinedKeyFieldTuples.Select((field, i) => 
					(i == 0 ? "var " : "") + "hashCode = " + (i == 0 ? "" : "(hashCode * 397) ^ ") + $"(int) {field.Name};").ToList();
				foreach (var expr in hashExpr)
				{
					sw.WriteLine(expr);
				}
				sw.WriteLine("return hashCode;");
				sw.WriteLine("}");
				
				sw.WriteLine("}");
				sw.WriteLine("}");
			}

			sw.WriteLine("}");

			//内部实现
			sw.WriteLine($"static partial class Internal");
			sw.WriteLine("{");

			sw.WriteLine($"public static class {Name}TableLoader");
			sw.WriteLine("{");
			//内部结构变量
			if(dataList != null)
			{
				sw.WriteLine($"public static {Name}Table[] ConfigList {{get; private set;}}");
				sw.WriteLine($"public static readonly Dictionary<uint, {Name}Table> ConfigDic = new Dictionary<uint, {Name}Table>();");
				if (combinedKeyExists)
				{
					sw.WriteLine($"public static readonly Dictionary<{Name}TableCombinedKey, {Name}Table> ConfigDic2 = new Dictionary<{Name}TableCombinedKey, {Name}Table>();");
				}
				
				sw.WriteLine($"public static int ConfigCount;");
				sw.WriteLine();
			}

			if (settingEntries != null && settingEntries.Count > 0)
			{
				foreach (var entry in settingEntries)
				{
					sw.WriteLine($"public static {entry.ColumnType.TypeName} {entry.Name} {{get; private set;}}");
				}

				sw.WriteLine();
			}
			
			//写入pool变量
			var pooledTypeNameSet = new HashSet<string>();
			void RecursivelyWritePoolTypeDef(IPooledColumnType columnType)
			{
				if (columnType == null || pooledTypeNameSet.Contains(columnType.TypeName))
				{
					return;
				}
				
				pooledTypeNameSet.Add(columnType.TypeName);
				sw.WriteLine($"private static {columnType.TypeName}[] {GetPoolName(columnType)} = null;");
				if (columnType is BuiltinColumnTypes.GenericArrayColumn arrayColumnType)
				{
					RecursivelyWritePoolTypeDef(arrayColumnType.BaseType as IPooledColumnType);
				}
			}
            
            //通过columnHeader分析表格中存在的类型
            foreach (var columnHeader in DataView.ColumnHeaders)
			{
				RecursivelyWritePoolTypeDef(columnHeader.ColumnType as IPooledColumnType);
			}
            
            //通过表格中的数据分析存在的类型，主要是为了Setting Sheet中的数据
            foreach (var pair in Binary.Pools)
            {
                var typeName = pair.Key.TypeName;
                var poolName = GetPoolName(pair.Key);
                if (!pooledTypeNameSet.Contains(typeName))
                {
                    pooledTypeNameSet.Add(typeName);
                    sw.WriteLine($"private static {typeName}[] {poolName} = null;");
                }
            }
            
			{
				// 写入自定义class定义
				foreach (var keyValuePair in classes)
				{
					sw.WriteLine();
					var typeName = keyValuePair.Key;
					sw.WriteLine($"private static {typeName} Load{NormalizeName(typeName)}(IConfigBinary binary)");
					sw.WriteLine("{");
					for (var index = 0; index < keyValuePair.Value.Count; index++)
					{
						var valuePair = keyValuePair.Value[index];
						sw.WriteLine($"var field{index+1} = {GetReadFromBinaryString(valuePair.Key)};");
					}

					var strings = Enumerable.Range(1, keyValuePair.Value.Count).Select(i => $"field{i}");
					sw.WriteLine($"return new {typeName}({string.Join(",", strings.ToArray())});");

					sw.WriteLine("}");
				}
				// 写入Array的定义
				var arrays =
					from p in Binary.Pools
					where p.Key is IArrayColumnType
					select p.Key as IArrayColumnType;
				foreach (var array in arrays)
				{
					sw.WriteLine();
					var typeName = array.TypeName;
					sw.WriteLine($"private static {typeName} Load{NormalizeName(typeName.Replace("[]", "Array"))}(IConfigBinary binary)");
					sw.WriteLine("{");
					sw.WriteLine($"var count = binary.ReadInt();");
					sw.WriteLine($"var arr = new {NewArray(array.BaseType.TypeName)};");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");
					sw.WriteLine($"arr[i] = {GetReadFromBinaryString(array.BaseType)};");
					sw.WriteLine("}");
					sw.WriteLine($"return arr;");
					sw.WriteLine("}");
				}
			}
			//Load
			{
				sw.WriteLine();
				//内部初始化函数
				sw.WriteLine($"public static async UniTask Load()");
				sw.WriteLine("{");

				sw.WriteLine($"var binary = await ConfigLoader.LoadConfigBytesAsync($\"Excel/{Name.ToLowerInvariant()}Table.bytes\");");
				sw.WriteLine();

				// 先初始化Primitive类型的Pool
				foreach (var pair in Binary.Pools)
				{
					if (!IsPrimitivePooledColumnType(pair.Key))
						continue;
					var typeName = pair.Key.TypeName;
					var poolName = GetPoolName(pair.Key);
					sw.WriteLine("{");
					sw.WriteLine($"int count = binary.ReadInt();");
					sw.WriteLine($"{poolName} =  new {NewArray(typeName)};");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");
					sw.WriteLine($"{poolName}[i] = {GetReadFromPoolString(pair.Key)};");
					sw.WriteLine("}");
					sw.WriteLine("}");
				}
				// 再初始化非Primitive类型的Pool
				foreach (var pair in Binary.Pools)
				{
					if (IsPrimitivePooledColumnType(pair.Key))
						continue;
					var typeName = pair.Key.TypeName;
					var poolName = GetPoolName(pair.Key);
					sw.WriteLine("{");
					sw.WriteLine($"int count = binary.ReadInt();");
					sw.WriteLine($"{poolName} =  new {NewArray(typeName)};");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");
					sw.WriteLine($"{poolName}[i] = {GetReadFromPoolString(pair.Key)};");
					sw.WriteLine("}");
					sw.WriteLine("}");
				}

				// 初始化main chunk
				if (dataList != null)
				{
					sw.WriteLine("{");
					sw.WriteLine($"int count = binary.ReadInt();");
					sw.WriteLine($"ConfigCount = count;");
					sw.WriteLine($"ConfigList = new {Name}Table[count];");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");

					foreach (var columnHeader in DataView.ColumnHeaders)
					{
						sw.WriteLine($"var {columnHeader.Name} = {GetReadFromBinaryString(columnHeader.ColumnType)};");
					}
					sw.WriteLine($"var cfg = new {Name}Table({string.Join(", ", DataView.ColumnHeaders.Select(c => c.Name).ToArray())});");
					sw.WriteLine($"ConfigList[i] = cfg;");
					sw.WriteLine($"ConfigDic.Add(Id, cfg);");
					if (combinedKeyExists)
					{
						var fieldNames = from f in combinedKeyFieldTuples select $"{f.Name}";
						sw.WriteLine($"ConfigDic2.Add(new {Name}TableCombinedKey({string.Join(", ", fieldNames.ToArray())}), cfg);");
					}

					sw.WriteLine("}");
					sw.WriteLine("}");
				}

				if (settingEntries != null && settingEntries.Count > 0)
				{
					sw.WriteLine("{");
					foreach (var entry in settingEntries)
					{
						sw.WriteLine($"{entry.Name} = {GetReadFromBinaryString(entry.ColumnType)};");
					}

					sw.WriteLine("}");
				}

				sw.WriteLine("}");
			}
			//Clear
			{
				sw.WriteLine();
				//内部初始化函数
				sw.WriteLine($"public static void Clear()");
				sw.WriteLine("{");
				if (dataList != null)
				{
					sw.WriteLine($"ConfigCount = 0;");
					sw.WriteLine($"ConfigList = null;");
					sw.WriteLine($"ConfigDic.Clear();");
					if (combinedKeyExists)
					{
						sw.WriteLine("ConfigDic2.Clear();");
					}
				}

				sw.WriteLine("}");
			}
			sw.WriteLine("}");

			sw.WriteLine("}");

			sw.WriteLine("}");
		}
	}
}
