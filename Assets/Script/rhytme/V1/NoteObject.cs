using UnityEngine;
using System.Collections.Generic;

public class NoteObject : MonoBehaviour
{
    public static List<NoteObject> activeNotes = new();

    [Header("Lane")]
    public int lane;
    public bool canBePressed;

    [Header("Hold Note")]
    public float duration = 0f;          // > 0 = hold note (en secondes de maintien)
    private bool isHoldNote;
    private bool isHolding;
    public bool finished;
    private float holdTime;

    [Header("Visuals")]
    public Transform activator;
    public Transform holdBar;           // barre qui se remplit pendant hold
    public SpriteRenderer noteBody;     // sprite principal de la note

    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject GoodEffect;
    public GameObject PerfectEffect;
    public GameObject MissEffect;

    private bool hasBeenHit = false;
    private float fullBarLength;        // longueur initiale de la barre (en scale Y)

    // --- Mouvement / chute ---
    private BeatScroller globalBeatScroller;

    // --- Jugement hold ---
    // 0 = aucune, 1 = Normal, 2 = Good, 3 = Perfect
    private int holdStartAccuracy = 0;

    private void Awake()
    {
        // l’Activator
        if (activator == null)
        {
            GameObject act = GameObject.FindGameObjectWithTag("Activator");
            if (act != null)
                activator = act.transform;
            else
                Debug.LogError("NoteObject : Activator introuvable !");
        }

        // HoldBar enfant
        if (holdBar == null)
            holdBar = transform.Find("HoldBar");

        if (holdBar != null)
            fullBarLength = holdBar.localScale.y; // longueur de base du prefab

        // Récupère le BeatScroller GLOBAL dans la scène
        globalBeatScroller = FindFirstObjectByType<BeatScroller>();
    }

    // Appelé par le spawner juste après avoir set duration + lane
    public void InitFromChart()
    {
        // --- Orientation visuelle en fonction de la lane ---
        Transform visual = transform.Find("visual"); // NOM EXACT DE TON CHILD
        if (visual != null)
        {
            switch (lane)
            {
                case 0: visual.right = Vector3.left;  break;
                case 1: visual.right = Vector3.down;  break;
                case 2: visual.right = Vector3.up;    break;
                case 3: visual.right = Vector3.right; break;
            }
        }
        else
        {
            Debug.LogWarning($"NoteObject ({name}) : child 'visual' introuvable, orientation impossible.");
        }

        // --- Hold / Tap ---
        isHoldNote = duration > 0.01f;

        if (!isHoldNote && holdBar != null)
        {
            // TAP : cacher complètement la barre
            holdBar.gameObject.SetActive(false);
            var sr = holdBar.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            return;
        }

        if (isHoldNote && holdBar != null)
        {
            holdBar.gameObject.SetActive(true);
            var sr = holdBar.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;

            float unitsPerSecond = 2f;
            float totalLength = Mathf.Max(0.1f, duration * unitsPerSecond);
            fullBarLength = totalLength;

            // Longueur initiale de la barre
            holdBar.localScale = new Vector3(
                holdBar.localScale.x,
                fullBarLength,
                holdBar.localScale.z
            );

            // Centre positionné à mi-hauteur pour que le haut colle à la tête
            holdBar.localPosition = new Vector3(0f, fullBarLength * 0.5f, 0f);
        }
    }

    void Update()
    {
        // Si on est en train de hold une note, on annule la chute globale
        if (isHoldNote && isHolding && !finished && globalBeatScroller != null)
        {
            transform.position += new Vector3(0f, globalBeatScroller.beatTempo * Time.deltaTime, 0f);
        }

        if (!isHoldNote || !isHolding || finished) return;

        holdTime += Time.deltaTime;

        if (holdBar != null)
        {
            float progress = Mathf.Clamp01(holdTime / duration);
            float currentLength = Mathf.Lerp(fullBarLength, 0f, progress);

            holdBar.localScale = new Vector3(
                holdBar.localScale.x,
                currentLength,
                holdBar.localScale.z
            );

            holdBar.localPosition = new Vector3(0f, currentLength * 0.5f, 0f);
        }

        if (holdTime >= duration)
        {
            // on a tenu assez longtemps -> success = true
            FinishHold(true);
        }

        UpdateApproachScaleAndGlow();
    }

    private void UpdateApproachScaleAndGlow()
    {
        if (activator == null || noteBody == null) return;

        // distance verticale jusqu'à la ligne d'activation
        float dy = Mathf.Abs(transform.position.y - activator.position.y);

        // plus la note est proche (dy -> 0), plus elle est grosse
        float maxScale = 1.15f;
        float minScale = 0.9f;
        float maxDistance = 2.5f; // distance sur laquelle l’effet agit (à adapter à ta scène)

        float t = Mathf.Clamp01(1f - (dy / maxDistance));
        float scale = Mathf.Lerp(minScale, maxScale, t);
        noteBody.transform.localScale = new Vector3(scale, scale, 1f);

        // Optionnel : légère variation de couleur (ex: plus blanche près de la ligne)
        Color baseColor = Color.white;
        Color targetColor = Color.Lerp(baseColor, Color.yellow, t * 0.4f);
        noteBody.color = targetColor;
    }

    private void OnEnable()
    {
        if (!activeNotes.Contains(this))
            activeNotes.Add(this);
    }

    private void OnDisable()
    {
        activeNotes.Remove(this);
    }

    void LateUpdate()
    {
        if (holdBar != null)
            holdBar.rotation = Quaternion.identity;
    }

