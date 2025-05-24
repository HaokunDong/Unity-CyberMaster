using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Everlasting.Extend;
using TypeName;
using UnityEngine;
using static EverlastingEditor.Config.Export.BuiltinColumnTypes;

namespace EverlastingEditor.Config.Export
{
    static partial class Define
    {
        public static readonly char[] ARRAY_SPLITTER = new char[3] { ',', ';', '|' };
    }

    public interface IColumnType
    {
        string TypeName { get; }
        int ArrayLevel { get; }
        string DefaultValue { get; }
        IData Parse(string excelString);
        void WriteToBinary(IConfigBinary binary, IData obj);
    }

	public interface IPrimitiveColumnType : IColumnType
	{
		Type PrimitiveType { get; }
	}

	public interface IPooledColumnType : IColumnType
    {
        void WriteToPool(IConfigBinary binary, IPooledData obj);
    }

    public interface IRefIdColumnType : IColumnType
    {
        string ConfigName { get; }
    }

	public interface IStructuredColumnType : IColumnType
	{
		List<KeyValuePair<IColumnType, string>> Fields { get; }
	}

    public interface ICustomColumnType : IStructuredColumnType
	{
        
    }

    public interface IEnumColumnType : IPrimitiveColumnType
	{
        List<KeyValuePair<string, int>> Fields { get; }
    }

	public interface IIdColumnType : IColumnType
	{

	}

	public interface IArrayColumnType : IColumnType
	{
		IColumnType BaseType { get; }
	}

    abstract class ColumnType : IColumnType
    {
        public virtual string TypeName { get; protected set; }
        public int ArrayLevel { get; protected set; } = -1;
	    public string DefaultValue { get; protected set; } = "";
        public abstract IData Parse(string excelString);
        public abstract void WriteToBinary(IConfigBinary binary, IData obj);
    }

    public class ColumnTypeParser
    {
        public List<IColumnType> CustomTypes { get; } = new List<IColumnType>();

	    private static string GenerateTypeNameByColumnName(string columnName)
	    {
		    return columnName + "Type";
	    }

        public IColumnType ParseColumnType(string excelString, string columnName)
        {
            IColumnType type = null;
            foreach(var builtinType in BuiltinColumnTypes.BuiltinTypes)
            {
                if(excelString.StartsWith(builtinType.TypeName, StringComparison.OrdinalIgnoreCase))
                {
                    type = builtinType;
                    excelString = excelString.Substring(builtinType.TypeName.Length);
                    break;
                }
            }
            if (type == null)
            {
                //先处理各种类型
                if (excelString.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    type = ParseIdType(ref excelString);
                }
                else if(excelString.StartsWith("struct", StringComparison.OrdinalIgnoreCase) || 
                    excelString.StartsWith("class", StringComparison.OrdinalIgnoreCase))
                {
                    type = ParseCustomType(ref excelString, columnName);
                }
                else if(excelString.StartsWith("enum", StringComparison.OrdinalIgnoreCase))
                {
                    type = ParseEnumType(ref excelString, columnName);
                }
                else
                {
                    throw new Exception("解析失败，不支持的类型");
                }
                CustomTypes.Add(type);
            }
            while(excelString != "")
            {
                if (excelString.StartsWith("[]"))
                {
                    excelString = excelString.Substring(2).TrimStart();
                    type = new BuiltinColumnTypes.ArrayColumn(type);
                    CustomTypes.Add(type);
                }
                else
                {
                    throw new Exception($"含有多余的末尾{excelString}");
                }
            }
            return type;
        }

        private IColumnType ParseEnumType(ref string excelString, string columnName)
        {
            excelString = excelString.Substring(4).TrimStart();
            if (excelString.StartsWith("{"))
            {
                if (excelString.IndexOf('{', 1) >= 0)
                {
                    throw new Exception("Enum暂时不支持嵌套");
                }
                int end = excelString.IndexOf('}', 1);
                if (end >= 0)
                {
                    string inner = excelString.Substring(1, end - 1).Trim();
                    excelString = excelString.Substring(end + 1).TrimStart();
                    if (inner == "")
                    {
                        throw new Exception("Enum内容不能为空");
                    }
                    string[] fieldDefs = inner.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<KeyValuePair<string, int>> fields = new List<KeyValuePair<string, int>>();
                    foreach (var fieldDef in fieldDefs)
                    {
                        string[] fieldInfo = fieldDef.Trim().Split('=');
                        if (fieldInfo.Length > 2)
                        {
                            throw new Exception("Enum Field格式不正确，应为 字段名[=int值]");
                        }
                        string fieldName = fieldInfo[0];
                        int value;
                        if (fieldInfo.Length > 1)
                            value = int.Parse(fieldInfo[1]);
                        else
                            value = fields.Count != 0 ? fields.Last().Value + 1 : 0;
                        if (fields.FindIndex((pair) => pair.Key == fieldName) >= 0)
                        {
                            throw new Exception($"解析Enum失败，存在重复的键{fieldName}");
                        }
                        fields.Add(new KeyValuePair<string, int>(fieldName, value));
                    }
                    EnumColumnType enumColumnType = new EnumColumnType(GenerateTypeNameByColumnName(columnName), fields);
                    return enumColumnType;
                }
            }
            throw new Exception("Enum语法不正确，应为enum{字段名[=值], ...}");
        }

