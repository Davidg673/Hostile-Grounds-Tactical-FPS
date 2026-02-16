using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class WeaponHandler : MonoBehaviour
{
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
    bool canClickMouseTwo = true;
    public static bool buyTimeout;
    public static bool inBuyZone;
    [SerializeField] GameObject uspInstance;
    [SerializeField] GameObject glockInstance;

    //Keybinds
    KeyCode buyMenuKey = KeyCode.B;
    KeyCode fireKey = KeyCode.Mouse1;
    KeyCode reloadKey = KeyCode.R;
    KeyCode swithKey = KeyCode.Q;
    KeyCode dropKey = KeyCode.G;


    void Start()
    {
        LoadDataFromSaved();

        MenuManager.onChangeData+=LoadDataFromSaved;
    }

    void OnDestroy()
    {
        MenuManager.onChangeData-=LoadDataFromSaved;
        canRun=true;
        buyTimeout=false;
        inBuyZone=false;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {

        if (Input.GetKeyDown(buyMenuKey))
        {

            if (!buyTimeout && inBuyZone && !MenuManager.running)
            {
                ToggleCursorState(false);
                canRun = false;
                PlayerMovement.canMove = false;
                CameraController.canRun = false;
                BuyableMenuController.canRun = true;
                GameController.gameRunning = false;
                OnBuyMenuOpened?.Invoke();
            }
            if (buyTimeout)
            {
                GameController.DisplayMessage("90 seconds have passed, you cannot buy anymore!", 2f);
            }
            else if (!inBuyZone)
            {
                GameController.DisplayMessage("You need to be in a buy zone!", 2f);
            }
        }

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


        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (throwables.Count > 0)
            {
                if (throwables.Count == 1 && currentThrowable != currentHeld)
                {
                    previousHeld = currentHeld;
                    currentHeld = currentThrowable;
                    if (previousHeld != null) previousHeld.SetActive(false);
                    currentHeld.SetActive(true);
                }

                else
                {
                    int index = throwables.IndexOf(currentThrowable);
                    int nextIndex = (index + 1) % throwables.Count; //wrap around the list

                    if (throwables[nextIndex] == currentThrowable) nextIndex = (nextIndex + 1) % throwables.Count; //prevent switching to same grenade

                    currentThrowable = throwables[nextIndex];
                    previousHeld = currentHeld;
                    currentHeld = currentThrowable;

                    if (previousHeld != null) previousHeld.SetActive(false);

                    currentHeld.SetActive(true);

                }

                OnWeaponSwitched?.Invoke();
            }
        }


        if (Input.GetKeyDown(swithKey) && previousHeld != null)
        {
            GameObject tempHolder = previousHeld;
            previousHeld = currentHeld;
            currentHeld = tempHolder;


            previousHeld.SetActive(false);
            currentHeld.SetActive(true);

            OnWeaponSwitched?.Invoke();
        }

        if (!GameController.gameRunning || !canRun || MenuManager.running) return;

        weaponUseHold = Input.GetKey(fireKey);
        weaponSecondaryUseHold = Input.GetMouseButton(1);

        if (Input.GetKeyDown(fireKey))
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
            if (hideCursor && canClickMouseTwo)
            {
                ToggleCursorState(true);
                OnWeaponUseSecondaryStarted?.Invoke();
                canClickMouseTwo = false;
                Invoke(nameof(ResetMouseTwo), 0.2f);
            }

        }


        if (Input.GetMouseButtonUp(0)) OnWeaponUseFinished?.Invoke();

        if (Input.GetKeyDown(reloadKey)) OnWeaponReloadPressed?.Invoke();

        if (Input.GetKeyDown(KeyCode.F)) OnWeaponInspectPressed?.Invoke();

        if (Input.GetKeyDown(dropKey) && currentHeld != knife)
        {
            DropWeapon();
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
        GameController.DisplayMessage("You cannot carry anymore", 2f);
        return false;
    }

    public bool PickUpWeapon(GameObject parentObj, int currentBulletsInStock, int currentBulletsInMag)
    {
        if (primaryWeapon == null || secondaryWeapon == null)
        {
            WeaponLogic tempScript = parentObj.GetComponent<WeaponLogic>();

            tempScript.currentBulletsInMag = currentBulletsInMag;
            tempScript.currentBulletsInStock = currentBulletsInStock;

            if (tempScript.weaponType == WeaponLogic.Type.Primary)
            {
                if (primaryWeapon == null) primaryWeapon = parentObj;
                else return false;
            }
            else
            {
                if (secondaryWeapon == null) secondaryWeapon = parentObj;
                else return false;
            }


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
            Throwable grenadeScript = currentHeld.GetComponent<Throwable>();

            GameObject grenadeInstance = Instantiate(grenadeScript.dropPrefab, raycastSource.position - new Vector3(0f, 0.5f, 0f), Quaternion.identity);
            Rigidbody rb = grenadeInstance.GetComponent<Rigidbody>();
            grenadeInstance.SetActive(true);

            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
            rb.AddForce(Camera.main.transform.forward * 3f, ForceMode.Impulse);

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
        CameraController.canRun = true;
        GameController.gameRunning = true;
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
        GameObject tempObj = Instantiate(parentScript.weaponPrefab, raycastSource.position - new Vector3(0f, 0.5f, 0f), Quaternion.identity);
        Rigidbody rb = tempObj.GetComponent<Rigidbody>();
        tempObj.SetActive(true);

        //Get script to store data about the parent weapon
        DroppedWeapon instanceScript = tempObj.GetComponent<DroppedWeapon>();

        int ammoInStock = parentScript.currentBulletsInStock;
        int ammoInMag = parentScript.currentBulletsInMag;

        instanceScript.SetVariables(ammoInStock, ammoInMag);

        //Add force
        rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        rb.AddForce(Camera.main.transform.forward * 4f, ForceMode.Impulse);
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

    public bool CanAddGrenade(GameObject grenade)
    {
        Throwable grenadeScript = grenade.GetComponent<Throwable>();
        Dictionary<Throwable.Type, int> grenadeDictionary = ReturnGrenadeTypeCount(); //Get dictionary with how many of each type is in the list
        Throwable.Type type = grenadeScript.type;
        int maxAllowed = 0;

        switch (type)
        {
            case Throwable.Type.Fire: maxAllowed = 1; break;
            case Throwable.Type.Smoke: maxAllowed = 1; break;
            case Throwable.Type.Flash: maxAllowed = 2; break;
            case Throwable.Type.HE: maxAllowed = 1; break;
            case Throwable.Type.Decoy: maxAllowed = 2; break;

        }
        if (grenadeDictionary[grenadeScript.type] < maxAllowed)
            return true;
        else return false;
    }

    private Dictionary<Throwable.Type, int> ReturnGrenadeTypeCount()
    {
        var grenadeList = GetAllGrenadesEnumerable();
        Dictionary<Throwable.Type, int> dictionary = new Dictionary<Throwable.Type, int>()
        {
            {Throwable.Type.Fire,0},
            {Throwable.Type.Smoke,0},
            {Throwable.Type.Flash,0},
            {Throwable.Type.Decoy,0},
            {Throwable.Type.HE,0}
        };  //make preset dictionary to add count to


        foreach (GameObject grenade in grenadeList)
        {
            Throwable.Type type = grenade.GetComponent<Throwable>().type;

            dictionary[type] = dictionary[type] + 1;   //increment counter by 1
        }

        return dictionary;

    }

    private IEnumerable<GameObject> GetAllGrenadesEnumerable()
    {
        return throwables;
    }

    public void InBuyZone(GameController.Team team)
    {
        if (team == GameController.playerTeam) inBuyZone = true;
    }

    public void OutOfBuyZone(GameController.Team team)
    {
        if (team == GameController.playerTeam) inBuyZone = false;

    }


    public void ResetWeapons(GameController.Team team)
    {
        primaryWeapon?.SetActive(false);
        primaryWeapon = null;
        secondaryWeapon.SetActive(false);

        if (team == GameController.Team.CT) secondaryWeapon = uspInstance;
        else secondaryWeapon = glockInstance;

        currentHeld = secondaryWeapon;
        previousHeld = knife;

        if (currentHeld!=null) currentHeld.SetActive(true);

        if (hideCursor)
            ToggleCursorState(true);    
        }

    public void ResetWeaponAmmo()
    {
        if (primaryWeapon != null) primaryWeapon.GetComponent<WeaponLogic>().RefreshBulletData();
        if (secondaryWeapon != null) secondaryWeapon.GetComponent<WeaponLogic>().RefreshBulletData();

    }

    private void LoadDataFromSaved()
    {
        buyMenuKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("buyMenu"));
        fireKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("fire"));
        reloadKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("reload"));
        swithKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("lastWeaponUsed"));
        dropKey= (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("dropWeapon"));
    }
}