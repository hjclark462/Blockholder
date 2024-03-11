using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderExample : MonoBehaviour
{
    public GameObject m_terrain;
    SquareGen m_script;
    public int m_size = 4;
    public bool m_circular = false;

    // Start is called before the first frame update
    void Start()
    {
        m_script = m_terrain.GetComponent<SquareGen>();
    }

    // Update is called once per frame
    void Update()
    {
        bool collision = false;
        for(int x=0; x < m_size; x++)
        {
            for(int y=0; y < m_size; y++) 
            {
                if(m_circular)
                {
                    if(Vector2.Distance(new Vector2(x-(m_size/2),y-(m_size/2)),Vector2.zero)<=(m_size/3))
                    {
                        if(RemoveBlock(x-(m_size/2), y-(m_size/2))) 
                        {
                            collision = true;
                        }
                    }
                }
                else
                {
                    if(RemoveBlock(x-(m_size/2),y-(m_size/2)))
                    {
                        collision = true;
                    }
                }
            }
        }
        if(collision)
        {
            m_script.m_update = true;
        }
    }

    bool RemoveBlock(float offsetX, float offsetY)
    {
        int x = Mathf.RoundToInt(transform.position.x + offsetX);
        int y = Mathf.RoundToInt(transform.position.y + offsetY);

        if(x < m_script.m_blocks.GetLength(0) && y < m_script.m_blocks.GetLength(1) && x >= 0 && y >= 0)
        {
            if (m_script.m_blocks[x, y]!=0)
            {
                m_script.m_blocks[x, y] = 0;
                return true;
            }
        }
        return false;
    }
}
