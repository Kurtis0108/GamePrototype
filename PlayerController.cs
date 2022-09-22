using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private PlayerInput playerInput;
    [SerializeField]
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private Transform cameraTransform;
    [SerializeField]
    private GameObject _object;
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float rotationSpeed = .8f;
    [SerializeField]
    private float sprintMultiplier = 1.0f;
    [SerializeField]
    public float stamina = 100.0f;

    bool sprintCoroutineStarted = false;


    private float gravityValue = -18.81f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        cameraTransform = Camera.main.transform;
        
    }

    void Update()
    {
        playerMove();
       if (sprintAction.ReadValue<float>() > 0f && sprintCoroutineStarted == false){
            StartCoroutine("sprintStaminaDrain");
            sprintCoroutineStarted = true;
       }

    }

    IEnumerator sprintStaminaDrain()
{
    stamina = stamina - 10.0f;
    yield return new WaitForSeconds(1.0f);
    sprintCoroutineStarted = false;
}



    void playerMove(){
        //Check if player is grounded, handled by player controller
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        //Poll moveAction's Vector 2 for input
        Vector2 input = moveAction.ReadValue<Vector2>();


        //Poll sprint button
        float sprinting = sprintAction.ReadValue<float>();
        if (stamina > 0f){
             sprintMultiplier = 1.0f + 5.0f*sprinting;
        } 
       else {sprintMultiplier = 1.0f;}


        //Convert inputs 2 degree vector into Vector3
        Vector3 move = new Vector3(input.x, 0, input.y);

        //Convert direction dictated by input to be relative to camera        
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;

        //Move the player
        controller.Move(move * Time.deltaTime * playerSpeed * sprintMultiplier);
        

        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    

        // Changes the height position of the player..
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
