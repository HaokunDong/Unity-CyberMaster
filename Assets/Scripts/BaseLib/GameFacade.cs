using Cysharp.Threading.Tasks;
using Everlasting.Config;
using Managers;
using System.Threading.Tasks;
using UnityEngine;

public class GameFacade : MonoBehaviour
{
#if UNITY_EDITOR
    public bool testCurrentScene = false;
#endif

    // Start is called before the first frame update
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = BattleData.FRAME_RATE;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Init().Forget();
    }

    // Update is called once per frame
    private void Update()
    {
        ManagerCenter.Ins.TickersMgr.Update(Time.deltaTime);
    }
    #region 初始化
    private async UniTask Init()
    {
        CoroutineMgr.Init();
        ManagerCenter.Ins.Init();
        await ResourceManager.Init();
        await InitTableData();
        await InitScriptableObjects();
        await OnInitEnd();
    }

    private void LateUpdate()
    {
        ManagerCenter.Ins.TickersMgr.LateUpdate(Time.deltaTime);
    }

    private async UniTask InitTableData()
    {
        await TableDataManager.Load();
    }

    private async UniTask InitScriptableObjects()
    {
        await ResourceManager.LoadFolderAb("Res/ScriptableObjects");
    }
    #endregion

    #region 初始化结束后
    private async Task OnInitEnd()
    {
        var gpObj = await ResourceManager.LoadAssetAsync<GameObject>(GamePlayTable.GetTableData(10001).Prefab, ResType.Prefab);
        var gp = gpObj.GetComponent<GamePlayRoot>();
        gp.Init();
    }
    #endregion

    private void OnDestroy()
    {
        CoroutineMgr.Uninit();
        ResourceManager.Dispose();
    }
}