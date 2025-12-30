using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class GuideManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Animator animator;
    [SerializeField] Transform player;

    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float arriveDistance = 0.35f;

    private bool isMoving = false;
    private Vector3 currentTarget;

    public Action OnDestinationReached;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0f;
        agent.autoBraking = true;
        agent.updateRotation = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!isMoving) return;

        float dist = Vector3.Distance(transform.position, currentTarget);

        if (dist <= arriveDistance)
        {
            isMoving = false;
            agent.isStopped = true;

            PlayAnimation(false);
            LookAtPlayer();

            OnDestinationReached?.Invoke();
        }
    }

    public void GoToTarget(Vector3 target)
    {
        StopAllCoroutines();
        StartCoroutine(GoToTargetSafe(target));
    }

    IEnumerator GoToTargetSafe(Vector3 target)
    {
        agent.isStopped = true;
        yield return null; 

        agent.ResetPath();
        agent.isStopped = false;

        currentTarget = target;
        isMoving = true;

        agent.SetDestination(target);
        PlayAnimation(true);
    }

    public void Talk(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.clip = clip;
        audioSource.Play();

        LookAtPlayer(); 
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 lookPos = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );

        transform.LookAt(lookPos);
    }

    void PlayAnimation(bool walking)
    {
        if (animator != null)
            animator.SetBool("isWalking", walking);
    }
}
