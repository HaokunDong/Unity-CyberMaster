using GameBase.Tickers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCenter : Singleton<ManagerCenter>
{
    public TickersManager TickersMgr;
    public CooldownManager CooldownMgr;
    public TimeLineManager TimeLineMgr;

    public void Init()
    {
        TickersMgr = new TickersManager();
        TickersMgr.Init();

        CooldownMgr = new CooldownManager();
        CooldownMgr.Init();

        TimeLineMgr = new TimeLineManager();
        TimeLineMgr.Init();
    }
}
