using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class Laser : MonoBehaviour
{
    private int damage;
    private bool hasHitShield = false;
    private bool hasHitPlayer = false;

    public int lifeTime = 3;

    [Header("Sprite Sheet Animation")]
    public Sprite[] loopFrames;          // frames pour l'animation en boucle (ex : laser actif)
    public float framesPerSecond = 12f;  // cadence d'animation

    [Header("SFX (assigner dans l'inspecteur)")]
    public AudioClip shieldHitSfx;
    public AudioClip playerHitSfx;
    public float sfxVolume = 1f;

    [Header("Audio (optionnel)")]
    public AudioSource sfxSource; // assigner si tu veux réutiliser une source existante (routée sur le groupe SFX)

    [Header("Life / Cut settings")]
    public bool immediateDestroyOnLifeEnd = true;    // true = couper tout et détruire immédiatement au lifeTime
                                                     // false = appeler DestroyWithImpact quand le temps est écoulé

    private SpriteRenderer spriteRenderer;
    private Coroutine spriteAnimCoroutine;
    private AudioMixerGroup sfxMixerGroup;

    public void SetDamage(int dmg)
    {
        damage = dmg;
        Debug.Log($"Laser damage set to {dmg}");
    }

    void Awake()
    {
        // Ensure an AudioSource exists (either assigned in inspector or created)
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // 2D
            }
        }

        // Try to find an AudioMixerGroup likely named "SFX" (case-insensitive substring match)
        var groups = Resources.FindObjectsOfTypeAll<AudioMixerGroup>();
        foreach (var g in groups)
        {
            if (g == null) continue;
            string n = g.name.ToLowerInvariant();
            if (n.Contains("sfx") || n.Contains("sfxs") || n.Contains("sound") || n.Contains("sfx_group"))
            {
                sfxMixerGroup = g;
                break;
            }
        }

        if (sfxMixerGroup != null && sfxSource != null)
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    void OnEnable()
    {
        hasHitShield = false;
        hasHitPlayer = false;

        // Cache components
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Start sprite-sheet loop animation if frames provided
        if (spriteRenderer != null && loopFrames != null && loopFrames.Length > 0)
        {
            spriteAnimCoroutine = StartCoroutine(PlayLoopAnimation());
        }

        // Safety destroy / cut in case the spawner didn't schedule one
        CancelInvoke(nameof(ForceDestroy));
        Invoke(nameof(ForceDestroy), lifeTime);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(ForceDestroy));
        StopSpriteAnimation();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[LASER] Collision with: {other.gameObject.name} (Tag: {other.tag})");

        // PRIORITÉ 1 : Bouclier (tag exact "Shield")
        if (other.CompareTag("Shield") && !hasHitShield)
        {
            // Délègue le traitement (son + destruction) à la méthode publique pour s'assurer que le son est joué
            OnBlockedByShield();
            return;
        }

        // PRIORITÉ 2 : Joueur (tag exact "PlayerSoul")
        if (other.CompareTag("PlayerSoul") && !hasHitPlayer && !hasHitShield)
        {
            hasHitPlayer = true;
            Debug.Log($" [LASER] HIT PLAYER for {damage} damage!");
            ChronosGameManager.Instance.DamagePlayer(damage);

            // Jouer le sfx via la source du laser (routée sur SFX si disponible)
            if (playerHitSfx != null)
            {
                if (sfxSource != null)
                    sfxSource.PlayOneShot(playerHitSfx, sfxVolume);
                else
                    SpawnOneShotAtPosition(playerHitSfx, transform.position);
            }

            // IMPORTANT : on N'APPELLE PLUS DestroyWithImpact ici pour laisser le laser vivre jusqu'à la fin de sa lifetime.
            // Si tu veux éviter que le laser touche le joueur plusieurs fois, hasHitPlayer empêche les traitements répétés.
            return;
        }
    }

    // Méthode publique appelée par les boucliers (JusticeShield) pour centraliser le traitement
    public void OnBlockedByShield()
    {
        if (hasHitShield) return;
        hasHitShield = true;
        Debug.Log($" [LASER] BLOCKED by shield! Processing block.");

        if (shieldHitSfx != null)
            SpawnOneShotAtPosition(shieldHitSfx, transform.position);

        DestroyWithImpact();
    }

    // Crée une AudioSource temporaire au monde pour jouer un clip, utile si l'objet émetteur va être détruit
    void SpawnOneShotAtPosition(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;
        GameObject go = new GameObject($"SFX_{clip.name}");
        go.transform.position = pos;
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D
        if (sfxMixerGroup != null) src.outputAudioMixerGroup = sfxMixerGroup;
        src.PlayOneShot(clip, sfxVolume);
        Destroy(go, clip.length + 0.1f);
    }

    // Détruit immédiatement (aucune animation d'outro)
    public void DestroyWithImpact()
    {
        // Avoid multiple calls
        if (!gameObject.activeInHierarchy) return;

        // Stop loop animation
        StopSpriteAnimation();

        // Pas d'animation supplémentaire : destruction immédiate
        Destroy(gameObject);

        // Ensure we won't call ForceDestroy later
        CancelInvoke(nameof(ForceDestroy));
    }

    IEnumerator PlayLoopAnimation()
    {
        if (spriteRenderer == null || loopFrames == null || loopFrames.Length == 0) yield break;
        float delay = 1f / Mathf.Max(0.0001f, framesPerSecond);
        int idx = 0;
        while (true)
        {
            spriteRenderer.sprite = loopFrames[idx];
            idx = (idx + 1) % loopFrames.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    void StopSpriteAnimation()
    {
        if (spriteAnimCoroutine != null)
        {
            StopCoroutine(spriteAnimCoroutine);
            spriteAnimCoroutine = null;
        }
    }

    // Fallback destroy if nothing else called it
    void ForceDestroy()
    {
        // Si on veut couper *tout* immédiatement : stoppe l'animation, les collisions, et détruit
        if (immediateDestroyOnLifeEnd)
        {
            CutEverythingImmediate();
            Destroy(gameObject);
            return;
        }

        // Sinon détruit via DestroyWithImpact
        DestroyWithImpact();
    }

    // Coupe tous les systèmes visuels / collisions sans jouer d'outro
    void CutEverythingImmediate()
    {
        CancelInvoke(nameof(ForceDestroy));

        // Stop sprite animation
        StopSpriteAnimation();

        // Désactive tous les colliders pour éviter de nouvelles collisions
        var cols = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) c.enabled = false;

        // Cache et disparaît le sprite immédiatement
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Désactive tout ParticleSystem présent
        var parts = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var p in parts) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Stoppe les audios enfants
        var audios = GetComponentsInChildren<AudioSource>(true);
        foreach (var a in audios) a.Stop();
    }
}