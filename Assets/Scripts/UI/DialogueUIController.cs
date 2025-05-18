using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueUIController : MonoBehaviour
{
    public static DialogueUIController Instance;

    [Header("根节点（整个对话界面）")]
    public GameObject rootPanel;

    [Header("头像")]
    public Image portraitLeft;
    public Image portraitRight;

    [Header("头像资源组")]
    public Sprite[] portraits;

    [Header("文本")]
    public TextMeshProUGUI dialogueText;

    [Header("选项组")]
    public GameObject choiceGroup;
    public Button choiceButtonPrefab;

    private List<Button> activeChoices = new();

    private void Awake()
    {
        Instance = this;
        rootPanel.SetActive(false);
        HideChoices();
    }

    public void ShowDialogue(string text, int portraitIndex, bool isLeft = true)
    {
        rootPanel.SetActive(true);
        dialogueText.text = text;

        if (portraitIndex >= 0 && portraitIndex < portraits.Length)
        {
            Sprite sprite = portraits[portraitIndex];

            if (isLeft)
            {
                portraitLeft.sprite = sprite;
                portraitLeft.gameObject.SetActive(true);
                portraitRight.gameObject.SetActive(false);
            }
            else
            {
                portraitRight.sprite = sprite;
                portraitRight.gameObject.SetActive(true);
                portraitLeft.gameObject.SetActive(false);
            }
        }
    }

    public void ShowChoices(List<string> options, System.Action<int> onSelected)
    {
        HideChoices();

        for (int i = 0; i < options.Count; i++)
        {
            Button btn = Instantiate(choiceButtonPrefab, choiceGroup.transform);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = options[i];

            int index = i;
            btn.onClick.AddListener(() =>
            {
                onSelected?.Invoke(index);
                HideChoices();
            });

            activeChoices.Add(btn);
        }
    }

    public void HideChoices()
    {
        foreach (var btn in activeChoices)
        {
            Destroy(btn.gameObject);
        }
        activeChoices.Clear();
    }

    public void HideDialogue()
    {
        rootPanel.SetActive(false);
        HideChoices();
    }
}
