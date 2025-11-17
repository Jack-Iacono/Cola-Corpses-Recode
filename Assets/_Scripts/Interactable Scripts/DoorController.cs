using UnityEngine;
using UnityEngine.AI;

public class DoorController : BasicInteractable
{
    [SerializeField] private MeshRenderer frontRenderer;
    [SerializeField] private MeshRenderer backRenderer;

    [SerializeField] private NavMeshObstacle obstacle;

    private Material frontMaterial;
    private Material backMaterial;

    private bool go = false;

    private int cost;
    private bool open = false;

    public void SetMaterials(Material front, Material back)
    {
        frontMaterial = front;
        backMaterial = back;

        frontRenderer.material = frontMaterial;
        backRenderer.material = backMaterial;
    }

    public void Open()
    {
        obstacle.enabled = false;
        gameObject.SetActive(false);
        open = true;
    }
}
