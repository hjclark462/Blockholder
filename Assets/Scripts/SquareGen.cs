using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

public class SquareGen : MonoBehaviour
{
    // Render List
    public List<Vector3> m_verts = new List<Vector3>();
    public List<int> m_tris = new List<int>();
    public List<Vector2> m_uvs = new List<Vector2>();

    Mesh m_mesh;

    public List<Vector3> m_colliderVerts = new List<Vector3>();
    public List<int> m_colliderTris = new List<int>();
    int m_colliderCount;

    MeshCollider m_collider;

    public int m_row = 1;
    public int m_col = 1;

    float m_tUnit = 0.25f;
    Vector2 m_tStone = new Vector2(3, 3);
    Vector2 m_tGrass = new Vector2(0, 1);

    int m_squareCount;

    public byte[,] m_blocks;
    public bool m_update = false;
    void Start()
    {
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_collider = GetComponent<MeshCollider>();

        GridGen();
        BuildMesh();
        UpdateMesh();
    }

    private void Update()
    {
        if (m_col <= 0)
        {
            m_col = 1;
        }
        if (m_row <= 0)
        {
            m_row = 1;
        }

        if (m_update)
        {
            BuildMesh();
            UpdateMesh();
            m_update = false;
        }
    }

    void GridGen()
    {
        m_blocks = new byte[m_row, m_col];

        for (int x = 0; x < m_blocks.GetLength(0); x++)
        {
            int stone = Noise(x, 0, 80, 15, 1);
            stone += Noise(x, 0, 50, 30, 1);
            stone += Noise(x, 0, 10, 10, 1);
            stone += 75;

            int dirt = Noise(x, 0, 100f, 35, 1);
            dirt += Noise(x, 100, 50, 30, 1);
            dirt += 75;

            for (int y = 0; y < m_blocks.GetLength(1); y++)
            {
                if (y < stone)
                {
                    m_blocks[x, y] = 1;

                    if (Noise(x, y, 12, 16, 1) > 10)
                    {
                        m_blocks[x, y] = 1;
                    }
                    if(Noise(x, y*2, 16, 14, 1)>10)
                    {
                        m_blocks[x, y] = 0;
                    }
                }
                else if (y < dirt)
                {
                    m_blocks[x, y] = 2;
                }
            }
        }
    }

    void BuildMesh()
    {
        for (int x = 0; x < m_blocks.GetLength(0); x++)
        {
            for (int y = 0; y < m_blocks.GetLength(1); y++)
            {
                if (m_blocks[x, y] != 0)
                {
                    ColliderGen(x, y);

                    if (m_blocks[x, y] == 1)
                    {
                        GenSquare(x, y, m_tStone);
                    }
                    else if (m_blocks[x, y] == 2)
                    {
                        GenSquare(x, y, m_tStone);
                    }
                }
            }
        }
    }

    void GenSquare(int x, int y, Vector2 tex)
    {
        m_verts.Add(new Vector3(x, y + 1, 0));
        m_verts.Add(new Vector3(x + 1, y + 1, 0));
        m_verts.Add(new Vector3(x + 1, y, 0));
        m_verts.Add(new Vector3(x, y, 0));

        int curTri = m_squareCount * 4;

        m_tris.Add(0 + curTri);
        m_tris.Add(1 + curTri);
        m_tris.Add(3 + curTri);
        m_tris.Add(1 + curTri);
        m_tris.Add(2 + curTri);
        m_tris.Add(3 + curTri);

        m_uvs.Add(new Vector2(m_tUnit * tex.x, m_tUnit * tex.y));
        m_uvs.Add(new Vector2(m_tUnit * tex.x + m_tUnit, m_tUnit * tex.y));
        m_uvs.Add(new Vector2(m_tUnit * tex.x + m_tUnit, m_tUnit * tex.y + m_tUnit));
        m_uvs.Add(new Vector2(m_tUnit * tex.x, m_tUnit * tex.y + m_tUnit));

        m_squareCount++;
    }

