using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitRegistry : MonoBehaviour
{
    public static UnitRegistry Instance { get; private set; }
    private HashSet<Unit> units = new HashSet<Unit>();

    public event Action<Unit> OnUnitRegistered;
    public event Action<Unit> OnUnitUnregistered;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(Unit u)
    {
        if (u == null) return;
        if (units.Add(u))
        {
            OnUnitRegistered?.Invoke(u);
        }
    }

    public void Unregister(Unit u)
    {
        if (u == null) return;
        if (units.Remove(u))
        {
            OnUnitUnregistered?.Invoke(u);
        }
    }
    public int CountByTeam(TeamType team)
    {
        int c = 0;
        foreach (var u in units)
            if (u != null && u.team == team) c++;
        return c;
    }

    public int TotalCount => units.Count;
}