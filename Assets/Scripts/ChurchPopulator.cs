using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChurchPopulator : MonoBehaviour
{

    [Header("Prefabs to use")]
    [SerializeField] private GameObject churchBeginning;
    [SerializeField] private GameObject churchEnd;
    [SerializeField] private GameObject churchHallway;
    private GameObject churchBeginningInstance;
    private GameObject churchEndInstance;

    [Header("Reference transforms")] 
    [SerializeField] private Transform backReference;
    [SerializeField] private Transform frontReference;
    [SerializeField] private GameObject wallDownGO;


    [Header("Spawner properties")] [SerializeField]
    public GameObject churchSpawnerParent;

    // Maintain the world pos of instantiated hall ways 
    private Dictionary<GameObject, Vector3> _hallWayInstancesPos;
    private float hallWayZMeshSize;
    private float currentHallWayZSize;
    private float wallDownActualSize;
    private float currentWallDownSize;

    private List<GameObject> _hallWayBackDynamicInstances;
    private List<GameObject> _hallWayFrontDynamicInstances;
    
    // Start is called before the first frame update
    void Start()
    {
        churchSpawnerParent = GameObject.Find("ChurchSpawnParent");
        //Instantiate the beginning and end of the church
        churchBeginningInstance = Instantiate(churchBeginning, churchSpawnerParent.transform);
        churchBeginningInstance.transform.position = frontReference.position;
        churchEndInstance = Instantiate(churchEnd, churchSpawnerParent.transform);
        churchEndInstance.transform.position = backReference.position;
        
        _hallWayInstancesPos = new Dictionary<GameObject, Vector3>();
        _hallWayBackDynamicInstances = new List<GameObject>();
        _hallWayFrontDynamicInstances = new List<GameObject>();

        wallDownActualSize = wallDownGO.GetComponent<MeshRenderer>().bounds.size.z;
        hallWayZMeshSize = churchHallway.GetComponentInChildren<MeshRenderer>().bounds.size.z;
        
        //Debug.Log($"Hall way actual width: {hallWayZMeshSize}");
        //Debug.Log($"Wall down actual width: {wallDownActualSize}");
        
        // Init 3 instances for the least size, starting from the back reference
        currentHallWayZSize = 3 * hallWayZMeshSize;
        for (int i = 0; i < 3; i++)
        {
            var hallWayInstance = Instantiate(churchHallway, churchSpawnerParent.transform);
            var instanceWorldPos = new Vector3(backReference.position.x, backReference.position.y, 
                backReference.position.z + i * hallWayZMeshSize);
            hallWayInstance.transform.position = instanceWorldPos;
            _hallWayInstancesPos.Add(hallWayInstance, instanceWorldPos);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        // Keep updating the position of the beginning and end church positions
        churchBeginningInstance.transform.position = frontReference.position;
        churchEndInstance.transform.position = backReference.position;

        foreach (var key in _hallWayInstancesPos.Keys)
        {
            key.transform.position = new Vector3(backReference.position.x, backReference.position.y,
                _hallWayInstancesPos[key].z);
        }
        
        currentWallDownSize = wallDownGO.GetComponent<MeshRenderer>().bounds.size.z;
        
        // Check to see if we need to remove anything dynamic hallways
        CheckAndRemoveTrailingHallWays();

        if ((currentWallDownSize - currentHallWayZSize) > 2 * hallWayZMeshSize)
        {
            // Instantiate 2 new hall way objects on either side
            var hallWayBackNewInstance = Instantiate(churchHallway, churchSpawnerParent.transform);
            hallWayBackNewInstance.transform.position = backReference.position;
            _hallWayBackDynamicInstances.Add(hallWayBackNewInstance);
            var hallWayFrontNewInstance = Instantiate(churchHallway, churchSpawnerParent.transform);
            _hallWayFrontDynamicInstances.Add(hallWayFrontNewInstance);
            var posOfNewFrontInstance = new Vector3(backReference.position.x, backReference.position.y, 
                backReference.position.z + (2 + _hallWayBackDynamicInstances.Count + _hallWayFrontDynamicInstances.Count) * hallWayZMeshSize);
            hallWayFrontNewInstance.transform.position = posOfNewFrontInstance;

            // Update current hall way size
            currentHallWayZSize = currentHallWayZSize + 2 * hallWayZMeshSize;
        }
    }

    public void DestroyAllInstantiatedPrefabsOnDestroy()
    {
        DestroyImmediate(churchBeginningInstance);
        DestroyImmediate(churchEndInstance);

        foreach (var hallway in _hallWayInstancesPos.Keys)
        {
            DestroyImmediate(hallway);
        }

        foreach (var bHallway in _hallWayBackDynamicInstances)
        {
            DestroyImmediate(bHallway);
        }

        foreach (var fHallway in _hallWayFrontDynamicInstances)
        {
            DestroyImmediate(fHallway);            
        }
    }

    private void CheckAndRemoveTrailingHallWays()
    {
        var totalHallWayNumbers = 3 + _hallWayBackDynamicInstances.Count + _hallWayFrontDynamicInstances.Count;
        if (currentWallDownSize < (3 + _hallWayBackDynamicInstances.Count + _hallWayFrontDynamicInstances.Count) *
            hallWayZMeshSize && totalHallWayNumbers > 3)
        {
            // Remove the last two added hall ways
            Debug.Log("Removing last two!");
            var lastBackHallWay = _hallWayBackDynamicInstances[_hallWayBackDynamicInstances.Count - 1];
            var lastFrontHallWay = _hallWayFrontDynamicInstances[_hallWayFrontDynamicInstances.Count - 1];
            _hallWayBackDynamicInstances.RemoveAt(_hallWayBackDynamicInstances.Count - 1);
            _hallWayFrontDynamicInstances.RemoveAt(_hallWayFrontDynamicInstances.Count - 1);
            DestroyImmediate(lastBackHallWay);
            DestroyImmediate(lastFrontHallWay);
            
            // Update current hall way size
            currentHallWayZSize = currentHallWayZSize - 2 * hallWayZMeshSize;
        }
    }
    
}
