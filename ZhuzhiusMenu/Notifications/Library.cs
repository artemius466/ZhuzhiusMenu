using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private List<Notification> notificationsToRemove = new List<Notification>();

        private GUIStyle customStyle;
        private Texture2D solidTexture;
        private bool isInitialized;

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
                InitializeResources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            CleanupResources();
        }

        private void InitializeResources()
        {
            if (!isInitialized)
            {
                solidTexture = CreateSolidColorTexture(Color.gray);
                customStyle = new GUIStyle
                {
                    normal = { background = solidTexture },
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(10, 10, 10, 10)
                };
                isInitialized = true;
            }
        }

        private void CleanupResources()
        {
            if (solidTexture != null)
            {
                Destroy(solidTexture);
                solidTexture = null;
            }
            notificationList.Clear();
            notificationsToRemove.Clear();
            isInitialized = false;
        }

        private void UpdateNotification(Notification notif)
        {
            if (notif == null) return;

            if (!notif.spawned)
            {
                HandleSpawnTransition(notif);
            }
            else if (notif.despawning)
            {
                HandleDespawnTransition(notif);
            }
        }

        private void HandleSpawnTransition(Notification notif)
        {
            if (notif.transitionProgress >= 1f)
            {
                notif.spawned = true;
                return;
            }

            UpdateTransition(notif, new Vector2(-300, 0), Vector2.zero,
                new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0));
        }

        private void HandleDespawnTransition(Notification notif)
        {
            if (notif.transitionProgress >= 1f)
            {
                notificationsToRemove.Add(notif);
                return;
            }

            UpdateTransition(notif, Vector2.zero, new Vector2(-300, 0),
                new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 255));
        }

        private void UpdateTransition(Notification notif, Vector2 startPos, Vector2 endPos, Color32 startColor, Color32 endColor)
        {
            notif.transitionProgress += Time.deltaTime / notif.transitionDuration;
            notif.transitionProgress = Mathf.Clamp01(notif.transitionProgress);

            notif.offset = Vector2.Lerp(startPos, endPos, notif.transitionProgress);
            notif.colorOffset = Color32.Lerp(startColor, endColor, notif.transitionProgress);
        }

        private void Update()
        {
            try
            {
                for (int i = notificationList.Count - 1; i >= 0; i--)
                {
                    if (i < notificationList.Count)
                    {
                        UpdateNotification(notificationList[i]);
                    }
                }

                if (notificationsToRemove.Count > 0)
                {
                    foreach (var notif in notificationsToRemove)
                    {
                        notificationList.Remove(notif);
                    }
                    notificationsToRemove.Clear();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating notifications: {ex.Message}");
            }
        }

        public void SendNotification(string text, float time = 5f, NotificationType type = NotificationType.message)
        {
            if (string.IsNullOrEmpty(text)) return;

            var notif = new Notification(text)
            {
                notificationType = type
            };
            notificationList.Add(notif);
            StartCoroutine(DeleteNotificationAfter(time, notif));
        }

        public void SendError(string text, float time = 5f)
        {
            SendNotification(text, time, NotificationType.error);
        }

        public void ClearNotifications()
        {
            foreach (var notification in notificationList)
            {
                if (notification != null)
                {
                    notification.transitionProgress = 0f;
                    notification.spawned = true;
                    notification.despawning = true;
                }
            }
        }

        private Texture2D CreateSolidColorTexture(Color32 color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnGUI()
        {
            if (!isInitialized || customStyle == null) return;

            int i = 0;
            foreach (var notif in notificationList)
            {
                if (notif == null) continue;

                string formattedText = notif.text.Replace("<color=green>", "")
                                              .Replace("</color>", "")
                                              .Replace("<color=red>", "");

                int sizex = Mathf.Min(13 * formattedText.Length, Screen.width - 40);
                Color32 textColor = GuiManager.TextColor;
                textColor.a -= notif.colorOffset.a;

                DrawNotificationBackground(notif, i, sizex, textColor);
                DrawNotificationContent(notif, i, sizex, textColor, notif.text);
                
                i++;
            }
        }

        private void DrawNotificationBackground(Notification notif, int index, int width, Color32 textColor)
        {
            GUI.color = notif.notificationType == NotificationType.message ? 
                textColor : new Color32(200, 0, 0, textColor.a);
            
            GUI.Box(new Rect(10 + notif.offset.x, Screen.height - 90 - (75 * index), 8, 60), "", customStyle);
            
            GUI.color = new Color32(30, 30, 30, textColor.a);
            GUI.Box(new Rect(18 + notif.offset.x, Screen.height - 90 - (75 * index), width, 60), "", customStyle);
        }

        private void DrawNotificationContent(Notification notif, int index, int width, Color32 textColor, string formattedText)
        {
            GUI.color = new Color32(255, 255, 255, textColor.a);
            
            string headerText = notif.notificationType == NotificationType.message ? "Notification" : "Error";
            GUI.Label(new Rect(25 + notif.offset.x, Screen.height - 85 - (75 * index), width - 10, 20), 
                     headerText, GuiManager.BigTextStyle);
            
            GUI.Label(new Rect(25 + notif.offset.x, Screen.height - 60 - (80 * index), width - 10, 35), 
                     formattedText, GuiManager.TextStyle);
        }

        private IEnumerator DeleteNotificationAfter(float seconds, Notification notif)
        {
            if (notif == null) yield break;
            
            yield return new WaitForSecondsRealtime(seconds);
            
            if (notif != null)
            {
                notif.transitionProgress = 0f;
                notif.despawning = true;
            }
        }
    }
}