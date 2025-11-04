using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    public static Dictionary<GameObject,  PopupController> Instances = new Dictionary<GameObject, PopupController>();

    private bool isAlive = false;

    [SerializeField]
    protected float lifetime = 1;
    protected float lifeTimer = 1;

    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private Image image;

    private Vector2 movementVector = Vector2.zero;

    private void Awake()
    {
        Instances.Add(gameObject, this);
    }

    private void Update()
    {
        if (isAlive)
        {
            if(lifeTimer > 0)
                lifeTimer -= Time.deltaTime;
            else
            {
                Deactivate();
            }

            transform.LookAt(PlayerController.movementController.transform);
            transform.position += ((transform.right * movementVector.x) + (Vector3.up * movementVector.y)) * Time.deltaTime;
            movementVector.y -= 20 * Time.deltaTime;
        }
    }

    public void Activate(Vector3 pos, string text, Color color)
    {
        this.image.gameObject.SetActive(false);
        this.text.gameObject.SetActive(true);

        this.text.text = text;
        this.text.color = color;

        Activate(pos);
    }
    public void Activate(Vector3 pos, Image image)
    {
        this.image.gameObject.SetActive(true);
        this.text.gameObject.SetActive(false);

        this.image = image;

        Activate(pos);
    }
    protected void Activate(Vector3 pos)
    {
        lifeTimer = lifetime;
        gameObject.SetActive(true);
        transform.position = pos;
        isAlive = true;

        transform.LookAt(PlayerController.movementController.transform);

        movementVector = new Vector2
            (
                Random.Range(-3, 3),
                Random.Range(7, 10)
            );
    }

    protected void Deactivate()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Instances.Remove(gameObject);
    }
}
