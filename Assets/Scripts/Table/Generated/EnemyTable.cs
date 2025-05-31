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
		public readonly string Prefab;
		public readonly string Graph;
		public readonly float CheckFaceFlipTime;
		public readonly float MoveSpeed;
		public readonly float JumpForce;
		public readonly float AttackDistance;
		public readonly float ComboAttackCD;
		public readonly Vector2[] AttackMoveSpeed;
		public readonly float MoveCD;
		public readonly float LeapAttackCD;
		public readonly float LeapAttackMoveSpeed;
		public readonly float LeapAttackRadius;
		public readonly float StabDistance;
		public readonly float StabAttackCD;
		public readonly float StabAttackMoveSpeed;
		public readonly Vector2 StabAttackSize;
		public readonly float PlayerTooFarRange;
		public readonly string PrimaryAttackSkillPath;
		
		public EnemyTable(uint Id, string EnemyName, string Prefab, string Graph, float CheckFaceFlipTime, float MoveSpeed, float JumpForce, float AttackDistance, float ComboAttackCD, Vector2[] AttackMoveSpeed, float MoveCD, float LeapAttackCD, float LeapAttackMoveSpeed, float LeapAttackRadius, float StabDistance, float StabAttackCD, float StabAttackMoveSpeed, Vector2 StabAttackSize, float PlayerTooFarRange, string PrimaryAttackSkillPath)
		{
			this.Id = Id;
			this.EnemyName = EnemyName;
			this.Prefab = Prefab;
			this.Graph = Graph;
			this.CheckFaceFlipTime = CheckFaceFlipTime;
			this.MoveSpeed = MoveSpeed;
			this.JumpForce = JumpForce;
			this.AttackDistance = AttackDistance;
			this.ComboAttackCD = ComboAttackCD;
			this.AttackMoveSpeed = AttackMoveSpeed;
			this.MoveCD = MoveCD;
			this.LeapAttackCD = LeapAttackCD;
			this.LeapAttackMoveSpeed = LeapAttackMoveSpeed;
			this.LeapAttackRadius = LeapAttackRadius;
			this.StabDistance = StabDistance;
			this.StabAttackCD = StabAttackCD;
			this.StabAttackMoveSpeed = StabAttackMoveSpeed;
			this.StabAttackSize = StabAttackSize;
			this.PlayerTooFarRange = PlayerTooFarRange;
			this.PrimaryAttackSkillPath = PrimaryAttackSkillPath;
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
					poolVector2Array =  new Vector2[count][];
					for(int i = 0; i < count; i++)
					{
						poolVector2Array[i] = LoadVector2Array(binary);
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
						var Prefab = poolString[binary.ReadInt()];
						var Graph = poolString[binary.ReadInt()];
						var CheckFaceFlipTime = binary.ReadFloat();
						var MoveSpeed = binary.ReadFloat();
						var JumpForce = binary.ReadFloat();
						var AttackDistance = binary.ReadFloat();
						var ComboAttackCD = binary.ReadFloat();
						var AttackMoveSpeed = poolVector2Array[binary.ReadInt()];
						var MoveCD = binary.ReadFloat();
						var LeapAttackCD = binary.ReadFloat();
						var LeapAttackMoveSpeed = binary.ReadFloat();
						var LeapAttackRadius = binary.ReadFloat();
						var StabDistance = binary.ReadFloat();
						var StabAttackCD = binary.ReadFloat();
						var StabAttackMoveSpeed = binary.ReadFloat();
						var StabAttackSize = new Vector2(binary.ReadFloat(),binary.ReadFloat());
						var PlayerTooFarRange = binary.ReadFloat();
						var PrimaryAttackSkillPath = poolString[binary.ReadInt()];
						var cfg = new EnemyTable(Id, EnemyName, Prefab, Graph, CheckFaceFlipTime, MoveSpeed, JumpForce, AttackDistance, ComboAttackCD, AttackMoveSpeed, MoveCD, LeapAttackCD, LeapAttackMoveSpeed, LeapAttackRadius, StabDistance, StabAttackCD, StabAttackMoveSpeed, StabAttackSize, PlayerTooFarRange, PrimaryAttackSkillPath);
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
