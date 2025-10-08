using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float slowWalkSpeed = 2f;
    public float normalWalkSpeed = 6f;
    private float currentSpeedMode = 6f;
    private float adjustedSpeed = 6f;
    public float normalizedSpeed;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 1f;
    public float slowestSpeed = 0.5f;
    private bool jumped = false;
    private bool jumpSlow = false;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    private Coroutine jumpSlowCoroutine;
    private Coroutine moveCameraOnJumpCoroutine;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isSlowWalking = Input.GetKey(KeyCode.LeftShift);


        if (characterController.isGrounded)
        {

            currentSpeedMode = isSlowWalking || jumpSlow ? slowWalkSpeed : normalWalkSpeed;  //make sure its not changed mid air
            if (jumped)
            {
                jumped = false;
                jumpSlow = true;

                if (jumpSlowCoroutine != null)
                {
                    StopCoroutine(jumpSlowCoroutine);
                }
                jumpSlowCoroutine = StartCoroutine(ResetJumpSlow());
                adjustedSpeed = Mathf.Max(adjustedSpeed * 0.75f, slowWalkSpeed);
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
            characterController.height = crouchHeight;

            if (characterController.isGrounded)
            {
                slowWalkSpeed = slowestSpeed;
                normalWalkSpeed = crouchSpeed;
            }

        }
        else
        {
            characterController.height = defaultHeight;
            slowWalkSpeed = 4f;
            normalWalkSpeed = 6f;
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
        if (characterController.isGrounded)
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
        moveCameraOnJumpCoroutine=null;

    }

}