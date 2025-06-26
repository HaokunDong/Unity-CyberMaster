using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using GameBase.Log;

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

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 初始化显示一段对话
    /// </summary>
    public void Show(string speakerName, Sprite portrait, string text, Action action = null, List<string> options = null)
    {
        gameObject.SetActive(true);

        speakerNameText.text = speakerName;
        speakerPortrait.sprite = portrait;
        dialogueText.text = text;
        onContinueClicked = action;

        if(optionsRoot != null)
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
            foreach (var opt in options)
            {
                var btnObj = Instantiate(optionButtonPrefab, optionsRoot);
                var btnText = btnObj.GetComponentInChildren<Text>();
                btnText.text = opt;
                btnObj.GetComponent<Button>().onClick.AddListener(() => OnOptionClicked(opt));
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
    }

    private void OnOptionClicked(string optionText)
    {
        Debug.Log($"选中选项：{optionText}");
        // 这里你可以触发对应选项逻辑
    }
}
