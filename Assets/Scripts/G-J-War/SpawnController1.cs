using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class SpawnController : MonoBehaviour
{
    public GJWarAreaController areaController;
    public enum Team { Glueman, Joyman };
    public Team TeamName;

    [SerializeField]
    private GameObject gluemanPrefab; // Glueman prefab

    [SerializeField]
    private Material gluemanMaterial; // Glueman material

    [SerializeField]
    private GameObject joymanPrefab;  // Joyman prefab

    [SerializeField]
    private Material joymanMaterial; // Joyman material

    private GameObject selectedPrefab;
    private Material selectedMaterial;

    public int maxMen = 10; // Max number of men to spawn
    private int menCount = 0;
    public GameObject[] men; // Array to store spawned man objects
    private int frameCount = 0;

    public float spawnRadius = 20.0f; // Radius around SpawnController to spawn

    void Start()
    {
        frameCount = 0;
        men = new GameObject[maxMen];
        selectedPrefab = TeamName == Team.Glueman ? gluemanPrefab : joymanPrefab;
        selectedMaterial = TeamName == Team.Glueman ? gluemanMaterial : joymanMaterial;
    }

    void Update()
    {
        for(int i = 0; i < maxMen; i++){
            if(men[i] == null){
                SpawnMan(i);
            }
        }
        frameCount++;
        if (frameCount >= 30)
        {
            frameCount = 0;

            if (menCount < maxMen)
            {
                SpawnMan(menCount);
            }
        }
    }

    void SpawnMan(int serial)
    {
        if (selectedPrefab != null)
        {
            Vector3 spawnPosition = transform.position + (Random.insideUnitSphere + new Vector3(1f, 0f, 1f)) * spawnRadius;
            spawnPosition.y = 0.5f;
            GameObject newMan = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

            if (selectedMaterial != null)
            {
                Renderer renderer = newMan.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = selectedMaterial;
                }
            }
            // newMan.transform.parent = areaController.transform; 
            men[serial] = newMan;
            men[serial].transform.localPosition = spawnPosition;
            menCount++;
        }
    }
}
