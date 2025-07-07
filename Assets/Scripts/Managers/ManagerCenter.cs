using GameBase.Tickers;

public class ManagerCenter : Singleton<ManagerCenter>
{
    public TickersManager TickersMgr;
    public CooldownManager CooldownMgr;
    public TimeLineManager TimeLineMgr;
    public DialogueManager DialogueMgr;
    public PlayerInputManager PlayerInputMgr;
    public SkillBoxManager SkillBoxMgr;

    public void Init()
    {
        TickersMgr = new TickersManager();
        TickersMgr.Init();

        CooldownMgr = new CooldownManager();
        CooldownMgr.Init();

        TimeLineMgr = new TimeLineManager();
        TimeLineMgr.Init();

        DialogueMgr = new DialogueManager();
        DialogueMgr.Init();

        PlayerInputMgr = new PlayerInputManager();
        PlayerInputMgr.Init();

        SkillBoxMgr = new SkillBoxManager();
        SkillBoxMgr.Init();
    }
}
