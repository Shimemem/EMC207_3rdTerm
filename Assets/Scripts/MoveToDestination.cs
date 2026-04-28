using UnityEngine;
using UnityEngine.AI;

public class MoveToDestination : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform destination;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(destination.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
