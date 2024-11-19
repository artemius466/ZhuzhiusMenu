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
    }

    public static class Buttons
    {
        public static int page = 0;
        public static int category = 0;

        public static readonly int mainCategory = 0;
        public static readonly int movementCategory = 1;
        public static readonly int overpoweredCategory = 2;
        public static readonly int spamCategory = 3;
        public static readonly int powerCategory = 4;
        public static readonly int soundsCategory = 5;

        public static Dictionary<Button, bool> buttons = new Dictionary<Button, bool>()
        {
            // Main
            { new Button {Name = "Movement Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenMovement()}, false },
            { new Button {Name = "Overpowered Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenOverpowered()}, false },
            { new Button {Name = "Spam Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenSpam()}, false },
            { new Button {Name = "Powerup Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenPower()}, false },
            { new Button {Name = "Sound Mods", Category = mainCategory, isToggleable = false, method =()=>Functions.OpenSounds()}, false },

            // Movement
            { new Button {Name = "Exit Movement Mods", Category = movementCategory, isToggleable = false, method =()=>Functions.OpenMain()}, false },
            { new Button {Name = "Speed Up Game", Category = movementCategory, isToggleable = false, method =()=>Functions.SpeedUp()}, false },
            { new Button {Name = "Slow Down Game", Category = movementCategory, isToggleable = false, method =()=>Functions.SlowDown()}, false },
            { new Button {Name = "Static Player", Category = movementCategory, isToggleable = true, enableMethod =()=>Functions.StaticPlayer(), disableMethod =()=>Functions.NormalPlayer()}, false },
            { new Button {Name = "Air Jump", Category = movementCategory, isToggleable = true, method =()=>Functions.AirJump()}, false },

            // Overpowered
            { new Button {Name = "Exit Overpowered Mods", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.OpenMain()}, false },
            { new Button {Name = "Auto Master", Category = overpoweredCategory, isToggleable = true, enableMethod =()=>Functions.AutoMasterEnable(), disableMethod =()=>Functions.AutoMasterDisable()}, false },
            { new Button {Name = "Set Master", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.SetMasterSelf()}, false },

            { new Button {Name = "Kill Player Gun [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.KillGun(), enableMethod=()=>Functions.SilentMasterStart(), disableMethod=()=>Functions.SilentMasterStop()}, false },
            { new Button {Name = "Kill All [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.KillAll()}, false },

            { new Button {Name = "Kick All [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.KickAll()}, false },

            { new Button {Name = "Instant Win [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.InstantWin()}, false },
            { new Button {Name = "Instant Win Gun [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.InstantWinGun(), enableMethod=()=>Functions.SilentMasterStart(), disableMethod=()=>Functions.SilentMasterStop()}, false },

            { new Button {Name = "Destroy All [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.DestroyAll()}, false },

            { new Button {Name = "Spawn Star [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.SpawnStar()}, false },
            
            { new Button {Name = "Place Bricks [SS] [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.PlaceBricks()}, false },
            { new Button {Name = "Drag Objects [SS] [<color=green>MASTER</color>]", Category = overpoweredCategory, isToggleable = true, method =()=>Functions.DragObjects()}, false },

            //{ new Button {Name = "Unlock NickNames", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.UnlockNickNames()}, false },
            //{ new Button {Name = "Unlock Join & Create Buttons", Category = overpoweredCategory, isToggleable = false, method =()=>Functions.UnlockJoinCreate()}, false },

            // Spam
            { new Button {Name = "Exit Spam Mods", Category = spamCategory, isToggleable = false, method =()=>Functions.OpenMain()}, false },
            { new Button {Name = "Fireball Rain [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateFireball(), enableMethod=()=>Functions.SilentMasterStart(), disableMethod=()=>Functions.SilentMasterStop()}, false },
            { new Button {Name = "Iceball Rain [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateIceball(), enableMethod=()=>Functions.SilentMasterStart(), disableMethod=()=>Functions.SilentMasterStop()}, false },
            { new Button {Name = "Power Up Rain [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateShit(), enableMethod=()=>Functions.SilentMasterStart(), disableMethod=()=>Functions.SilentMasterStop()}, false },
            { new Button {Name = "Enemy Rain [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateEnemies(), enableMethod=()=>Functions.SilentMasterStart(), disableMethod=()=>Functions.SilentMasterStop()}, false },
            { new Button {Name = "Coin Rain [<color=green>MASTER</color>]", Category = spamCategory, isToggleable = true, method =()=>Functions.RandomInstantiateCoin(), enableMethod=()=>Functions.SilentMasterStart(), disableMethod=()=>Functions.SilentMasterStop()}, false },

            { new Button {Name = "Exit Powerup Mods", Category = powerCategory, isToggleable = false, method =()=>Functions.OpenMain()}, false },
            { new Button {Name = "Be BIG [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/MegaMushroom")}, false },
            { new Button {Name = "Be Mini [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/MiniMushroom")}, false },
            { new Button {Name = "Be Ice [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/IceFlower")}, false },
            { new Button {Name = "Be Fire [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/FireFlower")}, false },
            { new Button {Name = "Be Blue Shell [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/BlueShell")}, false },
            { new Button {Name = "Be Star [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/Star")}, false },

            { new Button {Name = "Freeze all [<color=green>MASTER</color>]", Category = powerCategory, isToggleable = false, method =()=>Functions.FreezeAll()}, false },

            // Sounds
            { new Button {Name = "Exit Sound Mods", Category = soundsCategory, isToggleable = false, method =()=>Functions.OpenMain()}, false },
            { new Button {Name = "PlaySound Explode", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundExplode()}, false },
            { new Button {Name = "PlaySound Player Selected", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundPlayer()}, false },
            { new Button {Name = "PlaySound UI Quit", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundUI_Quit()}, false },
            { new Button {Name = "PlaySound 1UP", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundUI_1UP()}, false },
            { new Button {Name = "PlaySound Error", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundUI_Error()}, false },
            { new Button {Name = "PlaySound Player Death", Category = soundsCategory, isToggleable = false, method =()=>Functions.PlaySoundDeath()}, false },
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
    }
}
