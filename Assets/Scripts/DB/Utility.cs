using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Utility : MonoBehaviour
{
    //Timestamp
    public const string Timestamp = "Timestamp";

    //Notes
    public const string NotesComminuted = "Comminuted";
    public const string NotesTransverse = "Transverse";
    public const string NotesSpiral = "Spiral";
    public const string NotesOblique = "Oblique";
    public const string NotesIntactBone = "IntactBone";
    public const string NotesInnerBone = "InnerBone";

    //Quiz
    public const float ResolutionDelayTime = 1;
    public const string PrefsQuizHighScore = "Quiz_HighScore_Value";

    //User
    public const string PrefsUserID = "User_ID";
    public const string PrefsUserEmail = "User_Email";
    public const string PrefUsername = "Username";
    public const string PrefPassword = "Password";

    //Sound
    public const string Mute = "mute";

    //Check if first time action
    public const string FirstRotate = "FirstRotate";
    public const string FirstCut = "FirstCut";

    //Functions
    public static IEnumerator uploadFirebase()
    {
        UnityWebRequest request = new UnityWebRequest("http://google.com");
        yield return request.SendWebRequest();
        if (request.error == null)
        {
            var script = GameObject.FindGameObjectWithTag("DBManager").GetComponent<DBManager>();
            script.SyncButton();
        }
    }

    public static void DisplayMessage(string disablefeature, string enablefeature)
    {
        var script = GameObject.Find("DBManager").GetComponent<DBManager>();
        script.DisplayMessage("Alert", "You have to <b>DISABLE</b> the <color=#B5E61D>" + disablefeature 
            + " </color>feature <b>BEFORE</b> enabling the <color=#79E616>"
            + enablefeature + " </color>feature.");
    }

    public static void SavePrefsTimestamp()
    {
        // Get the current timestamp
        int timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        // Save the timestamp in PlayerPrefs
        PlayerPrefs.SetInt("Timestamp", timestamp);
    }
}
