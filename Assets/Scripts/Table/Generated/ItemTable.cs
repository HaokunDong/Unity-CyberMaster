using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Everlasting.Config.Loader;
using Cysharp.Threading.Tasks;

using ItemTableLoader = Everlasting.Config.Internal.ItemTableLoader;

namespace Everlasting.Config
{
	public class ItemTable
	{
		public readonly uint Id;
		public readonly string ItemName;
		public readonly string Prefab;
		
		public ItemTable(uint Id, string ItemName, string Prefab)
		{
			this.Id = Id;
			this.ItemName = ItemName;
			this.Prefab = Prefab;
		}
		public static int Count => ItemTableLoader.ConfigCount;
		public static ItemTable GetTableData(uint id) => ItemTableLoader.ConfigDic.TryGetValue(id, out var cfg) ? cfg : null;
		public static IEnumerable<ItemTable> All => ItemTableLoader.ConfigList;
		
	}
	static partial class Internal
	{
		public static class ItemTableLoader
		{
			public static ItemTable[] ConfigList {get; private set;}
			public static readonly Dictionary<uint, ItemTable> ConfigDic = new Dictionary<uint, ItemTable>();
			public static int ConfigCount;
			
			private static string[] poolString = null;
			
			public static async UniTask Load()
			{
				var binary = await ConfigLoader.LoadConfigBytesAsync($"Excel/itemTable.bytes");
				
				{
					int count = binary.ReadInt();
					poolString =  new string[count];
					for(int i = 0; i < count; i++)
					{
						poolString[i] = binary.ReadString();
					}
				}
				{
					int count = binary.ReadInt();
					ConfigCount = count;
					ConfigList = new ItemTable[count];
					for(int i = 0; i < count; i++)
					{
						var Id = binary.ReadUInt();
						var ItemName = poolString[binary.ReadInt()];
						var Prefab = poolString[binary.ReadInt()];
						var cfg = new ItemTable(Id, ItemName, Prefab);
						ConfigList[i] = cfg;
						ConfigDic.Add(Id, cfg);
					}
				}
			}
			
			public static void Clear()
			{
				ConfigCount = 0;
				ConfigList = null;
				ConfigDic.Clear();
			}
		}
	}
}
