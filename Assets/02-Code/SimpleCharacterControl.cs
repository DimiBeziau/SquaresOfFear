using UnityEngine;

public class SimpleCharacterControl : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 2f;
    [SerializeField] private Animator m_animator;
    [SerializeField] private Animator m_characterAnimator;
    [SerializeField] private Rigidbody m_rigidBody;

    private float m_currentV = 0f;
    private float m_currentH = 0f;

    private readonly float m_interpolation = 10f;

    private Vector3 m_currentDirection = Vector3.zero;

    void Update()
    {
        m_animator.SetBool("Grounded", true);
        if (m_characterAnimator != null)
            m_characterAnimator.SetBool("Grounded", true);

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = new Vector3(m_currentH, 0f, m_currentV);
        float directionLength = direction.magnitude;
        direction.y = 0f;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);
            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
            if (m_characterAnimator != null)
                m_characterAnimator.SetFloat("MoveSpeed", direction.magnitude);
        }
    }
}
