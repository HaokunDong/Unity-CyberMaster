using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineEndDetector : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject[] openGameObjects;

    void OnEnable()
    {
        if (director != null)
        {
            director.stopped += OnTimelineStopped;
        }
    }

    void OnDisable()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }

    private void OnTimelineStopped(PlayableDirector dir)
    {
        Run().Forget();
    }

    private async UniTask Run()
    {
        foreach(var obj in openGameObjects)
        {
            obj?.SetActive(true);
        }
        await UniTask.DelayFrame(1);
        director?.gameObject.SetActive(false);
    }
}
