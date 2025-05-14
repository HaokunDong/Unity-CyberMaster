using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wwise_Player_Basic : MonoBehaviour
{
    public AK.Wwise.Event ATK_1;
    public AK.Wwise.Event ATK_2;
    public AK.Wwise.Event ATK_3;
    public AK.Wwise.Event ATK_4;
    public AK.Wwise.Event ATK_5;
    public AK.Wwise.Event ATK_6;
    public AK.Wwise.Event BeAttacked;
    public AK.Wwise.Event BeStunned;
    public AK.Wwise.Event BounceAttack1;
    public AK.Wwise.Event BounceAttack2;
    public AK.Wwise.Event ChargeAttack;
    public AK.Wwise.Event Dodge;
    public AK.Wwise.Event Fall;
    public AK.Wwise.Event Idle;
    public AK.Wwise.Event Jump;
    public AK.Wwise.Event Landing;
    public AK.Wwise.Event Move;
    public AK.Wwise.Event Posture;
    public AK.Wwise.Event PostureEnter;
    public AK.Wwise.Event PostureMove;
    public AK.Wwise.Event Run;
    public AK.Wwise.Event WallSlide;
    public AK.Wwise.Event Other_Wield_Sword;
    public AK.Wwise.Event EX;


    private void PlayATK_1()
    {
        ATK_1.Post(gameObject);
    }



    private void PlayATK_2()
    {
        ATK_2.Post(gameObject);
    }



    private void PlayATK_3()
    {
        ATK_3.Post(gameObject);
    }



    private void PlayATK_4()
    {
        ATK_4.Post(gameObject);
    }



    private void PlayATK_5()
    {
        ATK_5.Post(gameObject);
    }



    private void PlayATK_6()
    {
        ATK_6.Post(gameObject);
    }



    private void PlayBeAttacked()
    {
        BeAttacked.Post(gameObject);
    }



    private void PlayBeStunned()
    {
        BeStunned.Post(gameObject);
    }



    private void PlayBounceAttack1()
    {
        BounceAttack1.Post(gameObject);
    }



    private void PlayBounceAttack2()
    {
        BounceAttack2.Post(gameObject);
    }



    private void PlayChargeAttack()
    {
        ChargeAttack.Post(gameObject);
    }



    private void PlayDodge()
    {
        Dodge.Post(gameObject);
    }



    private void PlayFall()
    {
        Fall.Post(gameObject);
    }



    private void PlayIdle()
    {
        Idle.Post(gameObject);
    }



    private void PlayJump()
    {
        Jump.Post(gameObject);
    }



    private void PlayLanding()
    {
        Landing.Post(gameObject);
    }



    private void PlayMove()
    {
        Move.Post(gameObject);
    }



    private void PlayPosture()
    {
        Posture.Post(gameObject);
    }



    private void PlayPostureEnter()
    {
        PostureEnter.Post(gameObject);
    }



    private void PlayPostureMove()
    {
        PostureMove.Post(gameObject);
    }



    private void PlayRun()
    {
        Run.Post(gameObject);
    }



    private void PlayWallSlide()
    {
        WallSlide.Post(gameObject);

    }

    private void PlayOtherWieldSword()
    {
        Other_Wield_Sword.Post(gameObject);

    }

    private void PlayEX()
    {
        EX.Post(gameObject);

    }


}