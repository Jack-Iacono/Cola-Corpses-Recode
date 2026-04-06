using UnityEngine;
using UnityEngine.AI;

public class DoorController : BasicInteractable
{
    [SerializeField] private MeshRenderer frontRenderer;
    [SerializeField] private MeshRenderer backRenderer;

    [SerializeField] private NavMeshObstacle obstacle;

    private Material frontMaterial;
    private Material backMaterial;

    private int cost;
    private bool open = false;

    public void Initialize(int distance)
    {
        open = false;
        cost = (distance + 1) * 10;
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
        // Add cost checking here
        obstacle.enabled = false;
        gameObject.SetActive(false);
        open = true;
    }
}
