using Cysharp.Threading.Tasks;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType
    {
        RAD,
        POI,
        PAR,
        DMG
    }
    public float m_lifeTime = 0;
    float m_startTime;
    public float m_damage = 0;
    public float m_poisonTick = 0;
    public float m_poisonTime = 0;
    public float m_paralysisTime = 0;
    public bool m_exploded = false;
    public float m_explosionRadius = 0;
    public byte m_block = 0;

    public ProjectileType m_type;

    public GameObject m_parent;

    void Start()
    {
        m_startTime = Time.realtimeSinceStartup;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == m_parent)
        {
            return;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            player.TakeDamage(m_damage);
            if (m_type == ProjectileType.RAD)
            {
                if (!m_exploded)
                {
                    m_exploded = true;
                    FindObjectOfType<TerrainModifier>().Explosion(gameObject.transform.position, m_explosionRadius, m_block);
                }
            }
            if (m_type == ProjectileType.POI)
            {
                player.AddDamageOverTime(m_poisonTime, m_poisonTick).Forget();
            }
            if (m_type == ProjectileType.PAR)
            {
                player.Paralyze(m_paralysisTime).Forget();
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            if (m_type == ProjectileType.RAD)
            {
                if (!m_exploded)
                {
                    m_exploded = true;
                    FindObjectOfType<TerrainModifier>().Explosion(gameObject.transform.position, m_explosionRadius, m_block);
                }
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.transform.parent.GetComponent<Blockholder>().TakeDamage(m_damage);
            GameManager.Instance.HitReticle().Forget();
        }
        if (collision.gameObject.layer != LayerMask.NameToLayer("Projectile"))
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (Time.realtimeSinceStartup >= m_startTime + m_lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
