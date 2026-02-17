using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Message System")]
    public static GameController Instance { get; set; }
    [SerializeField] private GameObject messageContainer;
    private TMP_Text messageText;
    private Coroutine messageRoutine;

    [Header("Round System")]
    [SerializeField] private int roundsInGame;
    [SerializeField] public static int roundsLeft = 3;
    public static int seconds = 0;
    public static int minutes = 7;
    public static int roundMinutes = 7;
    private float timeToSecond;

    public static bool gameRunning;
    public static bool AIRunning;
    public static string playerName="player";


    public static List<GameObject> activeCT = new List<GameObject>();
    public static List<GameObject> activeT = new List<GameObject>(); //current bots/players in the game to keep track of game state

    public List<GameObject> poolCT = new List<GameObject>();  //avaiable CT/T prefabs for instantiating. avoid changes this runtime
    public List<GameObject> poolT = new List<GameObject>();

    public List<Transform> spawnsCT = new List<Transform>(); //available spawns for each team
    public List<Transform> spawnsT = new List<Transform>();
    public List<GameObject> allEntities = new List<GameObject>();
    public List<GameObject> scoreboardAgents = new List<GameObject>();

    private int CTtoSpawn = 5;  // how many bots to spawn on each side
    private int TtoSpawn = 5;

    public static Team playerTeam;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject UI_elements;
    [SerializeField] private GameObject endGameMenu;

    public bool playWithBots = true;
    private int secondsPassed = 0;
    private bool buyPhase = true;
    private PlayerUI playerUIScript;

    [SerializeField] private GameObject cameraObj;
    Vector3 cameraOriginalPos;
    Quaternion cameraOriginalRot;
    [SerializeField] GameObject playerHands;



    public enum Team
    {
        CT,
        T
    }


    private void Awake()
    {
        messageText = messageContainer.GetComponent<TMP_Text>();
        cameraOriginalPos = Camera.main.transform.localPosition;
        cameraOriginalRot = Camera.main.transform.localRotation;


        if (Instance == null)
        {
            Instance = this;
        }
 
        Camera.main.enabled = true;
        playerUIScript = player.GetComponent<PlayerUI>();
        StartGame();
    }


    void OnDisable()
    {
        Instance=null;
        gameRunning=false;
        AIRunning=false;
        activeCT.Clear();
        activeT.Clear();
        PlayerHealth.OnPlayerDead -= HandlePlayerDeath;
    }

    void Update()
    {
        UpdateTimer();
        if (!buyPhase) CheckWinCondition();
    }

    public static void DisplayMessage(string message, float displayTime)
    {
        if (Instance != null) Instance.HandleMessageCall(message, displayTime);
        else Debug.LogWarning("Game Controller instance not found!");

    }


    private void HandleMessageCall(string message, float displayTime)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(DisplayMessageCoroutine(message, displayTime));
    }

    private IEnumerator DisplayMessageCoroutine(string message, float displayTime)
    {
        messageText.text = message;
        yield return new WaitForSeconds(displayTime);
        messageText.text = "";
    }


    /// <summary>
    /// Gets called every time the round ends. when called from last round, it measn the game is over
    /// </summary>
    public void ChangeRound()
    {
        gameRunning = false;

        if (roundsLeft == 1)
        {
            EndGame();
        }
        else
        {
            roundsLeft--;
            ResetRound();
        }
    }


    private void StartGame()
    {
        WeaponHandler weaponHandlerScript = player.GetComponent<WeaponHandler>();
        weaponHandlerScript.ResetWeapons(playerTeam);
        PlayerHealth.OnPlayerDead += HandlePlayerDeath;

        LoadSavedData();

        if (playerTeam == Team.CT) ScoreboardManager.GlobalAddElement(Team.CT);
        else ScoreboardManager.GlobalAddElement(Team.T);

        for (int x = 0; x < (playerTeam == Team.CT? CTtoSpawn-1 : CTtoSpawn); x++)
        {
            ScoreboardManager.GlobalAddElement(Team.CT);

        }
        
        for (int x = 0; x < (playerTeam == Team.T? TtoSpawn-1 : TtoSpawn); x++)
        {
            ScoreboardManager.GlobalAddElement(Team.T);
        }

        ResetRound();
    }

    private void EndGame()
    {
        UI_elements.SetActive(false);
        DisplayMessage("Match finished... Returning to menu", 5f);
        endGameMenu.SetActive(true);
        playerHands.SetActive(false);

        CameraController.canRun = false;
        gameRunning = false;
        AIRunning=false;
        PlayerMovement.canMove = false;
        cameraObj.transform.position += new Vector3(0f,2f,0f);
        cameraObj.transform.rotation = Quaternion.identity;


        Invoke(nameof(QuitToMenu), 5f);
    }

    private void ResetRound()
    {
        buyPhase = true;
        gameRunning = false;
        AIRunning=false;
        seconds = 5;
        minutes = 0;

        //handle player
        UI_elements.SetActive(true);
        WeaponHandler weaponHandlerScript = player.GetComponent<WeaponHandler>();
        weaponHandlerScript.ResetWeaponAmmo();
        PlayerHealth playerHealthScript = player.GetComponent<PlayerHealth>();
        playerHealthScript.health = 100;
        CameraController.canRun = true;

        cameraObj.transform.SetParent(player.transform);


        if (playerTeam == Team.CT)
        {
            player.tag = "CT";
            player.layer = LayerMask.NameToLayer("CT");
            PlayerUI.SetLayers(LayerMask.NameToLayer("T"), LayerMask.NameToLayer("CT"));
        }
        else
        {
            player.tag = "T";
            player.layer = LayerMask.NameToLayer("T");
            PlayerUI.SetLayers(LayerMask.NameToLayer("CT"), LayerMask.NameToLayer("T"));

        }


        SpawnEntities();

        Invoke(nameof(SetRunning), 5f);
    }

    public void QuitToMenu()
    {
        MenuManager.LoadScene("Main Menu");
    }

    void UpdateTimer()
    {

        timeToSecond += Time.deltaTime;
        if (timeToSecond >= 1f)
        {
            timeToSecond = 0f;
            seconds--;


            secondsPassed++;
            if (secondsPassed >= 40)
            {
                WeaponHandler.buyTimeout = true;
            }

            if (seconds < 0)
            {
                minutes--;
                seconds = 59;

            }
            if (minutes < 0 && !buyPhase)
            {
                seconds = 0;
                DisplayMessage("Counter Terrorists have won!", 5f);
                gameRunning = false;
                Invoke(nameof(EndRound), 5f);
            }
            if (minutes < 0 && buyPhase)
            {
                gameRunning = true;
                minutes = roundMinutes;
                buyPhase = false;
            }

        }
    }


    void SpawnEntities()
    {
        List<Transform> spawnsCTPool = new List<Transform>(spawnsCT);
        List<Transform> spawnsTPool = new List<Transform>(spawnsT);  //make duplicate list to remove items, avoiding same spawns for different entities
        if (playerTeam == Team.CT)
        {
            Transform spawnPoint = spawnsCTPool[Random.Range(0, spawnsCTPool.Count)];
            spawnsCTPool.Remove(spawnPoint);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.SetParent(null);
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;

            if (cc != null) cc.enabled = true;


            activeCT.Add(player);
        }
        else
        {
            Transform spawnPoint = spawnsTPool[Random.Range(0, spawnsTPool.Count)];
            spawnsTPool.Remove(spawnPoint);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.SetParent(null);
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;

            if (cc != null) cc.enabled = true;


            activeT.Add(player);
        }


        if (!playWithBots) return;



        for (int x = 0; x < (playerTeam == Team.CT? CTtoSpawn-1 : CTtoSpawn); x++)
        {
            GameObject ctPrefab = poolCT[Random.Range(0, poolCT.Count)];
            Transform spawnPoint = spawnsCTPool[Random.Range(0, spawnsCTPool.Count)];
            GameObject botInstance = Instantiate(ctPrefab, spawnPoint.position, new Quaternion(0f, Random.Range(-180, 180), 0f, 0f));
            botInstance.SetActive(true);

            activeCT.Add(botInstance);
            allEntities.Add(botInstance);

            int id = x + 1;
            botInstance.GetComponent<ScoreboardAgent>().id = id;
            if (roundsInGame!=roundsLeft)
                botInstance.GetComponent<ScoreboardAgent>().CallbackScoreboard(id,0,0,Random.Range(-500,0));

        }

        for (int x = 0; x < (playerTeam == Team.T? TtoSpawn-1 : TtoSpawn); x++)
        {
            GameObject tPrefab = poolT[Random.Range(0, poolT.Count)];
            Transform spawnPoint = spawnsTPool[Random.Range(0, spawnsTPool.Count)];
            GameObject botInstance = Instantiate(tPrefab, spawnPoint.position, new Quaternion(0f, Random.Range(-180, 180), 0f, 0f));
            botInstance.SetActive(true);

            activeT.Add(botInstance);
            allEntities.Add(botInstance);

            int id = x + 1 + CTtoSpawn;
            botInstance.GetComponent<ScoreboardAgent>().id = id;

            if (roundsInGame!=roundsLeft)
                botInstance.GetComponent<ScoreboardAgent>().CallbackScoreboard(id,0,0,Random.Range(-500,0));

        }
    }


    void EndRound()
    {
        secondsPassed = 0;

        PlayerHealth healthScript = player.GetComponent<PlayerHealth>();
        healthScript.health = 100;

        activeCT.Clear();
        activeT.Clear();
        foreach (GameObject entity in allEntities)
        {
            if (entity != player) Destroy(entity);
        }

        allEntities.Clear();
        buyPhase = true;

        ChangeRound();
    }


    void CheckWinCondition()
    {
        //Debug.Log($"game running:{gameRunning} bots: {playWithBots} buy phase {buyPhase}");
        if (gameRunning == false || !playWithBots || buyPhase) return;
    
    
        if (activeCT.Count <= 0)
        {
            gameRunning = false;
            AIRunning=false;
            DisplayMessage("Terrorists have won!", 5f);
            if (playerTeam == Team.CT) playerUIScript.money += 1400;
            if (playerTeam == Team.T) playerUIScript.money += 3250;

            Invoke(nameof(EndRound), 5f);

        }
        else if (activeT.Count <= 0)
        {
            gameRunning = false;
            AIRunning=false;
            DisplayMessage("Counter Terrorists have won!", 5f);
            if (playerTeam == Team.CT) playerUIScript.money += 3250;
            if (playerTeam == Team.T) playerUIScript.money += 1400;

            Invoke(nameof(EndRound), 5f);

        }
    }

    void SetRunning()
    {
        DisplayMessage("Round has begun", 3f);
        gameRunning = true;
        AIRunning=true;
    }

    void HandlePlayerDeath()
    {
        cameraObj.transform.SetParent(player.transform);
        playerHands.SetActive(true);
        cameraObj.transform.localPosition = cameraOriginalPos;
        cameraObj.transform.localRotation = cameraOriginalRot;

        EndRound();
    }

    void LoadSavedData()
    {
        playerTeam= PlayerPrefs.GetString("team").Equals("Counter Terrorists") ? Team.CT : Team.T;
        playerUIScript.money= (int) PlayerPrefs.GetFloat("startMoney");
        CTtoSpawn = (int) PlayerPrefs.GetFloat("numBots");
        TtoSpawn = (int) PlayerPrefs.GetFloat("numBots");
        roundMinutes= (int) PlayerPrefs.GetFloat("roundTime");
        roundsLeft = (int) PlayerPrefs.GetFloat("numRounds");
        playerName = PlayerPrefs.GetString("playerName");
        
        if (TtoSpawn <=0 && CTtoSpawn <=0) playWithBots=false;
    }
}
