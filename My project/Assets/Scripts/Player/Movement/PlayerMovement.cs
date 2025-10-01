using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float slowWalkSpeed = 4f;
    public float normalWalkSpeed = 10f;
    private float currentSpeedMode = 10f;
    private float adjustedSpeed = 10f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float slowestSpeed = 1f;
    private bool jumped = false;
    private bool jumpSlow= false;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    private Coroutine jumpSlowCoroutine;

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

            currentSpeedMode = isSlowWalking ||jumpSlow ? slowWalkSpeed : normalWalkSpeed;  //make sure its not changed mid air
            if (jumped)
            {
                jumped = false;
                jumpSlow = true;
                
                if (jumpSlowCoroutine != null)
                {
                    StopCoroutine(jumpSlowCoroutine);
                }
                jumpSlowCoroutine= StartCoroutine(ResetJumpSlow());
                adjustedSpeed = Mathf.Max(adjustedSpeed * 0.75f, slowWalkSpeed);
            }

            adjustedSpeed = AccelerateTowards(adjustedSpeed, currentSpeedMode, 20f);
    

        }

        float curSpeedX = canMove ? adjustedSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? adjustedSpeed * Input.GetAxis("Horizontal") : 0;

        Debug.Log(curSpeedX + "/" + adjustedSpeed);

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {

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
            normalWalkSpeed = 10f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
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

}