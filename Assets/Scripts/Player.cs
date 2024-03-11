using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class Player : MonoBehaviour
{
    public GameManager m_gm;
    public Controls m_input;

    public CamControls m_camera;
    public float m_camSensitivity = 10;
    Vector3 m_moveDirection = Vector3.zero;
    float m_gravity = 9.81f;
    public float m_gravityMultiplier = 1;
    public float m_walkSpeed = 5;
    public float m_runSpeed = 10;
    bool m_canMove = true;
    public float m_jumpSpeed = 5;
    public CharacterController m_characterController;
    float m_smoothTime = 0.3f;
    Vector2 m_moveDamp;

    float m_health;
    public float m_currentHP;
    public float m_healRate;

    public Collider m_melee;
    public float m_melessDamae;
    public bool m_canAttack = true;
    public float m_attackTime;

    public bool m_isBlocking = false;

    public float m_interactionDistance;
    
    void Awake()
    {
        m_input = new Controls();
        m_gm = GameManager.Instance;
        Transform t = transform;
        m_gm.m_player = this;
        //LoadSettings();
    }

    private void Start()
    {
        m_camera = FindObjectOfType<CamControls>();
        m_camera.m_parent = transform;
        m_characterController = GetComponent<CharacterController>();

        m_input.Enable();
        Cursor.lockState = CursorLockMode.Locked;


        //m_input.PlayerControls.MeleeAttack.performed += MeleeAttack;
        //m_input.PlayerControls.Interact.performed += Interact;
        //m_input.PlayerControls.Block.performed += Block;
        //m_input.PlayerControls.Block.canceled += StopBlock;
        //m_input.UI.Cancel.started += m_gm.m_mm.Cancel;
        //m_input.PlayerControls.Pause.started += PauseGame;

        m_currentHP = m_health;
        //m_gm.OnGameStateChanged += OnGameStateChanged;
        //m_gm.m_mm.UpdateHealth();        
    }

    void FixedUpdate()
    {       
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 move = m_input.PlayerControls.Move.ReadValue<Vector2>();
        if (move.magnitude < 0.1f)
        {
            move = Vector2.zero;
        }
        move = move.normalized;
        float x = m_canMove ? m_walkSpeed * move.y : 0;
        float y = m_canMove ? m_walkSpeed * move.x : 0;
        float yDirection = m_moveDirection.y;
        m_moveDirection = (forward * x) + (right * y);
        bool wasJumping = m_characterController.isGrounded;

        if(m_input.PlayerControls.Jump.IsPressed() && m_characterController.isGrounded) 
        {
            m_moveDirection.y = m_jumpSpeed;
        }
        else
        {
            m_moveDirection.y = yDirection;
        }

        if(!m_characterController.isGrounded)
        {
            m_moveDirection.y -= (m_gravity * m_gravityMultiplier) * Time.deltaTime;
        }       
        m_camera.MoveCamera(m_input.PlayerControls.CameraView.ReadValue<Vector2>(), m_gm.m_device == ControllerType.KEYBOARD ? m_camSensitivity / 10 : m_camSensitivity);
        m_characterController.Move(m_moveDirection * Time.deltaTime);
    }

    void Update()
    {
      //  UpdateInteracts();
    }

    public void TakeDamage(float damage)
    {
        m_currentHP -= damage;
        //Updatee UI
    }

    void MeleeAttack(InputAction.CallbackContext obj)
    {
        if(m_isBlocking)
        {
            return;
        }
        if(m_canAttack)
        {
            m_canAttack = false;
            m_melee.enabled = true;
            ResetAttack().Forget();            
        }
    }

    async UniTask ResetAttack()
    {
        float time = Time.time;
        while(Time.time < time + m_attackTime)
        {
            await UniTask.Yield();
        }
        m_canAttack = true;
    }

    void Block(InputAction.CallbackContext obj)
    {
        m_isBlocking = true;
    }

    void StopBlock(InputAction.CallbackContext obj)
    {
        m_isBlocking = false;
    }

    void UpdateInteracts()
    {
        Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if(Physics.Raycast(camRay, out hit, m_interactionDistance))
        {
            
        }
        else
        {

        }
    }
}
