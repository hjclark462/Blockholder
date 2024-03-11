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

        for (int x = 0; x < m_worldX; x++)
        {
            for (int z = 0; z < m_worldZ; z++)
            {
                int stone = PerlinNoise(x, 10, z, 10, 3, 1.2f);
                stone += PerlinNoise(x, 350, z, 20, 4, 0) + 10;

                int dirt = PerlinNoise(x, 100, z, 50, 3, 0) + 1;


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
            Debug.Log(x + " " + y + " " + z);
            m_chunks[x, y, z].m_update = true;
        }
    }

    public void UpdateBlock(int x, int y, int z, byte block)
    {
        if (x < m_worldX && x >= 0 && y < m_worldY && y >= 0 && z < m_worldZ && z >= 0)
        {
            Debug.Log(x + " " + y + " " + z);
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