    // --- Gestion des hold notes ---
    public void StartHold()
    {
        if (finished || !canBePressed)
            return;

        if (!isHoldNote)
        {
            // Note simple : comportement classique
            TryHit();
            return;
        }

        // Jugement de l'ENTRÉE du hold (comme un tap)
        float distance = Mathf.Abs(transform.position.y - activator.position.y);

        if (distance > 0.30f)
        {
            // Normal
            GameManagerRhytme.instance.NormalHit();
            holdStartAccuracy = 1;
            if (hitEffect != null)
            {
                var fx = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }
        else if (distance > 0.10f)
        {
            // Good
            GameManagerRhytme.instance.GoodHit();
            holdStartAccuracy = 2;
            if (GoodEffect != null)
            {
                var fx = Instantiate(GoodEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }
        else
        {
            // Perfect
            GameManagerRhytme.instance.PerfectHit();
            holdStartAccuracy = 3;
            if (PerfectEffect != null)
            {
                var fx = Instantiate(PerfectEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }

        // Si vraiment trop loin, on pourrait considérer un miss direct
        // mais ici on accepte toujours dès qu'on appuie.

        isHolding = true;
        holdTime = 0f;
        hasBeenHit = true; // on ne veut plus que TryHit() soit appelée
    }

    public void ReleaseHold()
    {
        if (!isHolding || finished)
            return;

        // On ne force plus la réussite ici, c'est FinishHold qui décide selon le ratio
        FinishHold(true);
    }

    void FinishHold(bool success)
    {
        if (finished) return;

        finished = true;
        isHolding = false;

        // Si on est déjà en "miss dur" (pas commencé correctement)
        if (!success || holdStartAccuracy == 0)
        {
            GameManagerRhytme.instance.NoteMissed();
            if (MissEffect != null)
            {
                var fxMiss = Instantiate(MissEffect, transform.position, Quaternion.identity);
                Destroy(fxMiss, 2f);
            }
            Destroy(gameObject);
            return;
        }

        // ratio de temps réellement tenu
        float ratio = Mathf.Clamp01(holdTime / duration);

        // Jugement final combiné entrée + durée
        // Tu peux affiner ces seuils :
        if (ratio >= 0.9f && holdStartAccuracy == 3)
        {
            // Très bonne entrée + presque tout tenu => Perfect supplémentaire
            GameManagerRhytme.instance.PerfectHit();
            if (PerfectEffect != null)
            {
                var fx = Instantiate(PerfectEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }
        else if (ratio >= 0.6f)
        {
            // Entrée OK + durée correcte => Good
            GameManagerRhytme.instance.GoodHit();
            if (GoodEffect != null)
            {
                var fx = Instantiate(GoodEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }
        else
        {
            // Pas tenu assez longtemps => Miss
            GameManagerRhytme.instance.NoteMissed();
            if (MissEffect != null)
            {
                var fx = Instantiate(MissEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }

        Destroy(gameObject);
    }

    private void MissHold()
    {
        isHolding = false;
        finished = true;
        GameManagerRhytme.instance.NoteMissed();
        if (MissEffect != null)
        {
            var fx = Instantiate(MissEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
        Destroy(gameObject);
    }

    // --- Notes normales ---
    public void TryHit()
    {
        if (finished || activator == null)
            return;

        if (isHoldNote) // les hold sont gérées par StartHold/ReleaseHold
            return;

        if (hasBeenHit)
            return;

        hasBeenHit = true;

        float distance = Mathf.Abs(transform.position.y - activator.position.y);

        if (distance > 0.30f)
        {
            GameManagerRhytme.instance.NormalHit();
            if (hitEffect != null)
            {
                var fx = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }
        else if (distance > 0.10f)
        {
            GameManagerRhytme.instance.GoodHit();
            if (GoodEffect != null)
            {
                var fx = Instantiate(GoodEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }
        else
        {
            GameManagerRhytme.instance.PerfectHit();
            if (PerfectEffect != null)
            {
                var fx = Instantiate(PerfectEffect, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }

        Destroy(gameObject);
    }

    // --- Gestion des collisions avec l'activator ---
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Activator"))
            canBePressed = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Activator") && !hasBeenHit)
        {
            canBePressed = false;

            if (isHoldNote && isHolding)
            {
                MissHold();
            }
            else if (!isHoldNote)
            {
                GameManagerRhytme.instance.NoteMissed();
                if (MissEffect != null)
                {
                    var fx = Instantiate(MissEffect, transform.position, Quaternion.identity);
                    Destroy(fx, 2f);
                }
                Destroy(gameObject);
            }
        }
    }

    // --- Utils ---
    public bool IsSimpleNotePressable()
    {
        return !finished && !isHoldNote;
    }

    public bool IsHoldNotePressable()
    {
        return !finished && isHoldNote && canBePressed;
    }

    public static NoteObject GetClosestNoteInLane(int lane)
    {
        // récupère un activator commun (toutes les notes utilisent le même)
        Transform activator = null;
        foreach (var n in activeNotes)
        {
            if (n != null && n.activator != null)
            {
                activator = n.activator;
                break;
            }
        }

        NoteObject closest = null;
        float bestDistance = float.MaxValue;
        const float MAX_HIT_WINDOW = 0.45f; // tolérance globale

        foreach (var note in activeNotes)
        {
            if (note == null) continue;
            if (note.lane != lane) continue;
            if (note.finished) continue;

            float distance = activator != null
                ? Mathf.Abs(note.transform.position.y - activator.position.y)
                : Mathf.Abs(note.transform.position.y);

            if (distance > MAX_HIT_WINDOW) continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = note;
            }
        }

        return closest;
    }
}