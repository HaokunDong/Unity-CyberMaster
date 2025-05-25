using GameBase.Tickers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCenter : Singleton<ManagerCenter>
{
    public TickersManager TickersMgr;

    public void Init()
    {
        TickersMgr = new TickersManager();
        TickersMgr.Init();
    }
}
