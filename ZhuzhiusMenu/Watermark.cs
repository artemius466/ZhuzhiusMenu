using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zhuzhius
{
    public class Watermark : MonoBehaviour
    {
        private static Watermark _instance;
        public static Watermark Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogWarning("Watermark instance is null!");
                return _instance;
            }
            private set => _instance = value;
        }

        private GUIStyle _customStyle;
        private Texture2D _solidTexture;
        private static Texture2D _icon;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeResources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeResources()
        {
            _solidTexture = CreateSolidColorTexture(Color.gray);
            _customStyle = new GUIStyle
            {
                normal = { background = _solidTexture },
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(10, 10, 10, 10)
            };
        }

        private void OnDestroy()
        {
            if (_solidTexture != null)
            {
                Destroy(_solidTexture);
                _solidTexture = null;
            }
            if (_icon != null)
            {
                Destroy(_icon);
                _icon = null;
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

        private Texture2D LoadTextureFromResource(string resourcePath)
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
                {
                    if (stream == null)
                    {
                        Debug.LogError($"Failed to load texture: Resource not found - {resourcePath}");
                        return null;
                    }

                    var fileData = new byte[stream.Length];
                    stream.Read(fileData, 0, (int)stream.Length);

                    var texture = new Texture2D(2, 2);
                    if (!texture.LoadImage(fileData))
                    {
                        Debug.LogError($"Failed to load texture data from {resourcePath}");
                        Destroy(texture);
                        return null;
                    }
                    return texture;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading texture {resourcePath}: {ex.Message}");
                return null;
            }
        }

        public void OnGUI()
        {
            GUI.color = GuiManager.TextColor;
            GUI.Label(new Rect(Screen.width - 490, Screen.height - 40, 320, 40), 
                     "Zhuzhius Mod Menu", 
                     GuiManager.VeryBigTextStyle);

            if (_icon == null)
            {
                _icon = LoadTextureFromResource("Zhuzhius.Resources.icon.png");
            }

            if (_icon != null)
            {
                var pos = new Rect(Screen.width - 128, Screen.height - 128, 128, 128);
                var matrix = GUI.matrix;

                GUIUtility.RotateAroundPivot(Mathf.Sin(Time.time * 2f) * 10f, pos.center);
                GUI.DrawTexture(pos, _icon);
                GUI.matrix = matrix;
            }

            DrawButtons();
        }

        private void DrawButtons()
        {
            int i = 0;
            foreach (var buttonInfo in Buttons.Buttons.ButtonsDict)
            {
                if (buttonInfo.Value)
                {
                    string functionName = buttonInfo.Key.Name.Replace("<color=green>", "").Replace("</color>", "").Replace("<color=red>", "");
                    int sizex = 13 * functionName.Length;

                    GUI.color = new Color32(30, 30, 30, 100);
                    GUI.Box(new Rect(Screen.width - sizex, 20*i, sizex, 20), "", _customStyle);

                    GUI.color = GuiManager.CurrentColor;
                    GUI.Box(new Rect(Screen.width - 5, 20 * i, 5, 20), "", _customStyle);

                    GUI.Label(new Rect(Screen.width - sizex + 5, 20 * i, 3200, 3200), functionName, GuiManager.SmallTextStyle);
                    i++;
                }
            }
        }
    }
}
