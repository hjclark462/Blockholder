using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCreateExample : MonoBehaviour
{
    public GameObject m_terrain;
    SquareGen m_script;
    public GameObject m_target;
    LayerMask m_layerMask = (1 << 0);

    void Start()
    {
        m_script = m_terrain.GetComponent<SquareGen>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        float distance = Vector3.Distance(transform.position, m_target.transform.position);

        if (Physics.Raycast(transform.position, (m_target.transform.position - transform.position).normalized, out hit, distance, m_layerMask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            Vector2 point = new Vector2(hit.point.x, hit.point.y);
            point += (new Vector2(hit.normal.x, hit.normal.y)) * 0.5f;

            int x = Mathf.RoundToInt(point.x - 0.5f);
            int y = Mathf.RoundToInt(point.y - 0.5f);

            m_script.m_blocks[x, y] = 1;
            m_script.m_update = true;
        }
        else
        {
            Debug.DrawLine(transform.position, m_target.transform.position, Color.blue);
        }
    }
}
