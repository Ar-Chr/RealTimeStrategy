using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [Header(nameof(UnitSpawner) + " Fields")]
    [SerializeField] private Unit unit;
    [SerializeField] private int maxUnitQueue;
    [Space]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 gatherPoint;
    [SerializeField] private bool gatherPointSet;
    [Space]
    [SerializeField] private GameObject queueIndicatorParentObject;
    [SerializeField] private TMP_Text remainingUnitsText;
    [SerializeField] private Image progressImage;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsChanged))]
    private int queuedUnits;

    [SyncVar(hook = nameof(ClientHandleTimerChanged))] 
    private float timer;

    private RTSPlayer player;
    private float progressImageVelocity;

    private void Update()
    {
        if (isServer)
        {
            UpdateUnitProduction();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        base.OnStartServer();

        player = connectionToClient.identity.GetComponent<RTSPlayer>();
        ClientHandleQueuedUnitsChanged(0, 0);
        ClientHandleTimerChanged(0, 0);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (player.Resources >= unit.Cost)
            if (queuedUnits < maxUnitQueue)
            {
                player.SubtractResources(unit.Cost);
                queuedUnits++;
            }
    }

    [Server]
    private void UpdateUnitProduction()
    {
        if (queuedUnits == 0)
            return;

        timer += Time.deltaTime;

        if (timer < unit.CreationTime)
            return;

        ProduceUnit();

        timer = 0;
    }

    [Server]
    private void ProduceUnit()
    {
        queuedUnits--;

        Unit unitInstance = Instantiate(
            unit,
            spawnPoint.position,
            spawnPoint.rotation);

        NetworkServer.Spawn(unitInstance.gameObject, connectionToClient);

        if (gatherPointSet)
            unitInstance.Movement.Move(gatherPoint);
    }

    [Command]
    public void SetGatherPoint(Vector3 position)
    {
        gatherPointSet = true;
        gatherPoint = position;
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasAuthority)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
            CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsChanged(int oldValue, int newValue)
    {
        remainingUnitsText.text = queuedUnits.ToString();

        if (queuedUnits == 0)
            queueIndicatorParentObject.SetActive(false);
        else if (oldValue == 0)
            queueIndicatorParentObject.SetActive(true);
    }

    private void ClientHandleTimerChanged(float oldValue, float newValue)
    {
        float newProgress = timer / unit.CreationTime;

        if (newProgress < progressImage.fillAmount)
        {
            progressImage.fillAmount = newProgress;
        }
        else
        {
            progressImage.fillAmount = Mathf.SmoothDamp(
                progressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f);
        }
    }

    #endregion
}
