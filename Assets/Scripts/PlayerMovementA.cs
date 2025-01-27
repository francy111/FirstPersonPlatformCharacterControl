using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class PlayerMovementA : MonoBehaviour
{
    public enum status {grounded, midAir, steepSlope, stomping};

    public CharacterController controller;
    public Vector3 playerFeetPosition;
    public status playerStatus;
    public float slopeAngle;
    public float maxSlopeAngle;
    public float slopeAngleTolerance;
    public float groundDistance;
    public float baseGroundDistanceTolerance;
    public float groundDistanceTolerance;
    public float maxForwardSlopeDistance;
    public RaycastHit downSlopeHit;
    public Vector3[] offsets = new Vector3[5];

    public float hSpeed;
    public float baseHSpeed;
    public float boostHSpeed;
    public float maxHSpeed;
    public float acceleration;
    public float midAirSpeedMultiplier;
    public float vSpeed;
    public float g;
    public bool airJump;
    public float lastJump;
    public float jumpHeight;
    public bool isStomping;

    public Vector3 horizontalMovement;
    public Vector3 move;

    public void Start()
    {
        offsets[0] = Vector3.zero;
        offsets[1] = new Vector3(0.5f, 0.0f, 0.0f);
        offsets[2] = new Vector3(-0.5f, 0.0f, 0.0f);
        offsets[3] = new Vector3(0.0f, 0.0f, 0.5f);
        offsets[4] = new Vector3(0.0f, 0.0f, -0.5f);
    }

    public void Update()
    {
        if (GroundCheck()) // Se almeno 1 restituisce vero allora abbiamo colpito una superficie, potremmo essere per aria o per terra, dipende dalla distanza
        {
            slopeAngle = Vector3.Angle(Vector3.up, downSlopeHit.normal);
            groundDistance = downSlopeHit.distance;
            groundDistanceTolerance = baseGroundDistanceTolerance + Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * 0.5f;
            if (groundDistance > groundDistanceTolerance)
            {
                playerStatus = status.midAir;
            }
            else
            {
                if (Mathf.Abs(slopeAngle) <= maxSlopeAngle + slopeAngleTolerance)
                {
                    playerStatus = status.grounded;
                }
                else
                {
                    playerStatus = status.steepSlope;
                }
            }
        }
        else // Nessuna superficie e' stata colpita, siamo per aria
        {
            playerStatus = status.midAir;
        }

        // Input elaboration
        horizontalMovement = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")).normalized;
        horizontalMovement = transform.TransformDirection(horizontalMovement);
        controller.Move((horizontalMovement*10f + Vector3.down * 40f) * Time.deltaTime);
        // Player movement
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = controller.transform.position - Vector3.up * (controller.height / 2);
        Vector3 ray = Vector3.down * controller.height;
        foreach (Vector3 offset in offsets)
        {
            Debug.DrawLine(origin + offset, origin + ray + offset, Color.red);
        }
    }



    private bool GroundCheck()
    {
        playerFeetPosition = controller.transform.position - Vector3.up * (controller.height / 2);
        foreach (Vector3 offset in offsets)
        {
            if (Physics.Raycast(playerFeetPosition + offset, Vector3.down, out downSlopeHit, controller.height/2))
            {
                return true;
            }
        }
        return false;
    }
}
