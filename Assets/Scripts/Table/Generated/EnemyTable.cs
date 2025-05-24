using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Everlasting.Config.Loader;
using Cysharp.Threading.Tasks;

using EnemyTableLoader = Everlasting.Config.Internal.EnemyTableLoader;

namespace Everlasting.Config
{
	public class EnemyTable
	{
		public readonly uint Id;
		public readonly string EnemyName;
		public readonly int EnemyHP;
		
		public EnemyTable(uint Id, string EnemyName, int EnemyHP)
		{
			this.Id = Id;
			this.EnemyName = EnemyName;
			this.EnemyHP = EnemyHP;
		}
		public static int Count => EnemyTableLoader.ConfigCount;
		public static EnemyTable GetTableData(uint id) => EnemyTableLoader.ConfigDic.TryGetValue(id, out var cfg) ? cfg : null;
		public static IEnumerable<EnemyTable> All => EnemyTableLoader.ConfigList;
		
	}
	static partial class Internal
	{
		public static class EnemyTableLoader
		{
			public static EnemyTable[] ConfigList {get; private set;}
			public static readonly Dictionary<uint, EnemyTable> ConfigDic = new Dictionary<uint, EnemyTable>();
			public static int ConfigCount;
			
			private static string[] poolString = null;
			
			public static async UniTask Load()
			{
				var binary = await ConfigLoader.LoadConfigBytesAsync($"Excel/enemyTable.bytes");
				
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
					ConfigList = new EnemyTable[count];
					for(int i = 0; i < count; i++)
					{
						var Id = binary.ReadUInt();
						var EnemyName = poolString[binary.ReadInt()];
						var EnemyHP = binary.ReadInt();
						var cfg = new EnemyTable(Id, EnemyName, EnemyHP);
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
