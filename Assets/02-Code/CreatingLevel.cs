using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatingLevel : MonoBehaviour
{
    public GameObject basicCube;
    public GameObject goldenCube;
    public GameObject blackCube;

    public PlatformLength platform;

    public static float timer = 0f;
    private float advanceInterval = 3f;
    private float cubeSpeed = 1f;
    private float penaltyCubeSpeed = 4f;
    private int penaltyAdvanceSteps = 5;

    private int currentLevel = 1;
    private const int maxLevel = 3;
    private int countToDestroy = 0;
    private bool allDestroyed = false;
    private bool waveEnded = false;
    private List<CubeMove> activeCubes = new List<CubeMove>();

    void Start()
    {
        if (platform == null)
            Debug.LogError("[CreatingLevel] Le champ 'Platform' est vide ! Glisse Floor depuis la Hierarchy vers ce champ dans l'Inspector de LevelManager.");

        SpawnWave();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= advanceInterval)
        {
            timer = 0f;
            AdvanceCubes();
        }

        if (!allDestroyed && countToDestroy > 0 && CubeMove.destroyedCubes >= countToDestroy)
            allDestroyed = true;

        int nonBlackMistakes = CubeMove.destroyedMistake - CubeMove.blackFallen;
        int accounted = CubeMove.destroyedCubes + nonBlackMistakes;
        if (!waveEnded && countToDestroy > 0 && accounted >= countToDestroy)
        {
            waveEnded = true;
            StartCoroutine(EndWave());
        }
    }

    void AdvanceCubes()
    {
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(cubeSpeed);
    }

    public void PenaltyAdvance()
    {
        timer = 0f;
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(penaltyCubeSpeed, true, penaltyAdvanceSteps);
    }

    IEnumerator EndWave()
    {
        // Avancer les cubes restants rapidement jusqu'à ce qu'ils tombent
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(penaltyCubeSpeed, true, penaltyAdvanceSteps);

        // Laisser le temps à l'animation de se jouer
        yield return new WaitForSeconds(2f);

        // Détruire ceux qui n'auraient pas encore disparu
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            Destroy(cube.gameObject);
        activeCubes.Clear();

        // Réaction de la plateforme
        if (platform != null)
        {
            if (allDestroyed)
                StartCoroutine(platform.Enlargement());
            else
                platform.Decrease();
        }

        CubeMove.destroyedCubes = 0;
        CubeMove.destroyedMistake = 0;
        CubeMove.blackFallen = 0;
        CubeMove.countAudio = 0;

        currentLevel++;
        if (currentLevel > maxLevel) currentLevel = 1;
        allDestroyed = false;
        waveEnded = false;
        SpawnWave();
    }

    void SpawnWave()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("level" + currentLevel);
        if (jsonFile == null)
        {
            Debug.LogWarning("Niveau introuvable : level" + currentLevel);
            return;
        }

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
                    activeCubes.Add(cube.GetComponent<CubeMove>());
                    countToDestroy++;
                }
                else if (type == 2)
                {
                    GameObject cube = Instantiate(goldenCube, pos, Quaternion.identity);
                    activeCubes.Add(cube.GetComponent<CubeMove>());
                    countToDestroy++;
                }
                else if (type == 3 && blackCube != null)
                {
                    GameObject cube = Instantiate(blackCube, pos, Quaternion.identity);
                    activeCubes.Add(cube.GetComponent<CubeMove>());
                }
            }
        }
    }
}
