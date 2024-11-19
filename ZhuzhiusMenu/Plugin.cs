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
using System.Linq;
using System.Drawing;
using System.Collections;

namespace Zhuzhius
{
    public struct PluginInfo
    {
        public const string version = "1.0.0";
    }
    [BepInPlugin("org.zhuzhius.zhuzhiusmenu", "ZHUZHIUS", PluginInfo.version)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool adminBuild { get; private set; }
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            adminBuild = false; // VERY SCARY DONT TURN IT ON!!!!!


            Logger = base.Logger;

            Console.Title = "ZHUZHIUS ON TOP";

            Logger.LogInfo($"--==WELCOME TO ZHUZHIUS==--");
            Logger.LogInfo($"\n\n\n\n\n\n\n\n▒███████▒ ██░ ██  █    ██ ▒███████▒ ██░ ██  ██▓ █    ██   ██████ \r\n▒ ▒ ▒ ▄▀░▓██░ ██▒ ██  ▓██▒▒ ▒ ▒ ▄▀░▓██░ ██▒▓██▒ ██  ▓██▒▒██    ▒ \r\n░ ▒ ▄▀▒░ ▒██▀▀██░▓██  ▒██░░ ▒ ▄▀▒░ ▒██▀▀██░▒██▒▓██  ▒██░░ ▓██▄   \r\n  ▄▀▒   ░░▓█ ░██ ▓▓█  ░██░  ▄▀▒   ░░▓█ ░██ ░██░▓▓█  ░██░  ▒   ██▒\r\n▒███████▒░▓█▒░██▓▒▒█████▓ ▒███████▒░▓█▒░██▓░██░▒▒█████▓ ▒██████▒▒\r\n░▒▒ ▓░▒░▒ ▒ ░░▒░▒░▒▓▒ ▒ ▒ ░▒▒ ▓░▒░▒ ▒ ░░▒░▒░▓  ░▒▓▒ ▒ ▒ ▒ ▒▓▒ ▒ ░\r\n░░▒ ▒ ░ ▒ ▒ ░▒░ ░░░▒░ ░ ░ ░░▒ ▒ ░ ▒ ▒ ░▒░ ░ ▒ ░░░▒░ ░ ░ ░ ░▒  ░ ░\r\n░ ░ ░ ░ ░ ░  ░░ ░ ░░░ ░ ░ ░ ░ ░ ░ ░ ░  ░░ ░ ▒ ░ ░░░ ░ ░ ░  ░  ░  \r\n  ░ ░     ░  ░  ░   ░       ░ ░     ░  ░  ░ ░     ░           ░  \r\n░                         ░                                      \n\n\n\n\n\n\n\n\n\n");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        public static void Inject()
        {
            GameObject _menu = new GameObject();
            ZhuzhiusMenu injected = _menu.AddComponent<ZhuzhiusMenu>();

            Logger.LogInfo($"--==Injected==--");
        }

        [HarmonyPatch(typeof(MainMenuManager), "Start")]
        [HarmonyPostfix]
        static void PostFixStart()
        {
            ZhuzhiusMenu menu = ZhuzhiusMenu.instance;
            if (menu == null)
            {
                Logger.LogInfo($"--==Injecting==--");
                Inject();
            }
        }

        [HarmonyPatch(typeof(GameManager), "EndGame")]
        [HarmonyPrefix]
        static void PrefixEndgame()
        {
            if (Functions.returnHost)
            {
                ZhuzhiusMenu menu = ZhuzhiusMenu.instance;

                if (menu.SetOldMaster)
                {
                    PhotonNetwork.SetMasterClient(menu.OldMaster);
                    menu.SetOldMaster = false;
                    menu.OldMaster = null;
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), "OnJoinedRoom")]
        [HarmonyPostfix]
        static void PostfixJoined()
        {
            if (Functions.AutoMaster && !PhotonNetwork.IsMasterClient)
            {
                Player toKick = PhotonNetwork.MasterClient;

                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);

                //Functions.BanPlayer(toKick);
            }
        }

        private static readonly string URL = "https://mariovsluigi.azurewebsites.net/auth/init";

