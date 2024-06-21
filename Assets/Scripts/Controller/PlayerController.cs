using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the movement of the player with given input from the input manager
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Speed at witch player moves")]
    public float moveSpeed = 2f;
    [Tooltip("Speed at witch player rotates to look at right or left (calculeted in degree)")]
    public float lookSpeed = 60f;
    [Tooltip("Power with witch player jumps")]
    public float jumpPower = 8f;
    [Tooltip("The gravity force")]
    public float gravity = 9.81f;

    [Header("Jump Timing")]
    public float jumpTimeLeniency = 0.1f;
    float timeToStopBeingLenient = 0;

    [Header("Required References")]
    [Tooltip("The player shooter script that fires projectiles")]
    public Shooter playerShooter;
    public Health playerHealth;
    public List<GameObject> disableWhileDead;
    bool  doubleJumpAvailable = false;

    //The character controller componet on the player 
    private CharacterController controller;
    private InputManager inputManager;
    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Start()
    {
        SetUpCharacterController();
        SetUpInputManager();
    }


    private void SetUpCharacterController()
    {
        controller = GetComponent<CharacterController>();
        if(controller == null )
        {
            Debug.LogError("The player controller script does not have a controller on the same gameObject");
        }
    }

    private void SetUpInputManager()
    {
        inputManager = InputManager.instance;
    }
    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Update()
    {

        if(playerHealth.currentLives <= 0)
        {
            foreach(GameObject inGameObject in disableWhileDead)
            {
                inGameObject.SetActive(false);
            }
            return;
        }
        else
        {
            foreach (GameObject inGameObject in disableWhileDead)
            {
                inGameObject.SetActive(true);
            }
        }
        ProcessMovement();
        ProcessRotation();
    }

        Vector3 moveDirection;
    void ProcessMovement()
    {
        //Get the input from the input manager
        float leftRightInput = inputManager.horizontalMoveAxis;
        float forwardBackwardInput = inputManager.verticalMoveAxis;
        bool jumpPress = inputManager.jumpPressed;

        //handle the control of the player while it is on the ground
        if (controller.isGrounded)
        {

            doubleJumpAvailable = true;
            timeToStopBeingLenient = Time.time + jumpTimeLeniency;
            //set the movement direction to be the received input, set y to 0 since we are on the ground 
            moveDirection = new Vector3(leftRightInput, 0, forwardBackwardInput);
            //set the move direction in realtion to the transform 
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection = moveDirection * moveSpeed;

            if(jumpPress )
            {
                moveDirection.y = jumpPower;
            }
            else
            {
                moveDirection = new Vector3(leftRightInput * moveSpeed, moveDirection.y, forwardBackwardInput * moveSpeed);
                moveDirection = transform.TransformDirection(moveDirection);
                if (jumpPress && Time.time < timeToStopBeingLenient)
                {
                    moveDirection.y = jumpPower;
                }
               else if (jumpPress && doubleJumpAvailable)
                {
                    moveDirection.y = jumpPower;
                    doubleJumpAvailable = false;
                }
            }
        }
        moveDirection.y -= gravity*Time.deltaTime;

        if(controller.isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -0.3f;
        }
        controller.Move(moveDirection*Time.deltaTime);
    }

    void ProcessRotation()
    {
        float horizontalLookInput = inputManager.horizontalLookAxis;
        Vector3 playerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(playerRotation.x, playerRotation.y + horizontalLookInput * lookSpeed * Time.deltaTime, playerRotation.z));
    }
}
