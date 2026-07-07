using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class Collectible : MonoBehaviour
{
    [Header("Visual")]
    public float rotateSpeed = 90f;
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 1.5f;

    [Header("Feedback")]
    public Color particleColor = new Color(1f, 0.85f, 0.2f);

    [Header("Scale Punch")]
    public bool enableScalePunch = true;
    public float punchScale = 1.5f;
    public float punchDuration = 0.05f;

    public Image TearImage;
    public Color TearImageColor;

   Vector3 startPos;
    static AudioClip collectClip;
    static Material sharedParticleMaterial;
    static JuiceFeedback cachedJuiceFeedback;
    bool collected;

    Animator animator;

    void Start()
    {
        startPos = transform.position;
        if (collectClip == null)
            collectClip = GenerateDing();
        if (sharedParticleMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null)
                sharedParticleMaterial = new Material(shader);
        }

        Color tempColor = TearImage.color;
        if (TearImage != null)
            tempColor.a = 0.1f;
            TearImageColor.a = tempColor.a;

        animator = TearImage.GetComponent<Animator>();

    }

    void Update()
    {
        if (collected) return;
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.CollectOrb();

        AudioSource.PlayClipAtPoint(collectClip, transform.position, 0.8f);
        SpawnParticles();

        if (cachedJuiceFeedback == null)
        {
            var cam = Camera.main;
            if (cam != null)
                cachedJuiceFeedback = cam.GetComponent<JuiceFeedback>();
            if (cachedJuiceFeedback == null)
                cachedJuiceFeedback = FindFirstObjectByType<JuiceFeedback>();
        }

        var juice = cachedJuiceFeedback;
        if (juice != null) juice.TriggerJuice();

        TearImageColor.a = 1f;
        TearImage.color = TearImageColor;
        if (animator != null)
            animator.SetTrigger("Collected");

        if (enableScalePunch)
            StartCoroutine(DoScalePunchThenDestroy());
        else
            Destroy(gameObject);
    }

    IEnumerator DoScalePunchThenDestroy()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * punchScale;

        float half = punchDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    void SpawnParticles()
    {
        var go = new GameObject("CollectBurst");
        go.transform.position = transform.position;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 4f;
        main.startSize = 0.12f;
        main.startColor = particleColor;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.5f;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        if (sharedParticleMaterial != null)
            renderer.sharedMaterial = sharedParticleMaterial;
        var mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", particleColor);
        mpb.SetColor("_Color", particleColor);
        renderer.SetPropertyBlock(mpb);

        ps.Play();
        Destroy(go, 1.5f);
    }

    static AudioClip GenerateDing()
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 12f);
            data[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.5f * envelope
                     + Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.3f * envelope;
        }

        var clip = AudioClip.Create("CollectDing", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
