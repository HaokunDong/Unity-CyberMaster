using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EverlastingEditor.Config.Export
{
    public interface IData
    {
        IColumnType ColumnType { get; }
        object Object { get; }
    }

    public interface IArrayData : IData
    {
        List<IData> Data { get; }
        int Count { get; }
        void Add(IData data);
    }

    public interface IPooledData : IData
    {
        IPooledColumnType PooledColumnType { get; }
	    int GetPoolIndex(IConfigBinary binary);
    }

	public interface IIdData : IData
	{
		string Alias { get; }
	}

    public static class DataExtension
    {
        public static void WriteToBinary(this IData self, IConfigBinary binary)
        {
            self.ColumnType.WriteToBinary(binary, self);
        }
        public static void WriteToPool(this IPooledData self, IConfigBinary binary)
        {
            self.PooledColumnType.WriteToPool(binary, self);
        }
    }

    public class Data : IData
    {
        public IColumnType ColumnType { get; }
        public object Object { get; }
        public Data(IColumnType columnType, object obj)
        {
            ColumnType = columnType;
            Object = obj;
        }
        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return ((Data)obj).Object.Equals(Object);
        }
    }

    public class PooledData : Data, IPooledData
    {
        public IPooledColumnType PooledColumnType { get; }
        public PooledData(IPooledColumnType columnType, object obj)
            : base(columnType, obj)
        {
            PooledColumnType = columnType;
        }

	    private int poolIndex = -1;

	    public int GetPoolIndex(IConfigBinary binary)
	    {
			if (poolIndex < 0)
			{
				Pool pool = binary.GetPool(PooledColumnType);
				poolIndex = pool.GetIndex(this);
		    }

		    return poolIndex;
		}
    }

    public class ArrayData : IArrayData
    {
        public IColumnType ColumnType { get; }
        public List<IData> Data { get; } = new List<IData>();
        public int Count => Data.Count;
        public object Object => Data;
        public ArrayData(IColumnType columnType)
        {
            ColumnType = columnType;
        }
        public void Add(IData data)
        {
            Data.Add(data);
        }
        public override int GetHashCode()
        {
			const int salt = 23;
	        var hash = 17;
			foreach (var obj in Data)
            {
	            hash += salt * obj.GetHashCode();
            }
            return hash;
        }
        public override bool Equals(object obj)
        {
            ArrayData r = (ArrayData)obj;
            if (r.Data.Count != Data.Count)
                return false;
            for(int i = 0; i < Data.Count; i++)
            {
                if (!Data[i].Equals(r.Data[i]))
                    return false;
            }
            return true;
        }
    }

    public class PooledArrayData : ArrayData, IPooledData
    {
        public IPooledColumnType PooledColumnType { get; }
        public PooledArrayData(IPooledColumnType columnType)
            : base(columnType)
        {
            PooledColumnType = columnType;
		}

	    private int poolIndex = -1;
	    public int GetPoolIndex(IConfigBinary binary)
	    {
		    if (poolIndex < 0)
		    {
                //先所有children生成index
                foreach (var data in Data)
                {
                    (data as IPooledData)?.GetPoolIndex(binary);
                }
			    Pool pool = binary.GetPool(PooledColumnType);
			    poolIndex = pool.GetIndex(this);
			}

		    return poolIndex;
	    }
	}

	public class IdData : Data, IIdData
	{
		public string Alias { get; }
		public IdData(IColumnType columnType, object obj, string atlas) 
			: base(columnType, obj)
		{
			Alias = atlas;
		}
	}
}
