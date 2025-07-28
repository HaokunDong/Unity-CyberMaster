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
		public readonly float MoveSpeed;
		public readonly float RunSpeed;
		public readonly float JumpForce;
		public readonly Vector2[] AttackDistance;
		
		public PlayerTable(uint Id, string Prefab, string Graph, float MoveSpeed, float RunSpeed, float JumpForce, Vector2[] AttackDistance)
		{
			this.Id = Id;
			this.Prefab = Prefab;
			this.Graph = Graph;
			this.MoveSpeed = MoveSpeed;
			this.RunSpeed = RunSpeed;
			this.JumpForce = JumpForce;
			this.AttackDistance = AttackDistance;
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
			private static Vector2[][] poolVector2Array = null;
			
			private static Vector2[] LoadVector2Array(IConfigBinary binary)
			{
				var count = binary.ReadInt();
				var arr = new Vector2[count];
				for(int i = 0; i < count; i++)
				{
					arr[i] = new Vector2(binary.ReadFloat(),binary.ReadFloat());
				}
				return arr;
			}
			
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
					poolVector2Array =  new Vector2[count][];
					for(int i = 0; i < count; i++)
					{
						poolVector2Array[i] = LoadVector2Array(binary);
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
						var MoveSpeed = binary.ReadFloat();
						var RunSpeed = binary.ReadFloat();
						var JumpForce = binary.ReadFloat();
						var AttackDistance = poolVector2Array[binary.ReadInt()];
						var cfg = new PlayerTable(Id, Prefab, Graph, MoveSpeed, RunSpeed, JumpForce, AttackDistance);
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
