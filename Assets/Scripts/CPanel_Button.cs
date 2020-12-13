using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CPanel_Button : Clickable
{

    [SerializeField] UnityEvent onCickEvent;

    [Header("Button Look")]
    [SerializeField] Sprite normalLook;
    [SerializeField] Sprite flippedLook;
    [SerializeField] string normalText;
    [SerializeField] string flippedText;

    public bool isFlipped = false;
    private Vector3 ogSize = Vector3.zero;
    private SpriteRenderer mySR;
    private GameObject myTMPObj;

    // Start is called before the first frame update
    private void Start()
    {
        mySR = GetComponent<SpriteRenderer>();
        ogSize = transform.localScale;
        myTMPObj = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    public override void AdditionalUpdate()
    {
        Color newColor = Color.Lerp(mySR.color, Color.white, 2.5f * Time.deltaTime);
        mySR.color = newColor;


        if (isFlipped)
        {
            mySR.sprite = flippedLook;
            myTMPObj.GetComponent<TextMeshPro>().text = flippedText;
        }
        else
        {
            mySR.sprite = normalLook;
            myTMPObj.GetComponent<TextMeshPro>().text = normalText;
        }
    }

    public override void Hover()
    {
        Vector3 newScale = Vector3.Lerp(transform.localScale, 1.25f * ogSize, 7f * Time.deltaTime);
        transform.localScale = newScale;

        myTMPObj.SetActive(true);
    }

    public override void Normal()
    {
        Vector3 newScale = Vector3.Lerp(transform.localScale, ogSize, 7f * Time.deltaTime);
        transform.localScale = newScale;

        myTMPObj.SetActive(false);
    }

    public override void OnTriggerDown()
    {
        if (onCickEvent != null)
        {
            onCickEvent.Invoke();

            mySR.color = Color.yellow;
            isFlipped = !isFlipped;
        }
    }
}
