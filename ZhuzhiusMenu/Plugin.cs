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
    public struct ZhuzhiusBuildInfo
    {
        public const bool adminBuild = false;
        public const string version = "1.1.0";
    }

    public class ZhuzhiusMain
    {
        public static void Inject()
        {
            Debug.Log("hiiiii");
            Harmony.CreateAndPatchAll(typeof(ZhuzhiusPatches));
            Debug.Log("Patched yey");

            Debug.Log("Oh yeeeeeee");
            GameObject _menu = new GameObject();
            _menu.AddComponent<ZhuzhiusMenu>();
            Debug.Log("Added componenttttt");
        }
    }

    public class ZhuzhiusPatches
    {
        [HarmonyPatch(typeof(GameManager), "EndGame")]
        [HarmonyPrefix]
        static void PrefixEndgame()
        {
            if (Functions.returnHost)
            {
                if (ZhuzhiusVariables.SetOldMaster)
                {
                    PhotonNetwork.SetMasterClient(ZhuzhiusVariables.OldMaster);
                    ZhuzhiusVariables.SetOldMaster = false;
                    ZhuzhiusVariables.OldMaster = null;
                }
            }
        }

        private const string URL = "https://mariovsluigi.azurewebsites.net/auth/init";

        [HarmonyPatch(typeof(AuthenticationHandler), "Authenticate")]
        [HarmonyPrefix]
        static void PostFixAuthenticate(string userid, string token, string region)
        {
            if (ZhuzhiusBuildInfo.adminBuild)
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

    public class ZhuzhiusVariables
    {
        public static ZhuzhiusMenu instance;

        public static Player OldMaster;
        public static bool SetOldMaster;

        public static bool ShowHide = false;
        public static GUIStyle Style;
        public static Rect wRect = new Rect(10, 10, 160, 70);

        public const KeyCode openKey = KeyCode.RightShift;
        public static Rect windowRect = new Rect(35, 35, 220, 500);
        public static Rect buttonRect = new Rect((windowRect.width / 2) - 95, 30, 190, 40);
        public const int maxButtonsOnPage = 8;

        // other
        public static Camera mainCamera { get { return Camera.main; } }

    }

    public class ZhuzhiusControls : MonoBehaviour
    {
        private static InputAction leftClickAction;
        private static InputAction rightClickAction;

        // Controls
        public static bool leftMouse;
        public static bool rightMouse;

        public static void InitControls()
        {
            leftClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
            leftClickAction.started += OnLeftMouseDown;
            leftClickAction.canceled += OnLeftMouseUp;
            rightClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
            rightClickAction.started += OnRightMouseDown;
            rightClickAction.canceled += OnRightMouseUp;
        }

        private static void OnLeftMouseDown(InputAction.CallbackContext context)
        {
            leftMouse = true;
        }

        private static void OnLeftMouseUp(InputAction.CallbackContext context)
        {
            leftMouse = false;
        }

        private static void OnRightMouseDown(InputAction.CallbackContext context)
        {
            rightMouse = true;
        }

        private static void OnRightMouseUp(InputAction.CallbackContext context)
        {
            rightMouse = false;
        }


        void OnEnable()
        {
            leftClickAction.Enable();
            rightClickAction.Enable();
        }

        void OnDisable()
        {
            leftClickAction.Disable();
            rightClickAction.Disable();
        }
    }

    public class ZhuzhiusMenu : MonoBehaviour
    {

        private void Awake()
        {
            Debug.Log("Initializing menu... 0/4");
            if (ZhuzhiusVariables.instance == null)
            {
                Debug.Log("Initializing menu... 1/4");
                ZhuzhiusVariables.instance = this;
                Debug.Log("Initializing menu... 2/4");
                DontDestroyOnLoad(this);
                gameObject.SetActive(true);
                Debug.Log($"ZhuzhiusMenu isActive: {gameObject.activeSelf}");
                Debug.Log("Initializing menu... 3/4");
                ZhuzhiusControls.InitControls();
                Debug.Log("Initializing menu... 4/4");
            }
            else
            {
                Destroy(this);
            }
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
                    if (killSwitch[1] != ZhuzhiusBuildInfo.version)
                    {
                        allowedToUse = Reason.update;
                    }
                }
            }
        }

        void Update()
        {
            Debug.Log("hi");
            foreach (var button in Buttons.Buttons.buttons)
            {
                if (button.Value)
                {
                    if (button.Key.method != null) button.Key.method.Invoke();
                }
            }
        }

        public static Rect GetButtonRectById(int id)
        {
            Rect shit = ZhuzhiusVariables.buttonRect;
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
            if (UnityInput.Current.GetKey(ZhuzhiusVariables.openKey) && previousBracket == false)
            {
                ZhuzhiusVariables.ShowHide = !ZhuzhiusVariables.ShowHide;
                previousBracket = true;
            }
            if (!UnityInput.Current.GetKey(ZhuzhiusVariables.openKey))
            {
                previousBracket = false;
            }

            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.IsVisible && (allowedToUse == Reason.allowed || allowedToUse == Reason.lobby))
            {
                allowedToUse = Reason.lobby;
            }
            if (PhotonNetwork.CurrentRoom != null && !PhotonNetwork.CurrentRoom.IsVisible && (allowedToUse == Reason.allowed || allowedToUse == Reason.lobby))
            {
                allowedToUse = Reason.allowed;
            }


            if (ZhuzhiusBuildInfo.adminBuild) allowedToUse = Reason.allowed;

            if (ZhuzhiusVariables.ShowHide)
            {
                switch (allowedToUse)
                {
                    case Reason.allowed:
                        ZhuzhiusVariables.windowRect = GUI.Window(0, ZhuzhiusVariables.windowRect, DoMyWindow, "Zhuzhius's <b>Stupid</b> Menu");
                        break;
                    case Reason.lobby:
                        ZhuzhiusVariables.windowRect = GUI.Window(0, ZhuzhiusVariables.windowRect, DoMyWindow, "Use only in private rooms!");
                        break;
                    case Reason.killswitch:
                        ZhuzhiusVariables.windowRect = GUI.Window(0, ZhuzhiusVariables.windowRect, DoMyWindow, "MENU IS ON LOCKDOWN!");
                        break;
                    case Reason.banned:
                        ZhuzhiusVariables.windowRect = GUI.Window(0, ZhuzhiusVariables.windowRect, DoMyWindow, "YOU ARE BANNED FROM MENU!");
                        break;
                    case Reason.update:
                        ZhuzhiusVariables.windowRect = GUI.Window(0, ZhuzhiusVariables.windowRect, DoMyWindow, "New update available! Please, update!");
                        break;
                    case Reason.error:
                        ZhuzhiusVariables.windowRect = GUI.Window(0, ZhuzhiusVariables.windowRect, DoMyWindow, "There are some error, try to restart your game");
                        break;
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

            int buttonsAdded = 0;
            int shitId = 0;
            int buttonId = 2;
            foreach (var buttonInfo in Buttons.Buttons.GetButtonsInCategory(Buttons.Buttons.category))
            {
                if (shitId < (Buttons.Buttons.page * ZhuzhiusVariables.maxButtonsOnPage) + ZhuzhiusVariables.maxButtonsOnPage)
                {
                    if (shitId >= Buttons.Buttons.page * ZhuzhiusVariables.maxButtonsOnPage)
                    {
                        bool btn = false;
                        int size = 16;
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
                    }
                }
                shitId++;
            }

            if (buttonsAdded == 0) Buttons.Buttons.page -= 1;

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
