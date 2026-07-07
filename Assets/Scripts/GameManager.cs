using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameDuration = 60f;
    public int maxHP = 5;

    [Header("References")]
    public HUDManager hud;
    public InputActionReference restartAction;
    //public FirstPersonController firstPersonController;

    [Header("Sounds")]
    public GameObject InGameSounds;
    public GameObject MenuSound;

    public int TotalOrbs { get; set; }
    public int CollectedOrbs { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsGameOver { get; private set; }

    public bool GameNotStarted;
    public int CurrentHP { get; private set; }
    float baseFixedDeltaTime;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        TimeRemaining = gameDuration;
        CurrentHP = maxHP;
        baseFixedDeltaTime = Time.fixedDeltaTime;
        GameNotStarted = true;
    }

    void Start()
    {
        if (GameNotStarted)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }   

    void toStart()
    {
        if (hud == null)
            hud = FindFirstObjectByType<HUDManager>();

        TotalOrbs = FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;

        //if (GameNotStarted) return;

        if (hud != null)
        {
            hud.UpdateOrbCount(CollectedOrbs, TotalOrbs);
            hud.UpdateHP(CurrentHP, maxHP);
        }

        InGameSounds.SetActive(true);
        MenuSound.SetActive(false);
    }

    void OnEnable()
    {
        restartAction?.action?.Enable();
    }

    void OnDisable()
    {
        restartAction?.action?.Disable();
    }

    void Update()
    {
        if (DidRequestRestart())
            RestartGame();

        if (IsGameOver) return;
        if (GameNotStarted) return;

        TimeRemaining -= Time.deltaTime;
        if (hud != null) hud.UpdateTimer(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndGame(false, "Time's Up!");
        }

        
    }

    bool DidRequestRestart()
    {
        //if (firstPersonController.FallTooMuch())
        //    return true;
        if (restartAction != null && restartAction.action != null && restartAction.action.WasPressedThisFrame())
            return true;
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
    }

    public void CollectOrb()
    {
        if (IsGameOver) return;
        if (GameNotStarted) return;
        CollectedOrbs++;
        if (hud != null) hud.UpdateOrbCount(CollectedOrbs, TotalOrbs);

        if (CollectedOrbs >= TotalOrbs)
            EndGame(true);
    }

    public void PlayerHitByEnemy()
    {
        if (IsGameOver) return;
        if (GameNotStarted) return;
        CurrentHP--;
        if (hud != null)
        {
            hud.UpdateHP(CurrentHP, maxHP);
            hud.DoScreenFlash(new Color(1f, 0f, 0f, 0.7f), 3f);
        }
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            EndGame(false, "You died!");
        }
    }

    void EndGame(bool won, string loseMessage = "Time's Up!")
    {
        IsGameOver = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InGameSounds.SetActive(false);

        if (hud != null)
        {
            if (won)
            {
                hud.PlayEndWinCutscene();
                if (hud.EndWinCutscene.isPlaying != true)
                    return;
                hud.ShowWin(gameDuration - TimeRemaining);
            }
            else
            {
                //hud.PlayEndLoseCutscene();
                //if (hud.EndLoseCutscene.isPlaying != true)
                //    return;
                hud.ShowLose(loseMessage);
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDeltaTime > 0f ? baseFixedDeltaTime : 0.02f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartGame()     {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        toStart();
        GameNotStarted = false;
    }
}