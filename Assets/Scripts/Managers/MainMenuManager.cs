using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameDataSO data;
    public Transform cam;
    public float swaySpeed;
    public float swayAmp;

    public Texture2D menuCursor;
    // public Transform cursorEffectParent;
    public Image blackScreen;

    Vector3 startPos;

    private void Start()
    {
        startPos = cam.position;
        Cursor.SetCursor(menuCursor, 16 * Vector2.one, CursorMode.ForceSoftware);
    }

    private void Update()
    {
        float offset = swayAmp * (2f * Mathf.PerlinNoise(Time.time * swaySpeed, 0f) - 1f);

        cam.position = startPos + offset * Vector3.right;

        // cursorEffectParent.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Break();
    }

    public void Play()
    {
        StartCoroutine(SceneLoadAnim());
    }

    IEnumerator SceneLoadAnim()
    {
        blackScreen.enabled = true;

        float timer = 0f;
        while (timer < data.respawnTime / 2f)
        {
            float t = timer / data.respawnTime * 2f;

            blackScreen.transform.localEulerAngles = new Vector3(0f, (1f - t) * 90f, 0f);

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        blackScreen.transform.localPosition = Vector3.zero;

        SceneManager.LoadScene(data.gameLevelName);
    }
}
