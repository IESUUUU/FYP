using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AudioSource))]

public class PlayVideo : MonoBehaviour
{
    // for video panel (fullscreen)
    [SerializeField] GameObject videoPanel;
    [SerializeField] GameObject horizontalTitle;
    [SerializeField] private int gridIndex;
    [SerializeField] private GameObject gridLayout;
    private GridLayoutGroup gridLayoutGroup;
    private Vector2 initGridCellSize;
    private RectTransform videoPanelRecTransform;
    private Vector2 initAnchorMin;
    private Vector2 initAnchorMax;
    private Vector2 initPivot;
    private Vector2 initOffsetMin;
    private Vector2 initOffsetMax;
    private int initSiblingIndex;
    [SerializeField] private CanvasScaler mainCanvas;

    //Raw Image to Show Video Images [Assign from the Editor]
    [SerializeField] private RawImage image;
    //Video To Play [Assign from the Editor]
    [SerializeField] private VideoClip videoToPlay;
    [SerializeField] private GameObject img_play;
    [SerializeField] private GameObject img_pause;
    [SerializeField] private GameObject title_video;

    [SerializeField] private GameObject btnPlayButton;
    
    private Vector2 initPosition;
    [SerializeField] private GameObject btnFullscreen;
    [SerializeField] private GameObject btnMinscreen;

    private VideoPlayer videoPlayer;
    private VideoSource videoSource;
    private AudioSource audioSource;

    private bool activePlay = false;
    void Awake()
    {
        // attach Grid Layout Group
        gridLayoutGroup = gridLayout.GetComponent<GridLayoutGroup>();
        // get initial grid cell size
        initGridCellSize = gridLayoutGroup.cellSize;
        // attach RectTransform of video panel
        videoPanelRecTransform = videoPanel.GetComponent<RectTransform>();
        // get initial RectTransform values (Video type panel)
        initAnchorMin = videoPanelRecTransform.anchorMin;
        initAnchorMax = videoPanelRecTransform.anchorMax;
        initPivot = videoPanelRecTransform.pivot;
        initOffsetMin = videoPanelRecTransform.offsetMin;
        initOffsetMax = videoPanelRecTransform.offsetMax;
        // get initial sibling index (video type panel)
        initSiblingIndex = videoPanel.transform.GetSiblingIndex();

        // get initial RectTransform of play button (this may cause fullscreen button not working)
        /*rectTransform = GameObject.FindGameObjectWithTag("btnPlay").GetComponent<RectTransform>();
        initAnchorMin = rectTransform.anchorMin;
        initAnchorMax = rectTransform.anchorMax;
        initPivot = rectTransform.pivot;
        initPosition = btnPlayButton.transform.position;*/

        //initPosPlayButton = btnPlayButton.transform.position;
        //Debug.Log(initPosition);
    }
    void Start()
    {
        StartCoroutine(setUpVideo());
    }
    private IEnumerator setUpVideo()
    {
        // add videoplayer and audiosource
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        audioSource = gameObject.AddComponent<AudioSource>();

        videoPlayer.playOnAwake = false;
        audioSource.playOnAwake = false;

        // play from video clip not from url
        videoPlayer.source = VideoSource.VideoClip;

        // Set Audio Output to AudioSource
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // Assign the Audio from Video to AudioSource to be played
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);


        // Set video To Play then prepare Audio to prevent Buffering
        videoPlayer.clip = videoToPlay;
        videoPlayer.Prepare();

        // Wait until video is prepared
        while (!videoPlayer.isPrepared)
        {
            //Debug.Log("Preparing Video");
            yield return null;
        }
        Debug.Log("Done Preparing Video");

