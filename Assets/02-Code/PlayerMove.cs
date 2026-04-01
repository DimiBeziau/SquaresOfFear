using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    private float speed = 3.0f;
    private float interpolation = 10f;
    private Rigidbody _rb;
    private Animator _animator;
    private Vector3 _currentDirection = Vector3.zero;
    public Animator characterAnimator;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = characterAnimator != null ? characterAnimator : GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal");
        float deltaZ = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(deltaX, 0f, deltaZ);

        if (direction != Vector3.zero)
        {
            _currentDirection = Vector3.Slerp(_currentDirection, direction, Time.deltaTime * interpolation);
            transform.rotation = Quaternion.LookRotation(_currentDirection);
        }

        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);

        if (_animator != null)
        {
            _animator.SetFloat("MoveSpeed", direction.magnitude);
            _animator.SetBool("Grounded", true);
        }

        if (transform.position.y < -20)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
