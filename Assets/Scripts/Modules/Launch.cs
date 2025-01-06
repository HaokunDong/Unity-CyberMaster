using UnityEngine;

public class Launch : MonoBehaviour
{
    public GameObject root;

    private void Awake()
    {
        OP.Ins.root = transform;
    }
    private void Start()
    {
        GameManager.Ins.Start();
    }
    private void Update()
    {
        TM.OnUpdate();
        CT.OnUpdate();
        GameManager.Ins.OnUpdate();
    }
}