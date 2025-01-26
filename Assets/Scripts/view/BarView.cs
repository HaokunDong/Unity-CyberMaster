using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SegmentInfo
{
    public float weight = 1;
    public Color color = Color.green;
    public string text = "";
}
public class BarInfo
{
    public List<SegmentInfo> segments = new List<SegmentInfo>();
    public string title = "";
    public float point_t = 0;//normalized
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
    const float moveSpeed = 0.5f;
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
        go.transform.localPosition = Vector3.zero;
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
        List<BarCom> list = barMap[key];
        for (int i = 0; i < list.Count; i++)
        {
            if (!barPos.ContainsKey(list[i]))
                barPos.Add(list[i], 0);
            barPos[list[i]] = i * spaceY;
        }
    }
    public void Update()
    {
        foreach (var item in barPos)
        {
            item.Key.transform.localPosition = new Vector3(0, Mathf.Lerp(item.Key.transform.localPosition.y, item.Value, moveSpeed * Time.deltaTime), 0);
        }
    }
}
