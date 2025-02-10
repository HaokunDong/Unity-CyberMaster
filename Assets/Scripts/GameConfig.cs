using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig", order = 0)]
public class GameConfig : ScriptableObject
{
    [Header("玩家受击扣减刃势值")]
    public float playerDecayLife_hitted = 15f;
    [Header("玩家防御扣减刃势值")]
    public float playerDecayLife_defense = 5f;
    [Header("玩家弹反增加刃势值")]
    public float playerIncreaseLife_bounce = 10f;
    [Header("玩家攻击增加刃势值")]
    public float playerIncreaseLife_attack = 3f;

}