        private IColumnType ParseCustomType(ref string excelString, string columnName)
        {
            if(excelString.StartsWith("struct", StringComparison.OrdinalIgnoreCase))
            {
                excelString = excelString.Substring(6);
            }
            else
            {
                excelString = excelString.Substring(5);
            }
            excelString = excelString.TrimStart();
            if(excelString.StartsWith("{"))
            {
                if(excelString.IndexOf('{', 1) >= 0)
                {
                    throw new Exception("Struct暂时不支持嵌套 " + excelString);
                }
                
                int end = excelString.IndexOf('}', 1);
                if(end >= 0)
                {
                    string inner = excelString.Substring(1, end - 1).Trim();
                    if (inner.IndexOf('[') >= 0)
                    {
                        throw new Exception("struct字段暂时不支持数组类型 " + inner);
                    }
                    
                    excelString = excelString.Substring(end + 1).TrimStart();
                    if (inner == "")
                    {
                        throw new Exception("Struct内容不能为空");
                    }
                    string[] fieldDefs = inner.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    List<KeyValuePair<IColumnType, string>> fields = new List<KeyValuePair<IColumnType, string>>();
                    foreach (var fieldDef in fieldDefs)
                    {
                        string[] fieldInfo = fieldDef.Trim().Split(' ');
                        if(fieldInfo.Length != 2)
                        {
                            throw new Exception("Struct Field格式不正确，应为 字段类型 字段名;");
                        }
                        IColumnType columnType;
                        try
                        {
                            columnType = ParseColumnType(fieldInfo[0], columnName);
                        }
                        catch(Exception e)
                        {
                            throw new Exception($"解析Struct字段{fieldInfo[0]}失败，{e.Message}", e);
                        }
                        if(fields.FindIndex((pair)=> pair.Value == fieldInfo[1]) >= 0)
                        {
                            throw new Exception($"解析Struct失败，存在重复的键{fieldInfo[1]}");
                        }
                        fields.Add(new KeyValuePair<IColumnType, string>(columnType, fieldInfo[1]));
                    }
                    CustomColumnType customColumnType = new CustomColumnType(GenerateTypeNameByColumnName(columnName), fields);
                    return customColumnType;
                }
            }
            throw new Exception("Struct语法不正确，应为struct/class{字段类型 字段名;...}");
        }

        private IColumnType ParseIdType(ref string excelString)
        {
	        excelString = excelString.Substring(2).TrimStart();
	        if (excelString == "")
	        {
				return new IdColumnType();
	        }
            
			if (excelString.StartsWith("("))
            {
                int end = excelString.IndexOf(')', 1);
                if (end > 0)
                {
                    string configName = excelString.Substring(1, end - 1);
                    excelString = excelString.Substring(end + 1);
                    return new RefIdColumnType(configName);
                }
            }

            if (excelString.StartsWith("["))
            {
                int end = excelString.IndexOf(']', 1);
                if (end > 0)
                {
                    var combinedIndices = excelString.Substring(1, end - 1).Split('&')
                        .Select(Utils.Label2ColumnNum).ToArray();

                    if (combinedIndices.Length == 0 || combinedIndices.IndexOf(0) >= 0 || combinedIndices.GroupBy(n => n).Any(c => c.Count() > 1))
                    {
                        throw new Exception($"组合key语法不正确: {excelString}");
                    }

                    if (combinedIndices.IndexOf(1) >= 0)
                    {
                        throw new Exception($"不支持使用id列作为组合key成员: {excelString}");
                    }
                    
                    excelString = excelString.Substring(end + 1);
                    return new CombinedIdColumnType(combinedIndices);
                }
            }
            
            throw new Exception("Id语法不正确，应为id(Config名)或id[组合key使用的列]");
        }
    }

