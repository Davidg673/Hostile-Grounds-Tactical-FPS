using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance {set; get;}
    public AudioSource hoverSound;
    public AudioSource clickSound;

    public GameObject settingsMenu;
    public GameObject startNewGameMenu;
    public GameObject mainMenu;
    public GameObject loadingMenu;
    public Slider loadingBar;
    public bool waitForInput = true;
    public TMP_Text loadPromptText;
    public KeyCode userPromptKey;

    /////Input fields
    public TMP_Dropdown mapDropdown;
    public TMP_Dropdown teamDropdown;
    public Slider numBotsSlider;
    public Slider roundTimeSlider;
    public Slider numRoundsSlider;
    public Slider startMoneySlider;

    public TMP_InputField playerNameInput;
    public TMP_Dropdown crosshairSizeDropdown;
    public TMP_Dropdown crosshairColourDropdown;
    public Slider playerSensitivitySlider;

    public TMP_Dropdown resolutionDropdown;
    public Toggle windowedModeToggle;
    public Slider soundEffectsSlider;
    public Slider brightnessSlider;

    public TMP_InputField moveForwardInput;
    public TMP_InputField moveBackInput;
    public TMP_InputField moveLeftInput;
    public TMP_InputField moveRightInput;
    public TMP_InputField walkInput;
    public TMP_InputField jumpInput;
    public TMP_InputField crouchInput;
    public TMP_InputField chatMessageInput;
    public TMP_InputField buyMenuInput;
    public TMP_InputField fireInput;
    public TMP_InputField reloadInput;
    public TMP_InputField lastWeaponUsedInput;
    public TMP_InputField dropWeaponInput;

    /////////// 
    private bool waitingForKey;
    private TMP_InputField listeningInputField;

    public static UnityAction onChangeData;
    public Volume globalVolume;
    private ColorAdjustments colorAdjust;
    private float baseBrightness;
    public GameObject UIelements;
    public static bool running;

    void Awake()
    {
        if (Instance==null) Instance=this;

        LoadSavedData();
        
        if (globalVolume!=null)
        {
            globalVolume.profile.TryGet(out colorAdjust);
            baseBrightness=colorAdjust.postExposure.value;       
        }
    }


    void OnDisable()
    {
        Instance=null;
        running=false;
    }
    public void LoadSavedData() //loads saved data (if any) into all option fields 
    {
        if (PlayerPrefs.HasKey("playerName")) playerNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("playerName");

        if (PlayerPrefs.HasKey("crosshairSize"))
        {
            switch (PlayerPrefs.GetString("crosshairSize"))
            {
                case "Small": crosshairSizeDropdown.value = 0; break;
                case "Medium": crosshairSizeDropdown.value = 1; break;
                case "Large": crosshairSizeDropdown.value = 2; break;
            }

            crosshairSizeDropdown.RefreshShownValue();

        };
        
        if (PlayerPrefs.HasKey("crosshairColour"))
        {
            switch (PlayerPrefs.GetString("crosshairColour"))
            {
                case "Green": crosshairColourDropdown.value = 0; break;
                case "Red": crosshairColourDropdown.value = 1; break;
                case "Blue": crosshairColourDropdown.value = 2; break;
                case "Light blue": crosshairColourDropdown.value = 3; break;
                case "Yellow": crosshairColourDropdown.value = 4; break;
            }

            crosshairColourDropdown.RefreshShownValue();
        };

        if (PlayerPrefs.HasKey("sensitivity")) playerSensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity");
        
        if (PlayerPrefs.HasKey("resolution"))
        {
            switch (PlayerPrefs.GetString("resolution"))
            {
                case "1920x1080": resolutionDropdown.value = 0; break;
                case "1680x1050": resolutionDropdown.value = 1; break;
                case "1600x1024": resolutionDropdown.value = 2; break;
                case "1440x900": resolutionDropdown.value = 3; break;
                case "1366x788": resolutionDropdown.value = 4; break;
                case "1280x1024": resolutionDropdown.value = 5; break;
                case "1280x800": resolutionDropdown.value = 6; break;
            }

            resolutionDropdown.RefreshShownValue();
        };

        if (PlayerPrefs.HasKey("windowed"))
        {
            windowedModeToggle.isOn = PlayerPrefs.GetInt("windowed") == 1 ? true : false;  
        }

        SetVideoSettings();

        if (PlayerPrefs.HasKey("soundVolume")) soundEffectsSlider.value = PlayerPrefs.GetFloat("soundVolume");

        if (PlayerPrefs.HasKey("brightness")) brightnessSlider.value = PlayerPrefs.GetFloat("brightness");


        if (PlayerPrefs.HasKey("moveForward")) moveForwardInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("moveForward");
        if (PlayerPrefs.HasKey("moveBack")) moveBackInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("moveBack");
        if (PlayerPrefs.HasKey("moveLeft")) moveLeftInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("moveLeft");
        if (PlayerPrefs.HasKey("moveRight")) moveRightInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("moveRight");
        if (PlayerPrefs.HasKey("walk")) walkInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("walk");
        if (PlayerPrefs.HasKey("jump")) jumpInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("jump");
        if (PlayerPrefs.HasKey("crouch")) crouchInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("crouch");
        if (PlayerPrefs.HasKey("chatMessage")) chatMessageInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("chatMessage");
        if (PlayerPrefs.HasKey("buyMenu")) buyMenuInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("buyMenu");
        if (PlayerPrefs.HasKey("fire")) fireInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("fire");
        if (PlayerPrefs.HasKey("reload")) reloadInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("reload");
        if (PlayerPrefs.HasKey("lastWeaponUsed")) lastWeaponUsedInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("lastWeaponUsed");
        if (PlayerPrefs.HasKey("dropWeapon")) dropWeaponInput.placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("dropWeapon");
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !waitingForKey && !BuyableMenuController.canRun)
        {   
            if (!mainMenu.activeSelf)
            {
                mainMenu.SetActive(true);
                settingsMenu.SetActive(false);
                startNewGameMenu.SetActive(false);

                WeaponHandler.ToggleCursorState(false);
                GameController.gameRunning=false;
                PlayerMovement.canMove = false;
                CameraController.canRun = false;
                UIelements.SetActive(false);

                running=true;

            }
            else if (settingsMenu.activeSelf || startNewGameMenu.activeSelf)
            {
            settingsMenu.SetActive(false);
            startNewGameMenu.SetActive(false);                
            }
            else
            {
                mainMenu.SetActive(false);
                WeaponHandler.ToggleCursorState(true);
                GameController.gameRunning=true;
                UIelements.SetActive(true);    
                PlayerMovement.canMove = true;
                CameraController.canRun = true;

                running=false;
            }
        }

        if (waitingForKey)
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code))
                {

                    if (code == KeyCode.Escape)
                    {
                        return;
                    }
                    string keyName = code.ToString();
 
                    listeningInputField.placeholder.GetComponent<TextMeshProUGUI>().text = code.ToString();
                    listeningInputField.text = keyName;

                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null); //stops input field listening for input

                    waitingForKey = false;
                }
            }
        }
    }


    public void PlayHover()
    {
        hoverSound.Play();
    }

    public void PlayClick()
    {
        clickSound.GetComponent<AudioSource>().Play();
    }

    public void UpdateSliderText(Slider slider)
    {
        TMP_Text textObj = slider.GetComponentInChildren<TMP_Text>();
        textObj.text = MathF.Round(slider.value,1).ToString();
    }

    public void UpdateVolumeSliderText(Slider slider)
    {
        TMP_Text textObj = slider.GetComponentInChildren<TMP_Text>();
        textObj.text = slider.value.ToString() + "%";
    }
    public void OnPointerEnter(Button button)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.color = new Color(255, 175, 0);
    }

    public void OnPointerExit(Button button)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.color = new Color(255, 255, 255);
    }

    public void OnSelectInput()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnInputValueChanged()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SaveData()
    {
        PlayerPrefs.SetString("map", mapDropdown.options[mapDropdown.value].text);
        PlayerPrefs.SetString("team", teamDropdown.options[teamDropdown.value].text);
        PlayerPrefs.SetFloat("numBots", numBotsSlider.value);
        PlayerPrefs.SetFloat("roundTime", roundTimeSlider.value);
        PlayerPrefs.SetFloat("numRounds", numRoundsSlider.value);
        PlayerPrefs.SetFloat("startMoney", startMoneySlider.value);

        PlayerPrefs.SetString("playerName", playerNameInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("crosshairSize", crosshairSizeDropdown.options[crosshairSizeDropdown.value].text);
        PlayerPrefs.SetString("crosshairColour", crosshairColourDropdown.options[crosshairColourDropdown.value].text);
        PlayerPrefs.SetFloat("sensitivity", playerSensitivitySlider.value);

        PlayerPrefs.SetString("resolution", resolutionDropdown.options[resolutionDropdown.value].text);
        PlayerPrefs.SetInt("windowed", windowedModeToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("soundVolume", soundEffectsSlider.value);
        PlayerPrefs.SetFloat("brightness", brightnessSlider.value);

        PlayerPrefs.SetString("moveForward", moveForwardInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("moveBack", moveBackInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("moveLeft", moveLeftInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("moveRight", moveRightInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("walk", walkInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("jump", jumpInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("crouch", crouchInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("chatMessage", chatMessageInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("buyMenu", buyMenuInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("fire", fireInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("reload", reloadInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("lastWeaponUsed", lastWeaponUsedInput.placeholder.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetString("dropWeapon", dropWeaponInput.placeholder.GetComponent<TextMeshProUGUI>().text);

        SetVideoSettings();
        onChangeData?.Invoke();
    }





    public void ListenForKeys(TMP_InputField inputfield)
    {
        listeningInputField = inputfield;
        Invoke(nameof(SetWaitingForKey), 0.1f);
    }

    public void SetName(string name)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;


    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    public static void LoadScene(string scene)
    {
        if (scene != "")
        {
            Instance.StartCoroutine(Instance.LoadAsynchronously(scene));
        }
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        mainMenu.SetActive(false);
        loadingMenu.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .95f);
            loadingBar.value = progress;

            if (operation.progress >= 0.9f && !waitForInput)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }


    private void SetWaitingForKey()
    {
        waitingForKey = true;
    }

    private void SetVideoSettings()
    {
        string res = PlayerPrefs.GetString("resolution");
        int xIndex = res.IndexOf("x");
        int width = int.Parse(PlayerPrefs.GetString("resolution").Substring(0, xIndex));
        int height = int.Parse(PlayerPrefs.GetString("resolution").Substring(xIndex+1));

        bool fullscreen= PlayerPrefs.GetInt("windowed") == 1 ? false : true;  

        Screen.SetResolution(width, height, fullscreen);

        AudioListener.volume = PlayerPrefs.GetFloat("soundVolume")/100;

        float savedValue=PlayerPrefs.GetFloat("brightness");
        float gammaValue= savedValue>100 ? (baseBrightness + savedValue/95) : (baseBrightness-savedValue/95);
        
        if (savedValue==100) gammaValue=baseBrightness;
        
        if (globalVolume!=null && colorAdjust!=null) colorAdjust.postExposure.value = gammaValue;

    }
}
