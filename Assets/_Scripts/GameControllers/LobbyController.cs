using UnityEngine;

public class LobbyController : MonoBehaviour
{
    private void Start()
    {
        PlayerController.movementController.Warp(Vector3.up);
        PlayerController.statusController.ResetHealth();
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
