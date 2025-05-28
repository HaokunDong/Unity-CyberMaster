using GameBase.Tickers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCenter : Singleton<ManagerCenter>
{
    public TickersManager TickersMgr;
    public CooldownManager CooldownManager;

    public void Init()
    {
        TickersMgr = new TickersManager();
        TickersMgr.Init();

        CooldownManager = new CooldownManager();
        CooldownManager.Init();
    }
}
