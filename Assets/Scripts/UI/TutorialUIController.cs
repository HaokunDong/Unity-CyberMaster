using UnityEngine;
using UnityEngine.UI;

public class TutorialUIController : MonoBehaviour
{
    public GameObject tutorialPanel;

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
    }

    public void HideTutorial()
    {
        tutorialPanel.SetActive(false);
    }
}
