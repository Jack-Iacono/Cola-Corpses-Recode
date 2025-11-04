using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private static GameObject currentPlayer;

    [SerializeField]
    private PrefabHandler prefabHandler;

    public static PlayerMovementController movementController { get; private set; }
    public static PlayerWeaponController weaponController { get; private set; }
    public static PlayerStatusController statusController { get; private set; }
    public static PlayerCameraController cameraController { get; private set; }
    public static PlayerInteractionController interactionController { get; private set; }

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
            cameraController = movementController.camCont;
        }
        else
            Destroy(this);
    }
}
