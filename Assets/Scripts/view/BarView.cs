using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SegmentInfo
{
    public float weight;
    public Color color;
    public string text;
}
public class BarInfo
{
    public List<SegmentInfo> segments;
    public string title;
    public float point_t;//normalized
    public float allWei
    {
        get
        {
            float total = 0;
            segments.ForEach(e => total += e.weight);
            return total;
        }
    }
    public int GetSegmentIdx()
    {
        float total = allWei;
        float progress = 0;
        for (int i = 0; i < segments.Count; i++)
        {
            if (progress + segments[i].weight / total >= point_t)
            {
                return i;
            }
            else
                progress += segments[i].weight / total;
        }
        return segments.Count - 1;
    }
}
public class BarView : Singleton<BarView>
{
    const float spaceY = 40;
    public Dictionary<string, List<BarCom>> barMap = new Dictionary<string, List<BarCom>>();
    private Dictionary<string, Transform> barParentMap = new Dictionary<string, Transform>();
    private Dictionary<BarCom, float> barPos = new Dictionary<BarCom, float>();
    public void Bind(string key, Transform parent)
    {
        if (!barParentMap.ContainsKey(key))
            barParentMap.Add(key, parent);
        else
            barParentMap[key] = parent;
    }
    public void Unbind(string key)
    {
        if (barParentMap.ContainsKey(key))
        {
            barParentMap.Remove(key);
            if (barMap.ContainsKey(key))
            {
                barMap[key].ForEach(e =>
                {
                    if (barPos.ContainsKey(e))
                        barPos.Remove(e);
                    e.gameObject.OPPush();
                });
            }
        }
    }
    public BarCom AddBar(BarInfo info, string key)
    {
        GameObject go = GlobalRef.Ins.barCom.OPGet();
        go.transform.SetParent(barParentMap[key]);
        BarCom com = go.GetComponent<BarCom>();
        if (!barMap.ContainsKey(key))
            barMap.Add(key, new List<BarCom>());
        barMap[key].Add(com);
        com.SetInfo(info);
        RefreshBarPos(key);
        return com;
    }
    public void RefreshBarPos(string key)
    {
        if (!barMap.ContainsKey(key))
            return;

    }
}
