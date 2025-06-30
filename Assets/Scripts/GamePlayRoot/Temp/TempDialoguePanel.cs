using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class TempDialoguePanel : MonoBehaviour
{
    public static TempDialoguePanel Instance { get; private set; }

    [Header("UI References")]
    public Image speakerPortrait;
    public Text speakerNameText;
    public Text dialogueText;
    public Transform optionsRoot;
    public GameObject continueIndicator;
    public GameObject optionButtonPrefab;
    // 当前对话结束时触发
    public Action onContinueClicked;
    public Action<int> onChoiceClicked;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 初始化显示一段对话
    /// </summary>
    public void Show(string speakerName, Sprite portrait, string text, Action action = null, List<string> options = null, Action<int> cAction = null)
    {
        gameObject.SetActive(true);

        if(speakerName != null)
            speakerNameText.text = speakerName;
        if (portrait != null)
            speakerPortrait.sprite = portrait;
        if (text != null)
            dialogueText.text = text;
        onContinueClicked = action;
        onChoiceClicked = cAction;

        if (optionsRoot != null)
        {
            foreach (Transform child in optionsRoot)
            {
                Destroy(child.gameObject);
            }
        }    
        // 清空旧选项
        

        // 判断是选项模式还是继续模式
        if (options != null && options.Count > 0)
        {
            continueIndicator.SetActive(false);
            for (int i = 0; i < options.Count; i++)
            {
                var opt = options[i];
                var btnObj = Instantiate(optionButtonPrefab, optionsRoot);
                var btnText = btnObj.GetComponentInChildren<Text>();
                btnText.text = opt;
                var index = i;
                btnObj.GetComponent<Button>().onClick.AddListener(() => OnOptionClicked(index));
            }
        }
        else
        {
            continueIndicator.SetActive(true);
        }
    }

    public void Hide() => gameObject.SetActive(false);

    /// <summary>
    /// 点击对话面板继续对话
    /// </summary>
    public void OnClickContinue()
    {
        onContinueClicked?.Invoke();
        onContinueClicked = null;
    }

    private void OnOptionClicked(int index)
    {
        onChoiceClicked?.Invoke(index);
    }
}
