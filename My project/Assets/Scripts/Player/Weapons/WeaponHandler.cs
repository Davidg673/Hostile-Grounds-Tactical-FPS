using Unity.VisualScripting;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class WeaponHandler : MonoBehaviour
{
    public UnityAction onJumpPressed;
    [HideInInspector] public bool weaponUseHold;
    [HideInInspector] public bool weaponSecondaryUseHold;
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject knife;
    private GameObject currentHeld;
    private GameObject previousHeld;
    [SerializeField] private bool hideCursor = true;
    public Transform raycastSource;

    public UnityAction OnWeaponUseStarted; //used to shoot
    public UnityAction OnWeaponUseFinished; //Used for grenades (long range throw)
    public UnityAction OnWeaponUseSecondaryStarted; //used for scopes / surpressors
    public UnityAction OnWeaponUseSecondaryFinished; //used for grenades (small range throw)

    public UnityAction OnWeaponReloadPressed;
    public static UnityAction OnWeaponInspectPressed;



    void Start()
    {
        if (hideCursor)
            ToggleCursorState(true);
        currentHeld = knife;
        previousHeld = secondaryWeapon;
        currentHeld.SetActive(true);

    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {

        if (Input.GetKeyDown(KeyCode.Space)) onJumpPressed?.Invoke();


        if (Input.GetKeyDown(KeyCode.Alpha1) && primaryWeapon!=null)
        {
            if (currentHeld != primaryWeapon)
            {
                previousHeld = currentHeld;
                currentHeld = primaryWeapon;

                if (previousHeld!=null) previousHeld.SetActive(false);
                currentHeld.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && secondaryWeapon!=null)
        {
            if (currentHeld != secondaryWeapon)
            {
                previousHeld = currentHeld;
                currentHeld = secondaryWeapon;

                if (previousHeld!=null) previousHeld.SetActive(false);
                currentHeld.SetActive(true);
            
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (currentHeld != knife)
            {
                previousHeld = currentHeld;
                currentHeld = knife;

                if (previousHeld!=null) previousHeld.SetActive(false);
                currentHeld.SetActive(true);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameObject tempHolder = previousHeld;
            previousHeld = currentHeld;
            currentHeld = tempHolder;

          
            if (previousHeld!=null) previousHeld.SetActive(false);
            currentHeld.SetActive(true);
        
        }


        weaponUseHold = Input.GetMouseButton(0);
        weaponSecondaryUseHold = Input.GetMouseButton(1);

        if (Input.GetMouseButtonDown(0))
        {
            if (hideCursor)
            {
                ToggleCursorState(true);
                OnWeaponUseStarted?.Invoke();
            }

        }

        if (Input.GetMouseButton(1))
        {
            if (hideCursor)
            {
                ToggleCursorState(true);
                OnWeaponUseSecondaryStarted?.Invoke();
            }

        }

        if (Input.GetMouseButtonUp(1)) OnWeaponUseSecondaryFinished?.Invoke();

        if (Input.GetMouseButtonUp(0)) OnWeaponUseSecondaryFinished?.Invoke();

        if (Input.GetKeyDown(KeyCode.R)) OnWeaponReloadPressed?.Invoke();

        if (Input.GetKeyDown(KeyCode.F)) OnWeaponInspectPressed?.Invoke();

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            // TODO: Implement Scroll menu behaviour up
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            // TODO: Implement Scroll menu behaviour down
        }



    }

    /// <summary>
    /// Toggles the cursor lock state and visibility.
    /// </summary>
    /// <param name="locked">Set to true to lock and hide the cursor, false to unlock and show it.</param>
    public static void ToggleCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

}
