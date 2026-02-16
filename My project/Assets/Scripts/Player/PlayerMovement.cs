using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public float lookSpeedX = 2f;
    public float baseLookSpeedX = 2f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 1f;
    public float slowestSpeed = 0.5f;
    private bool jumped = false;
    private bool jumpSlow = false; //slow down character when landing

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;

    public static bool canMove = true;

    private Coroutine jumpSlowCoroutine;
    public bool isCrouched;
    public bool isSlowWalking;


    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource sfxSourceFootsteps;
    [SerializeField] private AudioClip footstepSand;
    [SerializeField] private AudioClip footstepConcrete;
    [SerializeField] private AudioClip footstepMetal;
    [SerializeField] private AudioClip footstepWood;

    private AudioClip currentLandClip;
    [SerializeField] private AudioClip landClipSand;
    [SerializeField] private AudioClip landClipConcrete;
    [SerializeField] private AudioClip landClipMetal;
    [SerializeField] private AudioClip landClipWood;

    bool playSoundOnce = true;
    private Coroutine CheckMaterialRoutine;

    //Keybinds
    private KeyCode forwardKey = KeyCode.W;
    private KeyCode backKey = KeyCode.S;
    private KeyCode leftKey = KeyCode.A;
    private KeyCode rightKey = KeyCode.D;
    private KeyCode walkKey = KeyCode.LeftShift;
    private KeyCode jumpKey = KeyCode.Space;
    private KeyCode crouchKey = KeyCode.LeftControl;




    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentSpeedMode = normalWalkSpeed;
        adjustedSpeed = normalWalkSpeed;
        lookSpeedX = baseLookSpeedX;

        LoadDataFromSaved();
        
        MenuManager.onChangeData+=LoadDataFromSaved;
    }

    void OnDisable()
    {
        canMove=true;
        
        MenuManager.onChangeData-=LoadDataFromSaved;
    }

    void Update()
    {

        //Update ground material
        if (CheckMaterialRoutine == null) CheckMaterialRoutine = StartCoroutine(CheckGroundMaterial());

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        isSlowWalking = Input.GetKey(walkKey);
        bool isSuperSlowWalking = Input.GetKey(walkKey) && Input.GetKey(crouchKey);

        if (characterController.isGrounded) //make sure speed is not changed mid air
        {

            currentSpeedMode = isSlowWalking || jumpSlow ? slowWalkSpeed : normalWalkSpeed;  //slow down character based on Shift pressed or landing
            currentSpeedMode = isSuperSlowWalking ? slowestSpeed : currentSpeedMode; // if both control and shift are pressed, choose slowest speed
            currentSpeedMode = Input.GetKey(crouchKey) ? crouchSpeed : currentSpeedMode;

            if (jumped) //if landed from a jump
            {
                sfxSource.PlayOneShot(currentLandClip);
                playSoundOnce = false;
                sfxSourceFootsteps.Stop();
                Invoke(nameof(ResetLandCooldown), 0.8f);


                jumped = false;
                jumpSlow = true;

                if (jumpSlowCoroutine != null) StopCoroutine(jumpSlowCoroutine);
                jumpSlowCoroutine = StartCoroutine(ResetJumpSlow());

                adjustedSpeed = Mathf.Max(adjustedSpeed * 0.75f, slowWalkSpeed);  //returns largest of two so speed does not go below slow walk speed

            }

            adjustedSpeed = AccelerateTowards(adjustedSpeed, currentSpeedMode, 20f);


        }

        float inputX = 0f;
        float inputY = 0f;

        if (GameController.gameRunning && canMove)
        {
            if (Input.GetKey(forwardKey)) inputX = 1f;
            else if (Input.GetKey(backKey)) inputX = -1f;

            if (Input.GetKey(rightKey)) inputY = 1f;
            else if (Input.GetKey(leftKey)) inputY = -1f;
        }

        Vector3 move = (forward * inputX) + (right * inputY);

        if (move.magnitude > 1f)
        {
            move.Normalize();
            normalizedSpeed = move.magnitude;
        }

        ///Handle Footstep sound
        if (move.magnitude> 0.7 && playSoundOnce && characterController.isGrounded && !isSlowWalking && !isCrouched)
        {
            sfxSourceFootsteps.Play();
            playSoundOnce = false;

        }
        else if (move.magnitude<0.7 || !characterController.isGrounded || isCrouched || isSlowWalking)
        {
            sfxSourceFootsteps.Stop();
            playSoundOnce = true;
        }

        float movementDirectionY = moveDirection.y;
        moveDirection = move * adjustedSpeed;



        if (Input.GetKey(jumpKey) && canMove && characterController.isGrounded && GameController.gameRunning)
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

        if (Input.GetKey(crouchKey) && canMove)
        {
            isCrouched = true;
            characterController.height = crouchHeight;
        }
        else
        {
            isCrouched = false;
            characterController.height = defaultHeight;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
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

    private IEnumerator CheckGroundMaterial()
    {
        RaycastHit hit;
        float newPitch = 1.5f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {

            if (hit.collider.gameObject.tag == "Sand")
            {
                sfxSourceFootsteps.clip = footstepSand;
                newPitch = 1.8f;
                currentLandClip = landClipSand;
            }
            if (hit.collider.gameObject.tag == "Concrete")
            {
                sfxSourceFootsteps.clip = footstepConcrete;
                newPitch = 1.3f;
                currentLandClip = landClipConcrete;

            }
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wood"))
            {
                sfxSourceFootsteps.clip = footstepWood;
                newPitch = 1.8f;
                currentLandClip = landClipWood;
            }
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Metal"))
            {
                sfxSourceFootsteps.clip = footstepWood;
                newPitch = 1.8f;
                currentLandClip = landClipMetal;
            }
        }

        if (sfxSourceFootsteps.pitch != newPitch)
        {
            playSoundOnce = true;
            sfxSourceFootsteps.pitch = newPitch;
        }

        yield return new WaitForSeconds(0.5f);
        CheckMaterialRoutine = null;
    }

    private void ResetLandCooldown()
    {
        playSoundOnce = true;
    }

    private void LoadDataFromSaved()
    {
        forwardKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("moveForward"));
        backKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("moveBack"));
        leftKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("moveLeft"));
        rightKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("moveRight"));
        walkKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("walk"));
        jumpKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("jump"));
        crouchKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("crouch"));

        baseLookSpeedX= PlayerPrefs.GetFloat("sensitivity");
        lookSpeedX=baseLookSpeedX;
    }

}