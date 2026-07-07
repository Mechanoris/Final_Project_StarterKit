using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.Video;

public class HUDManager : MonoBehaviour
{
    [Header("In-Game HUD")]
    public Text timerText;
    public Text orbCountText;
    public Image crosshair;
    public Animator eyeTimerAnimator;
    public GameObject CollectedSquares;
    public GameObject Eye;
    public GameObject DialogueContainerStart;
    public GameObject DialogueContainerBabySceneEnter;

    [Header("End Screens")]
    public GameObject startPanel;
    public Text Title;
    public Button startButton;
    public Text startButtonText;
    public Button settingsButton;
    public Text settingsButtonText;
    public Button quitButton;
    public Text quitButtonText;
    public GameObject StartCutsceneRoot;
    public VideoPlayer StartCutscene;
    public GameObject EndWinCutsceneRoot;
    public VideoPlayer EndWinCutscene;
    public GameObject EndLoseCutsceneRoot;
    public VideoPlayer EndLoseCutscene;

    [Header("End Screens")]
    public GameObject winPanel;
    public Text winMessageText;
    public Button winRestartButton;

    public GameObject losePanel;
    public Text loseMessageText;
    public Button loseRestartButton;

    [Header("HP Bar")]
    public Image hpBarBackground;
    public Image hpBarFill;
    public Text hpText;

    [Header("Screen Flash")]
    public Image flashOverlay;

    [Header("Orb Count Punch")]
    public bool enableOrbCountPunch = true;
    public float orbPunchScale = 1.3f;
    public float orbPunchDuration = 0.15f;

    Color timerNormalColor = Color.white;
    Color timerUrgentColor = new Color(1f, 0.25f, 0.2f);
    Coroutine flashCoroutine;
    Coroutine orbPunchCoroutine;

    void Start()
    {
        if (startPanel != null) winPanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(PlayStartCutscene);

        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestart);
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(OnRestart);

        EnsureFlashOverlay();

        eyeTimerAnimator.SetFloat("Time", 0f);
    }

    void EnsureFlashOverlay()
    {
        if (flashOverlay != null)
        {
            flashOverlay.color = Color.clear;
            flashOverlay.raycastTarget = false;
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            parentCanvas = FindAnyObjectByType<Canvas>();
        if (parentCanvas == null) return;

        var go = new GameObject("FlashOverlay");
        go.transform.SetParent(parentCanvas.transform, false);
        go.transform.SetAsLastSibling();

        flashOverlay = go.AddComponent<Image>();
        flashOverlay.color = Color.clear;
        flashOverlay.raycastTarget = false;

        var rect = flashOverlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText == null) return;
        int seconds = Mathf.CeilToInt(Mathf.Max(0f, timeRemaining));
        int min = seconds / 60;
        int sec = seconds % 60;
        timerText.text = string.Format("{0}:{1:00}", min, sec);
        timerText.color = timeRemaining <= 10f ? timerUrgentColor : timerNormalColor;
        eyeTimerAnimator.SetFloat("Time", timeRemaining);
    }

    public void UpdateOrbCount(int collected, int total)
    {
        if (orbCountText != null)
            orbCountText.text = collected + " / " + total;

        if (enableOrbCountPunch && orbCountText != null)
            PunchOrbCount();
    }

    public void UpdateHP(int current, int max)
    {
        if (hpBarFill != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            hpBarFill.fillAmount = ratio;
            hpBarFill.color = Color.Lerp(new Color(0.85f, 0.15f, 0.15f), new Color(0.15f, 0.85f, 0.25f), ratio);
        }
        if (hpText != null)
            hpText.text = current + " / " + max;
    }

    public void ShowWin(float timeTaken)
    {
        if (winPanel == null) return;
        winPanel.SetActive(true);
        if (crosshair != null) crosshair.enabled = false;
        if (EndWinCutscene.isPlaying == true)
            return;
        if (winMessageText != null)
            winMessageText.text = "You Win!\nTime: " + timeTaken.ToString("F1") + "s";
    }

    public void ShowLose(string message = "Time's Up!")
    {
        if (losePanel == null) return;
        losePanel.SetActive(true);
        if (crosshair != null) crosshair.enabled = false;
        //if (EndLoseCutscene.isPlaying == true)
        //    return;
        if (loseMessageText != null)
            loseMessageText.text = message;
    }

    public void DoScreenFlash(Color color, float duration)
    {
        if (flashOverlay == null) return;
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine(color, duration));
    }

    IEnumerator FlashRoutine(Color color, float duration)
    {
        flashOverlay.color = color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Color c = color;
            c.a = Mathf.Lerp(color.a, 0f, t);
            flashOverlay.color = c;
            yield return null;
        }
        flashOverlay.color = Color.clear;
        flashCoroutine = null;
    }

    void PunchOrbCount()
    {
        if (orbPunchCoroutine != null)
            StopCoroutine(orbPunchCoroutine);
        orbPunchCoroutine = StartCoroutine(OrbPunchRoutine());
    }

    IEnumerator OrbPunchRoutine()
    {
        Transform t = orbCountText.transform;
        Vector3 original = Vector3.one;
        Vector3 punched = original * orbPunchScale;
        float half = orbPunchDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / half);
            t.localScale = Vector3.Lerp(original, punched, p);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / half);
            t.localScale = Vector3.Lerp(punched, original, p);
            yield return null;
        }

        t.localScale = original;
        orbPunchCoroutine = null;
    }

    void OnRestart()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    void OnStart()
    {
        if (true)
        {
            if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        }
    }

    void HideStartPanel()     
    {
        if (startPanel != null)
            startPanel.SetActive(false);
        if (crosshair != null)
            crosshair.enabled = true;
    }

    void PlayStartCutscene()
    {
        if (StartCutscene != null)
        {
            HideStartPanel();
            StartCutsceneRoot.SetActive(true);
            StartCutscene.Play();
            StartCutscene.loopPointReached += StartCutsceneEndReached;
        }
    }

    void StartCutsceneEndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed;
        StartCutsceneRoot.SetActive(false);
        OnStart();
        timerText.enabled = true;
         if (crosshair != null)
            crosshair.enabled = true;
        Eye.SetActive(true);
        CollectedSquares.SetActive(true);
        DialogueContainerStart.SetActive(true);

    }

    public void PlayEndWinCutscene()
    {
        EndWinCutsceneRoot.SetActive(true);
        EndWinCutscene.Play();
        EndWinCutscene.loopPointReached += EndWinCutsceneEndReached;
    }

    void EndWinCutsceneEndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed;
        EndWinCutsceneRoot.SetActive(false);
    }

    //public void PlayEndLoseCutscene()
    //{
    //    EndLoseCutsceneRoot.SetActive(true);
    //    EndLoseCutscene.Play();
    //    EndLoseCutscene.loopPointReached += EndLoseCutsceneEndReached;
    //}

    //void EndLoseCutsceneEndReached(UnityEngine.Video.VideoPlayer vp)
    //{
    //    vp.playbackSpeed = vp.playbackSpeed;
    //    EndLoseCutsceneRoot.SetActive(false);
    //}

    void PointerEnter()
    {        
        if (crosshair != null)
        crosshair.color = Color.red;
    }

}
