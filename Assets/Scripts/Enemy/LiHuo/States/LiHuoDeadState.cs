using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiHuoDeadState : EnemyState
{
    protected Enemy_Boss_LiHuo liHuo;
    protected Transform playerTrans;
    public LiHuoDeadState(EnemyStateMachine _stateMachine, Enemy _enemy, string _animBoolName, Enemy_Boss_LiHuo _liHuo)
        : base(_stateMachine, _enemy, _animBoolName)
    {
        liHuo = _liHuo;
    }

    public override void Enter()
    {
        base.Enter();

        playerTrans = PlayerManager.Ins.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        Debug.Log(GameManager.Ins.interactionDistance);

        if (liHuo.RelativeDistance() <= GameManager.Ins.interactionDistance)
        {
            Debug.Log("Player Come Here.");
            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("Execution");
                stateMachine.ChangeState(liHuo.beExecutedState);
            }
        }
    }
}
