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

namespace Zhuzhius
{
    public struct ZhuzhiusBuildInfo
    {
        public static bool adminBuild = false;
        public const string version = "1.3.0";
    }

    public class ZhuzhiusMain
    {
        public static string GetHWID()
        {
            string deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier; // Уникальный идентификатор устройства
            string deviceModel = SystemInfo.deviceModel;                      // Модель устройства
            string processorType = SystemInfo.processorType;                  // Тип процессора
            int processorCount = SystemInfo.processorCount;                   // Количество ядер

            return $"{deviceUniqueIdentifier}";
        }

        public static void Inject()
        {
            Harmony instance = new Harmony("default");
            instance.PatchAll(typeof(EndGamePatch));
            instance.PatchAll(typeof(UpdatePingPatch));
            instance.PatchAll(typeof(OnEventPatch));

            GameObject _menu = new GameObject();
            _menu.AddComponent<ZhuzhiusMenu>();

            Console.WriteLine(GetHWID());
        }
    }

    [HarmonyPatch(typeof(AuthenticationHandler), "Authenticate")]
    public class AntibanPatch
    {
        private const string URL = "https://mariovsluigi.azurewebsites.net/auth/init";

        [HarmonyPrefix]
        static bool PrefixAuthenticate(string userid, string token, string region)
        {
            string text = URL + "?";
            if (userid != null)
            {
                text = text + "&userid=" + userid;
            }
            if (token != null)
            {
                text = text + "&token=" + token;
            }
            UnityWebRequest client = UnityWebRequest.Get(text);
            client.certificateHandler = new MvLCertificateHandler();
            client.disposeCertificateHandlerOnDispose = true;
            client.disposeDownloadHandlerOnDispose = true;
            client.disposeUploadHandlerOnDispose = true;
            client.SendWebRequest().completed += delegate (AsyncOperation a)
            {
                if (client.result != UnityWebRequest.Result.Success)
                {
                    if (MainMenuManager.Instance)
                    {
                        //MainMenuManager.Instance.OpenErrorBox("you are banned, but antiban is on");
                        Notifications.NotificationManager.instance.SendNotification("You are banned, but antiban saved you!");
                        //MainMenuManager.Instance.OnDisconnected(DisconnectCause.CustomAuthenticationFailed);
                    }
                    //return;
                }
                AuthenticationValues authenticationValues = new AuthenticationValues();
                authenticationValues.AuthType = CustomAuthenticationType.None;
                authenticationValues.UserId = userid;
                authenticationValues.AddAuthParameter("data", "");
                PhotonNetwork.AuthValues = authenticationValues;
                PhotonNetwork.ConnectToRegion(region);
                client.Dispose();
            };
            Notifications.NotificationManager.instance.SendNotification("Antiban is <color=green>WORKING</color>");
            return false;
        }
    }

    [HarmonyPatch(typeof(GameManager), "EndGame")]
    public class EndGamePatch
    {
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
    }

