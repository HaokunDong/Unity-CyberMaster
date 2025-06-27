using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Everlasting.Config.Loader;
using Cysharp.Threading.Tasks;

using NPCTableLoader = Everlasting.Config.Internal.NPCTableLoader;

namespace Everlasting.Config
{
	public class NPCTable
	{
		public readonly uint Id;
		public readonly string NPCName;
		public readonly string Prefab;
		public readonly string DialogueHeadPath;
		public readonly string AIPath;
		public readonly string DialoguePath;
		
		public NPCTable(uint Id, string NPCName, string Prefab, string DialogueHeadPath, string AIPath, string DialoguePath)
		{
			this.Id = Id;
			this.NPCName = NPCName;
			this.Prefab = Prefab;
			this.DialogueHeadPath = DialogueHeadPath;
			this.AIPath = AIPath;
			this.DialoguePath = DialoguePath;
		}
		public static int Count => NPCTableLoader.ConfigCount;
		public static NPCTable GetTableData(uint id) => NPCTableLoader.ConfigDic.TryGetValue(id, out var cfg) ? cfg : null;
		public static IEnumerable<NPCTable> All => NPCTableLoader.ConfigList;
		
	}
	static partial class Internal
	{
		public static class NPCTableLoader
		{
			public static NPCTable[] ConfigList {get; private set;}
			public static readonly Dictionary<uint, NPCTable> ConfigDic = new Dictionary<uint, NPCTable>();
			public static int ConfigCount;
			
			private static string[] poolString = null;
			
			public static async UniTask Load()
			{
				var binary = await ConfigLoader.LoadConfigBytesAsync($"Excel/npcTable.bytes");
				
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
					ConfigList = new NPCTable[count];
					for(int i = 0; i < count; i++)
					{
						var Id = binary.ReadUInt();
						var NPCName = poolString[binary.ReadInt()];
						var Prefab = poolString[binary.ReadInt()];
						var DialogueHeadPath = poolString[binary.ReadInt()];
						var AIPath = poolString[binary.ReadInt()];
						var DialoguePath = poolString[binary.ReadInt()];
						var cfg = new NPCTable(Id, NPCName, Prefab, DialogueHeadPath, AIPath, DialoguePath);
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
