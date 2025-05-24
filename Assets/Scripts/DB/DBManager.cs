using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using Firebase.Extensions;
using Firebase.Database;
using UnityEngine.Networking;
using System.Linq;
using System;

public class DBManager : MonoBehaviour
{
    public static DBManager dbManager;

    [Header("Menu")]
    public GameObject signInButton;
    public GameObject loggedInContainer;
    public GameObject signOutButton;
    public GameObject profileButton;
    public TMP_Text welcomeText;

    [Header("ForgetPassword")]
    public TMP_InputField forgetEmailField;
    public GameObject forgetMessageContainer;
    public TMP_Text forgetMessage;

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

    [Header("Notes")]
    public TMP_InputField notesIntactBone;
    public TMP_InputField notesInnerBone;
    public TMP_InputField notesObliqueDisplaced;
    public TMP_InputField notesSpiral;
    public TMP_InputField notesTransverse;
    public TMP_InputField notesComminuted;

    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public GameObject signInMsgContainer;
    public TMP_Text signInMessage;

    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public GameObject signUpMsgContainer;
    public TMP_Text signUpMessage;
    public GameObject errEmail;
    public GameObject errPassword;
    public GameObject errUsername;
    public GameObject errPasswordVerify;

    [Header("Profile")]
    public GameObject profileScreen;
    public GameObject profileTitle;
    public GameObject msgContainer;
    public TMP_Text profileMessage;
    public TMP_Text displayEmail;
    public TMP_Text displayUsername;
    public TMP_InputField usernameToUpdate;
    public TMP_InputField emailToUpdate;
    public TMP_InputField userPassword;
    public TMP_InputField currentPassword;
    public TMP_InputField passwordToUpdate;
    public TMP_InputField verifyPassword;

    [Header("Message")]
    public TMP_Text title;
    public TMP_Text message;
    public GameObject messagePanel;
    public GameObject popUpMessage;
    public TMP_Text textPopUpMessage;

    [Header("Leaderboard")]
    public Transform leaderboardContent;
    public RowUI rowUI;

