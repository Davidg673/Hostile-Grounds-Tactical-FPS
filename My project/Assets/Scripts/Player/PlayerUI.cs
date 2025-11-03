using UnityEngine;
using TMPro;
using System.Collections;
public class PlayerUI : MonoBehaviour
{
    public float health;
    public int armor;
    public int money;

    public static PlayerUI Instance { get; set; }


    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text moneyText;

    WeaponHandler weaponHandlerScript;
    PlayerHealth playerHealth;
    public LayerMask teamLayer;
    public LayerMask enemyLayer;

    void Start()
    {
        weaponHandlerScript = GameObject.Find("Player").GetComponent<WeaponHandler>();
        playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Update()
    {

        health = playerHealth.health;

        healthText.text = health.ToString();
        armorText.text = armor.ToString();
        string singleDigitsText = (GameController.seconds < 10) ? "0" : "";
        timerText.text = GameController.minutes + ":" + singleDigitsText + GameController.seconds;
        moneyText.text = "$ " + money.ToString();
        UpdateAmmo();


    }


    void UpdateAmmo()
    {
        WeaponLogic weaponScript = weaponHandlerScript.currentHeld.GetComponent<WeaponLogic>();
        if (weaponScript != null) ammoText.text = weaponScript.currentBulletsInMag + "|   " + weaponScript.currentBulletsInStock;
        else ammoText.text = "";

    }


    public static void SetLayers(int enemyLayer, int teamLayer)
    {
        Instance.teamLayer = 1 << teamLayer;
        Instance.enemyLayer = 1 << enemyLayer;

    }

    public static void AddToMoney(int moneyToAdd)
    {
        Instance.money += moneyToAdd;
        if (Instance.money > 16000)
            Instance.money = 1600;
    }


    public static void AddToArmor(int armorToAdd)
    {
        Instance.armor += armorToAdd;
        if (Instance.armor>100)
            Instance.armor=100;
    }

}
