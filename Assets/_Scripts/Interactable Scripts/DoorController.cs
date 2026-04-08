using MapUtil;
using UnityEngine;
using UnityEngine.AI;

public class DoorController : BasicInteractable
{
    [SerializeField] private MeshRenderer frontRenderer;
    [SerializeField] private MeshRenderer backRenderer;

    [SerializeField] private NavMeshObstacle obstacle;

    private Wall wall;

    private Material frontMaterial;
    private Material backMaterial;

    private int cost;
    private bool open = false;

    private PlayerInventoryController invCont;

    public void Initialize(int distance, Wall wall)
    {
        open = false;
        cost = (distance + 1) * 1;
        this.wall = wall;

        invCont = PlayerController.Instance.inventoryController;
    }
    public void SetMaterials(Material front, Material back)
    {
        frontMaterial = front;
        backMaterial = back;

        frontRenderer.material = frontMaterial;
        backRenderer.material = backMaterial;
    }

    public void Open()
    {
        if(invCont.coins >= cost)
        {
            obstacle.enabled = false;
            gameObject.SetActive(false);
            open = true;

            wall.OpenDoor();

            invCont.ChangeCoins(SubtractPrice);
        }

        int SubtractPrice(int old)
        {
            return old - cost;
        }
    }
}
