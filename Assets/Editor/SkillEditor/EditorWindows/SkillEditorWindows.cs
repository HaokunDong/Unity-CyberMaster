using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using Tools;
using Color = UnityEngine.Color;

public class SkillEditorWindows : EditorWindow
{
    public static SkillEditorWindows Instance;

    private List<Box> DrawHitBoxes = null;
    private int selectDrawHitBox = -1;
    public static TrackItemBase selectedTrackItem = null;

    [MenuItem("Tools/技能编辑器 %&s")]
    public static void ShowExample()
    {
        SkillEditorWindows wnd = GetWindow<SkillEditorWindows>();
        wnd.titleContent = new GUIContent("技能编辑器");
    }

    private VisualElement root;
    public void CreateGUI()
    {
        SkillConfig.SetValidateAction(ResetView);

        Instance = this;
        root = rootVisualElement;
        selectedTrackItem = null;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SkillEditor/EditorWindows/SkillEditorWindows.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        InitTopMenu();
        InitTimeShaft();
        InitConsole();
        InitContent();

        skillConfig = LastConfig;

        if (skillConfig != null)
        {
            SkillConfigObjectField.value = skillConfig;
            CurrentFrameCount = skillConfig.FrameCount;
        }
        else
        {
            CurrentFrameCount = 100;
        }

        if (currentPreviewCharacterPrefab != null)
        {
            PreviewCharacterPrefabObjectField.value = currentPreviewCharacterPrefab;
        }
        if (currentPreviewCharacterObj != null)
        {
            PreviewCharacterObjectField.value = currentPreviewCharacterObj;
        }

        CurrentSelectFrameIndex = 0;
    }

    private void ResetView()
    {
        //ResetTrackData();
        //UpdateContentSize();
        //ResetTrack();

        SkillConfig temp = skillConfig;
        SkillConfigObjectField.value = null;
        SkillConfigObjectField.value = temp;
    }

    private void OnDestroy()
    {
        if (skillConfig != null) SaveConfig();
    }


    #region TopMenu 
    private const string skillEditorScenePath = "Assets/SkillEditor/SkillEditorScene.unity";
    private const string PreviewCharacterParentPath = "PreviewCharacterRoot";
    private string oldScenePath;

    private Button LoadEditorSceneButton;
    private Button LoadOldSceneButton;
    private Button SkillBasicButton;
    private Slider PlaySpeed;

    private ObjectField PreviewCharacterPrefabObjectField;
    private ObjectField PreviewCharacterObjectField;
    private ObjectField SkillConfigObjectField;
    private GameObject currentPreviewCharacterPrefab;
    private GameObject currentPreviewCharacterObj;
    private Toggle IsLoopSkillToggle;

    public GameObject PreviewCharacterObj { get => currentPreviewCharacterObj; }

    private void InitTopMenu()
    {
        LoadEditorSceneButton = root.Q<Button>(nameof(LoadEditorSceneButton));
        LoadOldSceneButton = root.Q<Button>(nameof(LoadOldSceneButton));
        SkillBasicButton = root.Q<Button>(nameof(SkillBasicButton));
        PlaySpeed = root.Q<Slider>(nameof(PlaySpeed));
        PlaySpeed.value = 1f;

        PreviewCharacterPrefabObjectField = root.Q<ObjectField>(nameof(PreviewCharacterPrefabObjectField));
        PreviewCharacterObjectField = root.Q<ObjectField>(nameof(PreviewCharacterObjectField));
        SkillConfigObjectField = root.Q<ObjectField>(nameof(SkillConfigObjectField));
        IsLoopSkillToggle = root.Q<Toggle>(nameof(IsLoopSkillToggle));

        LoadEditorSceneButton.clicked += LoadEditorSceneButtonClick;
        LoadOldSceneButton.clicked += LoadOldSceneButtonClick;
        SkillBasicButton.clicked += SkillBasicButtonClick;

        PreviewCharacterPrefabObjectField.RegisterValueChangedCallback(PreviewCharacterPrefabObjectFieldChanged);
        PreviewCharacterObjectField.RegisterValueChangedCallback(PreviewCharacterObjectFielChanged);
        SkillConfigObjectField.RegisterValueChangedCallback(SkillConfigObjectFieldChanged);
        IsLoopSkillToggle.RegisterValueChangedCallback(evt =>
        {
            if (skillConfig != null)
            {
                skillConfig.isLoopSkill = evt.newValue;
                SaveConfig();
            }
        });
    }



