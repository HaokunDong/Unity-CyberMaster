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
        //Application.targetFrameRate = 120;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Init().Forget();
    }

    // Update is called once per frame
    private void Update()
    {
        ManagerCenter.Ins.TickersMgr.Update(Time.deltaTime);
        ManagerCenter.Ins.CooldownManager.TickByMode(UpdateMode.Update, Time.deltaTime, Time.unscaledDeltaTime);
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

    private void FixedUpdate()
    {
        ManagerCenter.Ins.CooldownManager.TickByMode(UpdateMode.FixedUpdate, Time.deltaTime, Time.unscaledDeltaTime);
    }

    private void LateUpdate()
    {
        ManagerCenter.Ins.TickersMgr.LateUpdate(Time.deltaTime);
        ManagerCenter.Ins.CooldownManager.TickByMode(UpdateMode.LateUpdate, Time.deltaTime, Time.unscaledDeltaTime);
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
        gp.Init().Forget();
    }
    #endregion

    private void OnDestroy()
    {
        CoroutineMgr.Uninit();
        ResourceManager.Dispose();
    }
}