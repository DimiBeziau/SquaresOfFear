using System.Collections.Generic;
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

    private GameObject plane;
    private MarkerTrigger _trigger;

    public GameObject sphere;

    [Header("Audio")]
    public AudioClip sfxMark;
    public AudioClip sfxRun;
    public AudioClip sfxBonus;
    public AudioClip sfxShowBonus;
    public AudioClip sfxMalus;
    public AudioClip sfxClassic;
    private AudioSource _audio;
    private AudioSource _audioRun;
    private bool _wasRunning = false;
    private bool _gameOverTriggered = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = characterAnimator != null ? characterAnimator : GetComponentInChildren<Animator>();
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        // AudioSource dédié au son de course (en boucle)
        _audioRun = gameObject.AddComponent<AudioSource>();
        _audioRun.loop = true;
        _audioRun.clip = sfxRun;

        // Trigger invisible permanent, repositionné à chaque marquage
        GameObject triggerObj = new GameObject("MarkerTriggerZone");
        triggerObj.transform.SetParent(null);
        _trigger = triggerObj.AddComponent<MarkerTrigger>();
        _trigger.spherePrefab = sphere;
        _trigger.sfxBonus = sfxBonus;
        _trigger.sfxMalus = sfxMalus;
        _trigger.sfxClassic = sfxClassic;
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

        // Son de course
        bool isRunning = direction.magnitude > 0.1f;
        if (isRunning && !_audioRun.isPlaying && sfxRun != null)
            _audioRun.Play();
        else if (!isRunning && _audioRun.isPlaying)
            _audioRun.Stop();
        _wasRunning = isRunning;

        if (Input.GetMouseButtonDown(0)) // clic gauche : marquer la case
        {
            Destroy(plane);
            Vector3 markerPos = new Vector3(
                Mathf.Round(transform.position.x),
                0.99f,
                Mathf.Round(transform.position.z)
            );
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Destroy(plane.GetComponent<MeshCollider>());
            plane.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            plane.GetComponent<Renderer>().material.color = Color.blue;
            plane.transform.position = markerPos;

            // Positionne le trigger au centre du cube (y+1 au dessus du sol)
            _trigger.transform.position = new Vector3(markerPos.x, markerPos.y + 1f, markerPos.z);

            if (sfxMark != null) _audio.PlayOneShot(sfxMark);
        }

        if (Input.GetMouseButtonDown(1) && plane != null) // clic droit : déclencher
        {
            Destroy(plane);
            plane = null;
            _trigger.Activate(1f, 1f);
        }

        if (Input.GetMouseButtonDown(2) && _trigger.spawnedSpheres.Count > 0)
        {
            if (sfxShowBonus != null) _audio.PlayOneShot(sfxShowBonus);
            List<GameObject> toDetonate = new List<GameObject>(_trigger.spawnedSpheres);
            _trigger.spawnedSpheres.Clear();

            foreach (GameObject bomb in toDetonate)
            {
                if (bomb == null) continue;
                RaycastHit[] hits = Physics.SphereCastAll(bomb.transform.position, 1.1f, Vector3.down, 10f);
                foreach (RaycastHit h in hits)
                {
                    CubeMove target = h.collider.GetComponent<CubeMove>();
                    if (target == null) continue;

                    if (target.kind == CubeMove.CubeKind.Golden && sphere != null)
                    {
                        Vector3 spawnPos = new Vector3(
                            Mathf.Round(target.transform.position.x),
                            target.transform.position.y + 1f,
                            Mathf.Round(target.transform.position.z)
                        );
                        _trigger.spawnedSpheres.Add(Instantiate(sphere, spawnPos, Quaternion.identity));
                        target.ReactToHit(true);
                    }
                    else if (target.kind == CubeMove.CubeKind.Black)
                    {
                        target.ReactToHit(false);
                        CreatingLevel creatingLevel = FindFirstObjectByType<CreatingLevel>();
                        if (creatingLevel != null) creatingLevel.PenaltyAdvance();
                    }
                    else
                    {
                        target.ReactToHit(true);
                    }
                }
                Destroy(bomb);
            }
        }

        if (!_gameOverTriggered && transform.position.y < -20)
        {
            _gameOverTriggered = true;
            CreatingLevel creatingLevel = FindFirstObjectByType<CreatingLevel>();
            if (creatingLevel != null)
                creatingLevel.ShowGameOverMenu();
            else
                SceneManager.LoadScene("SquaresOfFear_scene");
        }
    }
}
