using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections.Generic;
using TMPro;

public enum GameState
{
    MENU,
    GAME,
    PAUSE,
    DEATH,
    INFO
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

    public WorldGen m_world;
    public TerrainModifier m_terrainModifier;    

    bool m_gameInit = true;
    public GameObject m_main;
    public GameObject m_hud;
    public GameObject m_pause;
    public GameObject m_end;
    public GameObject m_info;

    public Button m_newGame;
    public Button m_infoB;
    public Button m_quit;        
    public float m_startTime = 2f;
    public Button m_infoBack;

    public Slider m_health;
    public Slider m_attackTime;
    public Image m_reticleHit;
    public float m_reticleHitTime = 0.5f;    
    public float m_damageAlphaMax = 70;
    public float m_damageUpSpeed;
    public float m_damageWaitSpeed;
    public float m_damageDownSpeed;

    public Button m_resume;
    public Slider m_camSensitivity;
    public Button m_quitMenu;

    public Button m_respawnButton;
    public Button m_deathQuit;
    public float m_deathFade = 0.33f;
    public float m_endFade = 0.33f;

    public GameObject m_modeMinus;
    public GameObject m_modePlus;

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
        m_world = FindObjectOfType<WorldGen>();
        m_terrainModifier = FindObjectOfType<TerrainModifier>();
        GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(0.25f, 0.25f);
        GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(3, 2);
        UpdateGameState(GameState.MENU);

        //Main Menu Setup
        m_newGame.onClick.AddListener(delegate () { StartGame(); });
        m_infoB.onClick.AddListener(delegate () { Info(); });
        m_quit.onClick.AddListener(delegate () { QuitGame(); });

        //HUD Setup
        m_health.maxValue = m_player.m_health;
        m_attackTime.maxValue = m_player.m_attackTime;

        //Pause Menu Setup
        m_resume.onClick.AddListener(delegate () { ResumeGame(); });
        m_camSensitivity.onValueChanged.AddListener(m_player.ChangeSensitivity);
        m_camSensitivity.SetValueWithoutNotify(m_player.m_camSensitivity);
        m_quitMenu.onClick.AddListener(delegate () { MainMenu(); });

        //Info Setup                
        m_infoBack.onClick.AddListener(delegate () { MainMenu(); });

        //Death Setup
        m_respawnButton.onClick.AddListener(delegate () { MainMenu(); });
        m_respawnButton.gameObject.SetActive(false);
        m_deathQuit.onClick.AddListener(delegate () { QuitGame(); });
        m_deathQuit.gameObject.SetActive(false);
    }


    public void UpdateGameState(GameState state)
    {
        if (state == m_gameState)
            return;
        m_lastState = m_gameState;
        m_gameState = state;
        UpdateUI(state);
        switch (state)
        {
            case GameState.MENU:
                MainMenu();
                break;
            case GameState.GAME:
                if (m_lastState == GameState.MENU)
                {
                    m_world.StartGame();
                }
                else if (m_lastState == GameState.PAUSE)
                {
                    ResumeGame();
                }
                break;
            case GameState.PAUSE:
                PauseGame();
                break;
        }
    }

    void MainMenu()
    {
        Time.timeScale = 0;

        m_eventSystem.SetSelectedGameObject(m_newGame.gameObject);

        if (!(m_iDevice is Keyboard || m_iDevice is Mouse) && m_iDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_newGame.gameObject);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void Info()
    {
        UpdateGameState(GameState.INFO);
        if (!(m_iDevice is Keyboard || m_iDevice is Mouse) && m_iDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_infoBack.gameObject);
        }
    }
    
    void StartGame()
    {        
        Time.timeScale = 1;        
        Cursor.lockState = CursorLockMode.Locked;
        UpdateGameState(GameState.GAME);        
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
        UpdateGameState(GameState.GAME);
        Cursor.lockState = CursorLockMode.Locked;
        m_world.StartGame();
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        UpdateGameState(GameState.INFO);
        if (!(m_iDevice is Keyboard || m_iDevice is Mouse) && m_iDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_infoBack.gameObject);
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void InputDeviceChanged(InputEventPtr eventPtr, InputDevice device)
    {
        if (m_iDevice == device)
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

        m_iDevice = device;

    }

    void UpdateUI(GameState state)
    {
        m_main.SetActive(state == GameState.MENU);       
        m_hud.SetActive(state == GameState.GAME);
        m_pause.SetActive(state == GameState.PAUSE);
        m_end.SetActive(state == GameState.DEATH);
        m_info.SetActive(state == GameState.INFO);
    }

    public void UpdateModeUI()
    {
        m_modePlus.SetActive(m_player.m_canPlace);
        m_modeMinus.SetActive(!m_player.m_canPlace);
    }

    public void UpdateHealth()
    {
        m_health.value = m_player.m_currentHP;
        if (m_health.value <= 0)
        {         
            UpdateGameState(GameState.DEATH);
        }
    }
}