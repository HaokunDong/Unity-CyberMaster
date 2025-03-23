using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadInputManager : MonoBehaviour
{
    public static GamepadInputManager Instance { get; private set; }

    public bool BlockPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool ChargeAttackPressed { get; private set; }
    public bool DodgePressed { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {

        // 🎮 L1（格挡） 对应 Space
        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            BlockPressed = true;
            Debug.Log("🛡️ 按下 L1（格挡）");
        }
        else
        {
            BlockPressed = false;
        }

        // 🎮 Square（方块） 对应 J（普通攻击）
        if (Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            AttackPressed = true;
            Debug.Log("⚔️ 按下 Square（攻击）");
        }
        else
        {
            AttackPressed = false;
        }

        // 🎮 Triangle（三角） 对应 I（蓄力攻击）
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            ChargeAttackPressed = true;
            Debug.Log("🔺 按下 Triangle（蓄力攻击）");
        }
        else
        {
            ChargeAttackPressed = false;
        }

        // 🎮 Circle（圆圈） 对应 L（闪避）
        if (Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            DodgePressed = true;
            Debug.Log("⭕ 按下 Circle（闪避）");
        }
        else
        {
            DodgePressed = false;
        }
    }
}
