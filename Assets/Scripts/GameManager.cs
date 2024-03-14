using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;

using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.Profiling;

public enum GameState
{
    MENU,
    GAME,
    PAUSE,
    DEATH,
    INFO,
    END
}

public enum ControllerType
{
    KEYBOARD,
    XBOX,
    PS,
    SWITCH,
    GENERIC
}

public enum BonusType
{
    HEALTH,
    HEALRATE,
    DAMAGE,
    BLOCKRADIUS,
    SPEED,
    ATTACKRATE
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

    public VolumeProfile m_volume;
    ChromaticAberration m_chromatic;

    int m_round = 1;

    Vector3 m_menuPos;
    Quaternion m_menuRot;

    public WorldGen m_world;
    public TerrainModifier m_terrainModifier;
    public GameObject m_blockholder;
    public List<Blockholder> m_blockholders;

    public GameObject m_main;
    public GameObject m_hud;
    public GameObject m_pause;
    public GameObject m_death;
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

    public Button m_resume;
    public Slider m_camSensitivity;
    public Button m_quitMenu;

    public Button m_respawnButton;
    public Button m_deathQuit;

    public Button m_endQuit;
    public Button m_addHealth;
    public Button m_addHealRate;
    public Button m_addDamage;
    public Button m_addBlockRadius;
    public Button m_addSpeed;
    public Button m_addAttackRate;

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
        m_menuPos = new Vector3(64, 50, 110);
        m_menuRot = new Quaternion(0, 0, 0, 0);
        m_player.m_defPos = m_menuPos;
        m_player.m_defRot = m_menuRot;
        SetPlayerDefault();       

        //Main Menu Setup
        m_newGame.onClick.AddListener(delegate () { InitGame(); });
        m_infoB.onClick.AddListener(delegate () { Info(); });
        m_quit.onClick.AddListener(delegate () { QuitGame(); });

        //HUD Setup
        m_health.maxValue = m_player.m_health;
        m_attackTime.maxValue = m_player.m_attackTime;
        m_health.value = m_player.m_health;
        m_attackTime.value = m_player.m_attackTime;

        //Pause Menu Setup
        m_resume.onClick.AddListener(delegate () { ResumeGame(); });
        m_camSensitivity.onValueChanged.AddListener(m_player.ChangeSensitivity);
        m_camSensitivity.SetValueWithoutNotify(m_player.m_camSensitivity);
        m_quitMenu.onClick.AddListener(delegate () { QuitGame(); });

        //Info Setup                
        m_infoBack.onClick.AddListener(delegate () { MainMenu(); });

        //Death Setup
        m_respawnButton.onClick.AddListener(delegate () { Respawn(); });
        m_deathQuit.onClick.AddListener(delegate () { QuitGame(); });

