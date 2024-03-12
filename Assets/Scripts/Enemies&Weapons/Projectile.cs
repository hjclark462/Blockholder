using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float m_lifeTime = 5;
    float m_startTime;
    float m_damage;

    void Start()
    {
        m_startTime = Time.realtimeSinceStartup;
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        if (collision.gameObject.layer == LayerMask.GetMask("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(m_damage);
        }
        else if (collision.gameObject.layer == LayerMask.GetMask("Terrain"))
        {

        }
        else if (collision.gameObject.layer == LayerMask.GetMask("Enemy"))
        {
         //   collision.gameObject.GetComponent<Blockholder>().TakeDamage(m_damage);
        }
    }

    void Update()
    {
        if(Time.realtimeSinceStartup >= m_startTime + m_lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
