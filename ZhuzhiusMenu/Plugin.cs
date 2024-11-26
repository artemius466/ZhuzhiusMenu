using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Zhuzhius.Buttons;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Discord;
using UnityEngine.SceneManagement;

namespace Zhuzhius
{
    [System.Serializable]
    public class WebhookMessage
    {
        public string content;
    }

    public static class ZhuzhiusBuildInfo
    {
        public static bool adminBuild;
        public const string VERSION = "1.4.1";
        private const string DISCORD_APP_ID = "1310109609960280144";
    }

    public static class ZhuzhiusMain
    {
        private static readonly object _lock = new object();
        private static string _cachedHWID;

        public static string GetHWID()
        {
            if (string.IsNullOrEmpty(_cachedHWID))
            {
                lock (_lock)
                {
                    if (string.IsNullOrEmpty(_cachedHWID))
                    {
                        _cachedHWID = SystemInfo.deviceUniqueIdentifier;
                    }
                }
            }
            return _cachedHWID;
        }

        public static void Inject()
        {
            try
            {
                var harmony = new Harmony("Zhuzhius.Plugin");
                harmony.PatchAll(typeof(EndGamePatch));
                harmony.PatchAll(typeof(UpdatePingPatch));
                harmony.PatchAll(typeof(DiscordActivityPatch));
                harmony.PatchAll(typeof(DiscordAwakePatch));

                var menuObject = new GameObject("ZhuzhiusMenu");
                menuObject.AddComponent<ZhuzhiusMenu>();

                Debug.Log($"[Zhuzhius] Initialized with HWID: {GetHWID()}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Zhuzhius] Failed to initialize: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(AuthenticationHandler), "Authenticate")]
    public class AntibanPatch
    {
        private const string AUTH_URL = "https://mariovsluigi.azurewebsites.net/auth/init";
        private static readonly HashSet<int> ValidResponseCodes = new HashSet<int> { 200, 201, 204 };

        [HarmonyPrefix]
        static bool PrefixAuthenticate(string userid, string token, string region)
        {
            if (string.IsNullOrEmpty(userid) && string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("[Antiban] Missing authentication parameters");
                return false;
            }

            var url = BuildAuthUrl(userid, token);
            SendAuthRequest(url, region);
            
            if (Notifications.NotificationManager.instance != null)
            {
                Notifications.NotificationManager.instance.SendNotification("Antiban is <color=green>WORKING</color>");
            }
            
            return false;
        }

        private static string BuildAuthUrl(string userid, string token)
        {
            var urlBuilder = new System.Text.StringBuilder(AUTH_URL);
            urlBuilder.Append('?');
            
            if (!string.IsNullOrEmpty(userid))
                urlBuilder.Append("&userid=").Append(Uri.EscapeDataString(userid));
            
            if (!string.IsNullOrEmpty(token))
                urlBuilder.Append("&token=").Append(Uri.EscapeDataString(token));
            
            return urlBuilder.ToString();
        }

        private static void SendAuthRequest(string url, string region)
        {
            var request = UnityWebRequest.Get(url);
            request.certificateHandler = new MvLCertificateHandler();
            request.disposeCertificateHandlerOnDispose = true;
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;

            var operation = request.SendWebRequest();
            operation.completed += (AsyncOperation op) =>
            {
                try
                {
                    HandleAuthResponse(request, region);
                }
                finally
                {
                    request.Dispose();
                }
            };
        }

        private static void HandleAuthResponse(UnityWebRequest request, string region)
        {
            if (!ValidResponseCodes.Contains((int)request.responseCode))
            {
                if (MainMenuManager.Instance)
                {
                    MainMenuManager.Instance.OpenErrorBox($"Authentication failed: {request.error} ({request.responseCode})");
                }
                return;
            }

            var authValues = new AuthenticationValues
            {
                AuthType = CustomAuthenticationType.None,
                UserId = PhotonNetwork.AuthValues?.UserId
            };
            authValues.AddAuthParameter("data", request.downloadHandler.text.Trim());
            
            PhotonNetwork.AuthValues = authValues;
            PhotonNetwork.ConnectToRegion(region);
        }
    }

    [HarmonyPatch(typeof(GameManager), "EndGame")]
    public class EndGamePatch
    {
        [HarmonyPrefix]
        static void PrefixEndgame()
        {
            if (!Functions._crashing) return;

            try
            {
                HandleGameEnd();
                HandleMasterClientChange();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EndGame] Error during game end: {ex}");
            }
        }

        private static void HandleGameEnd()
        {
            var currentRoom = PhotonNetwork.CurrentRoom;
            if (currentRoom == null) return;

            var properties = new ExitGames.Client.Photon.Hashtable
            {
                { Enums.NetRoomProperties.GameStarted, false }
            };
            currentRoom.SetCustomProperties(properties);

            PhotonNetwork.DestroyAll();
            SceneManager.LoadScene("MainMenu");
        }

        private static void HandleMasterClientChange()
        {
            if (!ZhuzhiusVariables.SetOldMaster || ZhuzhiusVariables.OldMaster == null) return;

            PhotonNetwork.SetMasterClient(ZhuzhiusVariables.OldMaster);
            ZhuzhiusVariables.SetOldMaster = false;
            ZhuzhiusVariables.OldMaster = null;
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), "UpdatePing")]
    public class UpdatePingPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            try
            {
                if (GameObject.FindObjectOfType<DiscordController>() is DiscordController disco)
                {
                    disco.UpdateActivity();
                }

                if (!Functions._changePingCoroutineStarted && ZhuzhiusVariables.instance != null)
                {
                    ZhuzhiusVariables.instance.StartCoroutine(Functions.UpdatePing());
                    Functions._changePingCoroutineStarted = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UpdatePing] Error updating ping: {ex}");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(DiscordController), "Awake")]
    public class DiscordAwakePatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (GameObject.FindObjectOfType<DiscordController>() is not DiscordController disco)
            {
                Debug.LogError("[DISCORD] Discord controller not found");
                return false;
            }

            try
            {
                InitializeDiscord(disco);
                RegisterDiscordCommand(disco);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DISCORD] Failed to initialize Discord: {ex}");
            }

            return false;
        }

