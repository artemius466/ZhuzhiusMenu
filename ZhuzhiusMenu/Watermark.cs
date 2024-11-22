using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zhuzhius
{
    public class Watermark : MonoBehaviour
    {
        public static Watermark instance;

        private GUIStyle customStyle;
        private Texture2D solidTexture;
        private Texture2D CreateSolidColorTexture(Color32 color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
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

        public void OnGUI()
        {
            GUI.color = GuiManager.textColor;
            GUI.Label(new Rect(Screen.width-490, Screen.height - 40, 3200, 3200), $"Zhuzhius Mod Menu V{ZhuzhiusBuildInfo.version}", GuiManager.veryBigTextStyle);


            // shit
            int i = 0;
            foreach (var buttonInfo in Buttons.Buttons.buttons)
            {
                if (buttonInfo.Value)
                {
                    string functionName = buttonInfo.Key.Name.Replace("<color=green>", "").Replace("</color>", "").Replace("<color=red>", "");
                    int sizex = 13 * functionName.Length;

                    GUI.color = new Color32(30, 30, 30, 100);
                    GUI.Box(new Rect(Screen.width - sizex, 20*i, sizex, 20), "", customStyle);

                    GUI.color = GuiManager.currentColor;
                    GUI.Box(new Rect(Screen.width - 5, 20 * i, 5, 20), "", customStyle);

                    GUI.Label(new Rect(Screen.width - sizex + 5, 20 * i, 3200, 3200), functionName, GuiManager.smallTextStyle);
                    i++;
                }
            }
        }
    }
}
