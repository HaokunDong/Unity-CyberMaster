using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using Localization;
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
        ManagerCenter.Ins.TickersMgr?.Update(Time.deltaTime);
        ManagerCenter.Ins.CooldownMgr?.TickByMode(UpdateMode.Update, Time.deltaTime, Time.unscaledDeltaTime);
    }
    #region 初始化
    private async UniTask Init()
    {
        CoroutineMgr.Init();
        ManagerCenter.Ins.Init();
        await ResourceManager.Init();
        await InitTableData();
        await InitScriptableObjects();
        LocalizationManager.setting.selectedLanguage = LocalizationManager.String2Lan(PlayerPrefs.GetString(PlayerPrefsKey.Language, "Chinese"));
        LogUtils.Warning($"当前语言:{LocalizationManager.setting.selectedLanguage}");
        LogUtils.Warning($"多语言测试 key:coin text:{LocalizationManager.setting.Get("coin")}");
        await OnInitEnd();
    }

    private void FixedUpdate()
    {
        ManagerCenter.Ins.CooldownMgr?.TickByMode(UpdateMode.FixedUpdate, Time.deltaTime, Time.unscaledDeltaTime);
    }

    private void LateUpdate()
    {
        ManagerCenter.Ins.TickersMgr?.LateUpdate(Time.deltaTime);
        ManagerCenter.Ins.CooldownMgr?.TickByMode(UpdateMode.LateUpdate, Time.deltaTime, Time.unscaledDeltaTime);
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
        var gpObj = await ResourceManager.LoadAssetAsync<GameObject>(GamePlayTable.GetTableData(10003).Prefab, ResType.Prefab);
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