using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class AIController : NetworkBehaviour
{
    public NavMeshAgent agent;
    public float startWaitTime = 4;
    public float timeToRotate = 2;
    public float walkSpeed = 6;
    public float runSpeed = 9;

    public float viewRadius = 15;
    public float viewAngle = 90;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    public float meshResolution = 1f;
    public int edgeIterations = 4;
    public float edgeDistance = 0.5f;

    public GameObject[] waypoints;
    int currentPoint;

    Vector3 playerLastPosition = Vector3.zero;
    Vector3 playerPosition;

    float waitTime;
    float m_timeToRotate;
    bool m_PlayerInRange;
    bool playerNear;
    bool isPatrol;
    bool caughtPlayer;

    public Animator animator;
    public CharacterMovement shooting;
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    [SerializeField]
    private NetworkVariable<State> networkAIState = new NetworkVariable<State>();



    // Start is called before the first frame update
    void Start()
    {
        waypoints = GameObject.FindGameObjectsWithTag("WalkingPoint");
        animator = gameObject.GetComponent<Animator>();
        playerPosition = Vector3.zero;
        isPatrol = true;
        caughtPlayer = false;
        m_PlayerInRange = false;
        waitTime = startWaitTime;
        m_timeToRotate = timeToRotate;

        currentPoint = 0;
        agent = GetComponent<NavMeshAgent>();

        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.SetDestination(waypoints[currentPoint].transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        EnviromentView();
        ClientVisuals();

        if(!isPatrol)
        {
            Chasing();
            UpdateAIStateServerRpc(State.Chase);

        }
        else
        {
            Patroling();
            UpdateAIStateServerRpc(State.Patrol);
        }
    }

    private void Chasing()
    {
        playerNear = false;
        playerLastPosition = Vector3.zero;

        if(!caughtPlayer)
        {
            Move(runSpeed);
            agent.SetDestination(playerPosition);
        }
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            if(agent.remainingDistance <= 0.5f)
            {
                CaughtPlayer();
            }

            if(waitTime<= 0 && !caughtPlayer && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 6f)
            {
                caughtPlayer = false;
                isPatrol = true;
                playerNear = false;
                Move(walkSpeed);
                m_timeToRotate = timeToRotate;
                waitTime = startWaitTime;
                agent.SetDestination(waypoints[currentPoint].transform.position);
            }
            else
            {
                if(Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position)>= 2.5f)
                {
                    caughtPlayer = false;
                    Stop();
                    waitTime -= Time.deltaTime;
                }
            }
        }
    }

    private void Patroling()
    {
        if(playerNear)
        {
            if(m_timeToRotate <= 0)
            {
                Move(walkSpeed);
                LookingPlayer(playerLastPosition);
            }
            else
            {
                Stop();
                m_timeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            playerNear = false;
            playerLastPosition = Vector3.zero;
            agent.SetDestination(waypoints[currentPoint].transform.position);
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                if(waitTime <= 0)
                {
                    NextPoint();
                    Move(walkSpeed);
                    waitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    waitTime -= Time.deltaTime; 
                }
            }
        }
    }

    void Move(float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;
    }
    void Stop()
    {
        agent.isStopped = true;
        agent.speed = 0;
        UpdateAIStateServerRpc(State.Idle);

    }

    public void NextPoint()
    {
        currentPoint = (currentPoint + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentPoint].transform.position);
    }

    void CaughtPlayer()
    {
        caughtPlayer = true;
        UpdateAIStateServerRpc(State.Attack);
    }

    void LookingPlayer(Vector3 player)
    {
        agent.SetDestination(player);

        if(Vector3.Distance(transform.position, player) <= 0.3)
        {
            if (waitTime <= 0)
            {
                playerNear = false;
                Move(walkSpeed);
                agent.SetDestination(waypoints[currentPoint].transform.position);
                waitTime = startWaitTime;
                m_timeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                waitTime -= Time.deltaTime;
            }
        }
    }

    void EnviromentView()
    {
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward, dirToPlayer)< viewAngle / 2)
            {
                float dstToPlayer = Vector3.Distance(transform.position, player.position);
                if(!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    m_PlayerInRange = true;
                    isPatrol = false;
                }
                else
                {
                    m_PlayerInRange = false;
                }
            }
            if(Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                m_PlayerInRange = false;
            }

            if (m_PlayerInRange)
            {
                playerPosition = player.transform.position;
            }
        }
    }

    private void ClientVisuals()
    {
        if (networkAIState.Value == State.Patrol)
        {
            animator.SetBool("IsPatrolling", true);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsAttacking", false);

        }
        if (networkAIState.Value == State.Idle)
        {
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsAttacking", false);

        }
        if (networkAIState.Value == State.Chase)
        {
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsChasing", true);
            animator.SetBool("IsAttacking", false);
        }
        if (networkAIState.Value == State.Attack)
        {
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsAttacking", true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bullet")
        {
            shooting = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMovement>();
            shooting.score++;
            shooting.scoreT.text = shooting.score.ToString();
            Destroy(gameObject);
        }
    }

    [ServerRpc]
    public void UpdateAIStateServerRpc(State newState)
    {
        networkAIState.Value = newState;
    }
}
