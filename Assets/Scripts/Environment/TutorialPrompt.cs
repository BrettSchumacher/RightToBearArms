using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class TutorialPrompt : MonoBehaviour
{
    public TextMeshProUGUI text;
    public bool beginRegistered = false;
    public bool useConditions = true;
    public List<TransitionCondition> completionConditions;

    GameDataSO data;
    Collider2D player;
    bool registered = false;
    bool activated = false;
    bool completed = false;

    bool fading = false;

    // Start is called before the first frame update
    void Start()
    {
        data = GameManager.data;
        player = BearControllerSM.instance.GetComponent<Collider2D>();

        if (beginRegistered)
        {
            RegisterTutorial();
        }

        foreach (TransitionCondition condition in completionConditions)
        {
            condition.condition.Initialize(BearControllerSM.instance);
        }
    }

    private void Update()
    {
        if (!activated || completed || !useConditions) return;

        bool complete = true;
        foreach (TransitionCondition condition in completionConditions)
        {
            if (condition.condition.IsConditionMet() == condition.inverted)
            {
                complete = false;
                return;
            }
        }

        if (complete)
        {
            CompleteTutorial();
        }
    }

    public void RegisterTutorial()
    {
        registered = true;

        if (GetComponent<Collider2D>().IsTouching(player))
        {
            ActivateTutorial();
        }
    }

    public void ActivateTutorial()
    {
        if (!registered || activated || completed) return;

        activated = true;
        StartCoroutine(FadeInTutorial());
    }

    public void CompleteTutorial()
    {
        if (!activated || fading || completed) return;

        completed = true;

        StartCoroutine(FadeOutTutorial());
    }

    public void DeleteTutorial()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ActivateTutorial();
    }

    IEnumerator FadeInTutorial()
    {
        fading = true;

        float startTime = Time.time;
        float endTime = Time.time + data.textFadeInTime;

        Color startColor = text.color;
        Color endColor = text.color;

        startColor.a = 0f;
        endColor.a = 1f;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / data.textFadeInTime;
            text.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        text.color = endColor;
        fading = false;
    }

    IEnumerator FadeOutTutorial()
    {
        fading = true;

        float startTime = Time.time;
        float endTime = Time.time + data.textFadeOutTime;

        Color startColor = text.color;
        Color endColor = text.color;

        startColor.a = 1f;
        endColor.a = 0f;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / data.textFadeOutTime;
            text.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        text.color = endColor;
        fading = false;

        Destroy(gameObject);
    }
}