    private void Start()
    {
        if (dbManager == null)
        {
            dbManager = this;
            DontDestroyOnLoad(gameObject); // Mark the GameObject as persistent
        }
        dbManager = GetComponent<DBManager>();
    }
    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // Retrieve the updated data from the snapshot
        if (args.Snapshot != null && args.Snapshot.Exists)
        {
            //Debug.Log("Start time (retrieve real time data): " + Time.time * 1000);
            StartCoroutine(CheckAndUploadData());
            //Debug.Log("Finish time (retrieve real time data): " + Time.time * 1000);
        }
    }
    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
        DBreference.ValueChanged += HandleValueChanged;
    }

    IEnumerator CheckAndUploadData()
    {
        // Check for network connectivity
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connectivity");
            yield break;
        }

        string localNotesIntact = PlayerPrefs.GetString(Utility.NotesIntactBone);
        string localNotesInner = PlayerPrefs.GetString(Utility.NotesInnerBone);
        string localNotesSpiral = PlayerPrefs.GetString(Utility.NotesSpiral);
        string localNotesOblique = PlayerPrefs.GetString(Utility.NotesOblique);
        string localNotesTransverse = PlayerPrefs.GetString(Utility.NotesTransverse);
        string localNotesComminuted = PlayerPrefs.GetString(Utility.NotesComminuted);
        int localQuizScore = PlayerPrefs.GetInt(Utility.PrefsQuizHighScore);

        // check if user has logged in
        if (User != null)
        {
            // check Firebase for existing data
            var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            DataSnapshot snapshot = DBTask.Result;

            // check if user has logged in
            if (User != null)
            {
                if (snapshot.Child("notes").Exists)
                {
                    string cloudNotesIntact = snapshot.Child("notes").Child(Utility.NotesIntactBone).Value.ToString();
                    string cloudNotesInner = snapshot.Child("notes").Child(Utility.NotesInnerBone).Value.ToString();
                    string cloudNotesSpiral = snapshot.Child("notes").Child(Utility.NotesSpiral).Value.ToString();
                    string cloudNotesOblique = snapshot.Child("notes").Child(Utility.NotesOblique).Value.ToString();
                    string cloudNotesTransverse = snapshot.Child("notes").Child(Utility.NotesTransverse).Value.ToString();
                    string cloudNotesComminuted = snapshot.Child("notes").Child(Utility.NotesComminuted).Value.ToString();
                    int cloudQuizScore = Convert.ToInt32(snapshot.Child(Utility.PrefsQuizHighScore).Value);

                    // check if the cloud data is more recent
                    if (Convert.ToInt32(snapshot.Child(Utility.Timestamp).Value) > PlayerPrefs.GetInt(Utility.Timestamp))
                    {
                        PlayerPrefs.SetString(Utility.NotesIntactBone, cloudNotesIntact);
                        PlayerPrefs.SetString(Utility.NotesInnerBone, cloudNotesInner);
                        PlayerPrefs.SetString(Utility.NotesSpiral, cloudNotesSpiral);
                        PlayerPrefs.SetString(Utility.NotesOblique, cloudNotesOblique);
                        PlayerPrefs.SetString(Utility.NotesTransverse, cloudNotesTransverse);
                        PlayerPrefs.SetString(Utility.NotesComminuted, cloudNotesComminuted);
                        PlayerPrefs.SetInt(Utility.PrefsQuizHighScore, cloudQuizScore);
                        Utility.SavePrefsTimestamp();
                    }
                    else
                    {
                        DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesIntactBone).SetValueAsync(localNotesIntact);
                        DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesInnerBone).SetValueAsync(localNotesInner);
                        DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesOblique).SetValueAsync(localNotesOblique);
                        DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesSpiral).SetValueAsync(localNotesSpiral);
                        DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesTransverse).SetValueAsync(localNotesTransverse);
                        DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesComminuted).SetValueAsync(localNotesComminuted);
                        DBreference.Child("users").Child(User.UserId).Child(Utility.PrefsQuizHighScore).SetValueAsync(localQuizScore);
                        DBreference.Child("users").Child(User.UserId).Child(Utility.Timestamp).SetValueAsync(PlayerPrefs.GetInt(Utility.Timestamp));
                    }
                }
                // if there is no data in Firebase, upload local data
                else
                {
                    DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesIntactBone).SetValueAsync(localNotesIntact);
                    DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesInnerBone).SetValueAsync(localNotesInner);
                    DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesOblique).SetValueAsync(localNotesOblique);
                    DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesSpiral).SetValueAsync(localNotesSpiral);
                    DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesTransverse).SetValueAsync(localNotesTransverse);
                    DBreference.Child("users").Child(User.UserId).Child("notes").Child(Utility.NotesComminuted).SetValueAsync(localNotesComminuted);
                    DBreference.Child("users").Child(User.UserId).Child(Utility.PrefsQuizHighScore).SetValueAsync(localQuizScore);
                    DBreference.Child("users").Child(User.UserId).Child(Utility.Timestamp).SetValueAsync(PlayerPrefs.GetInt(Utility.Timestamp));
                }
                ShowNotesInfo();
            }
        }
    }
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != User)
        {
            bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && User != null)
            {
                Debug.Log("User signed out successfully");
            }
            User = auth.CurrentUser;
            if (signedIn)
            {
                Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            }
        }
    }
    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
    public void LeaderboardButton()
    {
        //Debug.Log("Start Time (Load Leaderboard): " + Time.time * 1000);
        StartCoroutine(LoadLeaderboard());
        //Debug.Log("Finish Time (Load Leaderboard): " + Time.time * 1000);
    }
    public void SignUpButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    public void SignInButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    public void SignOutButton()
    {
        StartCoroutine(SignOut());
    }
    public void ForgetPassword()
    {
        if (string.IsNullOrEmpty(forgetEmailField.text))
        {
            forgetMessageContainer.SetActive(true);
            forgetMessage.text = "Please enter your email";
            return;
        }
        ForgetPasswordSubmit(forgetEmailField.text);
    }
    public void SyncButton()
    {
        StartCoroutine(CheckAndUploadData());
    }
    public void UpdateProfileButton(string type)
    {
        switch (type)
        {
            case "username":
                StartCoroutine(UpdateUsernameAuth(usernameToUpdate.text));
                break;
            case "email":
                StartCoroutine(UpdateEmailAuth(emailToUpdate.text));
                break;
            case "password":
                StartCoroutine(UpdatePassword(currentPassword.text, passwordToUpdate.text, verifyPassword.text));
                break;
        }
    }
    public void DeleteAccountButton()
    {
        StartCoroutine(DeleteAccountAuth());  
    }

    //UI
    private void ChangeUI(string ui, string title)
    {
        var scriptUI = GameObject.Find(ui).GetComponent<ChangeUI>();
        var scriptTitle = GameObject.Find(title).GetComponent<ChangeTitle>();
        scriptUI.ShowHide();
        scriptTitle.changeTitle();
    }
    private void HomeScreen()
    {
        ChangeUI("UI_IntactFemur", "TitleIntactBone");
        SyncButton();
    }
    private void SignnedInScreen()
    {
        signInButton.SetActive(false);
        loggedInContainer.SetActive(true);
        signOutButton.SetActive(true);
        profileButton.SetActive(true);
        welcomeText.text = "Hi, " + User.DisplayName;

        //get data from firebase and set into local db
        StartCoroutine(LoadDataToPlayerPrefs());
    }
    private void SignedOutScreen()
    {
        signInButton.SetActive(true);
        loggedInContainer.SetActive(false);
        signOutButton.SetActive(false);
        profileButton.SetActive(false);
        SetEmptyToPlayerPrefs();
    }
    private void LeaderboardScreen()
    {
        ChangeUI("UI_Leaderboard", "TitleQuiz");
    }

    //Main Functions
    private IEnumerator Register(string _email, string _password, string _username)
    {
        UnityWebRequest request = new UnityWebRequest("http://google.com");
        yield return request.SendWebRequest();
        if (request.error == null)
        {
            if (_email == "")
            {
                SignUpError("email", "missing");
            }
            else if (_username == "")
            {
                //If the username field is blank show a warning
                SignUpError("username", "missing");
            }
            else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
            {
                //If the password does not match show a warning
                SignUpError("password", "unmatch");
            }
            else
            {
                if (validatePassword("register", passwordRegisterField.text, passwordRegisterVerifyField.text))
                {
                    //Call the Firebase auth signin function passing the email and password
                    var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

                    if (RegisterTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                        FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                        switch (errorCode)
                        {
                            case AuthError.MissingEmail:
                                SignUpError("email", "missing");
                                break;
                            case AuthError.MissingPassword:
                                SignUpError("password", "missing");
                                break;
                            case AuthError.WeakPassword:
                                SignUpError("password", "weak");
                                break;
                            case AuthError.EmailAlreadyInUse:
                                SignUpError("email", "occupied");
                                break;
                            case AuthError.InvalidEmail:
                                SignUpError("email", "invalid");
                                break;
                        }
                    }
                    else
                    {
                        //User has now been created
                        //Now get the result
                        User = RegisterTask.Result;

                        if (User != null)
                        {
                            //Create a user profile and set the username
                            UserProfile profile = new UserProfile { DisplayName = _username };

                            //Call the Firebase auth update user profile function passing the profile with the username
                            var ProfileTask = User.UpdateUserProfileAsync(profile);
                            //Wait until the task completes
                            yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                            if (ProfileTask.Exception != null)
                            {
                                //If there are errors handle them
                                Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                                FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                                signUpMessage.text = "Username Set Failed!";
                                signUpMsgContainer.SetActive(true);
                            }
                            else
                            {
                                //Username is now set
                                writeNewUser(User.UserId, User.DisplayName, User.Email);
                                SyncButton();
                                auth.SignOut();
                                //StartCoroutine(ShowTempMessage("Sign up successfully!"));
                                DisplayMessage("Message", "Sign up successfully!\n Sign in now.");
                                //Now return to login screen
                                ChangeUI("UI_SignIn","TitleIntactBone");
                            }
                        }
                    }
                }
            }
        }
        else
        {
            signUpMessage.text = "Network error.";
            signUpMsgContainer.SetActive(true);
        }
    }
    private IEnumerator Login(string _email, string _password)
    {
        UnityWebRequest request = new UnityWebRequest("http://google.com");
        yield return request.SendWebRequest();
        if (request.error == null)
        {
            //Call the Firebase auth signin function passing the email and password
            var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
            signUpMsgContainer.SetActive(false);

            if (LoginTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
                FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Login Failed!\n";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message += "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message += "Missing Password";
                        break;
                    case AuthError.WrongPassword:
                        message += "Wrong Password";
                        break;
                    case AuthError.InvalidEmail:
                        message += "Invalid Email";
                        break;
                    case AuthError.UserNotFound:
                        message += "Account does not exist";
                        break;
                }
                signInMessage.text = message;
                signInMsgContainer.SetActive(true);
            }
            else
            {
                //User is now logged in
                //Now get the result
                User = LoginTask.Result;

                //save in playerprefs
                PlayerPrefs.SetString(Utility.PrefsUserID, User.UserId);
                PlayerPrefs.SetString(Utility.PrefsUserEmail, User.Email);
                PlayerPrefs.SetString(Utility.PrefUsername, User.DisplayName);
                StartCoroutine(LoadDataToPlayerPrefs());

                signInMsgContainer.SetActive(false);
                //turn screen
                HomeScreen();
                SignnedInScreen();
                DisplayMessage("Message", "You have signed as " + User.DisplayName);
            }
        }
        else
        {
            signInMessage.text = "Network error.";
            signInMsgContainer.SetActive(true);
        }
    }
    private IEnumerator SignOut()
    {
        UnityWebRequest request = new UnityWebRequest("http://google.com");
        yield return request.SendWebRequest();
        if (request.error == null)
        {
            SyncButton();
            auth.SignOut();
            ClearSignInInfo();
            SetEmptyToPlayerPrefs();
            SignedOutScreen();
            HomeScreen();
            StartCoroutine(DisplayTempMessage("Signed out"));
        }
        else
        {
            DisplayMessage("Error", "Network error\nUnable to sign out.");
            //StartCoroutine(ShowTempMessage("Network error. Unable to sign out."));
        }
    }
    private void ForgetPasswordSubmit(string email)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SendPasswordResetEmailAsync was canceled");
                return;
            }
            if (task.IsFaulted)
            {
                foreach (var e in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = e as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        string message = "Action Failed!\n";
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        switch (errorCode)
                        {
                            case AuthError.MissingEmail:
                                message += "Missing Email";
                                break;
                            case AuthError.MissingPassword:
                                message += "Missing Password";
                                break;
                            case AuthError.WrongPassword:
                                message += "Wrong Password";
                                break;
                            case AuthError.InvalidEmail:
                                message += "Invalid Email";
                                break;
                            case AuthError.UserNotFound:
                                message += "Account does not exist";
                                break;
                        }
                        forgetMessage.text = message;
                        forgetMessageContainer.SetActive(true);
                    }
                }
                return;
            }
            forgetMessageContainer.SetActive(true);
            forgetMessage.text = "Successfully send email for resetting password";
        });
    }
    private IEnumerator DeleteAccountAuth()
    {
        if (User != null)
        {
            string userID = User.UserId;
            Debug.Log("UserID = " + userID);
            ReAuthenticate(User.Email, userPassword.text);
            var DeleteTask = User.DeleteAsync();

            yield return new WaitUntil(predicate: () => DeleteTask.IsCompleted);

            msgContainer.SetActive(false);
            if (DeleteTask.Exception != null)
            {
                profileMessage.text = "Account delete failed.Check your password.";
                Debug.LogWarning(message: $"Failed to register task with {DeleteTask.Exception}");
            }
            else
            {
                StartCoroutine(DeleteAccountDatabase(userID));
            }
        }
    }
    private IEnumerator DeleteAccountDatabase(string userid)
    {
        var DBTask = DBreference.Child("users").Child(userid).RemoveValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            ClearSignInInfo();
            SetEmptyToPlayerPrefs();
            SignedOutScreen();
            HomeScreen();
            Debug.Log("Account deleted successfully.");
        }
    }
    private IEnumerator UpdateEmailAuth(string newEmail)
    {
        if (User != null)
        {
            if (newEmail != User.Email)
            {
                if (userPassword.text.Length > 0)
                {
                    ReAuthenticate(User.Email, userPassword.text);
                    var EmailTask = User.UpdateEmailAsync(newEmail);
                    yield return new WaitUntil(predicate: () => EmailTask.IsCompleted);

                    if (EmailTask.Exception != null)
                    {
                        FirebaseException firebaseEx = EmailTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        string message = "";
                        switch (errorCode)
                        {
                            case AuthError.InvalidEmail:
                                message = "Invalid Email";
                                break;
                            case AuthError.MissingEmail:
                                message = "Missing Email";
                                break;
                            case AuthError.MissingPassword:
                                message = "Missing Password";
                                break;
                            case AuthError.EmailAlreadyInUse:
                                message = "Email already in used";
                                break;
                            case AuthError.WrongPassword:
                                message = "Wrong Password";
                                break;
                        }
                        if (message.Length > 0)
                        {
                            profileMessage.text = message;
                        }
                        else
                        {
                            profileMessage.text = "Password Incorrect.";
                        }
                        Debug.LogWarning(message: $"Failed to register task with {EmailTask.Exception}");
                    }
                    else
                    {
                        StartCoroutine(UpdateEmailDatabase(emailToUpdate.text));
                        profileMessage.text = "Email updated successfully.";
                        Debug.Log("Email updated successfully.");
                    }
                }
                else
                {
                    profileMessage.text = "Please enter your password.";
                }
            }
            else
            {
                profileMessage.text = "Please enter a different email.";
            }
            msgContainer.SetActive(true);
        }
    }
    private IEnumerator UpdateEmailDatabase(string newEmail)
    {
        if (!string.IsNullOrEmpty(newEmail))
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("email").SetValueAsync(newEmail);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                PlayerPrefs.SetString(Utility.PrefsUserEmail, newEmail);
            }
        }
    }
    private IEnumerator UpdatePassword(string currentPassword, string newPassword, string verifyPassword)
    {
        if (!string.IsNullOrEmpty(currentPassword))
        {
            ReAuthenticate(User.Email, currentPassword);
            if (validatePassword("update", newPassword, verifyPassword))
            {
                var PasswordTask = User.UpdatePasswordAsync(newPassword);

                yield return new WaitUntil(predicate: () => PasswordTask.IsCompleted);

                if (PasswordTask.Exception != null)
                {
                    profileMessage.text = "Password update failed. Wrong password.";
                    Debug.LogWarning(message: $"Failed to register task with {PasswordTask.Exception}");
                }
                else
                {
                    profileMessage.text = "Password updated successfully.";
                    Debug.Log("Password updated successfully.");
                }
            }
        }
        else
        {
            profileMessage.text = "Please fill in all the blank.";
        }
        msgContainer.SetActive(true);
    }
    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        if (!string.IsNullOrEmpty(_username))
        {
            if (User.DisplayName != _username)
            {
                UserProfile profile = new UserProfile { DisplayName = _username };

                var ProfileTask = User.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                if (ProfileTask.Exception != null)
                {
                    profileMessage.text = "Username update failed.";
                    Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                }
                else
                {
                    StartCoroutine(UpdateUsernameDatabase(usernameToUpdate.text));
                }
            }
            else
            {
                profileMessage.text = "Please enter a different username.";
            }
        }
        else
        {
            profileMessage.text = "Username cannot be blank.";
        }
        msgContainer.SetActive(true);
    }
    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        if (!string.IsNullOrEmpty(_username))
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                PlayerPrefs.SetString(Utility.PrefUsername, _username);
                welcomeText.text = "Hi, " + _username;
                profileMessage.text = "Username updated successfully.";
                Debug.Log("Username updated successfully.");
            }
            msgContainer.SetActive(true);
        }
    }

    //Other Functions
    private void writeNewUser(string userId, string name, string email)
    {
        User user = new User(name, email);
        string json = JsonUtility.ToJson(user);

        DBreference.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }
    private void ReAuthenticate(string email, string password)
    {
        Credential credential = EmailAuthProvider.GetCredential(email, password);

        if (User != null)
        {
            User.ReauthenticateAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("ReauthenticateAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("ReauthenticateAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User reauthenticated successfully.");
            });
        }
    }
    private bool validatePassword(string type, string newPassword, string verifyPassword)
    {
        string msg = "";
        if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(verifyPassword))
        {
            if (newPassword.Length >= 8)
            {
                if (StringContainDigit(newPassword) && StringContainLetter(newPassword))
                {
                    if (newPassword.Equals(verifyPassword))
                    {
                        return true;
                    }
                    else
                    {
                        msg = "Password does not match.";
                        MessageValidatePassword(type, msg);

                    }
                }
                else
                {
                    msg = "Password must contain both digits and letters.";
                    MessageValidatePassword(type, msg);
                }
            }
            else
            {
                msg = "Password length must at least 8.";
                MessageValidatePassword(type, msg);
            }
        }
        else
        {
            msg = "Please fill in all the blank.";
            MessageValidatePassword(type, msg);
        }
        return false;
    }
    private void MessageValidatePassword(string type, string msg)
    {
        if (type == "register")
        {
            signUpMessage.text = msg;
            signUpMsgContainer.SetActive(true);
        }
        else if (type == "update")
        {
            profileMessage.text = msg;
            msgContainer.SetActive(true);
        }
        Debug.LogWarning(msg);
    }
    private bool StringContainDigit(string str)
    {
        var ch = str.ToCharArray();
        foreach (char c in ch)
        {
            if (char.IsDigit(c))
            {
                return true;
            }
        }
        return false;
    }
    private bool StringContainLetter(string str)
    {
        var ch = str.ToCharArray();
        foreach (char c in ch)
        {
            if (char.IsLetter(c))
            {
                return true;
            }
        }
        return false;
    }
    public void ShowUserProfile()
    {
        displayUsername.text = PlayerPrefs.GetString(Utility.PrefUsername);
        displayEmail.text = PlayerPrefs.GetString(Utility.PrefsUserEmail);
        usernameToUpdate.text = PlayerPrefs.GetString(Utility.PrefUsername);
        emailToUpdate.text = PlayerPrefs.GetString(Utility.PrefsUserEmail);
    }
    private IEnumerator LoadDataToPlayerPrefs()
    {
        //get logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //data empty
            //if playerprefs is not empty
            if (!isPlayerPrefsEmpty())
            {
                SyncButton();
            }
            else
            {
                SetEmptyToPlayerPrefs();
            }
        }
        else
        {
            //data retrieved
            DataSnapshot snapshot = DBTask.Result;
            if (snapshot.Child("notes").Exists)
            {
                PlayerPrefs.SetString(Utility.NotesIntactBone, snapshot.Child("notes").Child(Utility.NotesIntactBone).Value.ToString());
                PlayerPrefs.SetString(Utility.NotesInnerBone, snapshot.Child("notes").Child(Utility.NotesInnerBone).Value.ToString());
                PlayerPrefs.SetString(Utility.NotesOblique, snapshot.Child("notes").Child(Utility.NotesOblique).Value.ToString());
                PlayerPrefs.SetString(Utility.NotesSpiral, snapshot.Child("notes").Child(Utility.NotesSpiral).Value.ToString());
                PlayerPrefs.SetString(Utility.NotesTransverse, snapshot.Child("notes").Child(Utility.NotesTransverse).Value.ToString());
                PlayerPrefs.SetString(Utility.NotesComminuted, snapshot.Child("notes").Child(Utility.NotesComminuted).Value.ToString());
                PlayerPrefs.SetString(Utility.PrefsQuizHighScore, snapshot.Child(Utility.PrefsQuizHighScore).Value.ToString());
                ShowNotesInfo();
            }
        }
    }
    private IEnumerator LoadLeaderboard()
    {
        UnityWebRequest request = new UnityWebRequest("http://google.com");
        yield return request.SendWebRequest();
        if (request.error == null && User != null)
        {
            SyncButton();
            //get all user data ordered by quiz score
            var DBTask = DBreference.Child("users").OrderByChild(Utility.PrefsQuizHighScore).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                int counter = 0;
                //data retrieved
                DataSnapshot snapshot = DBTask.Result;
                //destroy any existing leaderboard elements
                foreach (Transform child in leaderboardContent.transform)
                {
                    Destroy(child.gameObject);
                }

                // loop through every user id
                foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse())
                {
                    ++counter;
                    string curUserEmail = childSnapshot.Child("email").Value.ToString();
                    string username = childSnapshot.Child("username").Value.ToString();
                    string scores = childSnapshot.Child(Utility.PrefsQuizHighScore).Value.ToString();

                    // instantiate new leaderboard elements
                    var row = Instantiate(rowUI, leaderboardContent).GetComponent<RowUI>();
                    // to highlight current user data
                    if (curUserEmail == User.Email)
                    {
                        row.rank.text = "<color=#FFFB00><b>" + counter.ToString() + "</b></color>";
                        row.username.text = "<color=#FFFB00><b>" + username + "</b></color>";
                        row.score.text = "<color=#FFFB00><b>" + scores + "</b></color>";
                    }
                    else
                    {
                        row.rank.text = counter.ToString();
                        row.username.text = username;
                        row.score.text = scores;
                    }
                }
                LeaderboardScreen();
            }
        }
        else if (User == null)
        {
            DisplayMessage("Message", "Please sign in with an account before proceeding.");
        }
        else
        {
            DisplayMessage("Error", "Network error");
        }
    }
    private bool isPlayerPrefsEmpty()
    {
        string noteIntact = PlayerPrefs.GetString(Utility.NotesIntactBone);
        string noteInner = PlayerPrefs.GetString(Utility.NotesInnerBone);
        string noteCommunicated = PlayerPrefs.GetString(Utility.NotesComminuted);
        string noteSpiral = PlayerPrefs.GetString(Utility.NotesSpiral);
        string noteTransverse = PlayerPrefs.GetString(Utility.NotesTransverse);
        string noteObliqueDisplace = PlayerPrefs.GetString(Utility.NotesOblique);
        int quizscore = PlayerPrefs.GetInt(Utility.PrefsQuizHighScore);

        if (noteIntact != "" || noteInner != "" || noteCommunicated != "" || noteSpiral != "" || noteTransverse != ""
            || noteObliqueDisplace != "" || quizscore != 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private void ShowNotesInfo()
    {
        notesIntactBone.text = PlayerPrefs.GetString(Utility.NotesIntactBone);
        notesInnerBone.text = PlayerPrefs.GetString(Utility.NotesInnerBone);
        notesObliqueDisplaced.text = PlayerPrefs.GetString(Utility.NotesOblique);
        notesSpiral.text = PlayerPrefs.GetString(Utility.NotesSpiral);
        notesTransverse.text = PlayerPrefs.GetString(Utility.NotesTransverse);
        notesComminuted.text = PlayerPrefs.GetString(Utility.NotesComminuted);
    }
    private void ClearSignInInfo()
    {
        PlayerPrefs.SetString(Utility.PrefsUserID, "");
        PlayerPrefs.SetString(Utility.PrefUsername, "");
        PlayerPrefs.SetString(Utility.PrefsUserEmail, "");
    }
    private void SetEmptyToPlayerPrefs()
    {
        //set empty to local db
        PlayerPrefs.SetString(Utility.NotesIntactBone, "");
        PlayerPrefs.SetString(Utility.NotesInnerBone, "");
        PlayerPrefs.SetString(Utility.NotesOblique, "");
        PlayerPrefs.SetString(Utility.NotesSpiral, "");
        PlayerPrefs.SetString(Utility.NotesComminuted, "");
        PlayerPrefs.SetString(Utility.NotesTransverse, "");
        PlayerPrefs.SetInt(Utility.Timestamp, 0);
        PlayerPrefs.SetInt(Utility.PrefsQuizHighScore, 0);
        ShowNotesInfo();
    }
    private void SignUpError(string error, string suberror)
    {
        if (error == "email")
        {
            errEmail.SetActive(true);
            errUsername.SetActive(false);
            errPassword.SetActive(false);
            errPasswordVerify.SetActive(false);
            if (suberror == "missing")
            {
                signUpMessage.text = "Missing Email";
            }
            else if (suberror == "occupied")
            {
                signUpMessage.text = "Email already in used";
            }
            else if (suberror == "invalid")
            {
                signUpMessage.text = "Invalid Email";
            }
        }
        else if (error == "username")
        {
            signUpMessage.text = "Missing Username";
            errUsername.SetActive(true);
            errEmail.SetActive(false);
            errPassword.SetActive(false);
            errPasswordVerify.SetActive(false);
        }
        else if (error == "password")
        {
            errUsername.SetActive(false);
            errEmail.SetActive(false);
            errPassword.SetActive(true);
            if (suberror == "unmatch")
            {
                errPasswordVerify.SetActive(true);
                signUpMessage.text = "Password does not match!";
            }
            else if (suberror == "missing")
            {
                signUpMessage.text = "Missing password";
            }
            else if (suberror == "weak")
            {
                signUpMessage.text = "Weak password";
            }
        }
        signUpMsgContainer.SetActive(true);
    }

    public IEnumerator DisplayTempMessage(string message)
    {
        textPopUpMessage.text = message;
        popUpMessage.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        popUpMessage.SetActive(false);
    }
    public void DisplayMessage(string t, string m)
    {
        title.text = t;
        message.text = m;
        messagePanel.SetActive(true);
    }
}