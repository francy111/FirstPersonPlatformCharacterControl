using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [Header("Keybinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Movement")]
    public Vector3 move;
    public float speed;
    public float walkingSpeed;
    public float runningSpeed;

    [Header("Ground Check")]
    public bool grounded;

    [Header("Jumping & Gravity Control")]
    public float jumpHeight = 9;
    public bool secondJump = true;
    public float ySpeed = 0f; // Measured in m/s
    public float g = -15.7f;  // Gravity acceleration, measured in m/s^2

    void Start()
    {

    }

    void Update()
    {
        // We need to check if the player is on the ground or mid-air, and apply gravity in this case
        grounded = GroundCheck();
        if (grounded)
        {
            ySpeed = -2f; // Sweet spot, felt with heart <3
            secondJump = true;
        }
        else
            ySpeed += g * Time.deltaTime;  // v = a * t [m/s = m/s^2 * s]

        float horizontalMovement = Input.GetAxisRaw("Horizontal"); // With horizontal i mean A and D (or <- and ->)
        float verticalMovement = Input.GetAxisRaw("Vertical"); // With horizontal i mean W and S (or ^ and v)

        // Adjust the speed of the player depending if he's sprinting or not
        speed = Input.GetKey(sprintKey) ? runningSpeed : walkingSpeed;

        if (Input.GetKeyDown(jumpKey))
        {
            if (grounded) // If we are grounded we are performing the first jump
            {
                Jump();
            }
            else // We are mid-air, we are either performing the second jump from ground, or first jump mid-air
            {
                if (secondJump)
                {
                    secondJump = false;
                    Jump();
                }
            }
        }

        /*
         * Move the character in the direction dictated by the previous inputs
         *      with a magnitude of 'movementSpeed'
         *      Ofc the vector needs to be normalized, otherwise a diagonal movement would
         *      be faster than a vertical/horizontal one
         *      
         *      However we want to move in the direction the player is facing, not the 
         *      global 'x/z axis', so we would need to calculate the angle between the 
         *      'Z' and 'X' axis and the Y rotation, and rotate the final vector
         */
        move = transform.TransformDirection(
                new Vector3(horizontalMovement, 0f, verticalMovement).normalized * speed
            ) + Vector3.up * ySpeed;
        controller.Move(move * Time.deltaTime); // Automatically transforms the movement from being relative to the player to being absolute
        
    }

    private bool GroundCheck()
    {
        return controller.isGrounded;
    }

    private void Jump()
    {
        ySpeed = jumpHeight;
    }
}
