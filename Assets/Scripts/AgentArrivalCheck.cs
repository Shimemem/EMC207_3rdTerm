using UnityEngine;
using UnityEngine.AI;

public class AgentArrivalCheck : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] patrolPoints;
    private int pointIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //agent.SetDestination(destinationPos.position);
        NextPoint();
    }
    void NextPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[pointIndex].position;
        pointIndex = (pointIndex + 1) % patrolPoints.Length;
    }
    // Update is called once per frame
    void Update()
    {
        if(!agent.pathPending && agent.remainingDistance < 0.1f)
            NextPoint();
      
        //agent.SetDestination(destinationPos.position);
        //if (HasReachedDestination(agent))
        //{
        //    Debug.Log("I've Reached my Destination!");
        //}
    }

    

    //bool HasReachedDestination(NavMeshAgent _agent)
    //{
    //    if(agent.remainingDistance < agent.stoppingDistance) //Distance
    //        return false;
    //    if(agent.pathPending) //If AI have another path
    //        return false;
    //    if (agent.hasPath && agent.velocity.sqrMagnitude > 0) //If AI has path
    //        return false;
    //    return true;
    //}
}