	class IdColumnType : PrimitiveColumnType<uint>, IIdColumnType
    {
		public IdColumnType()
		{
		}

		public override IData Parse(string excelString)
		{
			int split = excelString.IndexOf('|');
			uint value;
			string atlas = null;
			if (split >= 0)
			{
				value = uint.Parse(excelString.Substring(0, split));
				atlas = excelString.Substring(split + 1);
			}
			else
			{
				value = uint.Parse(excelString);
			}

            return new IdData(this, value, atlas);
		}
	}

    class CombinedIdColumnType : IdColumnType
    {
        public readonly int[] CombinedColumnIndices;
        public CombinedIdColumnType(int[] combinedIndices)
        {
            CombinedColumnIndices = combinedIndices;
        }
    }

    class RefIdColumnType : PrimitiveColumnType<uint>, IRefIdColumnType
    {
        public string ConfigName { get; }
        public RefIdColumnType(string configName)
        {
            ConfigName = configName;
        }

        public override IData Parse(string excelString)
        {
	        uint value;
	        if (uint.TryParse(excelString, out value))
			{
				return new IdData(this, value, null);
			}
	        else
	        {
				throw new Exception("暂时不支持别名引用id");
	        }
        }
    }
    
    class BoolColumnType : PrimitiveColumnType<bool>
    {
        public override IData Parse(string excelString)
        {
            excelString = (excelString ?? "").Trim().ToLower();
            var res = !(excelString == "false" || excelString == "0");
            return new Data(this, res);
        }
    }

	sealed class CustomColumnType : ColumnType, ICustomColumnType, IPooledColumnType
    {
        public List<KeyValuePair<IColumnType, string>> Fields { get; }
        private int PoolIndex { get; set; } = -1;

        public CustomColumnType(string typeName, List<KeyValuePair<IColumnType, string>> fields)
        {
            Fields = fields;
	        TypeName = char.ToUpper(typeName[0]) + typeName.Substring(1);
            ArrayLevel = Fields.Max((pair) => pair.Key.ArrayLevel) + 1;
            if (ArrayLevel > 3)
            {
                throw new Exception("Array Define过深，不能超过三层");
            }
        }

        public override IData Parse(string excelString)
        {
	        if (excelString == "")
	        {
		        PooledArrayData arrayData = new PooledArrayData(this);
		        for (int i = 0; i < Fields.Count; i++)
		        {
			        var data = Fields[i].Key.Parse(Fields[i].Key.DefaultValue);
			        arrayData.Add(data);
		        }
		        return arrayData;
			}
	        else
	        {
				var splitter = Define.ARRAY_SPLITTER[ArrayLevel];
		        string[] result = excelString.Split(splitter);
		        if (result.Length != Fields.Count)
		        {
			        throw new Exception("参数数量和类型不符");
		        }
		        PooledArrayData arrayData = new PooledArrayData(this);
		        for (int i = 0; i < result.Length; i++)
		        {
			        var data = Fields[i].Key.Parse(result[i]);
			        arrayData.Add(data);
		        }
		        return arrayData;
			}
        }

        public override void WriteToBinary(IConfigBinary binary, IData obj)
        {
            binary.Write(((IPooledData)obj).GetPoolIndex(binary));
        }

	    public void WriteToPool(IConfigBinary binary, IPooledData obj)
        {
            PooledArrayData arrayData = (PooledArrayData)obj;
            //binary.Write(arrayData.Count); 无需写入数量
            foreach (var data in arrayData.Data)
            {
                data.WriteToBinary(binary);
            }
        }
    }

    sealed class EnumColumnType : ColumnType, IEnumColumnType
    {
        public List<KeyValuePair<string, int>> Fields { get; }
        private readonly Dictionary<string, int> fieldsDic = new Dictionary<string, int>();

        public EnumColumnType(string typeName, List<KeyValuePair<string, int>> fields)
        {
	        TypeName = char.ToUpper(typeName[0]) + typeName.Substring(1);
            Fields = fields;
            foreach(var p in fields)
            {
                fieldsDic.Add(p.Key, p.Value);
            }
        }

        public override IData Parse(string excelString)
        {
            if (string.IsNullOrEmpty(excelString))
            {
                return new Data(this, 0);
            }
            int value;
            if(fieldsDic.TryGetValue(excelString, out value))
            {
                return new Data(this, value);
            }
            throw new Exception($"Enum键{excelString}不存在");
        }

        public override void WriteToBinary(IConfigBinary binary, IData obj)
        {
            binary.Write((int)obj.Object);
        }

