using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using static WorldGen;

public class Blockholder : MonoBehaviour
{
    public byte[,,] m_data;

    public MeshFilter m_meshFilter;
    public Mesh m_mesh;
    public MeshCollider m_collider;

    List<Vector3> m_verts = new List<Vector3>();
    List<int> m_tris = new List<int>();
    List<Vector2> m_uvs = new List<Vector2>();

    float m_texUnit = 0.25f;
    Vector2 m_fleshTexture = new Vector2(0, 0);
    Vector2 m_pupilTexture = new Vector2(0, 1);
    Vector2 m_eyeTexture = new Vector2(0, 2);
    Vector2 m_eyebackTexture = new Vector2(0, 3);

    int m_faceCount;

    public bool m_update;

    public Player m_player;

    public Transform m_eyeOne;
    public Transform m_eyeTwo;
    public Transform m_eyeThree;
    public Transform m_eyeFour;

    public float m_shotDelay = 5f;
    public float m_projectileSpeed = 5f;
    public float m_projectileLifespan = 10f;
    public float m_basicDamage = 5f;
    public float m_explosionDamage = 10f;
    public float m_explosionRadius = 4f;
    public float m_posisonDamage = 4f;
    public float m_posisonTime = 3f;
    public float m_posisonTick = 1f;
    public float m_paralysisDamage = 2f;
    public float m_paralysisTime = 3f;

    float m_shotTime;
    bool m_canShoot = true;
    public float m_searchPlayerTime = 5f;
    float m_lostPlayerTime;
    public Projectile m_projectile;
    public GameObject m_projectileGO;

    public float m_hp = 50f;
    float m_currentHP;

