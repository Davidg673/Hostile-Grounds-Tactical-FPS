using System;
using System.Collections.Generic;
using NL;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.UI;

public class WeaponHandler : MonoBehaviour
{
    public UnityAction onJumpPressed;
    [HideInInspector] public bool weaponUseHold;
    [HideInInspector] public bool weaponSecondaryUseHold;
    public List<GameObject> throwables = new List<GameObject>();
    public int maxThrowables = 3;
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject knife;
    public GameObject currentThrowable;
    public GameObject currentHeld;
    private GameObject previousHeld;
    [SerializeField] private bool hideCursor = true;
    public Transform raycastSource;

    public UnityAction OnWeaponUseStarted; //used to shoot
    public UnityAction OnWeaponUseFinished; //Used for grenades (long range throw)
    public UnityAction OnWeaponUseSecondaryStarted; //used for scopes / surpressors
    public UnityAction OnWeaponUseSecondaryFinished; //used for grenades (small range throw)

    public UnityAction OnWeaponReloadPressed;
    public static UnityAction OnWeaponInspectPressed;

    public static UnityAction OnBuyMenuOpened;
    public UnityAction OnWeaponSwitched;
    public static bool canRun = true;
    bool canClickMouseTwo=true;

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
        if (canRun) HandleInput();
    }

    void HandleInput()
    {

        if (Input.GetKeyDown(KeyCode.Space)) onJumpPressed?.Invoke();


        if (Input.GetKeyDown(KeyCode.Alpha1) && primaryWeapon != null)
        {
            if (currentHeld != primaryWeapon)
            {
                previousHeld = currentHeld;
                currentHeld = primaryWeapon;

                if (previousHeld != null) previousHeld.SetActive(false);
                currentHeld.SetActive(true);

                OnWeaponSwitched?.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && secondaryWeapon != null)
        {
            if (currentHeld != secondaryWeapon)
            {
                previousHeld = currentHeld;
                currentHeld = secondaryWeapon;

                if (previousHeld != null) previousHeld.SetActive(false);
                currentHeld.SetActive(true);

                OnWeaponSwitched?.Invoke();

            }

        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (currentHeld != knife)
            {
                previousHeld = currentHeld;
                currentHeld = knife;

                if (previousHeld != null) previousHeld.SetActive(false);
                currentHeld.SetActive(true);

                OnWeaponSwitched?.Invoke();

            }
        }


        if (Input.GetKeyDown(KeyCode.Alpha4) && currentThrowable != currentHeld)
        {
            if (throwables.Count > 0)
            {
                if (throwables.Count == 1)
                {
                    previousHeld = currentHeld;
                    currentHeld = currentThrowable;
                    if (previousHeld != null) previousHeld.SetActive(false);
                    currentHeld.SetActive(true);
                }

                else
                {
                    int index = throwables.IndexOf(currentThrowable);
                    int nextIndex = (index + 1) % throwables.Count;
                    currentThrowable = throwables[nextIndex];
                    previousHeld = currentHeld;
                    currentHeld = currentThrowable;
                    if (previousHeld != null) previousHeld.SetActive(false);
                    currentHeld.SetActive(true);
                }
                
                OnWeaponSwitched?.Invoke();
            }
        }


        if (Input.GetKeyDown(KeyCode.Q) && previousHeld != null)
        {
            GameObject tempHolder = previousHeld;
            previousHeld = currentHeld;
            currentHeld = tempHolder;


            previousHeld.SetActive(false);
            currentHeld.SetActive(true);
            
            OnWeaponSwitched?.Invoke();
        }


        weaponUseHold = Input.GetMouseButton(0);
        weaponSecondaryUseHold = Input.GetMouseButton(1);

        if (Input.GetMouseButtonDown(0))
        {
            if (hideCursor)
            {
                ToggleCursorState(true);
                OnWeaponUseStarted?.Invoke();
                OnWeaponSwitched?.Invoke();
            }

        }

        if (Input.GetMouseButtonDown(1))
        {
            if (hideCursor &&canClickMouseTwo)
            {
                ToggleCursorState(true);
                OnWeaponUseSecondaryStarted?.Invoke();
                canClickMouseTwo = false;
                Invoke(nameof(ResetMouseTwo),0.2f);
            }

        }

        if (Input.GetMouseButtonUp(1)) OnWeaponUseSecondaryFinished?.Invoke();

        if (Input.GetMouseButtonUp(0)) OnWeaponUseFinished?.Invoke();

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

        //Handle weapon drop
        if (Input.GetKeyDown(KeyCode.G) && currentHeld != knife)
        {
            DropWeapon();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleCursorState(false);
            canRun = false;
            BuyableMenuController.canRun = true;
            PlayerMovement.canMove = false;
            CameraController.canRun = false;
            OnBuyMenuOpened?.Invoke();
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

    public void RemoveFromThrowables(GameObject throwable)
    {
        if (throwables.Count > 0)
        {
            throwables.Remove(throwable);
            if (throwables.Count > 0) currentThrowable = throwables[throwables.Count - 1];//make sure current grenade changes
            else currentThrowable = null;
        }
        else return;

        currentHeld.SetActive(false);

        if (primaryWeapon != null)  //switch to rifle
        {
            currentHeld = primaryWeapon;
            if (previousHeld == primaryWeapon)   //if player was already holding a rifle
            {
                if (secondaryWeapon != null) previousHeld = secondaryWeapon; //switch to pistol on 'Q' if available
                else previousHeld = knife; //default to knife on 'Q'
            }
        }
        else
        {
            if (secondaryWeapon != null) //same logic as above but for secondary
            {
                currentHeld = secondaryWeapon;
                if (previousHeld == secondaryWeapon) previousHeld = knife; //default to knife on 'Q'
            }
            else //if only knife in hands
            {
                currentHeld = knife;
                if (previousHeld == knife) //if no weapons and still some grenades left but previous was already knife
                {
                    if (throwables.Count > 0)
                    {
                        previousHeld = throwables[0];
                    }
                    else previousHeld = null; //no weapons and grenades left but the knife
                }
            }
        }
        currentHeld.SetActive(true);

    }

    public bool AddToThrowables(GameObject throwable)
    {

        if (throwables.Count < 3)
        {
            throwables.Add(throwable);
            currentThrowable = throwable;
            previousHeld = currentHeld;
            if (previousHeld != null) previousHeld.SetActive(false);
            currentHeld = throwable;
            currentHeld.SetActive(true);

            return true;
        }
        return false;
    }

    public bool PickUpWeapon(GameObject parentObj, int currentBulletsInStock, int currentBulletsInMag)
    {
        if (primaryWeapon == null || secondaryWeapon == null)
        {
            WeaponLogic tempScript = parentObj.GetComponent<WeaponLogic>();

            tempScript.currentBulletsInMag = currentBulletsInMag;
            tempScript.currentBulletsInStock = currentBulletsInStock;

            if (primaryWeapon == null) primaryWeapon = parentObj;
            else secondaryWeapon = parentObj;

            previousHeld = currentHeld;
            currentHeld = parentObj;
            if (previousHeld != null) previousHeld.SetActive(false);

            parentObj.SetActive(true);
            return true;
        }
        return false;
    }

    private void DropWeapon()
    {
        if (throwables.Contains(currentHeld)) //handle grenade drop
        {
            ThrowableBase grenadeScript = currentHeld.GetComponent<ThrowableBase>();

            GameObject grenadeInstance = Instantiate(grenadeScript.droppedGrenadePrefab, raycastSource.position, Quaternion.identity);
            Rigidbody rb = grenadeInstance.GetComponent<Rigidbody>();

            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
            rb.AddForce(Camera.main.transform.forward * 20f, ForceMode.Impulse);

            grenadeInstance.SetActive(true);

            RemoveFromThrowables(currentHeld);
            return;

        }
        else //Handle normal weapon
        {
            CreateWeaponInstance(currentHeld);
        }

        currentHeld.SetActive(false);  //Make sure the current weapon gets deactivated

        //////Handle switching weapons//////

        if (currentHeld == primaryWeapon) //case for dropping primary
        {

            if (secondaryWeapon != null) //switch to secondary
            {
                currentHeld = secondaryWeapon;
                if (previousHeld == secondaryWeapon) previousHeld = knife;  //player can switch using 'Q' to knife if secondary was already held
                //otherwise keep whatever the player was holding previously
            }
            else
            {
                currentHeld = knife; //if no secondary found, switch directly to knife
                if (previousHeld == knife)
                {
                    if (throwables.Count > 0)  //if the player was holding the knife and had remaining grenades, make the player able to switch to them on 'Q'
                    {
                        previousHeld = throwables[0];
                    }
                    else previousHeld = null;  //this means the player only has a knife in hand and cannot switch
                }
            }
            primaryWeapon = null;
        }

        else if (currentHeld == secondaryWeapon) //case for dropping secondary
        {
            if (primaryWeapon != null) //switch to primary
            {
                currentHeld = primaryWeapon;
                if (previousHeld == primaryWeapon) previousHeld = knife; //default to knife if previous was secondary already
            }
            else
            {
                currentHeld = knife;  //same as rifle logic above
                if (previousHeld == knife)
                {
                    if (throwables.Count > 0)
                    {
                        previousHeld = throwables[0];
                    }
                    else previousHeld = null;
                }

            }
            secondaryWeapon = null;
        }

        currentHeld.SetActive(true); //turn on the newly set current held weapon
    }

    public static void CloseBuyMenu()
    {
        PlayerMovement.canMove = true;
        canRun = true;
        CameraController.canRun = true;
        canRun = true;

    }

    public bool LoadNewWeapon(GameObject weapon)
    {
        Throwable grenadeScript = weapon.GetComponent<Throwable>();

        if (grenadeScript != null)
        {
            return AddToThrowables(weapon);
        }


        WeaponLogic weaponScript = weapon.GetComponent<WeaponLogic>();

        if (weaponScript.weaponType == WeaponLogic.Type.Primary)
        {
            if (primaryWeapon == null) //if no primary, just add the new weapon
            {
                primaryWeapon = weapon;
                currentHeld.SetActive(false);//reset the weapon for grab animation
                currentHeld = primaryWeapon;
                currentHeld.SetActive(true);
            }
            else
            {
                CreateWeaponInstance(primaryWeapon);
                currentHeld.SetActive(false); //switch to primary weapon
                primaryWeapon = weapon;
                currentHeld = primaryWeapon;
                if (currentHeld == weapon) Invoke(nameof(ResetPrimary), 0.1f); //if the new weapon matches the old weapon, reset animation
                else currentHeld.SetActive(true);
            }
        }

        if (weaponScript.weaponType == WeaponLogic.Type.Secondary)
        {
            if (secondaryWeapon == null)
            {
                secondaryWeapon = weapon;
                currentHeld.SetActive(false);
                currentHeld = secondaryWeapon;
                currentHeld.SetActive(true);
            }
            else
            {
                CreateWeaponInstance(secondaryWeapon);
                currentHeld.SetActive(false);
                secondaryWeapon = weapon;
                currentHeld = secondaryWeapon;
                if (currentHeld == weapon) Invoke(nameof(ResetSecondary), 0.1f);
                else currentHeld.SetActive(true);
            }
        }

        weaponScript.RefreshBulletData();


        return true;
    }

    void CreateWeaponInstance(GameObject weapon)
    {
        WeaponLogic parentScript = weapon.GetComponent<WeaponLogic>();
        GameObject tempObj = Instantiate(parentScript.weaponPrefab, raycastSource.position, Quaternion.identity);
        Rigidbody rb = tempObj.GetComponent<Rigidbody>();
        tempObj.SetActive(true);

        //Get script to store data about the parent weapon
        DroppedWeapon instanceScript = tempObj.GetComponent<DroppedWeapon>();

        int ammoInStock = parentScript.currentBulletsInStock;
        int ammoInMag = parentScript.currentBulletsInMag;

        instanceScript.SetVariables(ammoInStock, ammoInMag);

        //Add force
        rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        rb.AddForce(Camera.main.transform.forward * 5f, ForceMode.Impulse);
    }

    void ResetPrimary()
    {
        primaryWeapon.SetActive(true);
    }

    void ResetSecondary()
    {
        secondaryWeapon.SetActive(true);
    }

    void ResetMouseTwo()
    {
        canClickMouseTwo = true;
    }

}