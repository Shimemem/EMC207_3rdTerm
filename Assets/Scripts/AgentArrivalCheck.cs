using UnityEngine;
using UnityEngine.AI;

public class AgentArrivalCheck : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform destinationPosA, destinationPosB, destinationPosC;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(destinationPosA.position);
    }

    // Update is called once per frame
    void Update()
    {
        //agent.SetDestination(destinationPos.position);
        if (HasReachedDestination(agent))
        {
            Debug.Log("I've Reached my Destination!");
        }
    }

    bool HasReachedDestination(NavMeshAgent _agent)
    {
        if(agent.remainingDistance < agent.stoppingDistance) //Distance
            return false;
        if(agent.pathPending) //If AI have another path
            return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0) //If AI has path
            return false;
        return true;
    }
}
