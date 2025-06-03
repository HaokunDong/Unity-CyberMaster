using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SkillTrackItemStyleBase
{
    public Label root { get; protected set; }
    public VisualElement mainDragArea { get; protected set; }
    public VisualElement overLine { get; protected set; }
    protected Label titleLabel;

    public virtual void Init(SkillTrackStyleBase TrackStyle, int startFrameIndex, float frameUnitWidth)
    {
    }

    public virtual void SetBGColor(Color color)
    {
        root.style.backgroundColor = color;
    }

    public virtual void SetWidth(float width)
    {
        root.style.width = width;
    }

    public virtual void SetPosition(float x)
    {
        Vector3 pos = root.transform.position;
        pos.x = x;
        root.transform.position = pos;
    }

    internal void SetTitle(string str)
    {
        titleLabel.text = str;
    }
}
