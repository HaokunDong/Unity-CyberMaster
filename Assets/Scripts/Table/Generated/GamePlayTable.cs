using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Everlasting.Config.Loader;
using Cysharp.Threading.Tasks;

using GamePlayTableLoader = Everlasting.Config.Internal.GamePlayTableLoader;

namespace Everlasting.Config
{
	public class GamePlayTable
	{
		public readonly uint Id;
		public readonly string GamePlayName;
		public readonly string Prefab;
		
		public GamePlayTable(uint Id, string GamePlayName, string Prefab)
		{
			this.Id = Id;
			this.GamePlayName = GamePlayName;
			this.Prefab = Prefab;
		}
		public static int Count => GamePlayTableLoader.ConfigCount;
		public static GamePlayTable GetTableData(uint id) => GamePlayTableLoader.ConfigDic.TryGetValue(id, out var cfg) ? cfg : null;
		public static IEnumerable<GamePlayTable> All => GamePlayTableLoader.ConfigList;
		
	}
	static partial class Internal
	{
		public static class GamePlayTableLoader
		{
			public static GamePlayTable[] ConfigList {get; private set;}
			public static readonly Dictionary<uint, GamePlayTable> ConfigDic = new Dictionary<uint, GamePlayTable>();
			public static int ConfigCount;
			
			private static string[] poolString = null;
			
			public static async UniTask Load()
			{
				var binary = await ConfigLoader.LoadConfigBytesAsync($"Excel/gameplayTable.bytes");
				
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
					ConfigList = new GamePlayTable[count];
					for(int i = 0; i < count; i++)
					{
						var Id = binary.ReadUInt();
						var GamePlayName = poolString[binary.ReadInt()];
						var Prefab = poolString[binary.ReadInt()];
						var cfg = new GamePlayTable(Id, GamePlayName, Prefab);
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
