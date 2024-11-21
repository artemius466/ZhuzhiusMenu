using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;

namespace Zhuzhius.Notifications
{
    public class Notification
    {

        public Notification(string _text)
        {
            text = _text;
        }
        public string text = "";

        public Vector2 offset = new Vector2(-300, 0);
        public Color32 colorOffset = new Color32(0, 0, 0, 255);

        public float transitionDuration = 0.8f;
        public float transitionProgress = 0.0f;

        public bool spawned = false;
        public bool despawning = false;

        public NotificationManager.NotificationType notificationType;
    }

    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager instance;
        public List<Notification> notificationList = new List<Notification>();

        private GUIStyle customStyle;
        private Texture2D solidTexture;

        public enum NotificationType
        {
            error,
            message
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

                solidTexture = CreateSolidColorTexture(Color.gray);

                customStyle = new GUIStyle
                {
                    normal = { background = solidTexture },
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }

        }

        private void UpdateNotification(Notification notif)
        {
            if (!notif.spawned)
            {
                if (notif.transitionProgress == 1f)
                {
                    notif.spawned = true;
                }
                notif.transitionProgress += Time.deltaTime / notif.transitionDuration;

                notif.transitionProgress = Mathf.Clamp01(notif.transitionProgress);

                notif.offset = Vector2.Lerp(new Vector2(-300, 0), new Vector2(0, 0), notif.transitionProgress);
                notif.colorOffset = Color32.Lerp(new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0), notif.transitionProgress);
            }

            if (notif.spawned && notif.despawning)
            {
                if (notif.transitionProgress == 1f)
                {
                    notificationList.Remove(notif);
                }

                notif.transitionProgress += Time.deltaTime / notif.transitionDuration;

                notif.transitionProgress = Mathf.Clamp01(notif.transitionProgress);

                notif.offset = Vector2.Lerp(new Vector2(0, 0), new Vector2(-300, 0), notif.transitionProgress);
                notif.colorOffset = Color32.Lerp(new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 255), notif.transitionProgress);
            }
        }

        private void Update()
        {
            try
            {
                foreach (Notification notif in notificationList)
                {
                    UpdateNotification(notif);
                }
            }
            catch (Exception ex) { }// nvm
        }

        public IEnumerator DeleteNotificationAfter(float seconds, Notification notif)
        {
            yield return new WaitForSecondsRealtime(seconds);
            notif.transitionProgress = 0f;
            notif.despawning = true;
        }

        public void SendNotification(string text)
        {
            Notification notif = new Notification(text);
            notif.notificationType = NotificationType.message;

            notificationList.Add(notif);

            StartCoroutine(DeleteNotificationAfter(5f, notif));
        }

        public void SendError(string text)
        {
            Notification notif = new Notification(text);
            notif.notificationType = NotificationType.error;

            notificationList.Add(notif);

            StartCoroutine(DeleteNotificationAfter(5f, notif));
        }

        private Texture2D CreateSolidColorTexture(Color32 color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }


        public void OnGUI()
        {
            int i = 0;
            foreach (Notification notif in notificationList)
            {
                Color32 textColor = GuiManager.textColor;

                textColor.a -= notif.colorOffset.a;

                if (notif.notificationType == NotificationType.message) GUI.color = textColor;
                else GUI.color = new Color32(200, 0, 0, textColor.a);
                GUI.Box(new Rect(10 + (notif.offset.x), Screen.height - 90 - (75 * i), 8, 70), "", customStyle);

                GUI.color = new Color32(30, 30, 30, textColor.a);
                GUI.Box(new Rect(18 + (notif.offset.x), Screen.height - 90 - (75 * i), 280, 70), "", customStyle);

                GUI.color = new Color32(255, 255, 255, textColor.a);
                if (notif.notificationType == NotificationType.message) GUI.Label(new Rect(25 + (notif.offset.x), Screen.height - 90 - (75 * i), 300, 300), "<b><size=25>Notification</size></b>");
                else GUI.Label(new Rect(25 + (notif.offset.x), Screen.height - 90 - (75 * i), 300, 300), "<b><size=25>Error</size></b>");
                GUI.Label(new Rect(25 + (notif.offset.x), Screen.height - 60 - (75 * i), 300, 300), $"<b><size=17>{notif.text}</size></b>");

                //GUI.Label(new Rect(10+(notif.offset.x), Screen.height - 30 - (23 * i), 10000, 10000), $"<size=20><b>{notif.text}</b></size>");
                i++;
            }
        }
    }
}