using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static UnityAction OnGameFreeze;
    public static UnityAction OnGameUnfreeze;
    public static UnityAction OnGamePause;
    public static UnityAction OnGameUnpause;
    public static GameDataSO data;

    static bool gameFrozen = false;
    static bool gamePaused = false;
    static float prevTimescale;
    static float pauseCooldownTime;

    BearControllerSM player;
    public bool useDebugFPS = false;
    public int targetfps = 30;
    public GameDataSO gameData;
    public Image blackScreen;

    bool killingPlayer = false;
    bool pauseOverride = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Duplicate GameManager found, deleting self");
            Destroy(this);
            return;
        }

        instance = this;
        data = gameData;

        if (useDebugFPS)
        {
            Application.targetFrameRate = targetfps;
        }
        else
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
        }

        data.PauseGame += PauseGame;
        data.UnpauseGame += UnpauseGame;
        data.PauseInput += HandlePauseInput;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = BearControllerSM.instance;

        InputManager.ClearSchemeStack();
        StartCoroutine(LoadInAnim());

        gamePaused = false;
        gameFrozen = false;
    }

    private void OnDestroy()
    {
        data.PauseGame -= PauseGame;
        data.UnpauseGame -= UnpauseGame;
        data.PauseInput -= HandlePauseInput;

        instance = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void KillPlayer()
    {
        if (killingPlayer) return;

        StartCoroutine(RespawnAnim());
    }

    public static void FreezeGame()
    {
        if (gameFrozen)
        {
            return;
        }

        OnGameFreeze?.Invoke();

        prevTimescale = Time.timeScale;
        Time.timeScale = 0f;
        gameFrozen = true;
    }

    public void OnPauseInput()
    {
        if (Time.unscaledTime < pauseCooldownTime || pauseOverride) return;

        data.InvokePauseInput();
    }

    void HandlePauseInput()
    {
        if (!gamePaused)
        {
            data.InvokePause();
        }
    }

    public static void UnfreezeGame()
    {
        if (!gameFrozen)
        {
            return;
        }

        Time.timeScale = prevTimescale;
        gameFrozen = false;

        OnGameUnfreeze?.Invoke();
    }

    public static void PauseGame()
    {
        if (gamePaused || Time.unscaledTime < pauseCooldownTime)
        {
            return;
        }
        gamePaused = true;
        pauseCooldownTime = Time.unscaledTime + data.pauseCooldown;

        FreezeGame();
        InputManager.PushInputScheme(InputScheme.MENU);
        OnGamePause?.Invoke();

        SceneManager.LoadScene(data.pauseSceneName, LoadSceneMode.Additive);
    }

    public static void UnpauseGame()
    {
        if (!gamePaused || Time.unscaledTime < pauseCooldownTime)
        {
            return;
        }
        gamePaused = false;
        pauseCooldownTime = Time.unscaledTime + data.pauseCooldown;

        OnGameUnpause?.Invoke();
        InputManager.PopInputScheme();
        UnfreezeGame();

        SceneManager.UnloadSceneAsync(data.pauseSceneName);
    }

    public static void Quit()
    {
        if (instance != null)
        {
            instance.QuitToMenu();
        }
    }

    public static void ResetLevel()
    {
        instance?.ResetLevelHelper();
    }

    public void ResetLevelHelper()
    {
        SceneManager.UnloadSceneAsync(data.pauseSceneName);

        StartCoroutine(ExitAnim(SceneManager.GetActiveScene().name));
    }

    public void QuitToMenu()
    {
        SceneManager.UnloadSceneAsync(data.pauseSceneName);

        StartCoroutine(ExitAnim(data.mainMenuSceneName));
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Break();
    }

    IEnumerator ExitAnim(string sceneName)
    {
        InputManager.ChangeInputScheme(InputScheme.DISABLED);
        FreezeGame();

        blackScreen.enabled = true;

        float timer = 0f;
        while (timer < data.respawnTime / 2f)
        {
            float t = timer / data.respawnTime * 2f;

            blackScreen.transform.localEulerAngles = new Vector3(0f, (1f - t) * 90f, 0f);

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    IEnumerator LoadInAnim()
    {
        blackScreen.enabled = true;

        InputManager.ChangeInputScheme(InputScheme.DISABLED);
        UnfreezeGame();

        float timer = 0f;
        while (timer < data.respawnTime / 2f)
        {
            float t = timer / data.respawnTime * 2f;

            blackScreen.transform.localEulerAngles = new Vector3(t * 90f, 0f, 0f);

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        killingPlayer = false;
        pauseOverride = false;
        blackScreen.enabled = false;

        InputManager.ChangeInputScheme(InputScheme.INGAME);
    }

    IEnumerator RespawnAnim()
    {
        FreezeGame();

        killingPlayer = true;
        pauseOverride = true;
        blackScreen.enabled = true;

        float timer = 0f;
        while (timer < data.respawnTime / 2f)
        {
            float t = timer / data.respawnTime * 2f;

            blackScreen.transform.localEulerAngles = new Vector3(0f, (1f - t) * 90f, 0f);

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        player.ResetController(RespawnPoint.GetSpawnPoint());
        InputManager.ChangeInputScheme(InputScheme.DISABLED);
        UnfreezeGame();

        while (timer < data.respawnTime)
        {
            float t = timer / data.respawnTime * 2f - 1f;

            blackScreen.transform.localEulerAngles = new Vector3(t * 90f, 0f, 0f);

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        killingPlayer = false;
        pauseOverride = false;
        blackScreen.enabled = false;

        InputManager.ChangeInputScheme(InputScheme.INGAME);

    }
}
