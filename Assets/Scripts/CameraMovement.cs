using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float rotationX; // Angolo di rotazione della telecamera (su/giu)
    public float rotationY; // Angolo di rotazione del player (e quindi della telecamera) [destra/sinistra]

    public float horizontalSensitivity; // Velocita' di rotazione orizzontale (sinistra/destra)
    public float verticalSensitivity;   // Velocita' di rotazione verticale (su/giu)

    public Transform firstPersonCamera;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Camera.main.fieldOfView = 100.0f;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * horizontalSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * verticalSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationY += mouseX;

        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Rotazione orizzontale (Player si ruota la visuale orizzontalmente, ruota il player sull'asse Y, la telecamera lo segue in automatico)
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // Rotazione verticale (Il player vuole guardare in alto/basso, ruotiamo solamente la telecamera attorno all'asse X)
        firstPersonCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