	    public Type PrimitiveType => typeof(Enum);
    }


    static class BuiltinColumnTypes
    {
        public static readonly ColumnType[] BuiltinTypes = new ColumnType[]
        {
            new PrimitiveColumnType<int>(),
            new PrimitiveColumnType<uint>(),
            new PrimitiveColumnType<long>(),
            new PrimitiveColumnType<ulong>(),
            //new PrimitiveColumnType<bool>(),
            new PrimitiveColumnType<float>(),
            new BoolColumnType(),
            new StringColumnType(), 
            new DateTimeColumnType(),
            new Vector3ColumnType(),
            new Vector2ColumnType(),
            new ColorColumnType(),
        };

        public abstract class GenericArrayColumn : ColumnType, IPooledColumnType
        {
            public IColumnType BaseType { get; }
            public int FixedLength { get; protected set; } = 0;

	        protected GenericArrayColumn(IColumnType baseType)
            {
                BaseType = baseType;
                ArrayLevel = baseType.ArrayLevel + 1;
                if (ArrayLevel > 3)
                {
                    throw new Exception("Array Define过深，不能超过三层");
                }
            }

            public override IData Parse(string excelString)
            {
                var splitter = Define.ARRAY_SPLITTER[ArrayLevel];
                string[] result = excelString.Split(new []{splitter}, StringSplitOptions.RemoveEmptyEntries);
                if (FixedLength > 0)
                {
                    if (result.Length != FixedLength)
                    {
                        throw new Exception($"数组长度不符合定义的长度：{FixedLength}");
                    }
                }

	            ArrayData arrayData;
				//basetype是pooled的，本层不写入pool
				//if (BaseType is IPooledColumnType)
	            //{
		        //    arrayData = new ArrayData(this);
				//}
				//else
				{
					arrayData = new PooledArrayData(this);
				}
                foreach (var r in result)
                {
                    arrayData.Add(BaseType.Parse(r));
                }
                return arrayData;
            }

			public override void WriteToBinary(IConfigBinary binary, IData obj)
			{
				var pooledData = obj as IPooledData;
				if (pooledData != null)
				{
					binary.Write(pooledData.GetPoolIndex(binary));
				}
				//else
				//{
				//	IArrayData arrayData = (IArrayData)obj;
				//	binary.Write(arrayData.Count);
				//	foreach (var data in arrayData.Data)
				//	{
				//		data.WriteToBinary(binary);
				//	}
				//}
			}

			public virtual void WriteToPool(IConfigBinary binary, IPooledData obj)
            {
                PooledArrayData arrayData = (PooledArrayData)obj;
                binary.Write(arrayData.Count);
                foreach(var data in arrayData.Data)
                {
                    data.WriteToBinary(binary);
                }
            }
        }

        public class ArrayColumn : GenericArrayColumn, IArrayColumnType
		{
            public override string TypeName => BaseType.TypeName + "[]";

            public ArrayColumn(IColumnType baseType)
                : base(baseType)
            {
            }
        }

	    public sealed class StringColumnType : ColumnType, IPooledColumnType, IPrimitiveColumnType
		{
			public StringColumnType()
			{
				TypeName = "string";
			}
			public override IData Parse(string excelString)
		    {
			    return new PooledData(this, excelString);
		    }

			public override void WriteToBinary(IConfigBinary binary, IData obj)
			{
				binary.Write(((IPooledData)obj).GetPoolIndex(binary));
			}

			public void WriteToPool(IConfigBinary binary, IPooledData obj)
		    {
			    binary.Write((string)obj.Object);
		    }

			public Type PrimitiveType => typeof(string);
		}

		public class PrimitiveColumnType<T> : ColumnType, IPrimitiveColumnType
        {
            public PrimitiveColumnType()
            {
                TypeName = typeof(T).GetTypeNameString();
                DefaultValue = typeof(T) == typeof(bool) ? "false" : "0";
            }

            public override IData Parse(string excelString)
            {
				if(string.IsNullOrWhiteSpace(excelString))
					return new Data(this, default(T));
                return new Data(this, Convert.ChangeType(excelString, typeof(T)));
            }

            public override void WriteToBinary(IConfigBinary binary, IData data)
            {
                if(typeof(T) == typeof(int))
                {
                    binary.Write((int)data.Object);
                }
                else if (typeof(T) == typeof(uint))
                {
                    binary.Write((uint)data.Object);
                }
                else if (typeof(T) == typeof(float))
                {
                    binary.Write((float)data.Object);
                }
                else if (typeof(T) == typeof(long))
                {
                    binary.Write((long)data.Object);
                }
                else if (typeof(T) == typeof(ulong))
                {
                    binary.Write((ulong)data.Object);
                }
                else if (typeof(T) == typeof(bool))
                {
                    binary.Write((bool)data.Object);
                }
            }

