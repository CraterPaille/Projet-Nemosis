using System.Collections.Generic;
using UnityEngine;

public class GodManager : MonoBehaviour
{
    public static GodManager Instance { get; private set; }

    [Tooltip("List of all GodData assets in the project, assign in inspector")]
    public List<GodDataSO> gods = new List<GodDataSO>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerable<GodDataSO> GetUnlockedGods()
    {
        foreach (var g in gods) if (g.unlocked) yield return g;
    }

    public List<GodDataSO> GetEligibleForRandomUnlock()
    {
        var list = new List<GodDataSO>();
        foreach (var g in gods)
        {
            if (!g.unlocked)
            {
                // here you can add extra eligibility checks (requirements)
                list.Add(g);
            }
        }
        return list;
    }

    public GodDataSO UnlockRandomGod()
    {
        var eligible = GetEligibleForRandomUnlock();
        if (eligible.Count == 0) return null;
        var idx = Random.Range(0, eligible.Count);
        var god = eligible[idx];
        god.unlocked = true;
        SaveState();
        return god;
    }

    public GodDataSO GetGodById(string id)
    {
        return gods.Find(g => g != null && g.id == id);
    }

    // simple persistence example (you may want a proper save system)
    public void SaveState()
    {
        foreach (var g in gods)
        {
            PlayerPrefs.SetInt($"god_unlocked_{g.id}", g.unlocked ? 1 : 0);
            PlayerPrefs.SetInt($"god_relation_{g.id}", g.relation);
        }
        PlayerPrefs.Save();
    }

    public void LoadState()
    {
        foreach (var g in gods)
        {
            if (PlayerPrefs.HasKey($"god_unlocked_{g.id}"))
                g.unlocked = PlayerPrefs.GetInt($"god_unlocked_{g.id}") == 1;
            if (PlayerPrefs.HasKey($"god_relation_{g.id}"))
                g.relation = PlayerPrefs.GetInt($"god_relation_{g.id}");
        }
    }
}
