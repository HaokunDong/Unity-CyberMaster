using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wwise_Hit : MonoBehaviour
{

    public AK.Wwise.Event BlockHit_1;
    public AK.Wwise.Event BounceHit_1;
    public AK.Wwise.Event BounceHit_2;


    private void Play_BlockHit_1()
    {
        BlockHit_1.Post(gameObject);
    }



    private void Play_BounceHit_1()
    {
        BounceHit_1.Post(gameObject);
    }



    private void Play_BounceHit_2()
    {
        BounceHit_2.Post(gameObject);
    }


}
