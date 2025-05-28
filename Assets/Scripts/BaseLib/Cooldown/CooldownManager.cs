using System.Collections.Generic;

public class CooldownManager
{
    private readonly Dictionary<uint, CooldownGroup> _groupMap = new();
    private readonly List<CooldownGroup> _updateGroups = new();
    private readonly List<CooldownGroup> _fixedGroups = new();
    private readonly List<CooldownGroup> _lateGroups = new();

    public void Init()
    {
    }

    public CooldownGroup GetOrCreateGroup(uint id, UpdateMode mode)
    {
        if (!_groupMap.TryGetValue(id, out var group))
        {
            group = new CooldownGroup(mode);
            _groupMap[id] = group;
            GetListByMode(mode).Add(group);
        }
        return group;
    }

    public void RemoveOwner(uint id)
    {
        if (_groupMap.TryGetValue(id, out var group))
        {
            GetListByMode(group.UpdateMode).Remove(group);
            _groupMap.Remove(id);
        }
    }

    public void TickByMode(UpdateMode mode, float dt, float unscaledDt)
    {
        foreach (var group in GetListByMode(mode))
        {
            group.Tick(dt, unscaledDt);
        }
    }

    private List<CooldownGroup> GetListByMode(UpdateMode mode)
    {
        return mode switch
        {
            UpdateMode.FixedUpdate => _fixedGroups,
            UpdateMode.LateUpdate => _lateGroups,
            _ => _updateGroups,
        };
    }
}