using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    private GameObject currentPlayer;

    [SerializeField]
    private PrefabHandler prefabHandler;

    public PlayerMovementController movementController { get; private set; }
    public PlayerWeaponController weaponController { get; private set; }
    public PlayerStatusController statusController { get; private set; }
    public PlayerCameraController cameraController { get; private set; }
    public PlayerInteractionController interactionController { get; private set; }
    public PlayerInventoryController inventoryController { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            currentPlayer = Instantiate(prefabHandler.player);

            movementController = currentPlayer.GetComponent<PlayerMovementController>();
            weaponController = currentPlayer.GetComponent<PlayerWeaponController>();
            statusController = currentPlayer.GetComponent<PlayerStatusController>();
            interactionController = currentPlayer.GetComponent<PlayerInteractionController>();
            inventoryController = currentPlayer.GetComponent<PlayerInventoryController>();
            cameraController = movementController.camCont;
        }
        else
            Destroy(this);
    }
}
