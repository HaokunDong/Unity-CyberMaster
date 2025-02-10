using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig", order = 0)]
public class GameConfig : ScriptableObject
{
    [Header("玩家受击扣减刃势值")]
    public float playerDecayLife_hitted = 15f;
    [Header("玩家防御扣减刃势值")]
    public float playerDecayLife_defense = 5f;
    [Header("玩家弹反判定时间（前）")]
    public float playerBounceTimeBefore = 0.05f;
    [Header("玩家弹反判定时间（后）")]
    public float playerBounceTimeAfter = 0.1f;

}