        // Assign the Texture from Video to RawImage to be displayed
        image.texture = videoPlayer.texture;
    }
    private void playVideo()
    {
        videoPlayer.Play();
        audioSource.Play();
    }
    private void pauseVideo()
    {
        videoPlayer.Pause();
        audioSource.Pause();
    }
    public void togglePlayButton()
    {
        if (activePlay)
        {
            Debug.Log("Video paused");
            // update button image
            img_pause.SetActive(false);
            img_play.SetActive(true);

            // update title visibility
            title_video.SetActive(true);

            // update active status
            activePlay = false;

            // set position of play button
            /*rectTransform.anchorMin = initAnchorMin;
            rectTransform.anchorMax = initAnchorMax;
            rectTransform.pivot = initPivot;
            rectTransform.position = initPosition;*/
        }
        else
        {
            Debug.Log("Play button activated");
            // update button image
            img_pause.SetActive(true);
            img_play.SetActive(false);

            // update title visibility
            title_video.SetActive(false);

            // update active status
            activePlay = true;
            
            // set position of play button
            /*rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.position = new Vector2(17.5f, 17.5f);*/
        }
    }

    public void Fullscreen()
    {
        // set other videos in the gridlayout to be invisible except the one is being played
        ShowHideGridVideo("hide");
        btnMinscreen.SetActive(true);
        btnFullscreen.SetActive(false);

        // rotate screen and set full screen
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        //image.uvRect = new Rect(0.5f, 0.5f, Screen.width, Screen.height); // newX, newY, newW, newH

        // change grid cell size to make full screen achievable
        gridLayoutGroup.cellSize = new Vector2(mainCanvas.referenceResolution.x, mainCanvas.referenceResolution.y);

        //set grid layout and video panel to top
        SetTopFullScreenLayout();

        //gridLayoutGroup.transform.position = new Vector3(UI_Video_position.x, UI_Video_position.y, UI_Video_position.z + 1);
        //Debug.Log("GRID POSITION: " + gridLayoutGroup.transform.position);

        //Debug.Log("width: " + Screen.width + "\nheight: " + Screen.height);
        //Debug.Log("x: " + mainCanvas.referenceResolution.x + "\ny: " + mainCanvas.referenceResolution.y);
        //image.uvRect = new Rect(0.5f, 0.5f, mainCanvas.referenceResolution.x, mainCanvas.referenceResolution.y);

        //Debug.Log(image.uvRect);
        /*Debug.Log("Screen width: " + Screen.width //1560
            + "\nScreen Height" + Screen.height); //720*/
    }
    private void ShowHideGridVideo(string mode)
    {
        int size = gridLayoutGroup.transform.childCount;

        if (mode == "hide")
        {
            for (int i = 0; i < size; i++)
            {
                //gridLayoutGroup.cellSize = new Vector2(mainCanvas.referenceResolution.x, mainCanvas.referenceResolution.y);
                if (i != gridIndex)
                {
                    gridLayoutGroup.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                gridLayoutGroup.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    public void MinScreen()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        gridLayoutGroup.cellSize = initGridCellSize;
        ShowHideGridVideo("show");
        btnMinscreen.SetActive(false);
        btnFullscreen.SetActive(true);

        // set back layout
        SetBackScreenLayout(initSiblingIndex);
    }

    void SetTopFullScreenLayout()
    {
        var UI_Video = GameObject.FindGameObjectWithTag("UI_Video").transform;
        videoPanel.transform.SetParent(UI_Video);
        videoPanel.transform.SetAsLastSibling();
        
        videoPanelRecTransform.anchorMin = Vector2.zero;
        videoPanelRecTransform.anchorMax = Vector2.one;
        videoPanelRecTransform.pivot = new Vector2(0.5f, 0.5f);
        videoPanelRecTransform.offsetMin = Vector2.zero;
        videoPanelRecTransform.offsetMax = Vector2.zero;
    }
    void SetBackScreenLayout(int index)
    {
        var Video_Panel = GameObject.FindGameObjectWithTag("Video_Panel").transform;
        videoPanel.transform.SetParent(Video_Panel);
        videoPanel.transform.SetSiblingIndex(index);

        videoPanelRecTransform.anchorMin = initAnchorMin;
        videoPanelRecTransform.anchorMax = initAnchorMax;
        videoPanelRecTransform.pivot = initPivot;
        videoPanelRecTransform.offsetMin = initOffsetMin;
        videoPanelRecTransform.offsetMax = initOffsetMax;
    }
    void Update()
    {
        if (activePlay && videoPlayer != null)
        {
            // prevent unable to watch the video after several times of toggle between fullscreen and minscreen
            image.texture = videoPlayer.texture;
            // play video + fullscreen
            //image.uvRect = new Rect(Screen.width / 2, -Screen.height / 2, 0, 0);
            if (AudioListener.pause == true)
            {
                playVideo();
            }
            else
            {
                var script = GameObject.FindGameObjectWithTag("soundManager").GetComponent<BGMController>();
                script.SoundOnOff();
            }
            // set back to show all video after finish playing
        }
        else
        {
            pauseVideo();
        }
    }
}
