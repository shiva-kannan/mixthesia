using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeSceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LaunchMainScene()
    {
        SceneManager.LoadScene("Scenes/MainScene");
    }

    public void LaunchScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
    
    public void QuitApplication()
    {
        Application.Quit();
    }
}
