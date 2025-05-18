using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUIController : MonoBehaviour
{
    public static GameOverUIController Instance;

    public CanvasGroup victoryGroup;
    public CanvasGroup defeatGroup;

    [Header("动画参数")]
    public float delayBeforeShow = 1f;
    public float fadeDuration = 1f;
    public float displayDuration = 3f;

    private void Awake()
    {
        Instance = this;
        victoryGroup.gameObject.SetActive(false);
        defeatGroup.gameObject.SetActive(false);
    }

    public void ShowVictory()
    {
        StartCoroutine(FadeSequence(victoryGroup));
    }

    public void ShowDefeat()
    {
        StartCoroutine(FadeSequence(defeatGroup));
    }

    private IEnumerator FadeSequence(CanvasGroup group)
    {
        yield return new WaitForSeconds(delayBeforeShow);

        group.gameObject.SetActive(true);
        group.alpha = 0f;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(displayDuration);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        group.gameObject.SetActive(false);
    }
}