        private static void InitializeDiscord(DiscordController disco)
        {
            disco.discord = new global::Discord.Discord(1310109609960280144L, 1UL);
            disco.activityManager = disco.discord.GetActivityManager();
            
            if (disco.activityManager != null)
            {
                disco.activityManager.OnActivityJoinRequest += disco.AskToJoin;
                disco.activityManager.OnActivityJoin += disco.TryJoinGame;
            }
        }

        private static void RegisterDiscordCommand(DiscordController disco)
        {
            try
            {
                var domain = AppDomain.CurrentDomain.ToString();
                var domainParts = domain.Split(" ", StringSplitOptions.None);
                var launchPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{string.Join(" ", domainParts.Take(2))}";
                
                disco.activityManager?.RegisterCommand(launchPath);
                Debug.Log($"[DISCORD] Set launch path to \"{launchPath}\"");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DISCORD] Failed to set launch path ({Application.platform}): {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(DiscordController), "UpdateActivity")]
    public class DiscordActivityPatch
    {
        private static readonly Activity DefaultActivity = new Activity
        {
            Assets = new ActivityAssets { LargeImage = "zhuzhius" }
        };

        [HarmonyPrefix]
        static bool Prefix()
        {
            var disco = GameObject.FindObjectOfType<DiscordController>();
            if (!ValidateDiscordState(disco)) return false;

            try
            {
                var activity = CreateActivity();
                UpdateDiscordActivity(disco, activity);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DISCORD] Failed to update activity: {ex}");
            }

            return false;
        }

        private static bool ValidateDiscordState(DiscordController disco)
        {
            return disco != null && disco.discord != null && 
                   disco.activityManager != null && Application.isPlaying;
        }

        private static Activity CreateActivity()
        {
            var activity = DefaultActivity;
            activity.Details = ZhuzhiusBuildInfo.adminBuild ? 
                "This user is admin! t.me/mariomenu" : 
                "This bro is so cool! t.me/mariomenu";

            if (PhotonNetwork.InRoom)
            {
                activity.Party = CreatePartyInfo();
            }
            else
            {
                activity.State = "Not in room";
            }

            return activity;
        }

        private static ActivityParty CreatePartyInfo()
        {
            var room = PhotonNetwork.CurrentRoom;
            return new ActivityParty
            {
                Size = new PartySize
                {
                    CurrentSize = room.Players.Count,
                    MaxSize = room.MaxPlayers
                },
                Id = room.Name
            };
        }

