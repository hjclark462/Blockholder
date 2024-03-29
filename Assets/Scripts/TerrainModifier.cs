using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    WorldGen m_world;
    GameObject m_cameraGO;
    GameObject m_playerGO;
    public bool m_isReady = false;

    void Start()
    {
        m_world = GetComponent<WorldGen>();
        m_cameraGO = GameObject.FindGameObjectWithTag("MainCamera");
        m_playerGO = FindObjectOfType<Player>().gameObject;
    }
    void Update()
    {
        if (m_isReady)
        {
            LoadChunks(m_cameraGO.transform.position, 23, 24);
        }
    }

    public void Explosion(Vector3 pos, float radius, byte block)
    {
        for (int x = (int)-radius; x <= radius; x++)
        {
            for (int y = (int)-radius; y <= radius; y++)
            {
                for (int z = (int)-radius; z <= radius; z++)
                {
                    if (Vector3.Distance(new Vector3(x, y, z), Vector3.zero) <= radius)
                    {
                        SetBlockAt(pos + new Vector3(x, y, z), block);
                    }
                }
            }
        }
    }

    public void ReplaceBlock(float range, byte block)
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.distance < range)
            {
                Vector3 pos = hit.point;
                pos += ray.direction.normalized * 0.1f;

                SetBlockAt(pos, block);
            }
        }
    }

    public void AddBlock(float range, byte block)
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.distance < range)
            {
                Vector3 pos = hit.point;
                pos -= ray.direction.normalized * 0.1f;

                SetBlockAt(pos, block);
            }
        }
    }

    public void SetBlockAt(Vector3 pos, byte block)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (block != 0)
        {
            if (Vector3.Distance(m_playerGO.transform.position, new Vector3(x, y, z)) > 2f)
            {
                m_world.UpdateBlock(x, y, z, block);
                UpdateChunkAt(x, y, z);
            }
        }
        else
        {
            m_world.UpdateBlock(x, y, z, block);
            UpdateChunkAt(x, y, z);
        }
    }

    public void UpdateChunkAt(int x, int y, int z)
    {
        int updateX = Mathf.FloorToInt(x / m_world.m_chunkSize);
        int updateY = Mathf.FloorToInt(y / m_world.m_chunkSize);
        int updateZ = Mathf.FloorToInt(z / m_world.m_chunkSize);

        m_world.UpdateChunk(updateX, updateY, updateZ);

        if (x - (m_world.m_chunkSize * updateX) == 0 && updateX != 0)
        {
            m_world.UpdateChunk(updateX - 1, updateY, updateZ);
        }

        if (x - (m_world.m_chunkSize * updateX) == 15 && updateX != m_world.m_chunks.GetLength(0) - 1)
        {
            m_world.UpdateChunk(updateX + 1, updateY, updateZ);
        }

        if (y - (m_world.m_chunkSize * updateY) == 0 && updateY != 0)
        {
            m_world.UpdateChunk(updateX, updateY - 1, updateZ);
        }

        if (y - (m_world.m_chunkSize * updateY) == 15 && updateY != m_world.m_chunks.GetLength(1) - 1)
        {
            m_world.UpdateChunk(updateX, updateY + 1, updateZ);
        }

        if (z - (m_world.m_chunkSize * updateZ) == 0 && updateZ != 0)
        {
            m_world.UpdateChunk(updateX, updateY, updateZ - 1);
        }

        if (z - (m_world.m_chunkSize * updateZ) == 15 && updateZ != m_world.m_chunks.GetLength(2) - 1)
        {
            m_world.UpdateChunk(updateX, updateY, updateZ + 1);
        }
    }

    public void LoadChunks(Vector3 playerPos, float distanceToLoad, float distanceToUnload)
    {
        for (int x = 0; x < m_world.m_chunks.GetLength(0); x++)
        {
            for (int z = 0; z < m_world.m_chunks.GetLength(2); z++)
            {
                float distance = Vector2.Distance(new Vector2(x * m_world.m_chunkSize + (m_world.m_chunkSize * 0.5f), z * m_world.m_chunkSize + (m_world.m_chunkSize * 0.5f)), new Vector2(playerPos.x, playerPos.z));

                if (distance < distanceToLoad && GameManager.Instance.m_gameState == GameState.GAME)
                {
                    if (m_world.m_chunks[x, 0, z] == null)
                    {
                        m_world.GenerateColumn(x, z);
                    }
                }
                else if (distance > distanceToUnload || GameManager.Instance.m_gameState != GameState.GAME)
                {
                    if (m_world.m_chunks[x, 0, z] != null)
                    {
                        m_world.DegenerateColumn(x, z);
                    }
                }
            }
        }
    }
}
