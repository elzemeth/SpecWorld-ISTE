using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class GuideManager : MonoBehaviour
{
    [Header("Bileşenler")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Animator animator;
    [SerializeField] Transform player;

    [Header("Hareket Ayarları")]
    public float walkSpeed = 2.5f;
    public float arriveDistance = 0.5f;
    public float turnSpeed = 10f; 

    private bool isMoving = false;
    private Vector3 currentTarget;

    public Action OnDestinationReached;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0f;
        agent.autoBraking = true;
        agent.updateRotation = false;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!isMoving) return;

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = agent.velocity.normalized;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
            }
        }

        float dist = Vector3.Distance(transform.position, currentTarget);

        if (dist <= arriveDistance || (agent.hasPath && agent.remainingDistance <= arriveDistance))
        {
            StopMovementAndFinish();
        }
    }

    void StopMovementAndFinish()
    {
        isMoving = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero; 

        PlayAnimationState(false, false);

        LookAtPlayer(); 

        OnDestinationReached?.Invoke();
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

        PlayAnimationState(true, false);
    }

    public void Talk(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        if (audioSource.isPlaying) audioSource.Stop();

        audioSource.clip = clip;
        audioSource.Play();

        if (isMoving)
        {
            isMoving = false;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        LookAtPlayer();

        PlayAnimationState(false, true);

        StopAllCoroutines();
        StartCoroutine(WaitForTalkEnd(clip.length));
    }

    IEnumerator WaitForTalkEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        PlayAnimationState(false, false);
    }

    public void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 lookPos = new Vector3(
            player.position.x,
            transform.position.y, 
            player.position.z
        );

        transform.LookAt(lookPos);
    }

    void PlayAnimationState(bool walking, bool talking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", walking);
            animator.SetBool("isTalking", talking);
        }
    }
}