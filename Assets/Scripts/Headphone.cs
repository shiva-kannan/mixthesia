using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Headphone : MonoBehaviour
{
    [SerializeField] OpeningScene os;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // For dev testing
        if(Input.GetKey(KeyCode.H))
        {
            os.OpenUp();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            os.OpenUp();
        }
    }
}