    void ColliderGen(int x, int y)
    {
        if (IsBorderBlock(x, y + 1))
        {
            // Top
            m_colliderVerts.Add(new Vector3(x, y + 1, 1));
            m_colliderVerts.Add(new Vector3(x + 1, y + 1, 1));
            m_colliderVerts.Add(new Vector3(x + 1, y + 1, 0));
            m_colliderVerts.Add(new Vector3(x, y + 1, 0));

            ColliderTris();

            m_colliderCount++;
        }

        if (IsBorderBlock(x, y - 1))
        {
            // Bottom
            m_colliderVerts.Add(new Vector3(x, y, 1));
            m_colliderVerts.Add(new Vector3(x + 1, y, 1));
            m_colliderVerts.Add(new Vector3(x + 1, y, 0));
            m_colliderVerts.Add(new Vector3(x, y, 0));

            ColliderTris();

            m_colliderCount++;
        }

        if (IsBorderBlock(x - 1, y))
        {
            // Left
            m_colliderVerts.Add(new Vector3(x, y, 1));
            m_colliderVerts.Add(new Vector3(x, y + 1, 1));
            m_colliderVerts.Add(new Vector3(x, y + 1, 0));
            m_colliderVerts.Add(new Vector3(x, y, 0));

            ColliderTris();

            m_colliderCount++;
        }

        if (IsBorderBlock(x + 1, y))
        {
            // Right
            m_colliderVerts.Add(new Vector3(x + 1, y + 1, 1));
            m_colliderVerts.Add(new Vector3(x + 1, y, 1));
            m_colliderVerts.Add(new Vector3(x + 1, y, 0));
            m_colliderVerts.Add(new Vector3(x + 1, y + 1, 0));

            ColliderTris();

            m_colliderCount++;
        }

        // Face
        m_colliderVerts.Add(new Vector3(x, y + 1, 0));
        m_colliderVerts.Add(new Vector3(x + 1, y + 1, 0));
        m_colliderVerts.Add(new Vector3(x + 1, y, 0));
        m_colliderVerts.Add(new Vector3(x, y, 0));

        ColliderTris();

        m_colliderCount++;
    }

    void ColliderTris()
    {
        int curTri = m_colliderCount * 4;
        m_colliderTris.Add(0 + curTri);
        m_colliderTris.Add(1 + curTri);
        m_colliderTris.Add(3 + curTri);
        m_colliderTris.Add(1 + curTri);
        m_colliderTris.Add(2 + curTri);
        m_colliderTris.Add(3 + curTri);
    }

    bool IsBorderBlock(int x, int y)
    {
        if (x == -1 || x == m_blocks.GetLength(0) || y == -1 || y == m_blocks.GetLength(1)|| m_blocks[x, y]==0)
        {
            return true;
        }
        if(m_blocks[x, y] == 0)
        {
            return true;
        }
        return false;
    }
    void UpdateMesh()
    {
        m_mesh.Clear();
        m_mesh.vertices = m_verts.ToArray();
        m_mesh.triangles = m_tris.ToArray();
        m_mesh.uv = m_uvs.ToArray();
        m_mesh.Optimize();
        m_mesh.RecalculateNormals();

        m_squareCount = 0;
        m_verts.Clear();
        m_tris.Clear();
        m_uvs.Clear();

        Mesh colMesh = new Mesh();
        colMesh.vertices = m_colliderVerts.ToArray();
        colMesh.triangles = m_colliderTris.ToArray();
        m_collider.sharedMesh = colMesh;

        m_colliderVerts.Clear();
        m_colliderTris.Clear();
        m_colliderCount = 0;
    }

    int Noise(int x, int y, float scale, float mag, float exp)
    {
        return (int)(Mathf.Pow((Mathf.PerlinNoise(x / scale, y / scale) * mag), (exp)));
    }
}
