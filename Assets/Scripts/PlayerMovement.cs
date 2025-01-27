using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum status { grounded, midAir, steepSlope };

    public CharacterController controller;
    public new Transform camera;
    public Vector3 playerHeadPosition;
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

    public float hSpeed;
    public float baseHSpeed;
    public float crouchHSpeed;
    public float boostHSpeed;
    public float maxHSpeed;
    public float minimumSlideSpeed;
    public float acceleration;
    public float midAirSpeedMultiplier;
    public float steepSlopeSpeedMultiplier;
    public float vSpeed;
    public float vAcceleration;
    public float g;
    public float stompingAcceleration;
    public float slidingDeceleration;
    public bool airJump;
    public float lastJump;
    public float jumpHeight;
    public bool isStomping;
    public bool isJumping;
    public bool isCrouching;
    public bool isSliding;
    public float minJumpTime;
    public float groundMinJumpTime;
    public float slopeMinJumpTime;
    public float checkRadius;
    public float checkDistance;

    public float standingHeight;
    public Vector3 standingCenter;
    public float crouchingHeight;
    public Vector3 crouchingCenter;
    public float slidingHeight;
    public Vector3 slidingCenter;

    public Vector3 horizontalMovement;
    public Vector3 move;
    public Vector3 debug;

    public void Start()
    {
        hSpeed = baseHSpeed;

        vAcceleration = g;
        checkRadius = controller.radius;
        checkDistance = controller.height / 2;

        standingHeight = controller.height;
        standingCenter = Vector3.zero;

        crouchingHeight = standingHeight * 0.75f;
        crouchingCenter = Vector3.down * 0.25f;

        slidingHeight = standingHeight * 0.5f;
        slidingCenter = Vector3.down * 0.5f;
    }

    public void Update()
    {
        /* * * * * * * * * * * * * * * Status Update* * * * * * * * * * * * * * * * * * * * *\
         *                                                                                  *
         * We cast an infinite ray downwards, starting from the players feet, the distance  *
         * from the feet to the surface hit, as well as the angle of the surface, are used  *
         * to determine if the player is [1] Grounded, [2] Mid air or on a [3] Steep Slope  *
         *                                                                                  *
         * We also cast a ray, still from the feet, onwards, in the direction the player is *
         * walking, to determine if he's going to step over something (going up a slope)    *
         * To do that, we need the direction the player is walking/has last walked          *
         *                                                                                  *
         \* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        if (GroundCheck())
        {
            slopeAngle = Vector3.Angle(Vector3.up, downSlopeHit.normal);
            groundDistance = downSlopeHit.distance;
            groundDistanceTolerance = baseGroundDistanceTolerance + (Mathf.Tan(slopeAngle * Mathf.Deg2Rad) / 3.0f);

            if (groundDistance > groundDistanceTolerance)
            {
                playerStatus = status.midAir;
            }
            else
            {
                if (slopeAngle <= maxSlopeAngle + slopeAngleTolerance)
                {
                    playerStatus = status.grounded;
                }
                else
                {
                    playerStatus = status.steepSlope;
                }
            }
        }
        else
        {
            playerStatus = status.midAir;
        }

        /* * * * * * * * * * * * * * * Input elaboration* * * * * * * * * * * * * * * * * * *\
         *                                                                                  *
         * First we check for input that can be done independently from the status, such as *
         * WASD movement, then, depending on status, we check for status dependent input    *
         *                                                                                  *
         \* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        horizontalMovement = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")).normalized;
        
        /* Transform the direction from local to global
         * This means the direction is now relative to where the player is looking.
         * In other words, the movement is now relative to the player's X and Z axis
         * and not the world (global) X and Z
         */
        horizontalMovement = transform.TransformDirection(horizontalMovement);

        /* For every state the player can be in there will be
         * different abilities he will be able to perform
         */
        switch (playerStatus)
        {

            case status.grounded:

                minJumpTime = groundMinJumpTime;

                /* We need to reset the player's vertical speed in order to have a reliable fall
                 * from platforms (we could 'farm' vertical speed to instantly reach the ground)
                 * However we do not want to reset it right after jumping (doing so would nullify it)
                 * 
                 */
                if (Time.time > lastJump + minJumpTime)
                {
                    isJumping = false;
                    airJump = true;
                    vSpeed = 0;
                }

                /* Stomping ends when hitting a ground, so we flag the player as no longer stomping
                 * resetting his vertical speed and the vertical acceleration as only g
                 * 
                 */
                if (isStomping)
                {
                    isStomping = false;
                    vSpeed = 0;
                    vAcceleration = g;
                }

                /* To make a double jump possible, we need a "first" jump, this
                 * is going to be performed from the ground
                 */
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (isCrouching) // Decrouch if necessary
                    {
                        StandUp();
                        isCrouching = false;
                    }
                    isJumping = true;
                    lastJump = Time.time;
                    vSpeed = jumpHeight;
                }

                /* Crouching means just moving a little slower and with a lower camera
                 * Sliding means continue moving with a lower camera and gradually losing speed
                 * Depending on the current speed either one or the other will be performed
                 */
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (!isJumping)
                    {
                        if (hSpeed >= minimumSlideSpeed)
                        {
                            if (horizontalMovement.magnitude > 0)
                            {
                                isCrouching = false;
                                isSliding = true;
                                Slide();
                                hSpeed += slidingDeceleration * Time.deltaTime;
                            }
                            else
                            {
                                isSliding = false;
                                isCrouching = true;
                                Crouch();
                            }
                        }
                        else
                        {
                            isSliding = false;
                            isCrouching = true;
                            Crouch();
                        }
                    }
                    else
                    {
                        isSliding = false;
                        isCrouching = false;
                        StandUp();
                    }
                }
                else
                {
                    isSliding = false;
                    isCrouching = false;
                    StandUp();
                }

                /* Boosting means gaining momentainly a very high speed
                 * in the direction we are moving
                 */
                if (Input.GetKeyDown(KeyCode.LeftShift)) { }

                /* We rotate the directional movement versor to be parallel to the slope we are walking on
                 * in the direction we are moving
                 */
                horizontalMovement = Vector3.ProjectOnPlane(horizontalMovement, downSlopeHit.normal).normalized;
                move = horizontalMovement * hSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) + Vector3.up * vSpeed;
                break;

            case status.midAir:

                /* While mid air we continuously apply gravity
                 * v = a * <d>t 
                 */
                vSpeed += vAcceleration * Time.deltaTime;

                /* To make a double jump possible, we give the opportunity to jump
                 * once from the ground, and once mid air, as well as on slopes
                 */
                if (Input.GetKeyDown(KeyCode.Space)) 
                {
                    if (airJump)
                    {
                        isJumping = true;
                        airJump = false;
                        lastJump = Time.time;
                        vSpeed = jumpHeight;
                        if (isStomping) //Interrupt stomp if needed
                        {
                            isStomping = false;
                            vAcceleration = g;
                        }
                    }
                }

                /* Stomping means interrupting directional input and 
                 * falling to the ground below us, not only under the effect
                 * of gravity but another acceleration as well
                 */
                if (Input.GetKeyDown(KeyCode.LeftControl)) 
                {
                    isStomping = true;
                    if (isJumping) // Interrupt jump if needed
                    { 
                        isJumping = false; 
                        vSpeed = 0; 
                    }
                    vAcceleration = g + stompingAcceleration;
                }

                /* Boosting means gaining momentainly a very high speed
                 * in the direction we are moving
                 */
                if (Input.GetKeyDown(KeyCode.LeftShift)) { }

                move = (Vector3.up * vSpeed) + (isStomping ? Vector3.zero : horizontalMovement * hSpeed * midAirSpeedMultiplier);
                break;

            case status.steepSlope:

                minJumpTime = slopeMinJumpTime;

                /* When jumping into a steep slope, we don't want the player to keep his positive
                 * vertical velocity, so, if he is jumping INTO the slope (time check below, we don't
                 * want to reset it when jumping from the slope) he gets the vertical speed reset to 0
                 * so he starts to slide down
                 */
                if (isJumping && Time.time > lastJump + minJumpTime)
                {
                    isJumping = false;
                    vSpeed = 0;
                }

                /* While on a steep slope we want the player to continuously slide it
                 * We use the same method as in mid-air, however, not all acceleration is making
                 * the player go down, as he slides on the slope, we only consider the parallel
                 * component of g with respect to the slope
                 */
                vSpeed += vAcceleration * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;

                /* To make a double jump possible, we give the opportunity to jump
                 * once from the ground, and once mid air, as well as on slopes
                 */
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (airJump)
                    {
                        isJumping = true;
                        airJump = false;
                        lastJump = Time.time;
                        vSpeed = jumpHeight;
                        if (isStomping)// Interrupt stomp if needed
                        {
                            isStomping = false;
                            vAcceleration = g;
                        }
                    }
                }

                /* Stomping means interrupting directional input and 
                 * falling to the ground below us, not only under the effect
                 * of gravity but another acceleration as well
                 */
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    isStomping = true;
                    if (isJumping) // Interrupt jump if needed
                    {
                        isJumping = false;
                        vSpeed = 0;
                    }
                    vAcceleration = g + stompingAcceleration;
                }

                horizontalMovement = Vector3.ProjectOnPlane(horizontalMovement, downSlopeHit.normal).normalized;
                move = ((isJumping ? Vector3.up : Vector3.ProjectOnPlane(Vector3.up, downSlopeHit.normal).normalized) * vSpeed) 
                    + (isStomping ? Vector3.zero : horizontalMovement * hSpeed * steepSlopeSpeedMultiplier);
                break;

            default:
                Debug.Log("Kinda biggy error :)");
                break;
        }

        /* * * * * * * * * * * * * * * Applying movement* * * * * * * * * * * * * * * * * * *\
         *                                                                                  *
         * After calculating the movement direction and intensity we apply it to the player *
         *                                                                                  *
         \* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        controller.Move(move * Time.deltaTime);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(playerHeadPosition, checkRadius);
        Gizmos.DrawSphere(playerFeetPosition, checkRadius);
        Gizmos.DrawSphere(playerFeetPosition + Vector3.down * checkDistance, checkRadius);
    }

    private bool GroundCheck()
    {
        playerHeadPosition = controller.transform.position + Vector3.up * (checkDistance - checkRadius);
        playerFeetPosition = controller.transform.position + Vector3.down * (checkDistance - checkRadius);
        return Physics.CapsuleCast(playerHeadPosition, playerFeetPosition, checkRadius, Vector3.down, out downSlopeHit, checkDistance);
    }

    private void StandUp()
    {
        controller.height = standingHeight;
        controller.center = standingCenter;
        camera.localPosition = Vector3.up * 0.5f;
        hSpeed = baseHSpeed;
    }

    private void Crouch()
    {
        controller.height = crouchingHeight;
        controller.center = crouchingCenter;
        camera.localPosition = Vector3.up * 0.25f;
        hSpeed = crouchHSpeed;
    }

    private void Slide()
    {
        controller.height = slidingHeight;
        controller.center = slidingCenter;
        camera.localPosition = Vector3.zero;
    }
}
