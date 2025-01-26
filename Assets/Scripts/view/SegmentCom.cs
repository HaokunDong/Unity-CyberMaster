using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SegmentCom : MonoBehaviour
{
    public Image img;
    public Text text;
    public void SetInfo(SegmentInfo info)
    {
        img.color = info.color;
        text.text = info.text;
    }
}
