using UnityEngine;

public class LobbyController : MonoBehaviour
{
    private void Start()
    {
        PlayerController.Instance.movementController.Warp(Vector3.up);
        PlayerController.Instance.statusController.ResetHealth();
    }

    public void StartTestGame()
    {
        SceneController.LoadGameTestScene();
    }
    public void StartGame()
    {
        SceneController.LoadGameScene();
    }
}
