using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zhuzhius.Buttons
{
    public class Button
    {
        public string Name { get; set; }
        public int Category { get; set; }
        public bool IsToggleable { get; set; }
        public Buttons.ButtonType Type { get; set; } = Buttons.ButtonType.Button;
        public string ButtonText { get; set; } = string.Empty;

        public Action Method { get; set; }
        public Action EnableMethod { get; set; }
        public Action DisableMethod { get; set; }

        public Action<string> MethodText { get; set; }
        public Action<string> EnableMethodText { get; set; }
        public Action<string> DisableMethodText { get; set; }
    }

    public static class Buttons
    {
        public enum ButtonType
        {
            Button,
            ButtonAndText,
            Text
        }

        private static readonly Dictionary<Button, bool> _buttons = new Dictionary<Button, bool>();
        
        public static IReadOnlyDictionary<Button, bool> ButtonsDict => _buttons;

        public static int CurrentPage { get; set; }
        public static int CurrentCategory { get; set; }

        public const int MAIN_CATEGORY = 0;
        public const int MOVEMENT_CATEGORY = 1;
        public const int OVERPOWERED_CATEGORY = 2;
        public const int SPAM_CATEGORY = 3;
        public const int POWER_CATEGORY = 4;
        public const int SOUNDS_CATEGORY = 5;
        public const int VISUAL_CATEGORY = 6;

        static Buttons()
        {
            InitializeButtons();
        }

        private static void InitializeButtons()
        {
            // Main Category
            AddButton(new Button { Name = "Movement Mods", Category = MAIN_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(MOVEMENT_CATEGORY) });
            AddButton(new Button { Name = "Visual Mods", Category = MAIN_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(VISUAL_CATEGORY) });
            AddButton(new Button { Name = "Overpowered Mods", Category = MAIN_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(OVERPOWERED_CATEGORY) });
            AddButton(new Button { Name = "Spam Mods", Category = MAIN_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(SPAM_CATEGORY) });
            AddButton(new Button { Name = "Powerup Mods", Category = MAIN_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(POWER_CATEGORY) });
            AddButton(new Button { Name = "Sound Mods", Category = MAIN_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(SOUNDS_CATEGORY) });
            AddButton(new Button { Name = "Clear Notifications", Category = MAIN_CATEGORY, IsToggleable = false, Method = () => Notifications.NotificationManager.instance.ClearNotifications() });

            // Movement Category
            AddButton(new Button { Name = "Exit Movement Mods", Category = MOVEMENT_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(MAIN_CATEGORY) });
            AddButton(new Button { Name = "Speed Up Game", Category = MOVEMENT_CATEGORY, IsToggleable = false, Method = () => Functions.SpeedUp() });
            AddButton(new Button { Name = "Slow Down Game", Category = MOVEMENT_CATEGORY, IsToggleable = false, Method = () => Functions.SlowDown() });
            AddButton(new Button { Name = "Static Player", Category = MOVEMENT_CATEGORY, IsToggleable = true, EnableMethod = () => Functions.StaticPlayer(), DisableMethod = () => Functions.NormalPlayer() });

            // Visual Category
            AddButton(new Button { Name = "Exit Visual Mods", Category = VISUAL_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(MAIN_CATEGORY) });
            AddButton(new Button { Name = "Tracers", Category = VISUAL_CATEGORY, IsToggleable = true, Method = () => Functions.Tracers() });

            // Overpowered Category
            AddButton(new Button { Name = "Exit Overpowered Mods", Category = OVERPOWERED_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(MAIN_CATEGORY) });
            AddButton(new Button { Name = "Return Host On End Game", Category = OVERPOWERED_CATEGORY, IsToggleable = true, EnableMethod = () => Functions.ReturnHostEnable(), DisableMethod = () => Functions.ReturnHostDisable() });
            AddButton(new Button { Name = "Make Host Self", Category = OVERPOWERED_CATEGORY, IsToggleable = false, Method = () => Functions.SetMasterSelf() });
            AddButton(new Button { Name = "Kill Player Gun [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = () => Functions.KillGun() });
            AddButton(new Button { Name = "Kill All [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = false, Method = () => Functions.KillAll() });
            AddButton(new Button { Name = "Kick All [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = false, Method = () => Functions.KickAll() });
            AddButton(new Button { Name = "Instant Win [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = false, Method = () => Functions.InstantWin() });
            AddButton(new Button { Name = "Instant Win Gun [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = () => Functions.InstantWinGun() });
            AddButton(new Button { Name = "Destroy All [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = false, Method = () => Functions.DestroyAll() });
            AddButton(new Button { Name = "Spawn Star [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = false, Method = () => Functions.SpawnStar() });
            AddButton(new Button { Name = "Minecraft Mode [SS] [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = () => Functions.MinecraftMode() });
            AddButton(new Button { Name = "Drag Objects [SS] [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = () => Functions.DragObjects() });
            AddButton(new Button { Name = "Interact With Tiles [SS]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = () => Functions.InteractTile() });
            AddButton(new Button { Name = "Set Ping [SS]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, MethodText = Functions.SetPing, DisableMethodText = Functions.SetPingDisable, EnableMethodText = Functions.SetPingEnable, Type = Buttons.ButtonType.ButtonAndText });
            AddButton(new Button { Name = "Change Lobby Name [SS]", Category = OVERPOWERED_CATEGORY, IsToggleable = false, MethodText = Functions.SetLobbyName, Type = Buttons.ButtonType.ButtonAndText });
            AddButton(new Button { Name = "Add Debug Icon To Self [SS]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, EnableMethod = () => Functions.SetDebugPlayer(true), DisableMethod = () => Functions.SetDebugPlayer(false) });
            AddButton(new Button { Name = "Room Antiban", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = Functions.RoomAntiban });
            AddButton(new Button { Name = "Fortnite Mode [SS] [<color=green>HOST</color>]", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = Functions.FortniteMode });
            AddButton(new Button { Name = "Crash All Rooms On Server", Category = OVERPOWERED_CATEGORY, IsToggleable = true, Method = Functions.CrashRooms });

            // Spam Category
            AddButton(new Button { Name = "Exit Spam Mods", Category = SPAM_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(MAIN_CATEGORY) });
            AddButton(new Button { Name = "Fireball Rain [SS] [<color=green>MASTER</color>]", Category = SPAM_CATEGORY, IsToggleable = true, Method = () => Functions.RandomInstantiateFireball() });
            AddButton(new Button { Name = "Iceball Rain [SS] [<color=green>MASTER</color>]", Category = SPAM_CATEGORY, IsToggleable = true, Method = () => Functions.RandomInstantiateIceball() });
            AddButton(new Button { Name = "Power Up Rain [SS] [<color=green>MASTER</color>]", Category = SPAM_CATEGORY, IsToggleable = true, Method = () => Functions.RandomInstantiateShit() });
            AddButton(new Button { Name = "Enemy Rain [SS] [<color=green>MASTER</color>]", Category = SPAM_CATEGORY, IsToggleable = true, Method = () => Functions.RandomInstantiateEnemies() });
            AddButton(new Button { Name = "Coin Rain [SS] [<color=green>MASTER</color>]", Category = SPAM_CATEGORY, IsToggleable = true, Method = () => Functions.RandomInstantiateCoin() });

            // Power Category
            AddButton(new Button { Name = "Exit Powerup Mods", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(MAIN_CATEGORY) });
            AddButton(new Button { Name = "Be BIG [SS] [<color=green>MASTER</color>]", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/MegaMushroom") });
            AddButton(new Button { Name = "Be Mini [SS] [<color=green>MASTER</color>]", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/MiniMushroom") });
            AddButton(new Button { Name = "Be Ice [SS] [<color=green>MASTER</color>]", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/IceFlower") });
            AddButton(new Button { Name = "Be Fire [SS] [<color=green>MASTER</color>]", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/FireFlower") });
            AddButton(new Button { Name = "Be Blue [SS] Shell [<color=green>MASTER</color>]", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/BlueShell") });
            AddButton(new Button { Name = "Be Star [SS] [<color=green>MASTER</color>]", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.SpawnPrefabInPlayer(GameManager.Instance.localPlayer, "Prefabs/Powerup/Star") });
            AddButton(new Button { Name = "Freeze all [<color=green>MASTER</color>]", Category = POWER_CATEGORY, IsToggleable = false, Method = () => Functions.FreezeAll() });

            // Sounds Category
            AddButton(new Button { Name = "Exit Sound Mods", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.OpenCategory(MAIN_CATEGORY) });
            AddButton(new Button { Name = "Play Sound Explode [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlaySoundExplode() });
            AddButton(new Button { Name = "Play Sound Player Selected [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlaySoundPlayer() });
            AddButton(new Button { Name = "Play Sound UI Quit [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlaySoundUI_Quit() });
            AddButton(new Button { Name = "Play Sound 1UP [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlaySoundUI_1UP() });
            AddButton(new Button { Name = "Play Sound Error [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlaySoundUI_Error() });
            AddButton(new Button { Name = "Play Sound Player Death [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlaySoundDeath() });
            AddButton(new Button { Name = "Play Sound Start Game [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlaySoundStartGame() });
            AddButton(new Button { Name = "Play Sound Player Disconnect [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlayPlayerDisconnect() });
            AddButton(new Button { Name = "Play Sound Pause [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlayPause() });
            AddButton(new Button { Name = "Play Sound Player Connect [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlayPlayerConnect() });
            AddButton(new Button { Name = "Play Sound Match Win [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlayMatchWin() });
            AddButton(new Button { Name = "Play Sound Match Lose [SS]", Category = SOUNDS_CATEGORY, IsToggleable = false, Method = () => Functions.PlayMatchLose() });
        }

        private static void AddButton(Button button)
        {
            _buttons.Add(button, false);
        }

        public static void ToggleButton(Button button)
        {
            if (!_buttons.ContainsKey(button)) return;

            if (!button.IsToggleable)
            {
                button.Method?.Invoke();
                return;
            }

            _buttons[button] = !_buttons[button];

            if (_buttons[button])
            {
                button.EnableMethod?.Invoke();
                if (button.Type == Buttons.ButtonType.ButtonAndText)
                {
                    button.EnableMethodText?.Invoke(button.ButtonText);
                }
                Notifications.NotificationManager.instance?.SendNotification($"[<color=green>ON</color>] {button.Name}");
            }
            else
            {
                button.DisableMethod?.Invoke();
                if (button.Type == Buttons.ButtonType.ButtonAndText)
                {
                    button.DisableMethodText?.Invoke(button.ButtonText);
                }
                Notifications.NotificationManager.instance?.SendNotification($"[<color=red>OFF</color>] {button.Name}");
            }
        }

        public static IEnumerable<KeyValuePair<Button, bool>> GetButtonsInCategory(int category)
        {
            return _buttons.Where(b => b.Key.Category == category);
        }

        public static Button GetButtonByName(string name)
        {
            return _buttons.Keys.FirstOrDefault(b => b.Name == name);
        }
    }
}
