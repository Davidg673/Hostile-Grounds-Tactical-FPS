using UnityEngine;

public class ScopeScript : MonoBehaviour
{
    WeaponHandler weaponHandlerScript;
    CameraController cameraController;
    PlayerMovement playerMovement;
    [SerializeField] private GameObject scopeCanvas;
    private int zoomState = 0;
    private float currentFov = 60;
    private float targetFov = 60;
    [SerializeField] float zoomSpeed = 0.5f;


    void Awake()
    {
        weaponHandlerScript = GameObject.Find("Player").GetComponent<WeaponHandler>();
        cameraController = GameObject.Find("Player").GetComponentInChildren<CameraController>();
        playerMovement=GameObject.Find("Player").GetComponent<PlayerMovement>();

    }

   void OnEnable()
    {
        weaponHandlerScript.OnWeaponUseSecondaryStarted += ChangeZoomState;
        weaponHandlerScript.OnWeaponSwitched += ResetScope;
    }

    void OnDisable()
    {
        weaponHandlerScript.OnWeaponUseSecondaryStarted -= ChangeZoomState;
        weaponHandlerScript.OnWeaponSwitched -= ResetScope;
    }

    void Update()
    {
        HandleScopeMovement();
    }

    void ChangeZoomState()
    {
        WeaponLogic weaponScript = weaponHandlerScript.currentHeld.GetComponent<WeaponLogic>();
        
        if (weaponScript == null) return;

        if (weaponScript.fireMode == WeaponLogic.FireMode.Manual)
        {
            zoomState++;

            if (zoomState == 3)
            {
                ResetScope();
            }
            else if (zoomState==1)
            {
                scopeCanvas.SetActive(true);
                targetFov = 40f;
                cameraController.lookSpeedY = cameraController.baseLookSpeedY * 0.8f;
                playerMovement.lookSpeedX = playerMovement.baseLookSpeedX * 0.8f;
            }
            else
            {
                targetFov = 15f;
                cameraController.lookSpeedY = cameraController.baseLookSpeedY * 0.2f;
                playerMovement.lookSpeedX = playerMovement.baseLookSpeedX * 0.2f;
            }
        } 
    }


    void HandleScopeMovement()
    {
        if (Mathf.Abs(currentFov - targetFov) > 0.01f)
        {
            currentFov = Mathf.MoveTowards(currentFov, targetFov, zoomSpeed * Time.deltaTime);
            Camera.main.fieldOfView = currentFov;
        }
    }


    void ResetScope()
    {
        zoomState = 0;
        Camera.main.fieldOfView = 60f;
        scopeCanvas.SetActive(false);
        currentFov = 60f;
        targetFov = 60f;
        cameraController.lookSpeedY = cameraController.baseLookSpeedY;
        playerMovement.lookSpeedX = playerMovement.baseLookSpeedX;
    }

}
