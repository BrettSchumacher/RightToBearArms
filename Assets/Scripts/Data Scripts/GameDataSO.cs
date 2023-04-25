using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Data/GameData")]
public class GameDataSO : ScriptableObject
{
    public UnityAction PauseGame;
    public UnityAction UnpauseGame;
    public UnityAction PauseInput;

    public void InvokePause() { PauseGame?.Invoke(); }
    public void InvokeUnpause() { UnpauseGame?.Invoke(); }
    public void InvokePauseInput() { PauseInput?.Invoke(); }

    [Header("Screen Transition Values")]
    public float screenTransitionTime = 1.5f;
    public bool clearGrappleOnTransition = true;
    public float upTransitionSpeed = 5f;

    [Header("Pause Values")]
    public string pauseSceneName;
    public float pauseCooldown;
    public float pauseAnimLength;

    [Header("Death Values")]
    public float respawnTime = 0.5f;

    [Header("Main Menu Values")]
    public string mainMenuSceneName = "Main Menu";

    [Header("Game Level Data")]
    public string gameLevelName = "Demo";
}
