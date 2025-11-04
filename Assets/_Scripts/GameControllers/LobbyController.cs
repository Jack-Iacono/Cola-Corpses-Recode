using UnityEngine;

public class LobbyController : MonoBehaviour
{
    private void Start()
    {
        PlayerController.movementController.Warp(Vector3.up * 1000);
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
