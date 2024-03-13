using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.VisualScripting;

public class WorldGen : MonoBehaviour
{
    public byte[,,] m_data;
    public int m_worldX = 16;
    public int m_worldY = 16;
    public int m_worldZ = 16;

    public GameObject m_chunk;
    public ChunkGen[,,] m_chunks;
    public int m_chunkSize = 16;
    public Vector3Int m_perlinY = new Vector3Int(10,350,100);
    public Vector3 m_perlinScale = new Vector3(10.0f,20.0f,50.0f);
    public Vector3 m_perlinHeight = new Vector3(3.0f, 4.0f, 3.0f);
    public Vector3 m_perlinPower = new Vector3(1.2f, 0.0f, 0.0f);
    public Vector2Int m_perlinAdd = new Vector2Int(10, 1);
    TerrainModifier m_tm;

    public enum Face
    {
        Top,
        Bottom,
        North,
        South,
        West,
        East
    }

    void Start()
    {
        m_data = new byte[m_worldX, m_worldY, m_worldZ];              
        m_tm = FindObjectOfType<TerrainModifier>();
    }

    public void StartGame()
    {
        for (int x = 0; x < m_worldX; x++)
        {
            for (int z = 0; z < m_worldZ; z++)
            {
                int stone = PerlinNoise(x, m_perlinY[0], z, m_perlinScale[0], m_perlinHeight[0], m_perlinPower[0]);
                stone += PerlinNoise(x, m_perlinY[1], z, m_perlinScale[1], m_perlinHeight[1], m_perlinPower[1]) + m_perlinAdd[0];

                int dirt = PerlinNoise(x, m_perlinY[2], z, m_perlinScale[2], m_perlinHeight[2], m_perlinPower[2]) + m_perlinAdd[1];


                for (int y = 0; y < m_worldY; y++)
                {
                    if (y <= stone)
                    {
                        m_data[x, y, z] = 1;
                    }
                    else if (y < dirt + stone)
                    {
                        m_data[x, y, z] = 2;
                    }
                }
            }
        }

        m_chunks = new ChunkGen[Mathf.FloorToInt(m_worldX / m_chunkSize), Mathf.FloorToInt(m_worldY / m_chunkSize), Mathf.FloorToInt(m_worldZ / m_chunkSize)];
        for (int x = 0; x < m_chunks.GetLength(0); x++)
        {
            for (int y = 0; y < m_chunks.GetLength(1); y++)
            {
                for (int z = 0; z < m_chunks.GetLength(2); z++)
                {
                    GameObject chunk = Instantiate(m_chunk, new Vector3(x * m_chunkSize, y * m_chunkSize, z * m_chunkSize), new Quaternion(0, 0, 0, 0));

                    m_chunks[x, y, z] = chunk.GetComponent<ChunkGen>();
                    m_chunks[x, y, z].m_goWorld = gameObject;
                    m_chunks[x, y, z].m_size = m_chunkSize;
                    m_chunks[x, y, z].m_chunkX = x * m_chunkSize;
                    m_chunks[x, y, z].m_chunkY = y * m_chunkSize;
                    m_chunks[x, y, z].m_chunkZ = z * m_chunkSize;
                }
            }
        }
        m_tm.m_isReady = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public byte NeighbourBlockType(int x, int y, int z)
    {
        if (x >= m_worldX || x < 0 || y >= m_worldY || y < 0 || z >= m_worldZ || z < 0)
        {
            return (byte)1;
        }
        return m_data[x, y, z];
    }

    public void UpdateChunk(int x, int y, int z)
    {
        if (x < m_worldX / m_chunkSize && x >= 0 && y < m_worldY / m_chunkSize && y >= 0 && z < m_worldZ / m_chunkSize && z >= 0)
        {
            m_chunks[x, y, z].m_update = true;
        }
    }

    public void UpdateBlock(int x, int y, int z, byte block)
    {
        if (x < m_worldX && x >= 0 && y < m_worldY && y >= 0 && z < m_worldZ && z >= 0)
        {
            m_data[x, y, z] = block;
        }
    }

    public bool MaxBlock(Face face, int coord)
    {
        switch (face)
        {
            case Face.Top:
                {
                    return coord >= m_worldY;
                }
            case Face.Bottom:
                {
                    return coord < 0;
                }
            case Face.North:
                {
                    return coord >= m_worldZ;
                }
            case Face.South:
                {
                    return coord < 0;
                }
            case Face.East:
                {
                    return coord >= m_worldX;
                }
            case Face.West:
                {
                    return coord < 0;
                }
            default:
                return false;
        }
    }

    public void GenerateColumn(int x, int z)
    {

        for (int y = 0; y < m_chunks.GetLength(1); y++)
        {
            GameObject chunk = Instantiate(m_chunk, new Vector3(x * m_chunkSize, y * m_chunkSize, z * m_chunkSize), new Quaternion(0, 0, 0, 0));

            m_chunks[x, y, z] = chunk.GetComponent<ChunkGen>();
            m_chunks[x, y, z].m_goWorld = gameObject;
            m_chunks[x, y, z].m_size = m_chunkSize;
            m_chunks[x, y, z].m_chunkX = x * m_chunkSize;
            m_chunks[x, y, z].m_chunkY = y * m_chunkSize;
            m_chunks[x, y, z].m_chunkZ = z * m_chunkSize;
        }
    }

    public void DegenerateColumn(int x, int z)
    {
        for (int y = 0; y < m_chunks.GetLength(1); y++)
        {
            Destroy(m_chunks[x, y, z].gameObject);                     
        }
    }

    int PerlinNoise(int x, int y, int z, float scale, float height, float power)
    {
        float rValue;
        rValue = Noise.GetNoise(x / scale, y / scale, z / scale);
        rValue *= height;

        if (power != 0)
        {
            rValue = Mathf.Pow(rValue, power);
        }
        return (int)rValue;
    }
}
