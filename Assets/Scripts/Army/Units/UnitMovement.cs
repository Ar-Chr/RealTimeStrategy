using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Targeter targeter;
    [SerializeField] private float chaseRange;

    #region Server

    public override void OnStartServer()
    {
        Abstract_GameEnder.PlayerDied += ServerHandlePlayerDied;
    }

    public override void OnStopServer()
    {
        Abstract_GameEnder.PlayerDied -= ServerHandlePlayerDied;
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.Target;

        if (target != null)
        {
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if (agent.hasPath && agent.remainingDistance < agent.stoppingDistance)
            agent.ResetPath();
    }

    [Command]
    private void CmdMove(Vector3 destination)
    {
        Move(destination);
    }

    public void Move(Vector3 destination)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 4.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning(this.WithClassName("Invalid NavMesh destination."));
            return;
        }

        agent.SetDestination(destination);
    }

    private void ServerHandlePlayerDied(RTSPlayer player)
    {
        if (connectionToClient != player.connectionToClient)
            return;

        agent.ResetPath();
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();
        if (!targeter)
            targeter = GetComponent<Targeter>();
    }

    [Client]
    public bool TryMove(Vector3 destination)
    {
        if (!hasAuthority)
            return false;

        CmdMove(destination);
        return true;
    }

    #endregion
}