	        public Type PrimitiveType => typeof(T);
        }

        public sealed class DateTimeColumnType : ColumnType, IPrimitiveColumnType
        {
			public DateTimeColumnType()
            {
                TypeName = "DateTime";
            }

            public override IData Parse(string excelString)
            {
                if(string.IsNullOrWhiteSpace(excelString))
                {
                    return new Data(this, null);
                }
                return new Data(this, DateTime.ParseExact(excelString, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture));
            }

            public override void WriteToBinary(IConfigBinary binary, IData obj)
            {
                if(obj.Object == null)
                {
                    binary.Write(-1L);
                }
                else
                {
                    DateTime dt = (DateTime)obj.Object;
                    binary.Write(new DateTimeOffset(dt).ToUnixTimeSeconds());
				}
            }

	        public Type PrimitiveType => typeof(DateTime);
        }

        public sealed class Vector3ColumnType : ColumnType, IPrimitiveColumnType
        {
            public Vector3ColumnType()
            {
                TypeName = "Vector3";
                ArrayLevel = 0;
            }
            
            public override IData Parse(string excelString)
            {
                var arrayData = new ArrayData(this);
                string[] result;
                if (excelString.IsNullOrEmpty())
                {
                    result = new[] { "0", "0", "0" };
                }
                else
                {
                    var splitter = Define.ARRAY_SPLITTER[0];
                    result = excelString.Split(new []{splitter}, StringSplitOptions.RemoveEmptyEntries);
                    if (result.Length != 3)
                    {
                        throw new Exception($"Vector3长度不符合定义");
                    }
                }

                foreach (var r in result)
                {
                    arrayData.Add(new PrimitiveColumnType<float>().Parse(r));
                }
                
                return arrayData;
            }

            public override void WriteToBinary(IConfigBinary binary, IData obj)
            {
                List<IData> data = (List<IData>)obj.Object;
                binary.Write(data.Count > 0 ? (float)data[0].Object : 0);
                binary.Write(data.Count > 1 ? (float)data[1].Object : 0);
                binary.Write(data.Count > 2 ? (float)data[2].Object : 0);
            }

            public Type PrimitiveType => typeof(Vector3);
        }

		public sealed class Vector2ColumnType : ColumnType, IPrimitiveColumnType
		{
            public Vector2ColumnType()
            {
                TypeName = "Vector2";
                ArrayLevel = 0;
            }

            public override IData Parse(string excelString)
            {
                var arrayData = new ArrayData(this);
                string[] result;
                if (excelString.IsNullOrEmpty())
                {
                    result = new[] { "0", "0" };
                }
                else
                {
                    var splitter = Define.ARRAY_SPLITTER[0];
                    result = excelString.Split(new []{splitter}, StringSplitOptions.RemoveEmptyEntries);
                    if (result.Length != 2)
                    {
                        throw new Exception($"Vector2长度不符合定义");
                    }
                }

                foreach (var r in result)
                {
                    arrayData.Add(new PrimitiveColumnType<float>().Parse(r));
                }
                
                return arrayData;
            }

            public override void WriteToBinary(IConfigBinary binary, IData obj)
            {
                List<IData> data = (List<IData>)obj.Object;
                binary.Write(data.Count > 0 ? (float)data[0].Object : 0);
                binary.Write(data.Count > 1 ? (float)data[1].Object : 0);
			}
			public Type PrimitiveType => typeof(Vector2);
		}

        public sealed class ColorColumnType : ColumnType, IPrimitiveColumnType
		{
            public ColorColumnType()
            {
                TypeName = "Color";
            }

            public override IData Parse(string excelString)
            {
                int result;
                if(excelString.Length == 6)
                {
                    result = (int.Parse(excelString, NumberStyles.HexNumber) << 8) | 0xFF;
                }
                else if(excelString.Length == 8)
                {
                    result = int.Parse(excelString, NumberStyles.HexNumber);
                }
                else
                {
                    throw new Exception($"错误的颜色格式，应为RRGGBB或者RRGGBBAA格式的Hex字符串");
                }
                return new Data(this, result);
            }

            public override void WriteToBinary(IConfigBinary binary, IData obj)
            {
                binary.Write((int)obj.Object);
			}
			public Type PrimitiveType => typeof(Color);
		}
    }
}
