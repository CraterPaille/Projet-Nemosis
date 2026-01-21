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
    public Sprite[] loopFrames;
    public float framesPerSecond = 12f;

    [Header("SFX")]
    public AudioClip shieldHitSfx;
    public AudioClip playerHitSfx;
    public float sfxVolume = 1f;

    [Header("Audio")]
    public AudioSource sfxSource;

    [Header("Life / Cut settings")]
    public bool immediateDestroyOnLifeEnd = true;

    // Cache
    private SpriteRenderer spriteRenderer;
    private Coroutine spriteAnimCoroutine;
    private AudioMixerGroup sfxMixerGroup;
    private Transform cachedTransform;
    private WaitForSeconds frameDelay;
    private Collider2D[] cachedColliders;
    private ParticleSystem[] cachedParticles;
    private AudioSource[] cachedAudioSources;

    // Constantes
    private const string SHIELD_TAG = "Shield";
    private const string PLAYER_TAG = "PlayerSoul";
    private static ChronosGameManager gameManager;

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    void Awake()
    {
        cachedTransform = transform;

        // Cache les composants
        cachedColliders = GetComponentsInChildren<Collider2D>(true);
        cachedParticles = GetComponentsInChildren<ParticleSystem>(true);
        cachedAudioSources = GetComponentsInChildren<AudioSource>(true);

        // Setup AudioSource
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }
        }

        // Find AudioMixerGroup (une seule fois)
        if (sfxMixerGroup == null)
        {
            AudioMixerGroup[] groups = Resources.FindObjectsOfTypeAll<AudioMixerGroup>();
            foreach (AudioMixerGroup g in groups)
            {
                if (g == null) continue;
                string n = g.name.ToLowerInvariant();
                if (n.Contains("sfx"))
                {
                    sfxMixerGroup = g;
                    break;
                }
            }
        }

        if (sfxMixerGroup != null && sfxSource != null)
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    void OnEnable()
    {
        hasHitShield = false;
        hasHitPlayer = false;

        // Cache singleton
        if (gameManager == null)
            gameManager = ChronosGameManager.Instance;

        // Cache components
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Pré-calcule le frameDelay
        if (frameDelay == null && framesPerSecond > 0)
            frameDelay = new WaitForSeconds(1f / framesPerSecond);

        // Start animation
        if (spriteRenderer != null && loopFrames != null && loopFrames.Length > 0)
        {
            spriteAnimCoroutine = StartCoroutine(PlayLoopAnimation());
        }

        // Safety destroy
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
        // Shield priority
        if (other.CompareTag(SHIELD_TAG) && !hasHitShield)
        {
            OnBlockedByShield();
            return;
        }

        // Player hit
        if (other.CompareTag(PLAYER_TAG) && !hasHitPlayer && !hasHitShield)
        {
            hasHitPlayer = true;
            gameManager.DamagePlayer(damage);

            if (playerHitSfx != null)
            {
                if (sfxSource != null)
                    sfxSource.PlayOneShot(playerHitSfx, sfxVolume);
                else
                    SpawnOneShotAtPosition(playerHitSfx, cachedTransform.position);
            }
            return;
        }
    }

    public void OnBlockedByShield()
    {
        if (hasHitShield) return;
        hasHitShield = true;

        if (shieldHitSfx != null)
            SpawnOneShotAtPosition(shieldHitSfx, cachedTransform.position);

        DestroyWithImpact();
    }

    void SpawnOneShotAtPosition(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;

        GameObject go = new GameObject($"SFX_{clip.name}");
        go.transform.position = pos;
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;

        if (sfxMixerGroup != null)
            src.outputAudioMixerGroup = sfxMixerGroup;

        src.PlayOneShot(clip, sfxVolume);
        Destroy(go, clip.length + 0.1f);
    }

    public void DestroyWithImpact()
    {
        if (!gameObject.activeInHierarchy) return;

        StopSpriteAnimation();
        Destroy(gameObject);
        CancelInvoke(nameof(ForceDestroy));
    }

    IEnumerator PlayLoopAnimation()
    {
        if (spriteRenderer == null || loopFrames == null || loopFrames.Length == 0)
            yield break;

        int idx = 0;
        int frameCount = loopFrames.Length;

        while (true)
        {
            spriteRenderer.sprite = loopFrames[idx];
            idx = (idx + 1) % frameCount;
            yield return frameDelay;
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

    void ForceDestroy()
    {
        if (immediateDestroyOnLifeEnd)
        {
            CutEverythingImmediate();
            Destroy(gameObject);
            return;
        }

        DestroyWithImpact();
    }

    void CutEverythingImmediate()
    {
        CancelInvoke(nameof(ForceDestroy));
        StopSpriteAnimation();

        // Utilise le cache de colliders
        foreach (Collider2D c in cachedColliders)
        {
            if (c != null) c.enabled = false;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        // Utilise le cache de particles
        foreach (ParticleSystem p in cachedParticles)
        {
            if (p != null) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Utilise le cache d'audio sources
        foreach (AudioSource a in cachedAudioSources)
        {
            if (a != null) a.Stop();
        }
    }
}