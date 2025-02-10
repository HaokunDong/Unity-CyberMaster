using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRef : SingletonComp<GlobalRef>
{
    public GameObject barCom;
    public GameConfig cfg;
    public Transform hitEffectFolder;
    public GameObject hitEffectPrefab;
}
