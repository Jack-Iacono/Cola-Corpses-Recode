using UnityEngine;

public class CoinController : MonoBehaviour
{
    private BoxCollider col;
    LayerMask groundMask = 1 << 0;

    private PlayerInventoryController playerInventoryController;

    private void Awake()
    {
        col = GetComponent<BoxCollider>();
        Activate();
    }

    private void Start()
    {
        playerInventoryController = PlayerController.Instance.inventoryController;
    }

    public void Activate()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100, groundMask))
        {
            transform.position = hit.point + (Vector3.up * (col.size.z / 2));
            transform.LookAt(transform.position + hit.normal);
        }

        gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        transform.position = new Vector3(0, -10, 0);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == PlayerController.Instance.movementController.gameObject)
        {
            bool success = playerInventoryController.ChangeCoins(AddCoins);

            if(success)
                Deactivate();

            int AddCoins(int old)
            {
                return old + 1;
            }
        }
    }
}
