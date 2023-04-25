using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    public GameDataSO gameData;
    public RectTransform menuObject;
    public RectTransform dimObject;
    public GameObject defaultMenu;

    public Vector2 startSignOffset = new Vector2(0f, -1080f);
    public Vector2 startDimOffset = new Vector2(0f, 1080f);

    [Header("Opening Animation")]
    public AnimationCurve yPosOpening;
    public AnimationCurve yScaleOpening;

    [Header("Closing Animation")]
    public AnimationCurve yPosClosing;
    public AnimationCurve yScaleClosing;

    Stack<GameObject> menuStack;
    GameObject currentMenu;
    bool closing = false;
    Vector2 menuDefaultPos;
    Vector2 menuDefaultScale;
    Vector2 dimDefaultPos;

    // Start is called before the first frame update
    void Start()
    {
        menuStack = new Stack<GameObject>();
        currentMenu = defaultMenu;
        menuDefaultPos = menuObject.position;
        menuDefaultScale = menuObject.localScale;
        dimDefaultPos = dimObject.position;

        StartCoroutine(OpeningAnim(gameData.pauseAnimLength));

        gameData.PauseInput += CloseSubmenu;
    }

    private void OnDestroy()
    {
        gameData.PauseInput -= CloseSubmenu;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenSubmenu(GameObject submenu)
    {
        if (closing) return;

        currentMenu.SetActive(false);

        submenu.SetActive(true);
        menuStack.Push(submenu);
    }

    public void CloseSubmenu()
    {
        if (closing) return;

        if (menuStack.Count == 0)
        {
            closing = true;
            StartCoroutine(ClosingAnim(gameData.pauseAnimLength));
            return;
        }

        currentMenu.SetActive(false);
        currentMenu = menuStack.Pop();
        currentMenu.SetActive(true);
    }

    public void Quit()
    {
        GameManager.Quit();
    }

    public void ResetLevel()
    {
        GameManager.ResetLevel();
    }

    IEnumerator OpeningAnim(float animLength)
    {
        float startTime = Time.unscaledTime;
        float endTime = Time.unscaledTime + animLength;

        Vector2 startPos = menuDefaultPos + startSignOffset;
        Vector2 endPos = menuDefaultPos;

        Vector2 startDim = dimDefaultPos + startDimOffset;
        Vector2 endDim = dimDefaultPos;

        while (Time.unscaledTime < endTime)
        {
            float t = (Time.unscaledTime - startTime) / animLength;

            float posLerp = yPosOpening.Evaluate(t);
            menuObject.position = Vector2.Lerp(startPos, endPos, posLerp);

            float yScale = yScaleOpening.Evaluate(t);
            float xScale = 1f / yScale;
            menuObject.localScale = menuDefaultScale * new Vector2(xScale, yScale);

            dimObject.position = Vector2.Lerp(startDim, endDim, posLerp);

            yield return null;
        }

        menuObject.position = endPos;
        menuObject.localScale = menuDefaultScale;
        dimObject.position = endDim;
    }

    IEnumerator ClosingAnim(float animLength)
    {
        float startTime = Time.unscaledTime;
        float endTime = Time.unscaledTime + animLength;

        Vector2 startPos = menuDefaultPos + startSignOffset;
        Vector2 endPos = menuDefaultPos;

        Vector2 startDim = dimDefaultPos + startDimOffset;
        Vector2 endDim = dimDefaultPos;

        while (Time.unscaledTime < endTime)
        {
            float t = (endTime - Time.unscaledTime) / animLength;

            float posLerp = yPosOpening.Evaluate(t);
            menuObject.position = Vector2.Lerp(startPos, endPos, posLerp);

            float yScale = yScaleOpening.Evaluate(t);
            float xScale = 1f / yScale;
            menuObject.localScale = menuDefaultScale * new Vector2(xScale, yScale);

            dimObject.position = Vector2.Lerp(startDim, endDim, posLerp);

            yield return null;
        }

        gameData.InvokeUnpause();
    }
}
