using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wwise_Boss_Basic : MonoBehaviour
{
    public AK.Wwise.Event BO_ATK01_1;
    public AK.Wwise.Event BO_ATK01_2;
    public AK.Wwise.Event BO_ATK01_3;
    public AK.Wwise.Event BO_ATK02_1;
    public AK.Wwise.Event BO_ATK02_2;
    public AK.Wwise.Event BO_ATK02_3;
    public AK.Wwise.Event BO_ATK02_4;
    public AK.Wwise.Event BO_ATK02_5;
    public AK.Wwise.Event BO_ATK02_6;
    public AK.Wwise.Event BO_BeAttacked;
    public AK.Wwise.Event BO_BeStunned;
    public AK.Wwise.Event BO_BounceAttack1;
    public AK.Wwise.Event BO_BounceAttack2;
    public AK.Wwise.Event BO_ChargeAttack;
    public AK.Wwise.Event BO_Dodge;
    public AK.Wwise.Event BO_Fall;
    public AK.Wwise.Event BO_Idle;
    public AK.Wwise.Event BO_Jump;
    public AK.Wwise.Event BO_Landing;
    public AK.Wwise.Event BO_Move;
    public AK.Wwise.Event BO_Posture;
    public AK.Wwise.Event BO_PostureEnter;
    public AK.Wwise.Event BO_PostureMove;
    public AK.Wwise.Event BO_Run;
    public AK.Wwise.Event BO_WallSlide;


    private void Play_BO_ATK01_1()
    {
        BO_ATK01_1.Post(gameObject);
    }



    private void Play_BO_ATK01_2()
    {
        BO_ATK01_2.Post(gameObject);
    }



    private void Play_BO_ATK01_3()
    {
        BO_ATK01_3.Post(gameObject);
    }



    private void Play_BO_ATK02_1()
    {
        BO_ATK02_1.Post(gameObject);
    }



    private void Play_BO_ATK02_2()
    {
        BO_ATK02_2.Post(gameObject);
    }



    private void Play_BO_ATK02_3()
    {
        BO_ATK02_3.Post(gameObject);
    }


    private void Play_BO_ATK02_4()
    {
        BO_ATK02_4.Post(gameObject);
    }



    private void Play_BO_ATK02_5()
    {
        BO_ATK02_5.Post(gameObject);
    }



    private void Play_BO_ATK02_6()
    {
        BO_ATK02_6.Post(gameObject);
    }



    private void Play_BO_BeAttacked()
    {
        BO_BeAttacked.Post(gameObject);
    }



    private void Play_BO_BeStunned()
    {
        BO_BeStunned.Post(gameObject);
    }



    private void Play_BO_BounceAttack1()
    {
        BO_BounceAttack1.Post(gameObject);
    }



    private void Play_BO_BounceAttack2()
    {
        BO_BounceAttack2.Post(gameObject);
    }



    private void Play_BO_ChargeAttack()
    {
        BO_ChargeAttack.Post(gameObject);
    }



    private void Play_BO_Dodge()
    {
        BO_Dodge.Post(gameObject);
    }



    private void Play_BO_Fall()
    {
        BO_Fall.Post(gameObject);
    }



    private void Play_BO_Idle()
    {
        BO_Idle.Post(gameObject);
    }



    private void Play_BO_Jump()
    {
        BO_Jump.Post(gameObject);
    }



    private void Play_BO_Landing()
    {
        BO_Landing.Post(gameObject);
    }



    private void Play_BO_Move()
    {
        BO_BounceAttack2.Post(gameObject);
    }



    private void Play_BO_Posture()
    {
        BO_Posture.Post(gameObject);
    }



    private void Play_BO_PostureEnter()
    {
        BO_PostureEnter.Post(gameObject);
    }



    private void Play_BO_PostureMove()
    {
        BO_PostureMove.Post(gameObject);
    }



    private void Play_BO_Run()
    {
        BO_Run.Post(gameObject);
    }



    private void Play_BO_WallSlide()
    {
        BO_WallSlide.Post(gameObject);



    }

}