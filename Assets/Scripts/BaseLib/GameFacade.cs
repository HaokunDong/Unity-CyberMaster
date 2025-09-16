using Cysharp.Threading.Tasks;
using Everlasting.Config;
using GameBase.Log;
using Localization;
using Managers;
using System.IO;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
public class GameFacade : MonoBehaviour
{
    public uint GamePlayLevelId = 10003;
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Time.timeScale = 1;
        Init().Forget();
    }

    // Update is called once per frame
    private void Update()
    {
        ManagerCenter.Ins.TickersMgr?.Update(Time.deltaTime);
        ManagerCenter.Ins.CooldownMgr?.TickByMode(UpdateMode.Update, Time.deltaTime, Time.unscaledDeltaTime);
        CustomTimeSystem.Update();
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
        ResourceManager.LoadAssetAsync<GameObject>(Path.Combine("Effects", "RendererFeatureController"), ResType.Prefab).Forget();
        ResourceManager.LoadAssetAsync<GameObject>(Path.Combine("Effects", "GhostTrail"), ResType.Prefab).Forget();
#if UNITY_EDITOR
        GamePlayLevelId = (uint)EditorPrefs.GetInt("SelectedGamePlayLevelId", (int)GamePlayLevelId);
#endif
        var root = await World.Ins.LoadAGamePlayRoot(GamePlayLevelId, 0, null, Vector3.zero);
        if(root != null)
        {
            World.Ins.PlayerEnterGamePlayRoot(root);
        }
    }
    #endregion

    private void OnDestroy()
    {
        CoroutineMgr.Uninit();
        ResourceManager.Dispose();
    }
}