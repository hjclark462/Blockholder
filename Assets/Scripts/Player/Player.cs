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
    bool m_canMove = true;
    public float m_jumpSpeed = 5;
    public CharacterController m_characterController;

    Vector2 m_destroyArms = new Vector2(0.25f, 0.5f);
    Vector2 m_placeArms = new Vector2(0.25f, 0.0f);
    public bool m_canPlace = true;

    public float m_health;
    public float m_currentHP;
    public float m_healRate;

    public Collider m_collider;

    public bool m_canAttack = true;
    public float m_attackTime;
    public float m_attackRange;
    public GameObject m_projectileGO;
    public float m_projectileLifespan;
    public float m_projectileDamage;
    public float m_projectileSpeed;
    public float m_projectileRadius;

    public bool m_isBlocking = false;

    public GameObject m_leftArm;
    public GameObject m_rightArm;
    public float m_interactionDistance;

    void Awake()
    {
        m_input = new Controls();
        m_gm = GameManager.Instance;
        Transform t = transform;
        m_gm.m_player = this;
        m_currentHP = m_health;
        m_collider = GetComponent<CapsuleCollider>();
        m_leftArm.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(0.25f, 0.25f);
        m_leftArm.GetComponent<MeshRenderer>().material.mainTextureOffset = m_placeArms;
        m_rightArm.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(0.25f, 0.25f);
        m_rightArm.GetComponent<MeshRenderer>().material.mainTextureOffset = m_placeArms;
    }

    private void Start()
    {
        m_camera = FindObjectOfType<CamControls>();
        m_camera.m_parent = transform;
        m_characterController = GetComponent<CharacterController>();

        m_input.Enable();
        Cursor.lockState = CursorLockMode.Locked;

        m_input.PlayerControls.LeftHand.started += LeftHand;
        m_input.PlayerControls.RightHand.started += RightHand;
        m_input.PlayerControls.SwapPlace.started += SwapArmType;        

        m_currentHP = m_health;
        m_gm.UpdateHealth();        
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
        float x = m_canMove ? m_walkSpeed * move.y : 0;
        float y = m_canMove ? m_walkSpeed * move.x : 0;
        float yDirection = m_moveDirection.y;
        m_moveDirection = (forward * x) + (right * y);
        bool wasJumping = m_characterController.isGrounded;

        if (m_input.PlayerControls.Jump.IsPressed() && m_characterController.isGrounded)
        {
            m_moveDirection.y = m_jumpSpeed;
        }
        else
        {
            m_moveDirection.y = yDirection;
        }

        if (!m_characterController.isGrounded)
        {
            m_moveDirection.y -= (m_gravity * m_gravityMultiplier) * Time.deltaTime;
        }
        m_camera.MoveCamera(m_input.PlayerControls.CameraView.ReadValue<Vector2>(), m_gm.m_device == ControllerType.KEYBOARD ? m_camSensitivity / 10 : m_camSensitivity);
        m_characterController.Move(m_moveDirection * Time.deltaTime);
    }

    void Update()
    {

    }

    public void TakeDamage(float damage)
    {
        m_currentHP -= damage;        
        m_gm.UpdateHealth();
    }

    public async UniTask AddDamageOverTime(float time, float damage)
    {
        float second = 1.0f;
        while (time > 0)
        {
            second -= Time.deltaTime;
            time -= Time.deltaTime;
            if (second <= 0.0f)
            {
                second = 1.0f;
                TakeDamage(damage);
            }
            await UniTask.NextFrame();
        }
    }
    public async UniTask Paralyze(float time)
    {
        m_canMove = false;
        while (time > 0)
        {
            time -= Time.deltaTime;

            await UniTask.NextFrame();
        }
        m_canMove = true;
    }

    void LeftHand(InputAction.CallbackContext obj)
    {
        var projectileGO = Instantiate(m_projectileGO);
        var mainPS = projectileGO.GetComponentInChildren<ParticleSystem>().main;
        var trail = projectileGO.GetComponentInChildren<TrailRenderer>();
        var projectile = projectileGO.GetComponent<Projectile>();
        projectile.m_lifeTime = m_projectileLifespan;
        projectile.m_parent = gameObject;
        projectileGO.transform.SetPositionAndRotation(m_leftArm.transform.position, Quaternion.identity);

        if (m_canPlace)
        {
            mainPS.startColor = UnityEngine.Color.cyan;
            trail.startColor = UnityEngine.Color.cyan;
            projectile.m_type = Projectile.ProjectileType.RAD;
            projectile.m_explosionRadius = m_projectileRadius;
            projectile.m_damage = 0;
            projectile.m_block = (byte)Random.Range(1, 3);
        }
        else
        {
            mainPS.startColor = UnityEngine.Color.yellow;
            trail.startColor = UnityEngine.Color.yellow;
            projectile.m_type = Projectile.ProjectileType.DMG;
            projectile.m_damage = m_projectileDamage;
            projectile.m_block = 0;
        }

        Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 destination;
        if (Physics.Raycast(camRay, out hit, m_attackRange * 10, ~(1 << LayerMask.NameToLayer("Projectile"))))
        {
            destination = hit.point;
        }
        else
        {
            destination = camRay.GetPoint(m_attackRange * 10);
        }
        projectileGO.GetComponent<Rigidbody>().velocity = (destination - projectileGO.transform.position).normalized * m_projectileSpeed;
    }

    void RightHand(InputAction.CallbackContext obj)
    {
        if (m_canPlace)
        {
            m_gm.m_terrainModifier.AddBlock(m_interactionDistance, (byte)Random.Range(1, 3));
        }
        else
        {
            m_gm.m_terrainModifier.ReplaceBlock(m_interactionDistance, 0);
        }
    }

    void SwapArmType(InputAction.CallbackContext obj)
    {
        m_canPlace = !m_canPlace;

        if (m_canPlace)
        {
            m_leftArm.GetComponent<MeshRenderer>().material.mainTextureOffset = m_placeArms;
            m_rightArm.GetComponent<MeshRenderer>().material.mainTextureOffset = m_placeArms;
        }
        else
        {
            m_leftArm.GetComponent<MeshRenderer>().material.mainTextureOffset = m_destroyArms;
            m_rightArm.GetComponent<MeshRenderer>().material.mainTextureOffset = m_destroyArms;
        }
        m_gm.UpdateModeUI();
    }

    public void ChangeSensitivity(float change)
    {
        m_camSensitivity = change;
    }
}
