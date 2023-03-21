using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public InputScheme defaultScheme;

    PlayerInput input;
    InputScheme currentScheme;
    Stack<InputScheme> schemeStack;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Multiple Input Managers found, deleting self...");
            Destroy(this);
            return;
        }

        instance = this;
        input = GetComponent<PlayerInput>();
        currentScheme = defaultScheme;
        schemeStack = new Stack<InputScheme>();

        input.SwitchCurrentActionMap(InputSchemeToName(currentScheme));
    }

    public void ChangeInputSchemeHelper(InputScheme newScheme)
    {
        currentScheme = newScheme;
        input.SwitchCurrentActionMap(InputSchemeToName(currentScheme));
    }

    public void PushInputSchemeHelper(InputScheme newScheme)
    {
        schemeStack.Push(currentScheme);
        ChangeInputSchemeHelper(newScheme);
    }

    public void PopInputSchemeHelper()
    {
        if (schemeStack.Count == 0)
        {
            ChangeInputSchemeHelper(defaultScheme);
            return;
        }

        ChangeInputSchemeHelper(schemeStack.Pop());
    }


    public static string InputSchemeToName(InputScheme scheme)
    {
        switch (scheme)
        {
            case InputScheme.DIALOGUE:
                return "Dialogue";
            case InputScheme.MENU:
                return "Menu";
            default:
                return "InGame";
        }
    }

    public static void ChangeInputScheme(InputScheme newScheme)
    {
        instance.ChangeInputSchemeHelper(newScheme);
    }

    public static void PushInputScheme(InputScheme newScheme)
    {
        instance.PushInputSchemeHelper(newScheme);
    }

    public static void PopInputScheme()
    {
        instance.PopInputSchemeHelper();
    }
}

public enum InputScheme
{
    INGAME,
    MENU,
    DIALOGUE,
}

