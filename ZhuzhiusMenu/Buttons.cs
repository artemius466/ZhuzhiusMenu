using System;
using System.Collections.Generic;
using System.Text;
using static Enums;

namespace Zhuzhius.Buttons
{
    public class Button
    {
        public string Name;
        public int Category;
        public bool isToggleable;

        public Action method = null;
        public Action enableMethod = null;
        public Action disableMethod = null;

        public Action<string> methodText = null;
        public Action<string> enableMethodText = null;
        public Action<string> disableMethodText = null;

        public Buttons.buttonType type = Buttons.buttonType.button;
        public string btnText = "";
    }

    public static class Buttons
    {
        public enum buttonType
        {
            button,
            buttonAndText,
            text
        }

        public static int page = 0;
        public static int category = 0;

        public const int mainCategory = 0;
        public const int movementCategory = 1;
        public const int overpoweredCategory = 2;
        public const int spamCategory = 3;
        public const int powerCategory = 4;
        public const int soundsCategory = 5;
        public const int visualCategry = 6;

        public static Dictionary<Button, bool> buttons = new Dictionary<Button, bool>()
        {
            // Main
            { new Button {Name = "Movement Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenCategory(movementCategory), type=buttonType.buttonAndText}, false },
            { new Button {Name = "Visual Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenCategory(visualCategry)}, false },
            { new Button {Name = "Overpowered Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenCategory(overpoweredCategory)}, false },
            { new Button {Name = "Spam Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenCategory(spamCategory)}, false },
            { new Button {Name = "Powerup Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenCategory(powerCategory)}, false },
            { new Button {Name = "Sound Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenCategory(soundsCategory)}, false },
            { new Button {Name = "Clear Notifications", Category = mainCategory, isToggleable = false, method =()=>Notifications.NotificationManager.instance.ClearNotifications()}, false },
            //{ new Button {Name = "Toggle GUI", Category = mainCategory, isToggleable = true}, true },

            // Movement
            { new Button {Name = "Exit Movement Mods", Category = movementCategory, isToggleable = false, method =()=>Functions.OpenCategory(mainCategory)}, false },
            { new Button {Name = "Speed Up Game", Category = movementCategory, isToggleable = false, method =()=>Functions.SpeedUp()}, false },
            { new Button {Name = "Slow Down Game", Category = movementCategory, isToggleable = false, method =()=>Functions.SlowDown()}, false },
            { new Button {Name = "Static Player", Category = movementCategory, isToggleable = true, enableMethod =()=>Functions.StaticPlayer(), disableMethod =()=>Functions.NormalPlayer()}, false },
            //{ new Button {Name = "Air Jump", Category = movementCategory, isToggleable = true, method =()=>Functions.AirJump()}, false },
            
            // Visuals
            { new Button {Name = "Exit Visual Mods", Category = visualCategry, isToggleable = false, method =()=>Functions.OpenCategory(mainCategory)}, false },
            { new Button {Name = "Tracers", Category = visualCategry, isToggleable = true, method =()=>Functions.Tracers()}, false },

            // Overpowered
            { new Button {Name = "Exit Overpowered Mods", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.OpenCategory(mainCategory)}, false },
            { new Button {Name = "Return Host On End Game", Category = overpoweredCategory, isToggleable = true, enableMethod =()=>Functions.ReturnHostEnable(), disableMethod =()=>Functions.ReturnHostDisable()}, false },
            { new Button {Name = "Make Host Self", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.SetMasterSelf()}, false },

            { new Button {Name = "Kill Player Gun [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.KillGun()}, false },
            { new Button {Name = "Kill All [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.KillAll()}, false },

            { new Button {Name = "Kick All [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.KickAll()}, false },

            { new Button {Name = "Instant Win [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.InstantWin()}, false },
            { new Button {Name = "Instant Win Gun [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.InstantWinGun()}, false },

            { new Button {Name = "Destroy All [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.DestroyAll()}, false },

            { new Button {Name = "Spawn Star [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.SpawnStar()}, false },
            
            { new Button {Name = "Place Bricks [SS] [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.PlaceBricks()}, false },
            { new Button {Name = "Drag Objects [SS] [<color=green>HOST</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.DragObjects()}, false },
            { new Button {Name = "Interact With Tiles [SS]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.InteractTile()}, false },

            //{ new Button {Name = "Unlock NickNames", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.UnlockNickNames()}, false },
            //{ new Button {Name = "Unlock Join & Create Buttons", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.UnlockJoinCreate()}, false },

            // Spam
            { new Button {Name = "Exit Spam Mods", Category = spamCategory, isToggleable = false, method =()=>Functions.OpenCategory(mainCategory)}, false },
            { new Button {Name = "Fireball Rain [SS] [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateFireball()}, false },
            { new Button {Name = "Iceball Rain [SS] [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateIceball()}, false },
            { new Button {Name = "Power Up Rain [SS] [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateShit()}, false },
            { new Button {Name = "Enemy Rain [SS] [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateEnemies()}, false },
            { new Button {Name = "Coin Rain [SS] [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateCoin()}, false },

            { new Button {Name = "Exit Powerup Mods", Category = powerCategory, isToggleable = false, method =()=>Functions.OpenCategory(mainCategory)}, false },
            { new Button {Name = "Be BIG [SS] [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/MegaMushroom")}, false },
            { new Button {Name = "Be Mini [SS] [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/MiniMushroom")}, false },
            { new Button {Name = "Be Ice [SS] [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/IceFlower")}, false },
            { new Button {Name = "Be Fire [SS] [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/FireFlower")}, false },
            { new Button {Name = "Be Blue [SS] Shell [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/BlueShell")}, false },
            { new Button {Name = "Be Star [SS] [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/Star")}, false },

            { new Button {Name = "Freeze all [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.FreezeAll()}, false },

            // Sounds
            { new Button {Name = "Exit Sound Mods", Category = soundsCategory, isToggleable = false, method =()=>Functions.OpenCategory(mainCategory)}, false },
            { new Button {Name = "Play Sound Explode [SS]", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundExplode()}, false },
            { new Button {Name = "Play Sound Player Selected [SS]", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundPlayer()}, false },
            { new Button {Name = "Play Sound UI Quit [SS]", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundUI_Quit()}, false },
            { new Button {Name = "Play Sound 1UP [SS]", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundUI_1UP()}, false },
            { new Button {Name = "Play Sound Error [SS]", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundUI_Error()}, false },
            { new Button {Name = "Play Sound Player Death [SS]", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundDeath()}, false },
        };

        public static Dictionary<Button, bool> GetButtonsInCategory(int category)
        {
            Dictionary<Button, bool> output = new Dictionary<Button, bool>();
            foreach (var button in buttons)
            {
                if (button.Key.Category == category) output.Add(button.Key, button.Value);
            }

            return output;
        }

        public static KeyValuePair<Button, bool> GetButtonByname(string name)
        {
            foreach (var button in buttons)
            {
                if (button.Key.Name == name) return button;
            }

            return new KeyValuePair<Button, bool>();
        }
    }
}
