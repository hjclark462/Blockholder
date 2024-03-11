using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkGen : MonoBehaviour
{
    List<Vector3> m_verts = new List<Vector3>();
    List<int> m_tris = new List<int>();
    List<Vector2> m_uvs = new List<Vector2>();

    float m_texUnit = 0.25f;
    Vector2 m_stoneTexture = new Vector2(3, 1);    
    Vector2 m_dirtTexture = new Vector2(1, 1);
    Vector2 m_grassTopTexture = new Vector2(2, 1);    

    Mesh m_mesh;
    MeshCollider m_collider;

    int m_faceCount;

    public GameObject m_goWorld;
    WorldGen m_world;

    public int m_size = 16;
    public int m_chunkX;
    public int m_chunkY;
    public int m_chunkZ;

    public bool m_update;

    void Start()
    {
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_collider = GetComponent<MeshCollider>();

        m_world = m_goWorld.GetComponent<WorldGen>();

        MeshGen();
    }

    void LateUpdate()
    {
        if(m_update)
        {
            MeshGen();
        }
        m_update = false;
    }

    public void MeshGen()
    {
        for (int x = 0; x < m_size; x++)
        {
            for (int y = 0; y < m_size; y++)
            {
                for (int z = 0; z < m_size; z++)
                {
                    if (BlockType(x, y, z) != 0)
                    {
                        if (BlockType(x, y + 1, z) == 0 || MaxBlock(WorldGen.Face.Top, y + m_chunkY + 1))
                        {
                            CubeTop(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x, y - 1, z) == 0 || MaxBlock(WorldGen.Face.Bottom, y + m_chunkY - 1))
                        {
                            CubeBottom(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x + 1, y, z) == 0 || MaxBlock(WorldGen.Face.East, x + m_chunkX + 1))
                        {
                            CubeEast(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x - 1, y, z) == 0 || MaxBlock(WorldGen.Face.West, x + m_chunkX - 1))
                        {
                            CubeWest(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x, y, z + 1) == 0 || MaxBlock(WorldGen.Face.North, z + m_chunkZ + 1))
                        {
                            CubeNorth(x, y, z, BlockType(x, y, z));
                        }
                        if (BlockType(x, y, z - 1) == 0 || MaxBlock(WorldGen.Face.South, z + m_chunkZ - 1))
                        {
                            CubeSouth(x, y, z, BlockType(x, y, z));
                        }
                    }                    
                }
            }
        }
        UpdateMesh();
    }

    byte BlockType(int x, int y, int z)
    {
        return m_world.NeighbourBlockType(x + m_chunkX, y + m_chunkY, z + m_chunkZ);
    }

    bool MaxBlock(WorldGen.Face face, int coord)
    {        
            return m_world.MaxBlock(face, coord);     
    }

    void CubeTop(int x, int y, int z, byte block)
    {
        m_verts.Add(new Vector3(x, y + 1, z + 1));
        m_verts.Add(new Vector3(x + 1, y + 1, z + 1));
        m_verts.Add(new Vector3(x + 1, y + 1, z));
        m_verts.Add(new Vector3(x, y + 1, z));

        Vector2 texturePos = new Vector2(0, 0);

        if(BlockType(x, y, z)==1)
        {
            texturePos = m_stoneTexture;
        }
        else if(BlockType(x, y, z)==2)
        {
            texturePos = m_grassTopTexture;
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
            texturePos = m_stoneTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_dirtTexture;
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
            texturePos = m_stoneTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_dirtTexture;
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
            texturePos = m_stoneTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_dirtTexture;
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
            texturePos = m_stoneTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_dirtTexture;
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
            texturePos = m_stoneTexture;
        }
        else if (BlockType(x, y, z) == 2)
        {
            texturePos = m_dirtTexture;
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

        m_uvs.Add(new Vector2(m_texUnit * texturePos.x, m_texUnit * texturePos.y));
        m_uvs.Add(new Vector2(m_texUnit * texturePos.x + m_texUnit, m_texUnit * texturePos.y));
        m_uvs.Add(new Vector2(m_texUnit * texturePos.x + m_texUnit, m_texUnit * texturePos.y + m_texUnit));
        m_uvs.Add(new Vector2(m_texUnit * texturePos.x, m_texUnit * texturePos.y + m_texUnit));

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
