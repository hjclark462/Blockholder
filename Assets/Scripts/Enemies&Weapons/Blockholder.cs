using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
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
    float m_shotTime;
    public Projectile m_projectile;
    public GameObject m_projectileGO;

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

        MeshGen();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(m_player.transform.position);

        if (m_shotTime + m_shotDelay <= Time.time)
        {
            ShootProjectile();
            m_shotTime = Time.time;
        }
    }

    void ShootProjectile()
    {
        int eye = Random.Range(1, 4);
        GameObject projectile = new GameObject();

        switch (eye)
        {
            case 1:
                projectile = Instantiate(m_projectileGO, m_eyeOne.position + m_eyeOne.forward, Quaternion.identity);
                projectile.GetComponent<SphereCollider>().radius = 1;
                var main = projectile.GetComponentInChildren<ParticleSystem>().main;
                main.startSize = 1;
                main.startColor = UnityEngine.Color.red;
                break;
            case 2:
                projectile = Instantiate(m_projectileGO, m_eyeTwo.position + m_eyeTwo.forward, Quaternion.identity);
                projectile.GetComponent<SphereCollider>().radius = 1;
                main = projectile.GetComponentInChildren<ParticleSystem>().main;
                main.startSize = 1;
                main.startColor = UnityEngine.Color.green;
                break;
            case 3:
                projectile = Instantiate(m_projectileGO, m_eyeThree.position + m_eyeThree.forward, Quaternion.identity);
                projectile.GetComponent<SphereCollider>().radius = 1;
                main = projectile.GetComponentInChildren<ParticleSystem>().main;
                main.startSize = 1;
                main.startColor = UnityEngine.Color.blue;
                break;
            case 4:
                projectile = Instantiate(m_projectileGO, m_eyeFour.position + m_eyeFour.forward, Quaternion.identity);
                projectile.GetComponent<SphereCollider>().radius = 1;
                main = projectile.GetComponentInChildren<ParticleSystem>().main;
                main.startSize = 1;
                main.startColor = UnityEngine.Color.magenta;
                break;
        }
        projectile.GetComponent<Rigidbody>().velocity = -(projectile.transform.position - (m_player.gameObject.transform.position - m_player.gameObject.transform.up)).normalized * 5;
    }

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
}
