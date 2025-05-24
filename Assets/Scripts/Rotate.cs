using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    /*protected Vector3 posLastFrame;
    public Camera UICam;

    public float rotatespeed = 10f;
    private float startingPosition;
    private int turnspeed;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            posLastFrame = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            var delta = Input.mousePosition - posLastFrame;
            posLastFrame = Input.mousePosition;

            var axis = Quaternion.AngleAxis(-90f, Vector3.forward) * delta;
            transform.rotation = Quaternion.AngleAxis(delta.magnitude * 0.1f, axis) * transform.rotation;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startingPosition = touch.position.x;
                    break;
                case TouchPhase.Moved:
                    if (startingPosition > touch.position.x)
                    {
                        transform.Rotate(Vector3.back, -turnspeed * Time.deltaTime);
                    }
                    else if (startingPosition < touch.position.x)
                    {
                        transform.Rotate(Vector3.back, rotatespeed * Time.deltaTime);
                    }
                    break;
                case TouchPhase.Ended:
                    Debug.Log("Touch Phase Ended.");
                    break;
            }
        }
    }*/

    float speedRotation = 10;
    private void OnMouseDrag()
    {
        float x = Input.GetAxis("Mouse X") * speedRotation * Mathf.Deg2Rad;
        transform.Rotate(Vector3.down, x);
        float y = Input.GetAxis("Mouse Y") * speedRotation * Mathf.Deg2Rad;
        transform.Rotate(Vector3.right, y);
    }

    public bool isActive = false;

    void Update()
    {

        if (isActive)
        {

            if (Input.touchCount == 1)
            {
                Touch screenTouch = Input.GetTouch(0);

                if (screenTouch.phase == TouchPhase.Moved)
                {
                    transform.Rotate(screenTouch.deltaPosition.y, screenTouch.deltaPosition.x, 0f);
                }

                if (screenTouch.phase == TouchPhase.Ended)
                {
                    isActive = false;
                }
            }
        }
    }
}
