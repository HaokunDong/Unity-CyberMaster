using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Everlasting.Config.Loader;
using Cysharp.Threading.Tasks;

using PlayerTableLoader = Everlasting.Config.Internal.PlayerTableLoader;

namespace Everlasting.Config
{
	public class PlayerTable
	{
		public readonly uint Id;
		public readonly string Prefab;
		public readonly string Graph;
		public readonly float MaxMoveSpeed;
		public readonly float JumpForce;
		public readonly float DashSpeed;
		public readonly string PrimaryAttackSkillPath;
		public readonly string BlockSkillPath;
		
		public PlayerTable(uint Id, string Prefab, string Graph, float MaxMoveSpeed, float JumpForce, float DashSpeed, string PrimaryAttackSkillPath, string BlockSkillPath)
		{
			this.Id = Id;
			this.Prefab = Prefab;
			this.Graph = Graph;
			this.MaxMoveSpeed = MaxMoveSpeed;
			this.JumpForce = JumpForce;
			this.DashSpeed = DashSpeed;
			this.PrimaryAttackSkillPath = PrimaryAttackSkillPath;
			this.BlockSkillPath = BlockSkillPath;
		}
		public static int Count => PlayerTableLoader.ConfigCount;
		public static PlayerTable GetTableData(uint id) => PlayerTableLoader.ConfigDic.TryGetValue(id, out var cfg) ? cfg : null;
		public static IEnumerable<PlayerTable> All => PlayerTableLoader.ConfigList;
		
	}
	static partial class Internal
	{
		public static class PlayerTableLoader
		{
			public static PlayerTable[] ConfigList {get; private set;}
			public static readonly Dictionary<uint, PlayerTable> ConfigDic = new Dictionary<uint, PlayerTable>();
			public static int ConfigCount;
			
			private static string[] poolString = null;
			
			public static async UniTask Load()
			{
				var binary = await ConfigLoader.LoadConfigBytesAsync($"Excel/playerTable.bytes");
				
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
					ConfigList = new PlayerTable[count];
					for(int i = 0; i < count; i++)
					{
						var Id = binary.ReadUInt();
						var Prefab = poolString[binary.ReadInt()];
						var Graph = poolString[binary.ReadInt()];
						var MaxMoveSpeed = binary.ReadFloat();
						var JumpForce = binary.ReadFloat();
						var DashSpeed = binary.ReadFloat();
						var PrimaryAttackSkillPath = poolString[binary.ReadInt()];
						var BlockSkillPath = poolString[binary.ReadInt()];
						var cfg = new PlayerTable(Id, Prefab, Graph, MaxMoveSpeed, JumpForce, DashSpeed, PrimaryAttackSkillPath, BlockSkillPath);
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
