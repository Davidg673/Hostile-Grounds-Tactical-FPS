using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float slowWalkSpeed;
    public float normalWalkSpeed;
    private float currentSpeedMode;
    private float adjustedSpeed;
    public float normalizedSpeed;
    public float jumpPower;
    public float gravity;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 1f;
    public float slowestSpeed = 0.5f;
    private bool jumped = false;
    private bool jumpSlow = false; //slow down character when landing

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    private Coroutine jumpSlowCoroutine;
    private Coroutine moveCameraOnJumpCoroutine;
    public bool isCrouched;
    public bool isSlowWalking;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentSpeedMode = normalWalkSpeed;
        adjustedSpeed = normalWalkSpeed;
    }

    void Update()
    {   
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        isSlowWalking = Input.GetKey(KeyCode.LeftShift);
        bool isSuperSlowWalking = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl);

        if (characterController.isGrounded) //make sure speed is not changed mid air
        {

            currentSpeedMode = isSlowWalking || jumpSlow ? slowWalkSpeed : normalWalkSpeed;  //slow down character based on Shift pressed or landing
            currentSpeedMode = isSuperSlowWalking ? slowestSpeed : currentSpeedMode; // if both control and shift are pressed, choose slowest speed
            currentSpeedMode = Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : currentSpeedMode; 

            if (jumped) //if landed from a jump
            {
                jumped = false;
                jumpSlow = true;

                if (jumpSlowCoroutine != null) StopCoroutine(jumpSlowCoroutine);

                jumpSlowCoroutine = StartCoroutine(ResetJumpSlow());
                adjustedSpeed = Mathf.Max(adjustedSpeed * 0.75f, slowWalkSpeed);  //returns largest of two so speed does not go below slow walk speed
            }

            adjustedSpeed = AccelerateTowards(adjustedSpeed, currentSpeedMode, 20f);


        }
        float inputX = canMove ? Input.GetAxis("Vertical") : 0;
        float inputY = canMove ? Input.GetAxis("Horizontal") : 0;

        Vector3 move = (forward * inputX) + (right * inputY);

        if (move.magnitude > 1f)
        {
            move.Normalize();
            normalizedSpeed = move.magnitude;

        }



        float movementDirectionY = moveDirection.y;
        moveDirection = move * adjustedSpeed;



        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            if (moveCameraOnJumpCoroutine == null) StartCoroutine(MoveCameraOnJump());
            jumped = true;
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            isCrouched = true;
            characterController.height = crouchHeight;
            //TODO: Smoothing to be added

        }
        else
        {
            isCrouched = false;
            characterController.height = defaultHeight;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        GetNormalizedSpeed();

    }

    private void GetNormalizedSpeed()
    {
        if (characterController.isGrounded && !isCrouched && !isSlowWalking)
            normalizedSpeed = Mathf.InverseLerp(0, adjustedSpeed, Mathf.Clamp(characterController.velocity.magnitude, 0, adjustedSpeed));
        else
            normalizedSpeed = 0f;
    }

    float AccelerateTowards(float current, float target, float accelRate)
    {
        return Mathf.MoveTowards(current, target, accelRate * Time.deltaTime);
    }

    private IEnumerator ResetJumpSlow()
    {
        yield return new WaitForSeconds(0.5f);
        jumpSlow = false;
    }

    private IEnumerator MoveCameraOnJump()
    {
        //Handle camera jump logic here
        yield return null;
        moveCameraOnJumpCoroutine = null;

    }


}