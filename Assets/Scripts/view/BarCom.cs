using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BarCom : MonoBehaviour
{
    public BarInfo info;
    public RectTransform bottom;
    public RectTransform points;
    public Text title;
    public Transform segmentFolder;
    public GameObject segmentPrefab;
    private List<SegmentCom> segmentComs = new List<SegmentCom>();
    public float t
    {
        get => info.point_t;
        set
        {
            info.point_t = value;
            points.anchoredPosition = new Vector2(t * bottom.sizeDelta.x, 0);
        }
    }
    public void SetInfo(BarInfo info)
    {
        this.info = info;
        Refresh();
    }
    public void Refresh()
    {
        float total = info.allWei;
        float left = 0;
        List<SegmentCom> list = new List<SegmentCom>();
        for (int i = 0; i < info.segments.Count; i++)
        {
            if (i >= segmentComs.Count)
            {
                SegmentCom c = segmentPrefab.OPGet().GetComponent<SegmentCom>();
                segmentComs.Add(c);
                c.transform.SetParent(segmentFolder);
            }
            SegmentCom com = segmentComs[i];
            RectTransform rt = com.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(left, 0);
            rt.sizeDelta = new Vector2(info.segments[i].weight / total * bottom.sizeDelta.x, bottom.sizeDelta.y);
            rt.localScale = Vector3.one;
            left += rt.sizeDelta.x;
            com.SetInfo(info.segments[i]);
            list.Add(com);
        }
        for (int i = info.segments.Count; i < segmentComs.Count; i++)
        {
            segmentComs[i].gameObject.OPPush();
        }
        segmentComs = list;
    }
}