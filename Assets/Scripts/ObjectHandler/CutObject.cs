using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutObject : MonoBehaviour
{
    private bool firstCut = false;
    [SerializeField] private LineRenderer trailPrefab = null;
    [SerializeField] private Camera Cam;
    [SerializeField] private float clearSpeed = 1;
    [SerializeField] private float distanceFromCamera = 1;

    private LineRenderer currentTrail;
    private List<Vector3> points = new List<Vector3>();
    private bool activeCut = false;
    private int counter = 0;
    [SerializeField] private GameObject img_cut;
    [SerializeField] private GameObject img_cancelCut;
    public AudioClip cutSound;
    private AudioSource audioSource;
    [Header("UI_InnerBone")]
    [SerializeField] private GameObject[] showObjects;
    [SerializeField] private GameObject[] hideObjects;

    void Start()
    {
        if (!PlayerPrefs.HasKey(Utility.FirstCut) || PlayerPrefs.GetInt(Utility.FirstCut) == 1)
        {
            PlayerPrefs.SetInt(Utility.FirstCut, 1);
            firstCut = true;
        }
        else if (PlayerPrefs.GetInt(Utility.FirstCut) == 0)
        {
            firstCut = false;
        }
    }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = cutSound;
    }
    public void CutEffect()
    {
        if (activeCut)
        {
            Debug.Log("Cut inactivated");

            // update button image
            img_cancelCut.SetActive(false);
            img_cut.SetActive(true);

            // update active status
            activeCut = false;
        }
        else
        {
            Debug.Log("Cut activated");

            // update button image
            img_cut.SetActive(false);
            img_cancelCut.SetActive(true);

            if (firstCut)
            {
                Utility.DisplayMessage("cut", "rotate");
                PlayerPrefs.SetInt(Utility.FirstCut, 0);
                firstCut = false;
            }
            // update active status
            activeCut = true;
        }
    }
    void Update()
    {
        if (activeCut)
        {
            counter++;
            if (Input.GetMouseButtonDown(0)) //same with TouchPhase.Began
            {
                DestroyCurrentTrail();
                CreateCurrentTrail();
                AddPoint();
            }

            if (Input.GetMouseButton(0)) //same with Input.touchCount == 1, Input.touchCount > 0
            {
                AddPoint();
            }

            UpdateTrailPoints();
            ClearTrailPoints();

            if (Input.GetMouseButtonUp(0) && counter > 1 && points.Count > 1) //same with TouchPhase.Ended
            {
                StartCoroutine(turnScreen());
            }
        }
    }

    private void DestroyCurrentTrail()
    {
        if (currentTrail != null)
        {
            Destroy(currentTrail.gameObject);
            currentTrail = null;
            points.Clear();
        }
    }

    private void CreateCurrentTrail()
    {
        currentTrail = Instantiate(trailPrefab);
        currentTrail.transform.SetParent(transform, true);
    }

    private void AddPoint()
    {
        Vector3 mousePosition = Input.mousePosition;
        points.Add(Cam.ViewportToWorldPoint(new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, distanceFromCamera)));
    }

    private void UpdateTrailPoints()
    {
        if (currentTrail != null && points.Count > 1)
        {
            currentTrail.positionCount = points.Count;
            currentTrail.SetPositions(points.ToArray());
        }
        else
        {
            DestroyCurrentTrail();
        }
    }

    private void ClearTrailPoints()
    {
        float clearDistance = Time.deltaTime * clearSpeed;
        while (points.Count > 1 && clearDistance > 0)
        {
            float distance = (points[1] - points[0]).magnitude;
            if (clearDistance > distance)
            {
                points.RemoveAt(0);
            }
            else
            {
                points[0] = Vector3.Lerp(points[0], points[1], clearDistance / distance);
            }
            clearDistance -= distance;
        }
    }

    private IEnumerator turnScreen()
    {
        audioSource.Play();
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < hideObjects.Length; i++)
        {
            hideObjects[i].SetActive(false);
        }
        for (int i = 0; i < showObjects.Length; i++)
        {
            showObjects[i].SetActive(true);
        }
    }

    void OnDisable()
    {
        DestroyCurrentTrail();
    }
}