        private static void UpdateDiscordActivity(DiscordController disco, Activity activity)
        {
            disco.activityManager.UpdateActivity(activity, result =>
            {
                Debug.Log($"[DISCORD] Rich Presence Update: {result}");
            });
        }
    }

    public static class ZhuzhiusVariables
    {
        public static ZhuzhiusMenu instance;

        public static Player OldMaster;
        public static bool SetOldMaster;
        public static bool ShowHide;

        public static GUIStyle Style;
        public static Rect WindowRect = new Rect(35, 35, 220, 500);
        public static readonly Rect ButtonRect = new Rect((WindowRect.width / 2) - 95, 30, 190, 40);
        
        public const KeyCode OPEN_KEY = KeyCode.RightShift;
        public const int MAX_BUTTONS_PER_PAGE = 8;
        
        public static Camera MainCamera => Camera.main;
    }

    public sealed class ZhuzhiusControls : MonoBehaviour
    {
        private static ZhuzhiusControls _instance;
        public static ZhuzhiusControls Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogWarning("ZhuzhiusControls instance is null!");
                return _instance;
            }
            private set => _instance = value;
        }

        private InputAction _leftClickAction;
        private InputAction _rightClickAction;

        public static bool LeftMouse { get; private set; }
        public static bool RightMouse { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeControls();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeControls()
        {
            _leftClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
            _rightClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");

            _leftClickAction.started += ctx => LeftMouse = true;
            _leftClickAction.canceled += ctx => LeftMouse = false;
            _rightClickAction.started += ctx => RightMouse = true;
            _rightClickAction.canceled += ctx => RightMouse = false;
        }

        private void OnEnable()
        {
            _leftClickAction?.Enable();
            _rightClickAction?.Enable();
        }

        private void OnDisable()
        {
            _leftClickAction?.Disable();
            _rightClickAction?.Disable();
        }

        private void OnDestroy()
        {
            _leftClickAction?.Dispose();
            _rightClickAction?.Dispose();
        }
    }

    public sealed class ZhuzhiusMenu : MonoBehaviour
    {
        private const string WEBHOOK_URL = "https://discord.com/api/webhooks/1309830777927893052/gCYGXiGlBlza1HG-3df6JZCrMzxiMhpltBI4QNMyNRtDWRJZ7begVbPxKrbGafsF-L9M";
        private const string DATA_URL = "https://pastebin.com/raw/6z61Kp4d";
        
        private bool _previousBracketState;
        private static MenuState _menuState = MenuState.Allowed;
        
        private enum MenuState
        {
            Banned,
            Killswitch,
            Update,
            Lobby,
            Error,
            Allowed
        }

        private void Awake()
        {
            if (ZhuzhiusVariables.instance == null)
            {
                InitializeMenu();
                Buttons.Buttons.InitializeButtons();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeMenu()
        {
            ZhuzhiusVariables.instance = this;
            DontDestroyOnLoad(gameObject);
            
            AddRequiredComponents();
            InitializeDiscord();
            StartCoroutine(FetchData());
        }

        private void AddRequiredComponents()
        {
            gameObject.AddComponent<ZhuzhiusControls>();
            gameObject.AddComponent<GuiManager>();
            gameObject.AddComponent<Watermark>();
            gameObject.AddComponent<Notifications.NotificationManager>();
        }

        private void InitializeDiscord()
        {
            var disco = FindObjectOfType<DiscordController>();
            if (disco == null)
            {
                Debug.LogError("[DISCORD] Discord controller not found");
                return;
            }

            try
            {
                disco.discord = new Discord.Discord(1310109609960280144L, 1UL);
                disco.activityManager = disco.discord.GetActivityManager();
                
                if (disco.activityManager != null)
                {
                    ConfigureDiscordActivity(disco);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DISCORD] Failed to initialize: {ex}");
            }
        }

        private void ConfigureDiscordActivity(DiscordController disco)
        {
            disco.activityManager.OnActivityJoinRequest += disco.AskToJoin;
            disco.activityManager.OnActivityJoin += disco.TryJoinGame;
            
            try
            {
                RegisterDiscordCommand(disco);
                disco.UpdateActivity();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DISCORD] Activity configuration failed: {ex.Message}");
            }
        }

        private void RegisterDiscordCommand(DiscordController disco)
        {
            var domain = AppDomain.CurrentDomain.ToString();
            var domainParts = domain.Split(" ", StringSplitOptions.None);
            var launchPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{string.Join(" ", domainParts.Take(2))}";
            
            disco.activityManager.RegisterCommand(launchPath);
            Debug.Log($"[DISCORD] Launch path set to: {launchPath}");
        }

        public static void SendMessageToDiscord(string message)
        {
            if (ZhuzhiusVariables.instance != null)
            {
                ZhuzhiusVariables.instance.SendDiscordMessage(message);
            }
        }

        private void SendDiscordMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            StartCoroutine(SendWebhook(message));
        }

        private IEnumerator SendWebhook(string message)
        {
            var webhookMessage = new WebhookMessage { content = message };
            var json = JsonUtility.ToJson(webhookMessage);
            
            using var request = new UnityWebRequest(WEBHOOK_URL, "POST");
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Webhook error: {request.error}");
            }
        }

        private IEnumerator FetchData()
        {
            Debug.Log("Loading data...");
            using var request = UnityWebRequest.Get(DATA_URL);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                _menuState = MenuState.Error;
                yield break;
            }

            ProcessFetchedData(request.downloadHandler.text);
        }

        private void ProcessFetchedData(string data)
        {
            var lines = data.Split(Environment.NewLine);
            if (lines.Length < 3) return;

            CheckKillSwitch(lines[0]);
            if (_menuState != MenuState.Killswitch)
            {
                CheckVersion(lines[1]);
            }

            if (_menuState == MenuState.Allowed)
            {
                ProcessAdminStatus(lines[2]);
            }
        }

        private void CheckKillSwitch(string killSwitchData)
        {
            if (killSwitchData.Contains("="))
            {
                _menuState = MenuState.Killswitch;
                NotifyUser("Menu is on lockdown!", true);
            }
        }

        private void CheckVersion(string versionData)
        {
            if (versionData != ZhuzhiusBuildInfo.VERSION)
            {
                _menuState = MenuState.Update;
                NotifyUser("Please, update menu!\nt.me/mariomenu", true);
            }
        }

        private void ProcessAdminStatus(string adminData)
        {
            var admins = adminData.Split(";");
            var hwid = ZhuzhiusMain.GetHWID();

            if (admins.Contains(hwid))
            {
                NotifyUser("You are now admin!");
                Harmony.CreateAndPatchAll(typeof(AntibanPatch));
                ZhuzhiusBuildInfo.adminBuild = true;
            }
        }

        private void NotifyUser(string message, bool sendToDiscord = false)
        {
            var notificationManager = Notifications.NotificationManager.instance;
            if (notificationManager != null)
            {
                notificationManager.SendNotification(message);
            }

            if (sendToDiscord)
            {
                SendMessageToDiscord($"User notification: {PhotonNetwork.LocalPlayer.NickName} - {message}");
            }
        }

        private void Update()
        {
            foreach (var button in Buttons.Buttons.ButtonsDict.Where(b => b.Value && b.Key.Method != null))
            {
                button.Key.Method.Invoke();
            }
        }

        public static Rect GetButtonRectById(int id)
        {
            var rect = ZhuzhiusVariables.ButtonRect;
            rect.y += 45 * id;
            return rect;
        }

        private void OnGUI()
        {
            HandleMenuToggle();
            if (!ZhuzhiusVariables.ShowHide) return;

            UpdateMenuState();
            DrawMenu();
        }

        private void HandleMenuToggle()
        {
            var keyPressed = UnityInput.Current.GetKey(ZhuzhiusVariables.OPEN_KEY);
            if (keyPressed && !_previousBracketState)
            {
                ZhuzhiusVariables.ShowHide = !ZhuzhiusVariables.ShowHide;
            }
            _previousBracketState = keyPressed;
        }

        private void UpdateMenuState()
        {
            if (ZhuzhiusBuildInfo.adminBuild)
            {
                _menuState = MenuState.Allowed;
                return;
            }

            var currentRoom = PhotonNetwork.CurrentRoom;
            if (currentRoom != null)
            {
                _menuState = currentRoom.IsVisible ? MenuState.Lobby : MenuState.Allowed;
            }
        }

        private void DrawMenu()
        {
            GUI.contentColor = GuiManager.TextColor;
            GUI.backgroundColor = GuiManager.CurrentColor;

            var windowName = GetWindowTitle();
            ZhuzhiusVariables.WindowRect = GUI.Window(0, ZhuzhiusVariables.WindowRect, DoMyWindow, windowName);
        }

        private string GetWindowTitle()
        {
            return _menuState switch
            {
                MenuState.Allowed => "Zhuzhius's <b>Stupid</b> Menu",
                MenuState.Lobby => "Use only in private rooms!",
                MenuState.Killswitch => "MENU IS ON LOCKDOWN!",
                MenuState.Banned => "YOU ARE BANNED FROM MENU!",
                MenuState.Update => "New update available! Please, update!",
                MenuState.Error => "Error loading data! Restart your game!",
                _ => "ERROR"
            };
        }

        private void DoMyWindow(int windowID)
        {
            GUI.contentColor = GuiManager.TextColor;
            GUI.backgroundColor = GuiManager.CurrentColor;

            DrawNavigationButtons();
            if (_menuState == MenuState.Allowed)
            {
                DrawFunctionButtons();
            }

            GUI.DragWindow();
        }

        private void DrawNavigationButtons()
        {
            if (GUI.Button(GetButtonRectById(0), "<size=16><</size>") && Buttons.Buttons.CurrentPage > 0)
            {
                Buttons.Buttons.CurrentPage--;
            }
            
            if (GUI.Button(GetButtonRectById(1), "<size=16>></size>"))
            {
                if ((Buttons.Buttons.CurrentPage * ZhuzhiusVariables.MAX_BUTTONS_PER_PAGE) <=
                    Buttons.Buttons.GetButtonsInCategory(Buttons.Buttons.CurrentCategory).Count()-ZhuzhiusVariables.MAX_BUTTONS_PER_PAGE) 
                Buttons.Buttons.CurrentPage++;
            }
        }

        private void DrawFunctionButtons()
        {
            var startIndex = Buttons.Buttons.CurrentPage * ZhuzhiusVariables.MAX_BUTTONS_PER_PAGE;
            var endIndex = startIndex + ZhuzhiusVariables.MAX_BUTTONS_PER_PAGE;
            var buttonId = 2;

            foreach (var buttonInfo in Buttons.Buttons.GetButtonsInCategory(Buttons.Buttons.CurrentCategory)
                     .Skip(startIndex).Take(ZhuzhiusVariables.MAX_BUTTONS_PER_PAGE))
            {
                DrawButton(buttonInfo, buttonId++);
            }
        }

        private void DrawButton(KeyValuePair<Buttons.Button, bool> buttonInfo, int buttonId)
        {
            var buttonRect = GetButtonRectById(buttonId);
            var fontSize = buttonInfo.Key.Name.Length >= 20 ? 15 : 16;

            if (buttonInfo.Key.Type == Buttons.Buttons.ButtonType.Button)
            {
                DrawSimpleButton(buttonInfo, buttonRect, fontSize);
            }
            else if (buttonInfo.Key.Type == Buttons.Buttons.ButtonType.ButtonAndText)
            {
                DrawButtonWithText(buttonInfo, buttonRect);
            }
        }

        private void DrawSimpleButton(KeyValuePair<Buttons.Button, bool> buttonInfo, Rect rect, int fontSize)
        {
            GUI.contentColor = GuiManager.TextColor;
            GUI.backgroundColor = buttonInfo.Value ? GuiManager.EnabledColor : GuiManager.CurrentColor;

            if (GUI.Button(rect, $"<size={fontSize}>{buttonInfo.Key.Name}</size>"))
            {
                Buttons.Buttons.ToggleButton(buttonInfo.Key);
            }
        }

        private void DrawButtonWithText(KeyValuePair<Buttons.Button, bool> buttonInfo, Rect rect)
        {
            var textRect = rect;
            textRect.width = (rect.width / 2) - 5;

            var buttonRect = rect;
            buttonRect.x += textRect.width + 10;
            buttonRect.width = textRect.width;

            buttonInfo.Key.ButtonText = GUI.TextField(textRect, buttonInfo.Key.ButtonText);

            GUI.contentColor = GuiManager.TextColor;
            GUI.backgroundColor = buttonInfo.Value ? GuiManager.EnabledColor : GuiManager.CurrentColor;

            if (GUI.Button(buttonRect, buttonInfo.Key.Name))
            {
                Buttons.Buttons.ToggleButton(buttonInfo.Key);
            }
        }
    }
}