        //End Setup
        m_endQuit.onClick.AddListener(delegate () { QuitGame(); });
        m_addHealth.onClick.AddListener(delegate () { NextRound(BonusType.HEALTH); });
        m_addHealRate.onClick.AddListener(delegate () { NextRound(BonusType.HEALRATE); });
        m_addDamage.onClick.AddListener(delegate () { NextRound(BonusType.DAMAGE); });
        m_addBlockRadius.onClick.AddListener(delegate () { NextRound(BonusType.BLOCKRADIUS); });
        m_addSpeed.onClick.AddListener(delegate () { NextRound(BonusType.SPEED); });
        m_addAttackRate.onClick.AddListener(delegate () { NextRound(BonusType.ATTACKRATE); });
    }

    private void SetPlayerDefault()
    {
        m_player.gameObject.transform.SetPositionAndRotation(m_menuPos, m_menuRot);
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
                if (m_lastState == GameState.MENU || m_lastState == GameState.END || m_lastState == GameState.DEATH)
                {
                    StartGame();
                }
                else if (m_lastState == GameState.PAUSE)
                {
                    ResumeGame();
                }
                break;
            case GameState.PAUSE:
                PauseGame();
                break;
            case GameState.DEATH:
                Death();
                break;
            case GameState.END:
                End();
                break;
        }
    }

    void MainMenu()
    {
        Time.timeScale = 0;
        m_player.m_stopUpdate = true;

        m_eventSystem.SetSelectedGameObject(m_newGame.gameObject);

        if (m_iDevice is Gamepad)
        {
            m_eventSystem.SetSelectedGameObject(m_newGame.gameObject);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log("Cunt");
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void Info()
    {
        UpdateGameState(GameState.INFO);
        if (m_iDevice is Gamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            m_eventSystem.SetSelectedGameObject(m_infoBack.gameObject);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void InitGame()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UpdateGameState(GameState.GAME);
    }

    void StartGame()
    {      
        if (m_round > 1)
        {
            m_world.ClearData();
            m_world.m_perlinY = new Vector3Int(UnityEngine.Random.Range(1, 401), UnityEngine.Random.Range(1, 401), UnityEngine.Random.Range(1, 401));
            m_world.m_perlinScale = new Vector3(UnityEngine.Random.Range(5.0f, 60.0f), UnityEngine.Random.Range(5.0f, 60.0f), UnityEngine.Random.Range(5.0f, 60.0f));
            m_world.m_perlinHeight = new Vector3(UnityEngine.Random.Range(2.5f, 4.5f), UnityEngine.Random.Range(2.5f, 4.5f), UnityEngine.Random.Range(2.5f, 4.5f)); ;
            m_world.m_perlinPower = new Vector3(UnityEngine.Random.Range(0.0f, 2.0f), UnityEngine.Random.Range(0.0f, 2.0f), UnityEngine.Random.Range(0.0f, 2.0f)); ;
            m_world.m_perlinAdd = new Vector2Int(UnityEngine.Random.Range(0, 11), UnityEngine.Random.Range(0, 11)); ;
        }
        m_world.StartGame();
        for (int i = 0; i < m_round; i++)
        {
            int x = UnityEngine.Random.Range(0, m_world.m_chunks.GetLength(0));
            int z = UnityEngine.Random.Range(0, m_world.m_chunks.GetLength(2));
            int y = 0;
            for (int j = 0; j < m_world.m_worldY; j++)
            {
                if (m_world.m_data[x, j, z] != 0)
                {
                    y = j;
                }
            }
            GameObject blockholder = Instantiate(m_blockholder, new Vector3((x * m_world.m_chunkSize) + (0.5f * m_world.m_chunkSize), y + 8, (z * m_world.m_chunkSize) + (0.5f * m_world.m_chunkSize)), new Quaternion(0, 0, 0, 0));
            m_blockholders.Add(blockholder.GetComponent<Blockholder>());
        }

        int xp = UnityEngine.Random.Range(0, m_world.m_chunks.GetLength(0));
        int zp = UnityEngine.Random.Range(0, m_world.m_chunks.GetLength(2));
        int yp = 0;
        for (int i = 0; i < m_world.m_worldY; i++)
        {
            if (m_world.m_data[xp, i, zp] != 0)
            {
                yp = i;
            }
        }

        m_player.gameObject.transform.SetPositionAndRotation(new Vector3((xp * m_world.m_chunkSize) + (0.5f * m_world.m_chunkSize), yp + 10, (zp * m_world.m_chunkSize) + (0.5f * m_world.m_chunkSize)), Quaternion.identity);

        m_player.m_stopUpdate = false;
    }

    void ResumeGame()
    {
        m_player.m_stopUpdate = false;

        Time.timeScale = 1;
        UpdateGameState(GameState.GAME);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        m_world.StartGame();
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        m_player.m_stopUpdate = true;

        if (m_iDevice is Gamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            m_eventSystem.SetSelectedGameObject(m_infoBack.gameObject);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void Death()
    {
        Time.timeScale = 0;
        m_player.m_stopUpdate = true;

        if (m_iDevice is Gamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            m_eventSystem.SetSelectedGameObject(m_respawnButton.gameObject);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        SetPlayerDefault();
        for (int i = 0; i < m_blockholders.Count; i++)
        {
            Destroy(m_blockholders[i].gameObject);
        }
        m_blockholders.Clear();
    }

    public void CheckRoundEnd(Blockholder dead)
    {
        m_blockholders.Remove(dead);
        if (m_blockholders.Count == 0)
        {
            UpdateGameState(GameState.END);
        }
    }

    void NextRound(BonusType bonus)
    {
        m_round++;
        switch (bonus)
        {
            case BonusType.HEALTH:
                m_player.m_health *= 1.1f;
                break;
            case BonusType.HEALRATE:
                m_player.m_healRate += 0.1f;
                break;
            case BonusType.DAMAGE:
                m_player.m_projectileDamage *= 1.1f;
                break;
            case BonusType.BLOCKRADIUS:
                m_player.m_projectileRadius++;
                break;
            case BonusType.SPEED:
                m_player.m_walkSpeed *= 1.1f;
                break;
            case BonusType.ATTACKRATE:
                m_player.m_attackRate++;
                m_player.m_atackCooldown *= 1.1f;
                break;
        }
        SetPlayerDefault();
        m_player.m_currentAttackTime = m_player.m_attackTime;
        m_player.m_currentHP = m_player.m_health;
        UpdateAP();
        UpdateHealth();
        InitGame();
    }

    void Respawn()
    {
        m_player.m_stopUpdate = false;
        m_player.m_currentHP = m_player.m_health;
        m_player.m_currentAttackTime = m_player.m_attackTime;
        UpdateAP();
        UpdateHealth();
        InitGame();
    }

    void End()
    {
        Time.timeScale = 0;
        m_player.m_stopUpdate = true;
        SetPlayerDefault();

        if (m_iDevice is Gamepad)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            m_eventSystem.SetSelectedGameObject(m_respawnButton.gameObject);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
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
        if (m_gameState != GameState.GAME)
        {
            if (m_iDevice is Gamepad)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                m_eventSystem.SetSelectedGameObject(m_respawnButton.gameObject);
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }

    void UpdateUI(GameState state)
    {
        m_main.SetActive(state == GameState.MENU);
        m_hud.SetActive(state == GameState.GAME);
        m_player.m_leftArm.SetActive(state == GameState.GAME);
        m_player.m_rightArm.SetActive(state == GameState.GAME);
        m_pause.SetActive(state == GameState.PAUSE);
        m_end.SetActive(state == GameState.END);
        m_death.SetActive(state == GameState.DEATH);
        m_info.SetActive(state == GameState.INFO);
    }

    public async UniTask HitReticle()
    {
        float startTime = Time.time;
        m_reticleHit.enabled = true;
        while (Time.time <= startTime + m_reticleHitTime)
        {
            await UniTask.Yield();
        }
        if (m_reticleHit.enabled == true)
        {
            m_reticleHit.enabled = false;
        }
    }

    public void UpdateModeUI()
    {
        m_modePlus.SetActive(m_player.m_canPlace);
        m_modeMinus.SetActive(!m_player.m_canPlace);
    }

    public void UpdateHealth()
    {
        m_health.value = m_player.m_currentHP;
        if(m_volume.TryGet<ChromaticAberration>(out m_chromatic))
        {
            m_chromatic.intensity.value = 1 - (m_player.m_currentHP / m_player.m_health);
        }
        
        if (m_health.value <= 0)
        {
            UpdateGameState(GameState.DEATH);
        }
    }
    public void UpdateAP()
    {
        m_attackTime.value = m_player.m_currentAttackTime;
    }
}