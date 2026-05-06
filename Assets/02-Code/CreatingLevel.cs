using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreatingLevel : MonoBehaviour
{
    public GameObject basicCube;
    public GameObject goldenCube;
    public GameObject blackCube;

    public PlatformLength platform;

    [Header("Audio")]
    public AudioClip sfxStartLevel;
    public AudioClip sfxWin;
    public AudioClip sfxShowMalus;
    private AudioSource _audio;

    public static float timer = 0f;
    private bool timerActive = false;
    private float advanceInterval = 3f;
    private float cubeSpeed = 1f;
    private float penaltyCubeSpeed = 4f;
    private int blackPenaltyMinSteps = 1;
    private int blackPenaltyMaxSteps = 2;
    private int endWaveAdvanceSteps = 5;
    private float successEndWaveAdvanceMultiplier = 1.5f;
    private float spawnRiseDistance = 1f;
    private float spawnRiseDuration = 0.35f;

    private int currentLevel = 1;
    private const int maxLevel = 3;
    private int countToDestroy = 0;
    private int countBlack = 0;
    private bool allDestroyed = false;
    private bool waveEnded = false;
    private List<CubeMove> activeCubes = new List<CubeMove>();
    private bool waitingForMenuAction = false;
    private GameObject interLevelMenu;
    private Action currentMenuAction;
    private GameObject hudCanvas;
    private Text timerText;
    private float levelElapsedTime = 0f;
    private readonly Dictionary<int, float> lastLevelTimes = new Dictionary<int, float>();
    private bool clearTimesOnNextWave = false;

    void Awake()
    {
        timer = 0f;
    }

    void Start()
    {
        if (platform == null)
            Debug.LogError("[CreatingLevel] Le champ 'Platform' est vide ! Glisse Floor depuis la Hierarchy vers ce champ dans l'Inspector de LevelManager.");

        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        CreateTimerHUD();
        SpawnWave();
    }

    void Update()
    {
        if (!waitingForMenuAction && timerActive)
        {
            levelElapsedTime += Time.deltaTime;
            UpdateTimerText();
        }

        timer += Time.deltaTime;

        if (timer >= advanceInterval)
        {
            timer = 0f;
            AdvanceCubes();
        }

        bool allBasicGoldenDestroyed = countToDestroy > 0 && CubeMove.destroyedCubes >= countToDestroy;
        bool allBlackFallen = CubeMove.blackFallen >= countBlack;

        if (!allDestroyed && allBasicGoldenDestroyed)
            allDestroyed = true;

        if (!waveEnded && allBasicGoldenDestroyed && allBlackFallen)
        {
            waveEnded = true;
            StartCoroutine(EndWave());
        }
    }

    void AdvanceCubes()
    {
        if (!timerActive)
        {
            timerActive = true;
            if (hudCanvas != null) hudCanvas.SetActive(true);
            UpdateTimerText();
        }
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(cubeSpeed);
    }

    public void PenaltyAdvance()
    {
        timer = 0f;
        int penaltySteps = UnityEngine.Random.Range(blackPenaltyMinSteps, blackPenaltyMaxSteps + 1);
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(penaltyCubeSpeed, true, penaltySteps);

        PlayerMove player = FindFirstObjectByType<PlayerMove>();
        if (player != null) player.ClearMark();
        if (sfxShowMalus != null) _audio.PlayOneShot(sfxShowMalus);
    }

    IEnumerator EndWave()
    {
        // Avancer les cubes restants rapidement jusqu'à ce qu'ils tombent
        int endWaveSteps = allDestroyed
            ? Mathf.CeilToInt(endWaveAdvanceSteps * successEndWaveAdvanceMultiplier)
            : endWaveAdvanceSteps;

        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(penaltyCubeSpeed, true, endWaveSteps);

        // Laisser le temps à l'animation de se jouer
        yield return new WaitForSeconds(2f);

        // Détruire ceux qui n'auraient pas encore disparu
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            Destroy(cube.gameObject);
        activeCubes.Clear();

        if (sfxWin != null) _audio.PlayOneShot(sfxWin);

        CubeMove.destroyedCubes = 0;
        CubeMove.destroyedMistake = 0;
        CubeMove.blackFallen = 0;
        CubeMove.countAudio = 0;

        int completedLevel = currentLevel;
        currentLevel++;
        if (currentLevel > maxLevel) currentLevel = 1;
        allDestroyed = false;
        waveEnded = false;
        ShowInterLevelMenu(completedLevel, currentLevel);
    }

    void SpawnWave()
    {
        if (waitingForMenuAction)
            return;

        if (clearTimesOnNextWave)
        {
            clearTimesOnNextWave = false;
            lastLevelTimes.Clear();
        }

        timer = 0f;
        timerActive = false;
        levelElapsedTime = 0f;
        if (hudCanvas != null) hudCanvas.SetActive(false);
        TextAsset jsonFile = Resources.Load<TextAsset>("level" + currentLevel);
        if (jsonFile == null)
        {
            Debug.LogWarning("Niveau introuvable : level" + currentLevel);
            return;
        }

        if (sfxStartLevel != null) _audio.PlayOneShot(sfxStartLevel);

        Level level = JsonUtility.FromJson<Level>(jsonFile.text);
        string[] rows = level.wave.Split('/');
        int cols = rows[0].Split(' ').Length;

        if (platform != null)
        {
            platform.PlatformWidth(cols);
            CameraScript cam = FindObjectOfType<CameraScript>();
            if (cam != null) cam.Posit(cols);
        }

        countToDestroy = 0;
        countBlack = 0;
        float floorSurfaceY = 1.5f;
        float zStart = 11f;

        for (int z = 0; z < rows.Length; z++)
        {
            string[] colValues = rows[z].Split(' ');
            float xOffset = 0f;

            for (int x = 0; x < colValues.Length; x++)
            {
                int type = int.Parse(colValues[x]);
                Vector3 pos = new Vector3(xOffset + x, floorSurfaceY, zStart - z);

                if (type == 1)
                {
                    GameObject cube = Instantiate(basicCube, pos, Quaternion.identity);
                    RegisterSpawnedCube(cube, floorSurfaceY);
                    countToDestroy++;
                }
                else if (type == 2)
                {
                    GameObject cube = Instantiate(goldenCube, pos, Quaternion.identity);
                    RegisterSpawnedCube(cube, floorSurfaceY);
                    countToDestroy++;
                }
                else if (type == 3 && blackCube != null)
                {
                    GameObject cube = Instantiate(blackCube, pos, Quaternion.identity);
                    RegisterSpawnedCube(cube, floorSurfaceY);
                    countBlack++;
                }
            }
        }
    }

    void RegisterSpawnedCube(GameObject cubeObject, float floorSurfaceY)
    {
        if (cubeObject == null) return;

        CubeMove cubeMove = cubeObject.GetComponent<CubeMove>();
        if (cubeMove == null) return;

        cubeMove.PlaySpawnFromGround(floorSurfaceY, spawnRiseDistance, spawnRiseDuration);
        activeCubes.Add(cubeMove);
    }

    void ShowInterLevelMenu(int completedLevel, int nextLevel)
    {
        lastLevelTimes[completedLevel] = levelElapsedTime;

        string completedLevelTime = FormatDuration(levelElapsedTime);
        string timesSummary = BuildTimesSummary();

        string titleText = completedLevel == maxLevel
            ? "Niveau " + completedLevel + " termine !\nTemps du niveau : " + completedLevelTime + "\n\n" + timesSummary + "\n\nTu as fini la sequence.\nAppuie sur Continuer pour recommencer."
            : "Niveau " + completedLevel + " termine !\nTemps du niveau : " + completedLevelTime + "\n\n" + timesSummary + "\n\nProchain niveau : " + nextLevel;

        if (completedLevel == maxLevel)
            clearTimesOnNextWave = true;

        ShowOverlayMenu(titleText, "Continuer", ContinueToNextLevel);
    }

    void CreateTimerHUD()
    {
        if (hudCanvas != null)
            return;

        hudCanvas = new GameObject("TimerHUD");
        Canvas canvas = hudCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        hudCanvas.AddComponent<GraphicRaycaster>();

        GameObject timerTextObj = new GameObject("TimerText");
        timerTextObj.transform.SetParent(hudCanvas.transform, false);
        timerText = timerTextObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (timerText.font == null) timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timerText.color = Color.white;
        timerText.fontSize = 32;
        timerText.alignment = TextAnchor.UpperLeft;
        timerText.text = "Temps: 00:00.00";

        RectTransform textRect = timerTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);
        textRect.anchoredPosition = new Vector2(20f, -20f);
        textRect.sizeDelta = new Vector2(360f, 60f);
    }

    void UpdateTimerText()
    {
        if (timerText == null)
            return;

        timerText.text = "Temps: " + FormatDuration(levelElapsedTime);
    }

    string FormatDuration(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainingSeconds = seconds - (minutes * 60f);
        return string.Format("{0:00}:{1:00.00}", minutes, remainingSeconds);
    }

    string BuildTimesSummary()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Temps par niveau :");
        for (int i = 1; i <= maxLevel; i++)
        {
            if (lastLevelTimes.TryGetValue(i, out float time))
                sb.Append("\n- Niveau ").Append(i).Append(" : ").Append(FormatDuration(time));
        }

        return sb.ToString();
    }

    public void ShowGameOverMenu()
    {
        if (waitingForMenuAction)
            return;

        ShowOverlayMenu("Game Over !\nAppuie sur Continuer pour relancer la partie.", "Continuer", RestartGame);
    }

    void ShowOverlayMenu(string titleText, string buttonLabel, Action onButtonPressed)
    {
        waitingForMenuAction = true;
        currentMenuAction = onButtonPressed;
        Time.timeScale = 0f;

        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        if (interLevelMenu != null)
            Destroy(interLevelMenu);

        interLevelMenu = new GameObject("InterLevelMenu");
        Canvas canvas = interLevelMenu.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        interLevelMenu.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        interLevelMenu.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(interLevelMenu.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject textObj = new GameObject("Title");
        textObj.transform.SetParent(panelObj.transform, false);
        Text title = textObj.AddComponent<Text>();
        title.font = uiFont;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        title.fontSize = 40;
        title.text = titleText;
        RectTransform titleRect = textObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.55f);
        titleRect.anchorMax = new Vector2(0.9f, 0.85f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        GameObject buttonObj = new GameObject("ContinueButton");
        buttonObj.transform.SetParent(panelObj.transform, false);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        colors.pressedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(OnOverlayButtonPressed);
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.38f, 0.35f);
        buttonRect.anchorMax = new Vector2(0.62f, 0.45f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.font = uiFont;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        buttonText.fontSize = 28;
        buttonText.text = buttonLabel;
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
    }

    void OnOverlayButtonPressed()
    {
        if (!waitingForMenuAction)
            return;

        Action actionToRun = currentMenuAction;
        CloseOverlayMenu();
        actionToRun?.Invoke();
    }

    void CloseOverlayMenu()
    {
        waitingForMenuAction = false;
        currentMenuAction = null;
        Time.timeScale = 1f;
        if (interLevelMenu != null)
            Destroy(interLevelMenu);
    }

    void ContinueToNextLevel()
    {
        SpawnWave();
    }

    void RestartGame()
    {
        CubeMove.destroyedCubes = 0;
        CubeMove.destroyedMistake = 0;
        CubeMove.blackFallen = 0;
        CubeMove.countAudio = 0;
        timer = 0f;
        SceneManager.LoadScene("SquaresOfFear_scene");
    }

    void OnDestroy()
    {
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}
