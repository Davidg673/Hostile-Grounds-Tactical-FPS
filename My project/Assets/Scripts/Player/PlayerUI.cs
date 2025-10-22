using UnityEngine;
using TMPro;
using System.Collections;
public class PlayerUI : MonoBehaviour
{
    public int health;
    public int armor;
    public int money;

    public int seconds = 0;
    public int minutes = 7;

    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text moneyText;
    private float timeToSecond;
    bool gameRunning = true;
    WeaponHandler weaponHandlerScript;


    void Start()
    {
        weaponHandlerScript = GameObject.Find("Player").GetComponent<WeaponHandler>();
    }


    void Update()
    {
        if (gameRunning)
        {
            healthText.text = health.ToString();
            armorText.text = armor.ToString();
            timerText.text = minutes + ":" + seconds;
            moneyText.text = "$ " + money.ToString();

            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        timeToSecond += Time.deltaTime;
        if (timeToSecond >= 1f)
        {
            timeToSecond = 0f;
            seconds--;
            if (seconds < 0)
            {
                minutes--;
                seconds = 59;
            }
            if (minutes < 0) HandleEndRound();
        }
    }

    void HandleEndRound()
    {

    }

    void LateUpdate()
    {
        if (gameRunning)
            UpdatetAmmo();
    }

    void UpdatetAmmo()
    {
        WeaponLogic weaponScript = weaponHandlerScript.currentHeld.GetComponent<WeaponLogic>();
        if (weaponScript != null) ammoText.text = weaponScript.currentBulletsInMag + "|   " + weaponScript.currentBulletsInStock;
        else ammoText.text = "";

    }


}
