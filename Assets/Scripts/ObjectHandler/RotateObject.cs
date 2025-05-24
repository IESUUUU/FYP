using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateObject : MonoBehaviour
{
    private bool firstRotate = false;
    [SerializeField] private GameObject objectRotate;
    [SerializeField] private GameObject img_rotate;
    [SerializeField] private GameObject img_cancelRotate;
    public bool isActive = false;
    private Vector2 touchDeltaPosition;


    void Start()
    {
        if (!PlayerPrefs.HasKey(Utility.FirstRotate) || PlayerPrefs.GetInt(Utility.FirstRotate) == 1)
        {
            PlayerPrefs.SetInt(Utility.FirstRotate, 1);
            firstRotate = true;
        }
        else if (PlayerPrefs.GetInt(Utility.FirstRotate) == 0)
        {
            firstRotate = false;
        }
    }
    public void Rotate()
    {
        //counter++;
        if (objectRotate.tag == "cube")
        {
            Debug.Log("rotate inactivated");

            // update button image
            img_cancelRotate.SetActive(false);
            img_rotate.SetActive(true);

            // update collider status
            GetComponent<Collider>().enabled = false;

            // update tag
            objectRotate.tag = "Untagged";

            // update status 
            isActive = false;
        }
        else
        {
            Debug.Log("rotate activated");

            // update button image
            img_cancelRotate.SetActive(true);
            img_rotate.SetActive(false);

            // update collider status
            GetComponent<Collider>().enabled = true;

            // update tag
            objectRotate.tag = "cube";

            if (firstRotate)
            {
                Utility.DisplayMessage("rotate", "cut");
                PlayerPrefs.SetInt(Utility.FirstRotate, 0);
                firstRotate = false;
            }
            // update status 
            isActive = true;
        }
    }

    void Update()
    {
        if (isActive)
        {
            if (Input.touchCount == 1)
            {
                Touch screenTouch = Input.GetTouch(0);

                if (screenTouch.phase == TouchPhase.Moved)
                {
                    touchDeltaPosition = screenTouch.deltaPosition;
                    Vector3 rotationAxis = new Vector3(touchDeltaPosition.y, -touchDeltaPosition.x, 0);
                    float rotationAngle = touchDeltaPosition.magnitude * 0.35f;
                    objectRotate.transform.Rotate(rotationAxis, rotationAngle, Space.World);
                }

                if (screenTouch.phase == TouchPhase.Ended)
                {
                    isActive = false;
                }
            }
        }
    }
}
