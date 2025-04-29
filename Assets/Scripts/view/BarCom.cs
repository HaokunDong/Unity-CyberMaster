using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public static class BarComEvent
{
    public static readonly string MIN_ARRIVE = "MIN_ARRIVE";
    public static readonly string MAX_ARRIVE = "MAX_ARRIVE";
    public static readonly string ZERO = "ZERO";
}
public class BarCom : MonoBehaviour
{
    public RectTransform floatPoint;
    public RectTransform leftMask;
    public RectTransform rightMask;
    public RectTransform bar;
    public GameObject changePrefab;

    public float _t = 0;
    public float t
    {
        get => _t;
        set
        {
            bool flag = value != _t;
            float diff = value - _t;
            _t = Mathf.Clamp(value, -1f, 1f);
            float width = this.GetComponent<RectTransform>().rect.width;
            if (flag)
            {
                float posX = t * (width / 2);
                bar.anchoredPosition = new Vector2(posX, 0);
                floatPoint.anchoredPosition = new Vector2(posX, 0);
                if (diff > 0)
                {
                    GameObject go = changePrefab.OPGet();
                    RectTransform rt = go.GetComponent<RectTransform>();
                    go.transform.SetParent(leftMask, false);
                    float changeWid = rt.rect.width;
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition = new Vector2(0, 0);
                    rt.DOAnchorPos(new Vector2(changeWid, 0), 0.5f).onComplete = () =>
                    {
                        go.OPPush();
                    };
                    if (t >= 1)
                    {
                        this.Send(BarComEvent.MAX_ARRIVE);
                    }
                }
                else if (diff < 0)
                {
                    GameObject go = changePrefab.OPGet();
                    RectTransform rt = go.GetComponent<RectTransform>();
                    go.transform.SetParent(rightMask, false);
                    float changeWid = rt.rect.width;
                    rt.localScale = new Vector3(-1, 1, 1);
                    rt.anchoredPosition = new Vector2(-changeWid, 0);
                    rt.DOAnchorPos(new Vector2(-changeWid * 2, 0), 0.5f).onComplete = () =>
                    {
                        go.OPPush();
                    };
                    if (t <= -1)
                    {
                        this.Send(BarComEvent.MIN_ARRIVE);
                    }
                }
                else
                {
                    this.Send(BarComEvent.ZERO);
                }
            }
        }
    }
}