    [HarmonyPatch(typeof(MainMenuManager), "UpdatePing")]
    public class UpdatePingPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (!Functions.changePingCoroutineStarted) ZhuzhiusVariables.instance.StartCoroutine(Functions.UpdatePing());
            Functions.changePingCoroutineStarted = true;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(MainMenuManager), "OnEvent")]
    public class OnEventPatch
    {
        [HarmonyPrefix]
        static void Prefix(EventData e)
        {
            Debug.Log("OnEvent called");
            
            //if (Functions.reviveOnEnter && PhotonNetwork.InRoom && e.Code==1) 
            //{
            //    Functions.SetMasterSelf();
            //    var mainMenuInstance = MainMenuManager.Instance;
            //    FieldInfo joinedLateField = typeof(MainMenuManager).GetField("joinedLate", BindingFlags.NonPublic | BindingFlags.Instance);
            //    joinedLateField.SetValue(mainMenuInstance, false);
            //    ZhuzhiusVariables.instance.StartCoroutine(Functions.ReturnMaster());
            //}
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
        public static ZhuzhiusControls instance;

        private InputAction leftClickAction;
        private InputAction rightClickAction;

        // Controls
        public static bool leftMouse;
        public static bool rightMouse;

        public void Awake()
        {
            if (instance == null)
            {
                InitControls();
                instance = this;
            }
        }

        public void InitControls()
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
            if (ZhuzhiusVariables.instance == null)
            {
                ZhuzhiusVariables.instance = this;
                DontDestroyOnLoad(this);
                gameObject.SetActive(true);

                gameObject.AddComponent<ZhuzhiusControls>();
                gameObject.AddComponent<GuiManager>();
                gameObject.AddComponent<Watermark>();
                gameObject.AddComponent<Notifications.NotificationManager>();

                StartCoroutine(FetchData());
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
                    Notifications.NotificationManager.instance.SendNotification("Menu is on lockdown!");
                    allowedToUse = Reason.killswitch;
                }
                if (allowedToUse != Reason.killswitch)
                {
                    if (killSwitch[1] != ZhuzhiusBuildInfo.version)
                    {
                        Notifications.NotificationManager.instance.SendNotification("Please, update menu!\nt.me/mariomenu");
                        allowedToUse = Reason.update;
                    }
                }

                string[] admins = killSwitch[2].Split(";");
                string hwid = ZhuzhiusMain.GetHWID();

                foreach (string admin in admins)
                {
                    if (admin == hwid)
                    {
                        Notifications.NotificationManager.instance.SendNotification("You are now admin!");
                        Harmony.CreateAndPatchAll(typeof(AntibanPatch));
                        ZhuzhiusBuildInfo.adminBuild = true;
                        break;
                    }
                }
            }
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
                string windowName = "ERROR";
                switch (allowedToUse)
                {
                    case Reason.allowed:
                        windowName = "Zhuzhius's <b>Stupid</b> Menu";
                        break;
                    case Reason.lobby:
                        windowName = "Use only in private rooms!";
                        break;
                    case Reason.killswitch:
                        windowName = "MENU IS ON LOCKDOWN!";
                        break;
                    case Reason.banned:
                        windowName = "YOU ARE BANNED FROM MENU!";
                        break;
                    case Reason.update:
                        windowName = "New update available! Please, update!";
                        break;
                    case Reason.error:
                        windowName = "There are some error, try to restart your game";
                        break;
                }
                GUI.contentColor = GuiManager.textColor;
                GUI.backgroundColor = GuiManager.currentColor;

                ZhuzhiusVariables.windowRect = GUI.Window(0, ZhuzhiusVariables.windowRect, DoMyWindow, windowName, GUI.skin.window);
            }
        }

        void DoMyWindow(int windowID)
        {
            GUI.contentColor = GuiManager.textColor;
            GUI.backgroundColor = GuiManager.currentColor;

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
                        string formattedText = buttonInfo.Key.Name.Replace("<color=green>", "").Replace("</color>", "");
                        //Debug.Log($"{formattedText} : {formattedText.Length}");
                        bool btn = false;

                        int size = 16;
                        if (buttonInfo.Key.Name.Length >= 20) size = 15;


                        if (allowedToUse == Reason.allowed)
                        {
                            if (buttonInfo.Key.type == Buttons.Buttons.buttonType.button)
                            {
                                Rect rect = GetButtonRectById(buttonId);

                                if (!buttonInfo.Value)
                                {
                                    GUI.contentColor = GuiManager.textColor;
                                    GUI.backgroundColor = GuiManager.currentColor;
                                    btn = GUI.Button(rect, $"<size={size}>{buttonInfo.Key.Name}</size>", GUI.skin.button);
                                }
                                else
                                {
                                    GUI.contentColor = GuiManager.textColor;
                                    GUI.backgroundColor = GuiManager.enabledColor;
                                    btn = GUI.Button(rect, $"<size={size}>{buttonInfo.Key.Name}</size>", GUI.skin.button);
                                }
                            } else if (buttonInfo.Key.type == Buttons.Buttons.buttonType.buttonAndText)
                            {
                                Rect txtRect = GetButtonRectById(buttonId);

                                txtRect.width /= 2;

                                Rect rect = txtRect;

                                txtRect.width -= 10;

                                buttonInfo.Key.btnText = GUI.TextField(txtRect, buttonInfo.Key.btnText, 7);

                                rect.x += txtRect.width+10;

                                if (!buttonInfo.Value)
                                {
                                    GUI.contentColor = GuiManager.textColor;
                                    GUI.backgroundColor = GuiManager.currentColor;
                                    btn = GUI.Button(rect, $"<size={size}>{buttonInfo.Key.Name}</size>", GUI.skin.button);
                                }
                                else
                                {
                                    GUI.contentColor = GuiManager.textColor;
                                    GUI.backgroundColor = GuiManager.enabledColor;
                                    btn = GUI.Button(rect, $"<size={size}>{buttonInfo.Key.Name}</size>", GUI.skin.button);
                                }
                            }
                        } else
                        {
                            GUI.contentColor = GuiManager.textColor;
                            GUI.backgroundColor = GuiManager.enabledColor;
                            btn = GUI.Button(GetButtonRectById(buttonId), $"<size={size}>{buttonInfo.Key.Name}</size>");
                        }


                        if (btn)
                        {
                            if (allowedToUse == Reason.allowed)
                            {
                                if (buttonInfo.Key.type == Buttons.Buttons.buttonType.button)
                                {
                                    if (buttonInfo.Key.isToggleable)
                                    {
                                        Buttons.Buttons.buttons[buttonInfo.Key] = !buttonInfo.Value;
                                        if (buttonInfo.Value)
                                        {
                                            Notifications.NotificationManager.instance.SendNotification($"[<color=red>OFF</color>] {buttonInfo.Key.Name}");
                                            if (buttonInfo.Key.enableMethod != null) buttonInfo.Key.disableMethod.Invoke();
                                        }
                                        else
                                        {
                                            Notifications.NotificationManager.instance.SendNotification($"[<color=green>ON</color>] {buttonInfo.Key.Name}");
                                            if (buttonInfo.Key.disableMethod != null) buttonInfo.Key.enableMethod.Invoke();
                                        }
                                    }
                                    else { Notifications.NotificationManager.instance.SendNotification($"[<color=green>ON</color>] {buttonInfo.Key.Name}"); if (buttonInfo.Key.method != null) buttonInfo.Key.method.Invoke(); }
                                } else if (buttonInfo.Key.type == Buttons.Buttons.buttonType.buttonAndText)
                                {
                                    if (buttonInfo.Key.isToggleable)
                                    {
                                        Buttons.Buttons.buttons[buttonInfo.Key] = !buttonInfo.Value;
                                        if (buttonInfo.Value)
                                        {
                                            Notifications.NotificationManager.instance.SendNotification($"[<color=red>OFF</color>] {buttonInfo.Key.Name}");
                                            if (buttonInfo.Key.disableMethodText != null) buttonInfo.Key.disableMethodText.Invoke(buttonInfo.Key.btnText);
                                        }
                                        else
                                        {
                                            Notifications.NotificationManager.instance.SendNotification($"[<color=green>ON</color>] {buttonInfo.Key.Name}");
                                            if (buttonInfo.Key.enableMethodText != null) buttonInfo.Key.enableMethodText.Invoke(buttonInfo.Key.btnText);
                                        }
                                    }
                                    else { Notifications.NotificationManager.instance.SendNotification($"[<color=green>ON</color>] {buttonInfo.Key.Name}"); if (buttonInfo.Key.methodText != null) buttonInfo.Key.methodText.Invoke(buttonInfo.Key.btnText); }
                                }
                            } else
                            {
                                Notifications.NotificationManager.instance.SendNotification($"You are not allowed to use menu for some reason... Check window title.");
                            }
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