    // Start is called before the first frame update
    void Start()
    {
        m_mesh = m_meshFilter.mesh;
        m_player = FindObjectOfType<Player>();

        m_data = new byte[5, 5, 5];
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int z = 0; z < 5; z++)
                {
                    if (y == 0)
                    {
                        if (x == 0 || x == 4 || z == 0 || z == 4)
                        {
                            m_data[x, y, z] = 0;
                        }
                        else
                        {
                            m_data[x, y, z] = 1;
                        }
                    }
                    else if (y == 2)
                    {
                        if (x == 2 && z == 4)
                        {
                            m_data[x, y, z] = 2;
                        }
                        else if ((x == 0 && z == 0) || (x == 4 && z == 0) || (z == 4 && x == 0) || (x == 4 && z == 4))
                        {
                            m_data[x, y, z] = 0;
                        }
                        else
                        {
                            m_data[x, y, z] = 1;
                        }
                    }
                    else if (y == 4)
                    {
                        if (z == 4 && (x == 0 || x == 2 || x == 4))
                        {
                            m_data[x, y, z] = 2;
                        }
                        else if (x == 0 || x == 4 || z == 0 || z == 4)
                        {
                            m_data[x, y, z] = 0;
                        }
                        else
                        {
                            m_data[x, y, z] = 1;
                        }
                    }
                    else
                    {
                        if ((x == 0 && z == 0) || (x == 4 && z == 0) || (z == 4 && x == 0) || (x == 4 && z == 4))
                        {
                            m_data[x, y, z] = 0;
                        }
                        else
                        {
                            m_data[x, y, z] = 1;
                        }
                    }
                }
            }
        }

        m_shotTime = Time.time;
        m_currentHP = m_hp;

        MeshGen();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(m_eyeOne.position+ m_eyeOne.forward,  (m_player.transform.position + (-m_player.transform.up * 0.1f)) - (m_eyeOne.position+m_eyeOne.forward));
        Debug.DrawRay(ray.origin, ray.direction, UnityEngine.Color.magenta);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == m_player.m_collider)
            {
                m_lostPlayerTime = 0.0f;
                m_canShoot = true;
            }
            else
            {
                if(m_lostPlayerTime <= m_searchPlayerTime)
                {
                    m_lostPlayerTime += Time.deltaTime;
                }
                else
                {
                    m_canShoot=false;
                }
            }
        }
        transform.LookAt(m_player.transform.position);
        if (m_shotTime + m_shotDelay <= Time.time && m_canShoot)
        {
            ShootProjectile();
            m_shotTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        m_currentHP -= damage;
        if (m_currentHP <= 0)
        {
            GameManager.Instance.CheckRoundEnd(this);
            Destroy(gameObject);
        }
    }

    void ShootProjectile()
    {
        int eye = Random.Range(1, 5);
        var projectileGO = Instantiate(m_projectileGO);
        var mainPS = projectileGO.GetComponentInChildren<ParticleSystem>().main;
        var trail = projectileGO.GetComponentInChildren<TrailRenderer>();
        var projectile = projectileGO.GetComponent<Projectile>();
        projectile.m_lifeTime = m_projectileLifespan;
        projectile.m_parent = gameObject;

        switch (eye)
        {
            case 1:
                projectileGO.transform.SetPositionAndRotation(m_eyeOne.position + m_eyeOne.forward, Quaternion.identity);
                mainPS.startColor = UnityEngine.Color.red;
                trail.startColor = UnityEngine.Color.red;
                projectile.m_type = Projectile.ProjectileType.RAD;
                projectile.m_damage = m_explosionDamage;
                projectile.m_explosionRadius = m_explosionRadius;
                break;
            case 2:
                projectileGO.transform.SetPositionAndRotation(m_eyeTwo.position + m_eyeTwo.forward, Quaternion.identity);
                mainPS.startColor = UnityEngine.Color.green;
                trail.startColor = UnityEngine.Color.green;
                projectile.m_type = Projectile.ProjectileType.POI;
                projectile.m_damage = m_posisonDamage;
                projectile.m_poisonTick = m_posisonTick;
                projectile.m_poisonTime = m_posisonTime;
                break;
            case 3:
                projectileGO.transform.SetPositionAndRotation(m_eyeThree.position + m_eyeThree.forward, Quaternion.identity);
                mainPS.startColor = UnityEngine.Color.blue;
                trail.startColor = UnityEngine.Color.blue;
                projectile.m_type = Projectile.ProjectileType.DMG;
                projectile.m_damage = m_basicDamage;
                break;
            case 4:
                projectileGO.transform.SetPositionAndRotation(m_eyeFour.position + m_eyeFour.forward, Quaternion.identity);
                mainPS.startColor = UnityEngine.Color.magenta;
                trail.startColor = UnityEngine.Color.magenta;
                projectile.m_type = Projectile.ProjectileType.PAR;
                projectile.m_damage = m_paralysisDamage;
                projectile.m_paralysisTime = m_paralysisTime;
                break;
        }
        projectileGO.GetComponent<Rigidbody>().velocity = -(projectileGO.transform.position - (m_player.gameObject.transform.position - (m_player.gameObject.transform.up / 2))).normalized * m_projectileSpeed;
    }

    #region MeshGeneration
    public byte BlockType(int x, int y, int z)
    {
        if (x >= 5 || x < 0 || y >= 5 || y < 0 || z >= 5 || z < 0)
        {
            return (byte)1;
        }
        return m_data[x, y, z];
    }

    public bool MaxBlock(WorldGen.Face face, int coord)
    {
        switch (face)
        {
            case Face.Top:
                {
                    return coord >= 5;
                }
            case Face.Bottom:
                {
                    return coord < 0;
                }
            case Face.North:
                {
                    return coord >= 5;
                }
            case Face.South:
                {
                    return coord < 0;
                }
            case Face.East:
                {
                    return coord >= 5;
                }
            case Face.West:
                {
                    return coord < 0;
                }
            default:
                return false;
        }
    }
    public void MeshGen()
    {
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int z = 0; z < 5; z++)
                {
                    if (BlockType(x, y, z) != 0)
                    {
                        if (BlockType(x, y + 1, z) == 0 || MaxBlock(WorldGen.Face.Top, y + 1))
                        {
                            CubeTop(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x, y - 1, z) == 0 || MaxBlock(WorldGen.Face.Bottom, y - 1))
                        {
                            CubeBottom(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x + 1, y, z) == 0 || MaxBlock(WorldGen.Face.East, x + 1))
                        {
                            CubeEast(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x - 1, y, z) == 0 || MaxBlock(WorldGen.Face.West, x - 1))
                        {
                            CubeWest(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x, y, z + 1) == 0 || MaxBlock(WorldGen.Face.North, z + 1))
                        {
                            CubeNorth(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x, y, z - 1) == 0 || MaxBlock(WorldGen.Face.South, z - 1))
                        {
                            CubeSouth(x, y, z, BlockType(x, y, z));
                        }
                    }
                }
            }
        }
        UpdateMesh();
    }

    void CubeTop(int x, int y, int z, byte block)
    {
        m_verts.Add(new Vector3(x, y + 1, z + 1));
        m_verts.Add(new Vector3(x + 1, y + 1, z + 1));
        m_verts.Add(new Vector3(x + 1, y + 1, z));
        m_verts.Add(new Vector3(x, y + 1, z));

        Vector2 texturePos = new Vector2(0, 0);

        if (BlockType(x, y, z) == 1)
        {
            texturePos = m_fleshTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_eyeTexture;
        }

        GenCube(texturePos);
    }

    void CubeNorth(int x, int y, int z, byte block)
    {
        m_verts.Add(new Vector3(x + 1, y, z + 1));
        m_verts.Add(new Vector3(x + 1, y + 1, z + 1));
        m_verts.Add(new Vector3(x, y + 1, z + 1));
        m_verts.Add(new Vector3(x, y, z + 1));

        Vector2 texturePos = new Vector2(0, 0);

        if (BlockType(x, y, z) == 1)
        {
            texturePos = m_fleshTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_pupilTexture;
        }

        GenCube(texturePos);
    }

    void CubeEast(int x, int y, int z, byte block)
    {
        m_verts.Add(new Vector3(x + 1, y, z));
        m_verts.Add(new Vector3(x + 1, y + 1, z));
        m_verts.Add(new Vector3(x + 1, y + 1, z + 1));
        m_verts.Add(new Vector3(x + 1, y, z + 1));

        Vector2 texturePos = new Vector2(0, 0);

        if (BlockType(x, y, z) == 1)
        {
            texturePos = m_fleshTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_eyeTexture;
        }

        GenCube(texturePos);
    }

    void CubeSouth(int x, int y, int z, byte block)
    {
        m_verts.Add(new Vector3(x, y, z));
        m_verts.Add(new Vector3(x, y + 1, z));
        m_verts.Add(new Vector3(x + 1, y + 1, z));
        m_verts.Add(new Vector3(x + 1, y, z));

        Vector2 texturePos = new Vector2(0, 0);

        if (BlockType(x, y, z) == 1)
        {
            texturePos = m_fleshTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_eyebackTexture;
        }

        GenCube(texturePos);
    }

    void CubeWest(int x, int y, int z, byte block)
    {
        m_verts.Add(new Vector3(x, y, z + 1));
        m_verts.Add(new Vector3(x, y + 1, z + 1));
        m_verts.Add(new Vector3(x, y + 1, z));
        m_verts.Add(new Vector3(x, y, z));

        Vector2 texturePos = new Vector2(0, 0);

        if (BlockType(x, y, z) == 1)
        {
            texturePos = m_fleshTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_eyeTexture;
        }

        GenCube(texturePos);
    }

    void CubeBottom(int x, int y, int z, byte block)
    {
        m_verts.Add(new Vector3(x, y, z));
        m_verts.Add(new Vector3(x + 1, y, z));
        m_verts.Add(new Vector3(x + 1, y, z + 1));
        m_verts.Add(new Vector3(x, y, z + 1));

        Vector2 texturePos = new Vector2(0, 0);

        if (BlockType(x, y, z) == 1)
        {
            texturePos = m_fleshTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_eyeTexture;
        }

        GenCube(texturePos);
    }

    void GenCube(Vector2 texturePos)
    {
        int faceCount = m_faceCount * 4;

        m_tris.Add(faceCount);
        m_tris.Add(faceCount + 1);
        m_tris.Add(faceCount + 2);
        m_tris.Add(faceCount);
        m_tris.Add(faceCount + 2);
        m_tris.Add(faceCount + 3);

        m_uvs.Add(new Vector2(m_texUnit * texturePos.x + m_texUnit, m_texUnit * texturePos.y));
        m_uvs.Add(new Vector2(m_texUnit * texturePos.x + m_texUnit, m_texUnit * texturePos.y + m_texUnit));
        m_uvs.Add(new Vector2(m_texUnit * texturePos.x, m_texUnit * texturePos.y + m_texUnit));
        m_uvs.Add(new Vector2(m_texUnit * texturePos.x, m_texUnit * texturePos.y));

        m_faceCount++;
    }

    void UpdateMesh()
    {
        m_mesh.Clear();
        m_mesh.vertices = m_verts.ToArray();
        m_mesh.uv = m_uvs.ToArray();
        m_mesh.triangles = m_tris.ToArray();
        m_mesh.Optimize();
        m_mesh.RecalculateNormals();

        m_collider.sharedMesh = null;
        m_collider.sharedMesh = m_mesh;

        m_verts.Clear();
        m_uvs.Clear();
        m_tris.Clear();

        m_faceCount = 0;
    }
    #endregion
}
