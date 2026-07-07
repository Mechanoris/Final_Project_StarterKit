using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class JuiceFeedback : MonoBehaviour
{
    [Header("Effect Toggles")]
    public bool enableShake = true;
    public bool enableFOVKick = true;
    public bool enableChromaticPulse = true;
    public bool enableScreenFlash = true;
    public bool enableSlowMotion = true;

    [Header("Shake Settings")]
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.08f;

    [Header("FOV Kick")]
    public float fovKickAmount = 5f;
    public float fovKickDuration = 0.15f;

    [Header("Chromatic Aberration Pulse")]
    public float chromaticIntensity = 0.8f;
    public float chromaticDuration = 0.2f;

    [Header("Screen Flash")]
    public Color flashColor = new Color(1f, 1f, 1f, 0.4f);
    public float flashDuration = 0.15f;

    [Header("Slow Motion")]
    public float slowMotionScale = 0.1f;
    public float slowMotionDuration = 0.08f;

    float shakeTimer;
    float currentShakeDuration;
    float currentShakeMagnitude;
    Vector3 originalLocalPos;

    Camera cam;
    float baseFOV;
    float baseFixedDeltaTime;
    Volume postVolume;
    ChromaticAberration chromaticAberration;
    HUDManager hudManager;
    Coroutine fovCoroutine;
    Coroutine chromaticCoroutine;
    Coroutine slowMoCoroutine;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        cam = GetComponent<Camera>();
        if (cam != null)
            baseFOV = cam.fieldOfView;
        baseFixedDeltaTime = Time.fixedDeltaTime;

        postVolume = FindAnyObjectByType<Volume>();
        if (postVolume != null && postVolume.profile != null)
        {
            if (!postVolume.profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = postVolume.profile.Add<ChromaticAberration>(true);
                chromaticAberration.intensity.overrideState = true;
                chromaticAberration.intensity.value = 0f;
            }
        }

        hudManager = FindAnyObjectByType<HUDManager>();
    }

    public void TriggerJuice()
    {
        if (enableShake)
            Shake(shakeDuration, shakeMagnitude);
        if (enableFOVKick)
        {
            if (fovCoroutine != null) StopCoroutine(fovCoroutine);
            fovCoroutine = StartCoroutine(DoFOVKick());
        }
        if (enableChromaticPulse)
        {
            if (chromaticCoroutine != null) StopCoroutine(chromaticCoroutine);
            chromaticCoroutine = StartCoroutine(DoChromaticPulse());
        }
        if (enableScreenFlash && hudManager != null)
            hudManager.DoScreenFlash(flashColor, flashDuration);
        if (enableSlowMotion)
        {
            if (slowMoCoroutine != null) StopCoroutine(slowMoCoroutine);
            slowMoCoroutine = StartCoroutine(DoSlowMotion());
        }
    }

    public void Shake(float duration, float magnitude)
    {
        currentShakeDuration = duration;
        currentShakeMagnitude = magnitude;
        shakeTimer = duration;
    }

    void LateUpdate()
    {
        if (shakeTimer > 0f)
        {
            float progress = shakeTimer / currentShakeDuration;
            float dampedMag = currentShakeMagnitude * progress;
            transform.localPosition = originalLocalPos + Random.insideUnitSphere * dampedMag;
            shakeTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            transform.localPosition = originalLocalPos;
        }
    }

    IEnumerator DoFOVKick()
    {
        if (cam == null) yield break;

        float elapsed = 0f;
        while (elapsed < fovKickDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fovKickDuration;
            float curve = 1f - t;
            cam.fieldOfView = baseFOV + fovKickAmount * curve;
            yield return null;
        }
        cam.fieldOfView = baseFOV;
        fovCoroutine = null;
    }

    IEnumerator DoChromaticPulse()
    {
        if (chromaticAberration == null) yield break;

        float elapsed = 0f;
        while (elapsed < chromaticDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / chromaticDuration;
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticIntensity, 0f, t);
            yield return null;
        }
        chromaticAberration.intensity.value = 0f;
        chromaticCoroutine = null;
    }

    IEnumerator DoSlowMotion()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            yield break;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = baseFixedDeltaTime * Time.timeScale;

        float elapsed = 0f;
        while (elapsed < slowMotionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsGameOver)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = baseFixedDeltaTime;
        }
        slowMoCoroutine = null;
    }
}
