using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

public enum GameState
{
    MENU,
    GAME,
    PAUSE,
    DEATH
}

public enum ControllerType
{
    KEYBOARD,
    XBOX,
    PS,
    SWITCH,
    GENERIC
}


public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;
    public GameState m_lastState;
    public GameState m_gameState;    
    public Player m_player;

    public InputDevice m_iDevice;
    public ControllerType m_device;

    public EventSystem m_eventSystem;

    public static GameManager Instance
    {
        get
        {
            m_instance = FindObjectOfType<GameManager>();
            if (m_instance == null)
            {
                Debug.LogError("Game Manager is Null!!");
            }
            return m_instance;
        }
    }

    void Awake()
    {
        
    }

    void Start()
    {
        m_eventSystem = FindObjectOfType<EventSystem>();
        InputSystem.onEvent += InputDeviceChanged;
    }

    public void GameStateChanged(GameState state)
    {
        if (state == m_gameState)
            return;
        m_lastState = m_gameState;
        m_gameState = state;
        switch (state)
        {
            case GameState.MENU:
             //   MainMenu();
                break; 
            case GameState.GAME:
             //   Game();
                break;
            case GameState.PAUSE:
             //   PauseGame();
                break;
            case GameState.DEATH:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

  /*  void MainMenu()
    {
        Time.timeScale = 0;
    }

    void Game()
    {
        Time.timeScale = 1;
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
    }

    void PauseGame()
    {
        Time.timeScale = 0;
    }*/

    public void InputDeviceChanged(InputEventPtr eventPtr, InputDevice device)
    {
        if (m_iDevice == device )
        {
            return;
        }

        if (eventPtr.type != StateEvent.Type)
        {
            return;
        }

        bool validPress = false;

        foreach (InputControl control in eventPtr.EnumerateChangedControls(device, 0.01F))
        {
            validPress = true;
            break;
        }

        if (validPress is false) return;

        if (device is Keyboard || device is Mouse)
        {
            m_iDevice = device;
            if (m_device == ControllerType.KEYBOARD) return;
            SwapControls(ControllerType.KEYBOARD);
        }
        if (device is XInputController)
        {
            m_iDevice = device;
            SwapControls(ControllerType.XBOX);
        }
        else if (device is DualShockGamepad)
        {
            m_iDevice = device;
            SwapControls(ControllerType.PS);
        }
        else if (device is SwitchProControllerHID)
        {
            m_iDevice = device;
            SwapControls(ControllerType.SWITCH);
        }
        else if (device is Gamepad)
        {
            m_iDevice = device;
            SwapControls(ControllerType.GENERIC);
        }
    }

    void SwapControls(ControllerType controls)
    {
        m_device = controls;
        switch (controls)
        {
            case ControllerType.KEYBOARD:
            //    UpdateUIImages(m_keyboardImages);
                break;
            case ControllerType.XBOX:
             //   UpdateUIImages(m_xBoxImages);
                break;
            case ControllerType.PS:
           //     UpdateUIImages(m_pSImages);
                break;
            case ControllerType.SWITCH:
           //     UpdateUIImages(m_nintendoImages);
                break;
            case ControllerType.GENERIC:
             //   UpdateUIImages(m_genericImages);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(controls), controls, null);
        }
    }
}