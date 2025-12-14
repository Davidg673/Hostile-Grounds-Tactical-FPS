using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

public class BuyableMenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject firstMenuCanvas;
    public GameObject secondMenuCanvas;

    [Header("Weapon panels")]
    public GameObject secondMenuPistols;
    public GameObject secondMenuShotguns;
    public GameObject secondMenuSMGs;
    public GameObject secondMenuRifles;
    public GameObject secondMenuGrenades;
    public GameObject secondMenuEquipment;
    public static bool canRun;
    public static UnityAction OnResetWeaponPanels;
    public GameObject hoverClip;
    public GameObject clickClip;
    public GameObject playerObj;
    PlayerUI UIScript;

    [SerializeField] GameObject armor;
    [SerializeField] GameObject fullArmor;


    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buyClip;



    void Awake()
    {
        WeaponHandler.OnBuyMenuOpened += OpenMenu;
        UIScript = playerObj.GetComponent<PlayerUI>();

        //initialize button script
        foreach (Button button in secondMenuCanvas.GetComponentsInChildren<Button>(true))
        {
            GameObject buttonObj = button.gameObject;
            ButtonSate buttonScript = buttonObj.GetComponent<ButtonSate>();

            if (buttonScript == null) continue;

            buttonScript.Initialize();
        }

    }

    void OnDisable()
    {
        canRun=false;
        WeaponHandler.OnBuyMenuOpened-= OpenMenu;
    }

    void Update()
    {
        if (canRun) CheckForInput();
    }

    void OpenMenu()
    {
        canRun = false;
        menuCanvas.SetActive(true);
        secondMenuCanvas.SetActive(true);
        Invoke(nameof(StartRun), 0.2f);

        SetButtonStates();
    }

    public void CloseMenu()
    {
        Invoke(nameof(ResetRun), 0.2f);
        canRun = false;

        menuCanvas.SetActive(false);
        firstMenuCanvas.SetActive(true);
        secondMenuCanvas.SetActive(false);

        secondMenuPistols.SetActive(false);
        secondMenuShotguns.SetActive(false);
        secondMenuSMGs.SetActive(false);
        secondMenuRifles.SetActive(false);
        secondMenuEquipment.SetActive(false);

        OnResetWeaponPanels?.Invoke();
        WeaponHandler.ToggleCursorState(true);
    }

    void CheckForInput()
    {
        GameObject activePanel = null;
        Button pressedButton = null;

        if (firstMenuCanvas.activeSelf)
        {
            activePanel = firstMenuCanvas;
        }
        else if (secondMenuCanvas.activeSelf)
        {
            if (secondMenuPistols.activeSelf) activePanel = secondMenuPistols;
            if (secondMenuShotguns.activeSelf) activePanel = secondMenuShotguns;
            if (secondMenuSMGs.activeSelf) activePanel = secondMenuSMGs;
            if (secondMenuRifles.activeSelf) activePanel = secondMenuRifles;
            if (secondMenuGrenades.activeSelf) activePanel = secondMenuGrenades;
            if (secondMenuEquipment.activeSelf) activePanel = secondMenuEquipment;

        }


        //if Input is number 1,2,3..., find the button in the children objects of the first panel with the corresponding name.
        if (Input.GetKeyDown(KeyCode.Alpha1)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "1");
        if (Input.GetKeyDown(KeyCode.Alpha2)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "2");
        if (Input.GetKeyDown(KeyCode.Alpha3)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "3");
        if (Input.GetKeyDown(KeyCode.Alpha4)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "4");
        if (Input.GetKeyDown(KeyCode.Alpha5)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "5");
        if (Input.GetKeyDown(KeyCode.Alpha6)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "6");
        if (Input.GetKeyDown(KeyCode.Escape)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "Esc");
        if (Input.GetKeyDown(KeyCode.B)) pressedButton = activePanel.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "B");

        if (pressedButton != null && pressedButton.interactable)
            pressedButton?.onClick.Invoke();
    }

    public void BuyWeapon(GameObject weapon)
    {
        int weaponCost = 0;


        if (weapon == armor)
        {
            PlayerUI.AddToArmor(50);

            UIScript.money -= 650;
            if (UIScript.money < 0) UIScript.money = 0;
        
            CloseMenu();
            return;

        }

        if (weapon == fullArmor)
        {
        
            UIScript.money -= 1000;
            if (UIScript.money < 0) UIScript.money = 0;
            PlayerUI.AddToArmor(50);
        }

        WeaponHandler weaponHandlerScript = playerObj.GetComponent<WeaponHandler>();
        
        //Deduct cost of weapon from player balance
        WeaponLogic weaponScript = weapon.GetComponent<WeaponLogic>();
        ThrowableBase grenadeScript = weapon.GetComponent<ThrowableBase>();
        
        if (grenadeScript!=null && !weaponHandlerScript.CanAddGrenade(weapon))
        {
            CloseMenu();
            GameController.DisplayMessage("You cannot carry anymore!", 2f);
            return;
        }

        if (weaponScript) weaponCost = weaponScript.cost;
        else if (grenadeScript) weaponCost = grenadeScript.cost;
        else return; 
        
        UIScript.money -= weaponCost;
        if (UIScript.money < 0) UIScript.money = 0;

        weaponHandlerScript.LoadNewWeapon(weapon);
        sfxSource.PlayOneShot(buyClip);

        CloseMenu();
    }

    void ResetRun()
    {
        WeaponHandler.CloseBuyMenu();
    }

    public void PlayHover()
    {
        hoverClip.GetComponent<AudioSource>().Play();
    }

    public void PlayerClick()
    {
        clickClip.GetComponent<AudioSource>().Play();
    }
    
    void StartRun()
    {
        canRun = true;
    }
    private void SetButtonStates()
    {
        int playerBalance = UIScript.money;

        foreach (Button button in secondMenuCanvas.GetComponentsInChildren<Button>(true))
        {
            GameObject buttonObj = button.gameObject;
            ButtonSate buttonScript = buttonObj.GetComponent<ButtonSate>();

            if (buttonScript == null) continue;

            if (playerBalance >= buttonScript.cost) buttonScript.ChangeState(true);
            else buttonScript.ChangeState(false);
        }
    }

}
