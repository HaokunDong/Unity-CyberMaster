using System;
using System.Collections.Generic;
using UnityEngine;

public enum GamePlayAttributeType
{
    [InspectorName("无敌")]
    Invincible = 1,
    [InspectorName("霸体值")]
    SuperArmor = 2,
    [InspectorName("动作值")]
    ActionValue = 3,
}

public enum GamePlayAttributeAddType
{
    [InspectorName("当前技能帧")]
    SkillFrame = 1,
    Buff = 2,
}

[Serializable]
public class AddSkillAttribute
{
    public GamePlayAttributeType GPAType;
    public GamePlayAttributeAddType GPAAType;
    public float value;
    public float duration;

    public AddSkillAttribute()
    {
        GPAType = GamePlayAttributeType.Invincible;
        GPAAType = GamePlayAttributeAddType.SkillFrame;
        value = 1f;
        duration = 1f;
    }
}

public class SkillAttribute
{
    public GamePlayAttributeType Type { get; set; }
    public float Value { get; set; }
    public GamePlayAttributeAddType AddType { get; set; }
    public int FrameDuration { get; set; }  // 帧同步方式的持续帧数
    public float TimeDuration { get; set; } // Buff方式的持续时间(秒)
    public int StartFrame { get; set; }     // 开始帧
    public float StartTime { get; set; }    // 开始时间
    public bool IsActive { get; set; }
}

public class SkillAttributeData
{
    private readonly ISkillDriverUnit owner;
    private readonly SkillDriver skillDriver;
    private readonly Dictionary<GamePlayAttributeType, List<SkillAttribute>> attributes = new();
    private Func<int> getCurrentFrame;  // 获取当前技能帧的委托
    private Func<float> getDeltaTime;   // 获取deltaTime的委托

    public SkillAttributeData(ISkillDriverUnit owner, SkillDriver skillDriver, Func<int> getCurrentFrame, Func<float> getDeltaTime)
    {
        this.owner = owner;
        this.skillDriver = skillDriver;
        this.getCurrentFrame = getCurrentFrame;
        this.getDeltaTime = getDeltaTime;
    }

    /// <summary>
    /// 添加技能属性
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <param name="value">属性值</param>
    /// <param name="addType">添加方式</param>
    /// <param name="duration">持续时间（帧数或秒数，取决于添加方式）</param>
    public void AddAttribute(AddSkillAttribute aa)
    {
        if (!attributes.TryGetValue(aa.GPAType, out var attributeList))
        {
            attributeList = new List<SkillAttribute>();
            attributes[aa.GPAType] = attributeList;
        }

        var attribute = new SkillAttribute
        {
            Type = aa.GPAType,
            Value = aa.value,
            AddType = aa.GPAAType,
            IsActive = true
        };

        if (aa.GPAAType == GamePlayAttributeAddType.SkillFrame)
        {
            attribute.FrameDuration = Mathf.RoundToInt(aa.duration);
            attribute.StartFrame = getCurrentFrame();
        }
        else
        {
            attribute.TimeDuration = aa.duration;
            attribute.StartTime = Time.time;
        }

        attributeList.Add(attribute);
    }

    /// <summary>
    /// 移除指定类型的所有属性
    /// </summary>
    /// <param name="type">属性类型</param>
    public void RemoveAllAttributes(GamePlayAttributeType type)
    {
        if (attributes.TryGetValue(type, out var attributeList))
        {
            attributeList.Clear();
        }
    }

    /// <summary>
    /// 移除指定类型的特定属性
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <param name="predicate">筛选条件</param>
    public void RemoveAttributes(GamePlayAttributeType type, Predicate<SkillAttribute> predicate)
    {
        if (attributes.TryGetValue(type, out var attributeList))
        {
            for (int i = attributeList.Count - 1; i >= 0; i--)
            {
                var attr = attributeList[i];
                if (predicate(attr))
                {
                    attributeList.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// 获取指定类型的属性总值
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <returns>属性总值</returns>
    public float GetAttributeValue(GamePlayAttributeType type)
    {
        if (attributes.TryGetValue(type, out var attributeList))
        {
            float totalValue = 0;
            foreach (var attribute in attributeList)
            {
                if (attribute.IsActive)
                {
                    totalValue += attribute.Value;
                }
            }
            return totalValue;
        }
        return 0;
    }

    /// <summary>
    /// 检查是否有指定类型的激活属性
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <returns>是否有激活的属性</returns>
    public bool HasActiveAttribute(GamePlayAttributeType type)
    {
        if (attributes.TryGetValue(type, out var attributeList))
        {
            foreach (var attribute in attributeList)
            {
                if (attribute.IsActive)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 更新属性状态
    /// </summary>
    public void UpdateAttributes()
    {
        foreach (var kvp in attributes)
        {
            var type = kvp.Key;
            var attributeList = kvp.Value;

            for (int i = attributeList.Count - 1; i >= 0; i--)
            {
                var attribute = attributeList[i];
                bool isExpired = false;

                if (attribute.AddType == GamePlayAttributeAddType.SkillFrame)
                {
                    int currentFrame = getCurrentFrame();
                    if (currentFrame - attribute.StartFrame > attribute.FrameDuration)
                    {
                        isExpired = true;
                    }
                }
                else
                {
                    if (Time.time - attribute.StartTime >= attribute.TimeDuration)
                    {
                        isExpired = true;
                    }
                }

                if (isExpired)
                {
                    attribute.IsActive = false;
                    attributeList.RemoveAt(i);
                }
            }
        }
    }


    /// <summary>
    /// 清除所有属性
    /// </summary>
    public void ClearAllAttributes()
    {
        attributes.Clear();
    }
}