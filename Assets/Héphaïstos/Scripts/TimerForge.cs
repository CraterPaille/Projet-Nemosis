using UnityEngine;
using TMPro;
using System.Collections;


// SI une Lame est forgée, elle est ecrasées

public class TimerForge : MonoBehaviour
{
    public GameObject Anvil;
    public Lame[] Lames = new Lame[3];
    public bool[] TabCheck = new bool[3];

    public float forgeDuration = 5f;
    private float[] Timers;
    private Coroutine[] slotCoroutines;

    public TextMeshProUGUI timerText;

    void Awake()
    {
        int n = Lames.Length;
        Timers = new float[n];
        slotCoroutines = new Coroutine[n];
        for (int i = 0; i < n; i++)
        {
            Timers[i] = 5f;
            TabCheck[i] = false;
        }
    }

    void Update()
    {
        if (timerText != null)
        {
            string s = string.Empty;
            for (int i = 0; i < Lames.Length; i++)
            {
                s += $"Slot {i + 1} : ";
                s += TabCheck[i] ? "Forged" : Mathf.Ceil(Timers[i]).ToString("F0");
                s += " Type : " + (Lames[i] != null ? Lames[i].type.ToString("F1") : "N/A");
                if (i < Lames.Length - 1) s += "\n";
            }
            timerText.text = s;
        }
    }


    public bool EnqueueLame(Lame lame)
    {
        if (lame == null) return false;

        for (int idx = 0; idx < Lames.Length; idx++)
        {
            if (Lames[idx] == null || TabCheck[idx])
            {
                Lames[idx] = lame;
                TabCheck[idx] = false;
                Timers[idx] = forgeDuration;
                if (slotCoroutines[idx] == null)
                {
                    slotCoroutines[idx] = StartCoroutine(SlotForgeRoutine(idx));
                }
                return true;
            }
        }
        return false;
    }

    private IEnumerator SlotForgeRoutine(int slotIndex)
    {
        while (Timers[slotIndex] > 0f)
        {
            Timers[slotIndex] -= Time.deltaTime;
            yield return null;
        }

        Timers[slotIndex] = 0f;
        TabCheck[slotIndex] = true;

        if (Lames[slotIndex] != null)
        {
            Lames[slotIndex].isForged = true;
        }

        slotCoroutines[slotIndex] = null;
    }

    void OnMouseDown()
    {
        for (int i = 0; i < Lames.Length; i++)
        {
            if (TabCheck[i] && Lames[i] != null)
            {
                Anvil.GetComponent<Anvil>().LameOnAnvil = Lames[i];
                Lames[i] = null;
                TabCheck[i] = false;
                Timers[i] = 5f;
            }
        }
    }

}
