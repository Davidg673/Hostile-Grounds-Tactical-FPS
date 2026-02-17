using UnityEngine;

public class CameraController : MonoBehaviour
{

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    CharacterController characterController;
    Vector3 targetPosition;
    Vector3 basePosition;
    [SerializeField] GameObject weaponHolder;
    float smoothX = 0.2f;
    float smoothY = 0.05f;
    float velX = 0f, velY = 0f;
    public static bool canRun = true;
    public float lookSpeedY = 2f;
    public float baseLookSpeedY = 2f;
    public float lookXLimit = 45f;
    private float rotationX = 0;

    void Start()
    {
        characterController = GetComponentInParent<CharacterController>();
        basePosition = transform.localPosition;
        baseLookSpeedY = lookSpeedY;

        MenuManager.onChangeData+=LoadDataFromSaved;
    }

    void OnDestroy()
    {
        canRun=true;

        MenuManager.onChangeData-=LoadDataFromSaved;
    }

    void Update()
    {
        if (canRun) MoveCamera();

        if (!GameController.gameRunning) return;

        Vector3 currentPos = transform.localPosition;

        float mouseY = Input.GetAxisRaw("Mouse Y");
        if (mouseY < 0f)
        {
            targetRotation = Vector3.MoveTowards(targetRotation, Vector3.zero, Mathf.Abs(mouseY) * 10f);
        }
        //recoil return
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        transform.localRotation *= Quaternion.Euler(currentRotation);

        currentPos.x = Mathf.SmoothDamp(currentPos.x, targetPosition.x, ref velX, smoothX);
        currentPos.y = Mathf.SmoothDamp(currentPos.y, targetPosition.y, ref velY, smoothY);
        currentPos.z = Mathf.SmoothDamp(currentPos.z, targetPosition.z, ref velX, smoothX);

        transform.localPosition = currentPos;
        HandleCamMovement();
    }

    void LateUpdate()
    {
        //makes sure camera moves first, then the object to avoid visual stutters
        weaponHolder.transform.rotation = transform.rotation;

    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ)
    {
        targetRotation += new Vector3(-recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ)) * 2f;
    }

    private void MoveCamera()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
    private void HandleCamMovement()
    {
        //In Air Movement
        if (!characterController.isGrounded)
        {
            targetPosition.y = basePosition.y + 0.05f;
        }
        else
        {
            targetPosition.y = basePosition.y;

            if (Input.GetAxisRaw("Horizontal") > 0.05f)
            {
                targetPosition.y = basePosition.y + 0.01f;
                targetPosition.x = basePosition.x - 0.025f;
            }

            if (Input.GetAxisRaw("Horizontal") < -0.05f)
            {
                targetPosition.y = basePosition.y + 0.01f;
                targetPosition.x = basePosition.x + 0.025f;
            }

            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.05f)
            {
                targetPosition.z = basePosition.z + 0.05f;
                targetPosition.y = basePosition.y + 0.01f;
            }

            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.05f) targetPosition.z = basePosition.z;
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.05f) targetPosition.x = basePosition.x;
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.05f && Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.05f) targetPosition.y = basePosition.y;
        }
    }


    public void TiltCamera(float rotationForce, float randomFactor)
    {
        targetRotation += new Vector3(0f, 0f, rotationForce * Random.Range(-randomFactor, randomFactor));
    }

    private void LoadDataFromSaved()
    {
        baseLookSpeedY= PlayerPrefs.GetFloat("sensitivity");
        lookSpeedY=baseLookSpeedY;
    }

}