    /// <summary>
    /// 加载编辑器场景
    /// </summary>
    private void LoadEditorSceneButtonClick()
    {
        string currentpath = EditorSceneManager.GetActiveScene().path;
        if (currentpath == skillEditorScenePath) return;

        oldScenePath = currentpath;
        EditorSceneManager.OpenScene(skillEditorScenePath);
    }

    /// <summary>
    /// 回归旧场景
    /// </summary>
    private void LoadOldSceneButtonClick()
    {
        if (!string.IsNullOrEmpty(oldScenePath))
        {
            string currentpath = EditorSceneManager.GetActiveScene().path;
            if (currentpath == oldScenePath) return;

            EditorSceneManager.OpenScene(oldScenePath);
        }
        else
        {
            Debug.LogWarning("场景不存在");
        }
    }

    /// <summary>
    /// 查看技能基本信息
    /// </summary>
    private void SkillBasicButtonClick()
    {
        if (skillConfig != null)
        {
            Selection.activeObject = skillConfig;
        }
    }

    /// <summary>
    /// 角色预制体修改
    /// </summary>
    /// <param name="evt"></param>
    private void PreviewCharacterPrefabObjectFieldChanged(ChangeEvent<UnityEngine.Object> evt)
    {
        //避免在其他场景实例化
        string currentpath = EditorSceneManager.GetActiveScene().path;
        if (currentpath != skillEditorScenePath)
        {
            PreviewCharacterPrefabObjectField.value = null;
            return;
        }

        if (evt.newValue == currentPreviewCharacterPrefab) return;
        currentPreviewCharacterPrefab = (GameObject)evt.newValue;

        //销毁旧的
        if (currentPreviewCharacterObj != null) DestroyImmediate(currentPreviewCharacterObj);

        Transform parent = GameObject.Find(PreviewCharacterParentPath).transform;
        if (parent != null && parent.childCount > 0)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }
        //实例化新的
        if (evt.newValue != null)
        {
            currentPreviewCharacterObj = Instantiate(evt.newValue as GameObject, Vector3.zero, Quaternion.identity, parent);
            currentPreviewCharacterObj.transform.localEulerAngles = Vector3.zero;
            PreviewCharacterObjectField.value = currentPreviewCharacterObj;
        }
    }


    /// <summary>
    /// 角色预览对象修改
    /// </summary>
    /// <param name="evt"></param>
    private void PreviewCharacterObjectFielChanged(ChangeEvent<UnityEngine.Object> evt)
    {
        currentPreviewCharacterObj = (GameObject)evt.newValue;
    }



    /// <summary>
    /// 技能配置修改
    /// </summary>
    /// <param name="evt"></param>
    private void SkillConfigObjectFieldChanged(ChangeEvent<UnityEngine.Object> evt)
    {
        skillConfig = evt.newValue as SkillConfig;
        IsLoopSkillToggle.value = skillConfig?.isLoopSkill ?? false;
        CurrentSelectFrameIndex = 0;
        if (skillConfig == null)
        {
            CurrentFrameCount = 100;
        }
        else
        {
            AutoTrimByAnimationData(skillConfig);
            CurrentFrameCount = skillConfig.FrameCount;
        }
        DestroyTracks();
        //刷新轨道
        ResetTrack();
        LastConfig = skillConfig;
    }

    #endregion Config

    #region TimeShaft
    private IMGUIContainer timeShaft;//时间轴容器
    private IMGUIContainer selectLine;// 
    private VisualElement contentContainer;// ScrollView 容器,方便得出  ScrollView 往左往右拽的尺寸坐标 
    private VisualElement contentViewPort; //时间线的显示区域  

    private int currentSelectFrameIndex = -1;
    /// <summary>
    /// 鼠标位置+下方容器的位置
    /// </summary>
    public int CurrentSelectFrameIndex
    {
        get => currentSelectFrameIndex;
        set
        {
            //如果超出范围，更新最大帧
            if (value > CurrentFrameCount) CurrentFrameCount = value;
            currentSelectFrameIndex = Mathf.Clamp(value, 0, CurrentFrameCount);
            CurrentFrameTextField.value = currentSelectFrameIndex;
            UpdateTimerShaftView();
            SetDrawHitBoxes(skillConfig?.SkillHitBoxData?.TryGetHitBoxClipAtFrameBinary(currentSelectFrameIndex)?.HitBoxs);
            TickSkill();
        }
    }

    private int currentFrameCount;
    public int CurrentFrameCount
    {
        get => currentFrameCount;
        set
        {
            //if (currentFrameCount == value) return;

            currentFrameCount = value;
            FrameCountTextField.value = currentFrameCount;

            //同步给 skillConfig
            if (skillConfig != null)
            {
                skillConfig.FrameCount = currentFrameCount;
            }

            //Content 区域的尺寸变化
            UpdateContentSize();
        }
    }

    /// <summary>
    /// 当前内容区域的偏移坐标
    /// </summary>
    private float contentOffsetPos { get => Mathf.Abs(contentContainer.transform.position.x); }
    /// <summary>
    /// 当前帧在时间轴的像素坐标位置（鼠标位置+下方容器的移动位置）
    /// </summary>
    private float currentSelectFramePos { get => CurrentSelectFrameIndex * skillEditorConfig.FrameUnitWidth; }


    private bool timeShaftIsMouseEnter = false;

    private void InitTimeShaft()
    {
        ScrollView MainContentView = root.Q<ScrollView>("MainContentView");
        contentContainer = MainContentView.Q<VisualElement>("unity-content-container");
        contentViewPort = MainContentView.Q<VisualElement>("unity-content-viewport");

        timeShaft = root.Q<IMGUIContainer>("TimeShaft");
        selectLine = root.Q<IMGUIContainer>("SelectLine");


        timeShaft.onGUIHandler = DrawTimeShaft;
        timeShaft.RegisterCallback<WheelEvent>(TimeShaftWheel);
        timeShaft.RegisterCallback<MouseDownEvent>(TimeShaftMouseDown);
        timeShaft.RegisterCallback<MouseMoveEvent>(TimeShaftMouseMove);
        timeShaft.RegisterCallback<MouseUpEvent>(TimeShaftMouseUp);
        timeShaft.RegisterCallback<MouseOutEvent>(TimeShaftMouseOut);

        selectLine.onGUIHandler = DrawSelectLine;
    }

    private void DrawTimeShaft()
    {
        Handles.BeginGUI();
        Handles.color = Color.white;
        Rect rect = timeShaft.contentRect; //时间轴的尺寸

        //起始索引
        int index = Mathf.CeilToInt(contentOffsetPos / skillEditorConfig.FrameUnitWidth);
        //计算绘制起点的偏移
        float startOffset = 0;
        //10-(98 % 10)
        //=10-8=2
        if (index > 0) startOffset = skillEditorConfig.FrameUnitWidth - (contentOffsetPos % skillEditorConfig.FrameUnitWidth);

        int tickStep = SkillEditorConfig.MaxFrameWidthLV + 1 - (skillEditorConfig.FrameUnitWidth / SkillEditorConfig.StandframeUnitWidth);
        //tickStep = 10+1-(100/10)=1
        //tickStep = 11-9=2
        //tickStep = 11-8=3
        //tickStep = 11-1=10

        tickStep = Mathf.Clamp(tickStep / 2, 1, SkillEditorConfig.MaxFrameWidthLV);

        for (float i = startOffset; i < rect.width; i += skillEditorConfig.FrameUnitWidth)
        {
            //绘制长线条、文本
            if (index % tickStep == 0)
            {
                Handles.DrawLine(new Vector3(i, rect.height - 10), new Vector3(i, rect.height));
                string indexStr = index.ToString();
                GUI.Label(new Rect(i - indexStr.Length * 4.5f, 0, 35, 20), indexStr);
            }
            else
            {
                Handles.DrawLine(new Vector3(i, rect.height - 5), new Vector3(i, rect.height));
            }

            index += 1;
        }
        Handles.EndGUI();
    }

    private void TimeShaftWheel(WheelEvent evt)
    {
        int delta = (int)evt.delta.y;
        skillEditorConfig.FrameUnitWidth = Mathf.Clamp(skillEditorConfig.FrameUnitWidth - delta,
            SkillEditorConfig.StandframeUnitWidth, SkillEditorConfig.MaxFrameWidthLV * SkillEditorConfig.StandframeUnitWidth);

        UpdateTimerShaftView();
        UpdateContentSize();

        //TrackItem 的ResetView
        ResetTrack();
    }


    private void TimeShaftMouseDown(MouseDownEvent evt)
    {
        //让选中线位置卡在帧的位置上
        timeShaftIsMouseEnter = true;
        IsPlaying = false;
        int newValue = GetFrameIndexByMousePos(evt.localMousePosition.x);
        if (CurrentSelectFrameIndex != newValue)
        {
            CurrentSelectFrameIndex = newValue;
        }
    }
    private void TimeShaftMouseMove(MouseMoveEvent evt)
    {
        if (timeShaftIsMouseEnter)
        {
            int newValue = GetFrameIndexByMousePos(evt.localMousePosition.x);
            if (CurrentSelectFrameIndex != newValue)
            {
                CurrentSelectFrameIndex = newValue;
            }
        }
    }

    private void TimeShaftMouseUp(MouseUpEvent evt)
    {
        timeShaftIsMouseEnter = false;
    }

    private void TimeShaftMouseOut(MouseOutEvent evt)
    {
        timeShaftIsMouseEnter = false;
    }

    /// <summary>
    /// 根据鼠标坐标获取帧像素索引；
    /// 鼠标位置+下方容器的位置
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private int GetFrameIndexByMousePos(float x)
    {
        return GetFrameIndexByPos(x + contentOffsetPos);
    }

    public int GetFrameIndexByPos(float x)
    {
        return Mathf.RoundToInt(x / skillEditorConfig.FrameUnitWidth);
    }


    private void DrawSelectLine()
    {
        //判断当前选中帧是否在视图范围内
        if (currentSelectFramePos >= contentOffsetPos)
        {
            Handles.BeginGUI();
            Handles.color = Color.white;
            float x = currentSelectFramePos - contentOffsetPos;
            Handles.DrawLine(new Vector3(x, 0), new Vector3(x, contentViewPort.contentRect.height + timeShaft.contentRect.height));
            Handles.EndGUI();
        }
    }

    private void UpdateTimerShaftView()
    {
        timeShaft.MarkDirtyLayout();//标记为需要立刻重新绘制的
        selectLine.MarkDirtyLayout();//标记为需要立刻重新绘制的
    }

    private void AutoTrimByAnimationData(SkillConfig config)
    {
        var maxFrame = 0;
        var tracks = config.GetTracks();
        foreach (var kvp in config.SkillAnimationData.skillClipDict)
        {
            if (kvp.Key + kvp.Value.DurationFrame > maxFrame)
            {
                maxFrame = kvp.Key + kvp.Value.DurationFrame;
            }
        }
        config.FrameCount = maxFrame;
        SaveConfig();
    }

    #endregion

    #region Console
    private Button ZeroFrameButton;
    private Button PreviouFrameButton;
    private Button PlayButton;
    private Button NextFrameButton;
    private IntegerField CurrentFrameTextField;
    private IntegerField FrameCountTextField;

    private void InitConsole()
    {
        ZeroFrameButton = root.Q<Button>(nameof(ZeroFrameButton));
        PreviouFrameButton = root.Q<Button>(nameof(PreviouFrameButton));
        PlayButton = root.Q<Button>(nameof(PlayButton));
        NextFrameButton = root.Q<Button>(nameof(NextFrameButton));

        CurrentFrameTextField = root.Q<IntegerField>(nameof(CurrentFrameTextField));
        FrameCountTextField = root.Q<IntegerField>(nameof(FrameCountTextField));

        ZeroFrameButton.clicked += ZeroFrameButtonClicked;
        PreviouFrameButton.clicked += PreviouFrameButtonClicked;
        PlayButton.clicked += PlayButtonClicked;
        NextFrameButton.clicked += NextFrameButtonClicked;

        CurrentFrameTextField.RegisterValueChangedCallback(CurrentFrameTextFieldValueChanged);
        FrameCountTextField.RegisterValueChangedCallback(FrameCountTextFieldValueChanged);

    }

    private void ZeroFrameButtonClicked()
    {
        IsPlaying = false;
        CurrentSelectFrameIndex = 0;
    }

    private void PreviouFrameButtonClicked()
    {
        IsPlaying = false;
        CurrentSelectFrameIndex -= 1;
    }

    private void PlayButtonClicked()
    {
        IsPlaying = !IsPlaying;
    }

    private void NextFrameButtonClicked()
    {
        IsPlaying = false;
        CurrentSelectFrameIndex += 1;
    }

    private void CurrentFrameTextFieldValueChanged(ChangeEvent<int> evt)
    {
        if (CurrentSelectFrameIndex != evt.newValue) CurrentSelectFrameIndex = evt.newValue;
    }
    private void FrameCountTextFieldValueChanged(ChangeEvent<int> evt)
    {
        if (CurrentFrameCount != evt.newValue) CurrentFrameCount = evt.newValue;
    }


    #endregion

    #region config
    private static SkillConfig LastConfig;

    private SkillConfig skillConfig;
    public SkillConfig SkillConfig { get => skillConfig; }
    private SkillEditorConfig skillEditorConfig = new SkillEditorConfig();

    public void SaveConfig()
    {
        if (skillConfig != null)
        {
            EditorUtility.SetDirty(skillConfig);
            AssetDatabase.SaveAssetIfDirty(skillConfig);
            skillConfig.SkillHitBoxData.OnSave();
        }
    }

    #endregion

    #region  Track
    private VisualElement trackMenuParent;
    private VisualElement ContentListView;
    private ScrollView MainContentView;
    private List<EditorSkillTrackBase> trackList = new List<EditorSkillTrackBase>();

    private void InitContent()
    {
        trackMenuParent = root.Q<VisualElement>("TrackMenuList");
        ContentListView = root.Q<VisualElement>(nameof(ContentListView));
        MainContentView = root.Q<ScrollView>(nameof(MainContentView));
        MainContentView.verticalScroller.valueChanged += MainContentViewverticalValueChanged;
        UpdateContentSize();

        InitTrack();
    }

    private void MainContentViewverticalValueChanged(float obj)
    {
        Vector3 pos = trackMenuParent.transform.position;
        pos.y = contentContainer.transform.position.y;
        trackMenuParent.transform.position = pos;
    }

    private void InitTrack()
    {
        if (skillConfig == null) return;
        InitTrack<AnimationTrack, SkillAnimationClip>(skillConfig.SkillAnimationData.skillClipDict, "动画");
        InitTrack<AttackTimeWindowTrack, SkillAttackTimeWindowClip>(skillConfig.SkillAttackTimeWindowData.skillClipDict, "攻击时间段");
        InitTrack<HitBoxTrack, SkillHitBoxClip>(skillConfig.SkillHitBoxData.skillClipDict, "打击");
        InitTrack<VelocityTrack, SkillVelocityClip>(skillConfig.SkillVelocityData.skillClipDict, "速度");
    }

    private void OnEnable()
    {
        SetDrawHitBoxes(null);
        if(PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if(prefabStage != null)
            {
                var root = prefabStage.prefabContentsRoot;
                if(root != null)
                {
                    var animator = root.GetComponentInChildren<Animator>(true);
                    currentPreviewCharacterObj = animator?.gameObject;
                }
            }
        }

        // 注册到 SceneView 的绘制事件
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SetDrawHitBoxes(null);
        // 注销事件
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void InitTrack<T, C>(Dictionary<int, C> dict, string title) where T : EditorSkillTrackBase<C> where C : SkillClipBase
    {
        T track = Activator.CreateInstance<T>();
        track.Init(trackMenuParent, ContentListView, skillEditorConfig.FrameUnitWidth, dict, title);
        trackList.Add(track);
    }

    private void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Delete)
            {
                if(selectedTrackItem != null)
                {
                    SkillEditorInspector.currentTrack.DeleteTrackItem(selectedTrackItem.FrameIndex);
                    Selection.activeObject = null;
                    SkillEditorInspector.currentTrack.ResetView();
                    selectedTrackItem = null;
                }
            }
            else if(e.keyCode == KeyCode.LeftArrow)
            {
                CurrentSelectFrameIndex--;
            }
            else if(e.keyCode == KeyCode.RightArrow)
            {
                CurrentSelectFrameIndex++;
            }
        }
    }

    private void ResetTrack()
    {
        if (skillConfig == null)
        {
            //清理掉所有轨道
            DestroyTracks();
        }
        else
        {
            //如果轨道列表里面没有数据，说明没有轨道；存在配置的，需要初始化
            if (trackList.Count == 0)
            {
                InitTrack();
            }

            //更新视图
            for (int i = 0; i < trackList.Count; i++)
            {
                trackList[i].ResetView(skillEditorConfig.FrameUnitWidth);
            }
        }
    }

    private void DestroyTracks()
    {
        for (int i = 0; i < trackList.Count; i++)
        {
            trackList[i].Destroy();
        }
        trackList.Clear();
    }

    /// <summary>
    /// Content 区域的尺寸变化
    /// </summary>
    private void UpdateContentSize()
    {
        ContentListView.style.width = skillEditorConfig.FrameUnitWidth * CurrentFrameCount;
    }

    public void ShowTrackItemOnInspector(TrackItemBase trackItem, EditorSkillTrackBase track)
    {
        SkillEditorInspector.SetTrackItem(trackItem, track);
        Selection.activeObject = this;
    }
    #endregion

    #region Preview
    private bool isPlaying;
    public bool IsPlaying
    {
        get => isPlaying;
        set
        {
            isPlaying = value;
            if (isPlaying)
            {
                startTime = DateTime.Now;
                startFrameIndex = currentSelectFrameIndex;
            }
        }
    }

    private DateTime startTime;
    private int startFrameIndex;


    private void Update()
    {
        if (IsPlaying)
        {
            //得到时间差
            float time = (float)DateTime.Now.Subtract(startTime).TotalSeconds * PlaySpeed.value;

            //确定时间轴的帧率
            float frameRate = skillConfig != null ? skillConfig.FrameRate : skillEditorConfig.DefaultFrameRate;

            //根据时间差计算当前的选中帧
            CurrentSelectFrameIndex = (int)((time * frameRate) + startFrameIndex);

            //到达最后一帧自动暂停
            if (CurrentSelectFrameIndex == CurrentFrameCount)
            {
                IsPlaying = false;
            }
        }
    }

    private void TickSkill()
    {
        //驱动技能表现
        if (skillConfig != null && currentPreviewCharacterObj != null)
        {
            //驱动动画表现
            for (int i = 0; i < trackList.Count; i++)
            {
                trackList[i].TickView(currentSelectFrameIndex);
            }
        }
    }

    private void SetDrawHitBoxes(List<Box> list)
    {
        if(DrawHitBoxes != list)
        {
            selectDrawHitBox = -1;
        }
        DrawHitBoxes = list;
    }

    public void SelectAHitBox(int index)
    {
        selectDrawHitBox = index;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if(DrawHitBoxes != null && DrawHitBoxes.Count > 0)
        {
            var index = 0;
            foreach (var box in DrawHitBoxes)
            {
                var vs = MathEx.GetRotatedRectVertices(box.center, box.size, box.rotation);
                Vector3[] array = new Vector3[4]
                {
                    new Vector3(vs[0].x, vs[0].y, 0f),
                    new Vector3(vs[1].x, vs[1].y, 0f),
                    new Vector3(vs[2].x, vs[2].y, 0f),
                    new Vector3(vs[3].x, vs[3].y, 0f)
                };
                Handles.DrawSolidRectangleWithOutline(array, selectDrawHitBox == index ? new Color(1, 1, 0, 0.2f) : new Color(1, 0, 0, 0.2f), Color.yellow);
                index++;
            }

            HandleUtility.Repaint(); // 保证 SceneView 刷新
        }
    }
    #endregion
}

public class SkillEditorConfig
{
    /// <summary>
    /// 每帧的标准单位像素刻度
    /// </summary>
    public const int StandframeUnitWidth = 10;

    /// <summary>
    /// 分10级
    /// </summary>
    public const int MaxFrameWidthLV = 10;

    /// <summary>
    /// 当前的帧单位刻度（受缩放而变化）
    /// </summary>
    public int FrameUnitWidth = 10;

    /// <summary>
    /// 默认帧率
    /// </summary>
    public float DefaultFrameRate = 10;

}