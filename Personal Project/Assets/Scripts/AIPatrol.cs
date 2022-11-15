using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class AIPatrol : NetworkBehaviour
{
    private NavMeshAgent agent;
    public Transform[] points;
    private int destPoint = 0;
    public float speed;

    public Animator animator;

    public enum State
    {
        Idle,
        Walk,
        Run
    }

    [SerializeField]
    private NetworkVariable<State> networkAIState = new NetworkVariable<State>();

    // Use this for initialization
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();
        speed = agent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        GotoNextPoint();
        ClientVisuals();
        if (speed > 0 && speed < 4)
        {
            UpdateAIStateServerRpc(State.Walk);
        }
        if (speed < 0.1)
        {
            UpdateAIStateServerRpc(State.Idle);
        }
        else if (speed > 4)
        {
            UpdateAIStateServerRpc(State.Run);
        }
    }

    void GotoNextPoint()
    {
        if (points.Length == 0)
            return;

        agent.destination = points[destPoint].position;

        destPoint = (destPoint + 1) % points.Length;
    }

    private void ClientVisuals()
    {
        if(networkAIState.Value == State.Walk)
        {
            animator.SetFloat("Walk", 1);
        }
        if (networkAIState.Value == State.Idle)
        {
            animator.SetFloat("Walk", 0);
        }
        if (networkAIState.Value == State.Run)
        {
            animator.SetFloat("Walk", 4);
        }
    }

    [ServerRpc]

    public void UpdateAIStateServerRpc(State newState)
    {
        networkAIState.Value = newState;
    }
}