        [HarmonyPatch(typeof(AuthenticationHandler), "Authenticate")]
        [HarmonyPrefix]
        static void PostFixAuthenticate(string userid, string token, string region)
        {
            if (adminBuild)
            {
                AuthenticationValues authenticationValues = new AuthenticationValues();
                authenticationValues.AuthType = CustomAuthenticationType.None;
                authenticationValues.UserId = userid;
                authenticationValues.AddAuthParameter("data", "");
                PhotonNetwork.AuthValues = authenticationValues;
                Debug.Log("ANTIBAN IS COOKING!");

                //PhotonNetwork.ConnectToRegion(region);

                return;
            }
        }
    }

    public class ZhuzhiusMenu : MonoBehaviour
    {
        public static ZhuzhiusMenu instance;

        public Player OldMaster;
        public bool SetOldMaster;

        public bool ShowHide = false;
        public GUIStyle Style;
        public Rect wRect = new Rect(10, 10, 160, 70);

        // Private controls
        private InputAction leftClickAction;
        private InputAction rightClickAction;

        // Controls
        public static bool leftMouse;
        public static bool rightMouse;

        // other
        public static Camera mainCamera { get {  return Camera.main; } }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }

            InitControls();
        }

        private void InitControls()
        {
            leftClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
            leftClickAction.started += OnLeftMouseDown;
            leftClickAction.canceled += OnLeftMouseUp;
            Plugin.Logger.Log(LogLevel.Message, "Left Click Action Initialized");
            rightClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
            rightClickAction.started += OnRightMouseDown;
            rightClickAction.canceled += OnRightMouseUp;
            Plugin.Logger.Log(LogLevel.Message, "Right Click Action Initialized");
        }

        private IEnumerator FetchData()
        {
            Debug.Log("Loading data...");
            UnityWebRequest request = UnityWebRequest.Get("https://pastebin.com/raw/6z61Kp4d");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                allowedToUse = Reason.error;
            }
            else
            {
                string[] killSwitch = request.downloadHandler.text.Split(Environment.NewLine);

                if (killSwitch[0].Contains("="))
                {
                    allowedToUse = Reason.killswitch;
                }
                if (allowedToUse != Reason.killswitch)
                {
                    if (killSwitch[1] != PluginInfo.version)
                    {
                        allowedToUse = Reason.update;
                    }
                }
            }
        }

        public GameObject GetClick()
        {
            if (leftMouse)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

                worldPosition.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
                return hit.collider.gameObject;
            }
            else
            {
                return null;
            }
        }

        private void OnLeftMouseDown(InputAction.CallbackContext context)
        {
            leftMouse = true;
        }

        private void OnLeftMouseUp(InputAction.CallbackContext context)
        {
            leftMouse = false;
        }

        private void OnRightMouseDown(InputAction.CallbackContext context)
        {
            rightMouse = true;
        }

        private void OnRightMouseUp(InputAction.CallbackContext context)
        {
            rightMouse = false;
        }


        void OnEnable()
        {
            leftClickAction.Enable();
            rightClickAction.Enable();

            StartCoroutine(FetchData());
        }

        void OnDisable()
        {
            leftClickAction.Disable();
            rightClickAction.Disable();
        }

        void Update()
        {
            foreach (var button in Buttons.Buttons.buttons)
            {
                if (button.Value)
                {
                    if (button.Key.method != null) button.Key.method.Invoke();
                }
            }
        }

        public static readonly KeyCode openKey = KeyCode.RightShift;
        public static Rect windowRect = new Rect(35, 35, 220, 500);
        public static Rect buttonRect = new Rect((windowRect.width / 2) - 95, 30, 190, 40);
        public static readonly int maxButtonsOnPage = 8;

        public static Rect GetButtonRectById(int id)
        {
            Rect shit = buttonRect;
            shit.y += 45*id;
            return shit;
        }

        private bool previousBracket = false;

        enum Reason
        {
            banned,
            killswitch,
            update,
            lobby,
            error,
            allowed
        }
        private static Reason allowedToUse = Reason.allowed;

        void OnGUI()
        {
            if (UnityInput.Current.GetKey(openKey) && previousBracket == false)
            {
                ShowHide = !ShowHide;
                previousBracket = true;
            }
            if (!UnityInput.Current.GetKey(openKey))
            {
                previousBracket = false;
            }

            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.IsVisible && (allowedToUse == Reason.allowed || allowedToUse == Reason.lobby))
            {
                allowedToUse = Reason.lobby;
            }

            if (Plugin.adminBuild) allowedToUse = Reason.allowed;

            if (ShowHide)
            {
                if (allowedToUse == Reason.allowed)
                {
                    windowRect = GUI.Window(0, windowRect, DoMyWindow, "Zhuzhius's <b>Stupid</b> Menu");
                } 
                else if (allowedToUse == Reason.lobby)
                {
                    windowRect = GUI.Window(0, windowRect, DoMyWindow, "Use only in private rooms!");
                }
                else if (allowedToUse == Reason.killswitch) 
                {
                    windowRect = GUI.Window(0, windowRect, DoMyWindow, "MENU IS ON LOCKDOWN!");
                } 
                else if (allowedToUse == Reason.banned)
                {
                    windowRect = GUI.Window(0, windowRect, DoMyWindow, "YOU ARE BANNED FROM MENU!");
                }
                else if (allowedToUse == Reason.update)
                {
                    windowRect = GUI.Window(0, windowRect, DoMyWindow, "New update available! Please, update!");
                }
                else if (allowedToUse == Reason.error)
                {
                    windowRect = GUI.Window(0, windowRect, DoMyWindow, "There are some error, try to restart your game");
                }
            }
        }

        void DoMyWindow(int windowID)
        {
            if (GUI.Button(GetButtonRectById(0), "<size=16><</size>"))
            {
                if (Buttons.Buttons.page !=0) Buttons.Buttons.page--;
            }
            if (GUI.Button(GetButtonRectById(1), "<size=16>></size>"))
            {
                Buttons.Buttons.page++;
            }

            //Buttons.Buttons.category = 2;
            //Buttons.Buttons.page = 1;

            int buttonsAdded = 0;
            int shitId = 0;
            int buttonId = 2;
            //Debug.Log(((Buttons.Buttons.page) * 9) + maxButtonsOnPage);
            foreach (var buttonInfo in Buttons.Buttons.GetButtonsInCategory(Buttons.Buttons.category))
            {
                if (shitId < (Buttons.Buttons.page * maxButtonsOnPage) + maxButtonsOnPage)
                {
                    if (shitId >= Buttons.Buttons.page * maxButtonsOnPage)
                    {
                        bool btn = false;
                        int size = 16;
                        //if (buttonInfo.Key.Name.Length >= 12) size = 15;
                        if (buttonInfo.Key.Name.Length >= 24) size = 13;

                        if (allowedToUse == Reason.allowed)
                        {
                            if (!buttonInfo.Value)
                            {
                                btn = GUI.Button(GetButtonRectById(buttonId), $"<size={size}>{buttonInfo.Key.Name}</size>");
                            }
                            else
                            {
                                btn = GUI.Button(GetButtonRectById(buttonId), $"<color=blue><size={size}>{buttonInfo.Key.Name}</size></color>");
                            }
                        } else
                        {
                            btn = GUI.Button(GetButtonRectById(buttonId), $"<color=red><size={size}>{buttonInfo.Key.Name}</size></color>");
                        }


                        if (btn && allowedToUse == Reason.allowed)
                        {
                            if (buttonInfo.Key.isToggleable)
                            {
                                Buttons.Buttons.buttons[buttonInfo.Key] = !buttonInfo.Value;
                                if (buttonInfo.Value)
                                {
                                    if (buttonInfo.Key.enableMethod != null) buttonInfo.Key.disableMethod.Invoke();
                                }
                                else
                                {
                                    if (buttonInfo.Key.disableMethod != null) buttonInfo.Key.enableMethod.Invoke();
                                }
                            }
                            else { if (buttonInfo.Key.method != null) buttonInfo.Key.method.Invoke(); }
                        }
                        buttonsAdded++;
                        buttonId++;
                        //Debug.Log("added");
                        
                    }
                }
                //Debug.Log(buttonId);
                shitId++;
            }

            if (buttonsAdded == 0) Buttons.Buttons.page -= 1;

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
