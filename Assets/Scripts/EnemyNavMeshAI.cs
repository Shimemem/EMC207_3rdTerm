using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshAI : MonoBehaviour
{
    private enum EnemyStates
    {
        Idle,
        Patrol, // Walking
        Chase   // Running
    }
    [Header("References")]
    [SerializeField] Transform player;  // The player that the enemy will chase
    [SerializeField] Transform[] patrolPoints;  // Patrol points where enemy will walk

    [Header("Detection")]
    [SerializeField] private float chaseRange = 10.0f;  // Start Chasing
    [SerializeField] private float loseRange = 16.0f;   // Stops Chasing(lost)

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float patrolStoppingDistance = 0.2f;
    [SerializeField] private float chaseStoppingDistance = 1f;
    [SerializeField] private float waypointReachDistance = 0.5f;
    [SerializeField] private float waitTimeAtPoint = 1.5f;  // How long the enemy waits at each patrol point
    [SerializeField] private bool randomPatrol = false;
    [SerializeField] private float facePlayerSpeed = 8f;

    [Header("Animation")]
    [SerializeField] string speedParameter = "Speed";
    [SerializeField] float animationDampTime = 0.1f;    // Makes the animation transition not look sudden

    private NavMeshAgent agent;
    private Animator animator;
    private EnemyStates currentState;
    private bool stateInitialized;
    private int patrolIndex;
    private float waitTimer;

    private bool HasPatrolPoints    // property that chekcs if the patrol points array exists
    {
        get 
        {
            return patrolPoints != null && patrolPoints.Length > 0;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        // If patrol point exist, start the patrol point
        if (HasPatrolPoints)
        {
            if (HasPatrolPoints)
            {
                ChangeState(EnemyStates.Patrol);
            }
            else
            {
                ChangeState(EnemyStates.Idle);
            }
        }
    }
    private void Update()
    {
        CheckForPlayer();
        UpdateAnimation();
        switch (currentState)
        {
            case EnemyStates.Idle:
                UpdateIdle();
                break;
            case EnemyStates.Patrol:
                UpdatePatrol();
                break;
            case EnemyStates.Chase:
                UpdateChase();
                break;
            default:
                break;
        }
    }

    private void CheckForPlayer()
    {
        if (player == null) // If there is no player references, do nothing
        {
            return;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Measure distance between the enemy and the player
        if (currentState != EnemyStates.Chase && distanceToPlayer <= chaseRange)    //
        {
            ChangeState(EnemyStates.Chase);
        }
        else if (currentState != EnemyStates.Chase && distanceToPlayer >= loseRange)    // If the enemy is chasing but the player gets too far
        {
            ChangeState(EnemyStates.Patrol);
        }

        else if (currentState != EnemyStates.Patrol && distanceToPlayer >= loseRange)   // 
        {
            ChangeState(EnemyStates.Patrol);
        }
    }

    // Method changes enemy's current state
    private void ChangeState(EnemyStates newState)
    {
        // If the state was already initialized and the enemy is already in this state
        // Do not restart the same state again
        if (stateInitialized && currentState == newState)
        {
            return;
        }
        // Mark that at least one state has nnow been initialized
        stateInitialized = true;
        // Stores the new state
        currentState = newState;

        switch (currentState)   // Run the correct "Enter" Method
        {
            case EnemyStates.Idle:
                EnterIdle();
                break;
            case EnemyStates.Patrol:
                EnterPatrol();
                break;
            case EnemyStates.Chase:
                EnterChase();
                break;
            default:
                break;
        }
    }


    // Idle State, method runs once when the enemy enters Idle state
    private void EnterIdle()
    {
        agent.isStopped = true; // prevents NavMeshAgent from moving
        agent.ResetPath();  // Clear the current path
        waitTimer = 0;  // Resets wait timer
    }
    private void UpdateIdle()
    {
        // If the navmesh does anything during idle
    }
    // Patrol State
    private void EnterPatrol()
    {
        agent.isStopped = false;    // Allows AI to Move
        agent.speed = walkSpeed;    // Sets the speed to walking speed
        agent.stoppingDistance = patrolStoppingDistance;    // Stops the AI on certain distance during partrol
        waitTimer = 0;
        SetCurrentPatrolDestination();  // Move towards the current patrol point
    }
    private void UpdatePatrol()
    {
        if (!HasPatrolPoints)   // If no patrol points, switch to Idle
        {
            ChangeState(EnemyStates.Idle);
            return;
        }
        if (!ReachedDestination())  // If the AI has not reached destination, keep moving
        {
            return;
        }
        if (ReachedDestination())
        {
            agent.isStopped = true;
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)   // if Ai waited enough on the patrol point
            {
                waitTimer = 0;  // reset wait timer
                ChooseNextPatrolPoint();
                agent.isStopped = false;
                SetCurrentPatrolDestination();
            }
        }
    }
    // Chase State
    private void EnterChase()
    {
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = chaseStoppingDistance;
        waitTimer = 0;
    }
    private void UpdateChase()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (player == null)   // If the player no longer exist, FAILSAFE
        {
            if (HasPatrolPoints)
            {
                ChangeState(EnemyStates.Patrol);   
            }
            else
            {
                ChangeState(EnemyStates.Idle);
            }
        }
        if (distanceToPlayer <= chaseStoppingDistance)  // If the enemy is close enough to the player, stop moving
        {
            // Attack State
            agent.isStopped = true;
            agent.ResetPath();
        }
        else    // If the player 
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        return;
    }

    // Chekcs if the AI reached its current Destination
    private bool ReachedDestination()
    {
        if (agent.pathPending)
        {
            return false;
        }
        // Sometimes remaining distance can be infinity while the path is unknown
        // If that happens, do not count as reached
        if (agent.remainingDistance == Mathf.Infinity)
        {
            return false;
        }
        // Use whichever valus is bigger
        // Agents stopping distance or our custom waypoint reach
        float reachedDistance = Mathf.Max(agent.stoppingDistance, waypointReachDistance);   
        // If the remaining distance is small enough, the destination is reached
        return agent.remainingDistance <= reachedDistance;
    }

    private void SetCurrentPatrolDestination()  // Method sends the enemy to the current patrol point
    {
        if (!HasPatrolPoints)   // If there are no patrol points, do nothing
        {
            return;
        }
        if (randomPatrol && patrolPoints.Length > 1)
        {
            int nextIndex = patrolIndex;
        }
        Transform point = patrolPoints[patrolIndex];
        if (point == null)  // If this patrol point is missing, choose another one
        {
            ChooseNextPatrolPoint();
            point = patrolPoints[patrolIndex];
        }
        if (point != null)  // If patrol is valid, Set it's Destination
        {
            agent.SetDestination(point.position);
        }
    }
    private void ChooseNextPatrolPoint()    // Method chooses the next patrol point
    {
        if (!HasPatrolPoints) { return; }   // If there are no patrol points, do nothing
        if (randomPatrol && patrolPoints.Length > 1)
        {
            int nextIndex = patrolIndex; // logic for random patrolling
            while (nextIndex == patrolIndex)
            {
                nextIndex = Random.Range(0, patrolPoints.Length);
            }
            patrolIndex = nextIndex;
        }
        else
        {
            patrolIndex++;  // move to the next patrol point
            if (patrolIndex >= patrolPoints.Length) // resets the patrol point to zero
            {
                patrolIndex = 0;
            }
        }
    }

    private void UpdateAnimation()
    {
        float animationSpeed = 0;

        bool isMoving = agent.velocity.magnitude > 0.05 && !agent.isStopped;    // if agent is moving
        if (currentState == EnemyStates.Patrol && isMoving)
        {
            animationSpeed = 0.5f;
        }
        else if (currentState == EnemyStates.Chase && isMoving)
        {
            animationSpeed = 1.0f;
        }

        // IDLE = 0
        // WALK = 0.5
        // RUN = 1

        animator.SetFloat(speedParameter, animationSpeed, animationDampTime, Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
 