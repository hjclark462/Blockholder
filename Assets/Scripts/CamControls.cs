using UnityEngine;

public class CamControls : MonoBehaviour
{
    float m_xRot;
    float m_yRot;
    public Transform m_parent;

    private void Start()
    {
        m_xRot = transform.rotation.eulerAngles.x;
        m_yRot = transform.rotation.eulerAngles.y;
    }

    public void SetRotation(Vector3 rotation)
    {
        m_xRot = rotation.x;
        m_yRot = rotation.y;
        m_parent.rotation = Quaternion.Euler(0, m_yRot, 0);
        transform.localRotation = Quaternion.Euler(m_xRot, 0, 0);
    }

    public void MoveCamera(Vector2 mouse, float sensitivity)
    {
        m_xRot -= mouse.y * Time.deltaTime * sensitivity;
        m_xRot = Mathf.Clamp(m_xRot, -90, 90);
        m_yRot += mouse.x * Time.deltaTime * sensitivity;
        m_parent.rotation = Quaternion.Euler(0, m_yRot, 0);
        transform.localRotation = Quaternion.Euler(m_xRot, 0, 0);
    }
};