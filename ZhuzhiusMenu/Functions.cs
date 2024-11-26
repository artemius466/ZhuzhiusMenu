using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Linq;

namespace Zhuzhius
{
    public static class Functions
    {
        #region Constants
        private const float NORMAL_GRAVITY = 3.25f;
        private const float KOYOTE_TIME = 0.06f;
        private const float LINE_WIDTH = 0.05f;
        private const float LINE_DURATION = 0.01f;
        #endregion

        #region Static Fields
        private static bool _previousMouse;
        private static GameObject _selectedObject;
        private static bool _asd;
        public static bool _crashing;
        private static int _currentPing;
        private static bool _changePing;
        public static bool _changePingCoroutineStarted;
        public static bool _returnHost;
        #endregion

        #region Player Management
        public static PhotonView GetPhotonViewByPlayer(Player player)
        {
            if (player == null) return null;
            return GameObject.FindObjectsOfType<PhotonView>()
                           .FirstOrDefault(view => view.OwnerActorNr == player.ActorNumber);
        }

        public static GameObject GetClick()
        {
            if (!ZhuzhiusControls.LeftMouse) return null;

            var mousePosition = Mouse.current.position.ReadValue();
            var worldPosition = ZhuzhiusVariables.MainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0;

            var hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            return hit.collider?.gameObject;
        }
        #endregion

        #region Visual Effects
        public static void Tracers()
        {
            if (GameManager.Instance?.localPlayer == null) return;

            foreach (var player in GameManager.Instance.players)
            {
                if (player == GameManager.Instance.localPlayer) continue;
                CreateTracerLine(player);
            }
        }

        private static void CreateTracerLine(PlayerController targetPlayer)
        {
            try
            {
                var line = new GameObject("TracerLine");
                var liner = line.AddComponent<LineRenderer>();
                ConfigureLineRenderer(liner);
                SetLinePositions(liner, targetPlayer);
                ZhuzhiusVariables.instance.StartCoroutine(DeleteLine(line));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating tracer line: {e.Message}");
                CleanupAllLines();
            }
        }

        private static void ConfigureLineRenderer(LineRenderer liner)
        {
            var startColor = GuiManager.EnabledColor;
            startColor.a -= 100;
            
            liner.startColor = startColor;
            liner.endColor = GuiManager.TextColor;
            liner.startWidth = LINE_WIDTH - 0.03f;
            liner.endWidth = LINE_WIDTH;
            liner.positionCount = 2;
            liner.useWorldSpace = true;
        }

        private static void SetLinePositions(LineRenderer liner, PlayerController targetPlayer)
        {
            liner.SetPosition(0, GameManager.Instance.localPlayer.transform.position);
            liner.SetPosition(1, targetPlayer.transform.position);
        }

        private static void CleanupAllLines()
        {
            var lines = GameObject.FindObjectsOfType<LineRenderer>();
            foreach (var line in lines)
            {
                GameObject.Destroy(line.gameObject);
            }
        }

        private static IEnumerator DeleteLine(GameObject line)
        {
            yield return new WaitForSecondsRealtime(LINE_DURATION);
            UnityEngine.Object.Destroy(line);
        }
        #endregion

        #region Navigation
        public static void OpenCategory(int category)
        {
            Buttons.Buttons.CurrentCategory = category;
        }
        #endregion

        #region Game Control
        public static void SpeedUp() => Time.timeScale *= 2;
        public static void SlowDown() => Time.timeScale /= 2;

        public static void StaticPlayer()
        {
            if (!GameManager.Instance?.localPlayer) return;
            
            var playerController = GameManager.Instance.localPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.normalGravity = 0f;
                playerController.body.velocity = Vector3.zero;
            }
        }

        public static void NormalPlayer()
        {
            if (!GameManager.Instance?.localPlayer) return;
            
            var playerController = GameManager.Instance.localPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.normalGravity = NORMAL_GRAVITY;
            }
        }

        public static void AirJump()
        {
            if (!GameManager.Instance?.localPlayer) return;
            
            var playerController = GameManager.Instance.localPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.bounce = true;
                playerController.onGround = true;
                playerController.koyoteTime = KOYOTE_TIME;
                playerController.propeller = false;
                playerController.startedSliding = false;
            }
        }
        #endregion

        #region Host Management
        public static void ReturnHostEnable() => _returnHost = true;
        public static void ReturnHostDisable() => _returnHost = false;

        public static void SetMasterSelf()
        {
            if (!PhotonNetwork.InRoom)
            {
                Notifications.NotificationManager.instance?.SendError("Not in a room!");
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("Already host!");
                return;
            }

            if (!ZhuzhiusVariables.SetOldMaster)
            {
                ZhuzhiusVariables.OldMaster = PhotonNetwork.MasterClient;
                ZhuzhiusVariables.SetOldMaster = true;
            }

            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            Notifications.NotificationManager.instance?.SendNotification("You are now host!");
        }

        public static IEnumerator ReturnMaster()
        {
            yield return new WaitForSecondsRealtime(0.2f);
            if (ZhuzhiusVariables.OldMaster != null)
            {
                PhotonNetwork.SetMasterClient(ZhuzhiusVariables.OldMaster);
                ZhuzhiusVariables.SetOldMaster = false;
                ZhuzhiusVariables.OldMaster = null;
            }
        }
        #endregion

        #region Kill All
        public static void KillAll()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            foreach (var player in PhotonNetwork.PlayerListOthers)
            {
                GetPhotonViewByPlayer(player).RPC("Death", RpcTarget.All, false, false);
            }
        }
        #endregion

        #region Kill Gun
        public static void KillGun()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var target = GetClick();
            if (target != null)
            {
                target.GetPhotonView().RPC("Death", RpcTarget.All, false, false);
            }
        }
        #endregion

        #region Instant Win
        public static void InstantWin()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            PhotonNetwork.RaiseEvent(19, PhotonNetwork.LocalPlayer, NetworkUtils.EventAll, SendOptions.SendReliable);
        }
        #endregion

        #region Instant Win Gun
        public static void InstantWinGun()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var target = GetClick();
            if (target != null)
            {
                PhotonNetwork.RaiseEvent(19, target.GetPhotonView().Owner, NetworkUtils.EventAll, SendOptions.SendReliable);
            }
        }
        #endregion

        #region Spawn Star
        public static void SpawnStar()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var position = GameManager.Instance.localPlayer.transform.position + (GameManager.Instance.localPlayer.GetComponent<PlayerController>().facingRight ? Vector3.right : Vector3.left) + new Vector3(0, 0.2f, 0);
            PhotonNetwork.InstantiateRoomObject("Prefabs/BigStar", position, Quaternion.identity);
        }
        #endregion

        #region Kick All
        public static void KickAll()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/1-Up", new Vector3(0, 0, 0), Quaternion.identity);
        }
        #endregion

        #region Destroy All
        public static void DestroyAll()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            PhotonNetwork.DestroyAll();
        }
        #endregion

        #region Random Instantiate Fireball
        public static void RandomInstantiateFireball()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var position = RandomPointOfPlayers();
            PhotonNetwork.InstantiateRoomObject("Prefabs/Fireball", position, Quaternion.identity);
        }
        #endregion

        #region Random Instantiate Iceball
        public static void RandomInstantiateIceball()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var position = RandomPointOfPlayers();
            PhotonNetwork.InstantiateRoomObject("Prefabs/IceBall", position, Quaternion.identity);
        }
        #endregion

        #region Random Instantiate Shit
        public static void RandomInstantiateShit()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var position = RandomPointOfPlayers();
            PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/MegaMushroom", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/FireFlower", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/IceFlower", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/PropellerMushroom", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/Star", position, Quaternion.identity);
        }
        #endregion

        #region Random Instantiate Enemies
        public static void RandomInstantiateEnemies()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var position = RandomPointOfPlayers();
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/BlueKoopa", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Bobomb", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/BulletBill", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Goomba", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Koopa", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/PiranhaPlant", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/RedKoopa", position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Spiny", position, Quaternion.identity);
        }
        #endregion

        #region Random Instantiate Coin
        public static void RandomInstantiateCoin()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            var position = RandomPointOfPlayers();
            PhotonNetwork.InstantiateRoomObject("Prefabs/LooseCoin", position, Quaternion.identity);
        }
        #endregion

        #region Spawn Prefab In Player
        public static int SpawnPrefabInPlayer(GameObject player, string prefab)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return -1;
            }

            var obj = PhotonNetwork.InstantiateRoomObject(prefab, player.transform.position, Quaternion.identity);
            return obj.GetPhotonView().ViewID;
        }
        #endregion

        #region Freeze All
        public static void FreezeAll()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            foreach (var player in GameManager.Instance.players)
            {
                var targetPos = player.transform.position;
                targetPos.x -= 1;
                targetPos.y += 1;
                var coolPos = targetPos;

                for (float i = 0.1f; i <= 1f; i += 0.1f)
                {
                    coolPos = targetPos;
                    PhotonNetwork.InstantiateRoomObject("Prefabs/IceBall", coolPos, Quaternion.identity);
                    targetPos.x += i;
                }
            }
        }
        #endregion

        #region Drag Objects
        public static void DragObjects()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            if (ZhuzhiusControls.LeftMouse && !_previousMouse)
            {
                _previousMouse = true;
                var mousePosition = Mouse.current.position.ReadValue();
                var worldPosition = ZhuzhiusVariables.MainCamera.ScreenToWorldPoint(mousePosition);
                worldPosition.z = 0;
                var hit = Physics2D.Raycast(worldPosition, Vector2.zero);
                if (hit.collider != null)
                {
                    _selectedObject = hit.collider.gameObject;
                    if (_selectedObject.name == "Hitbox")
                    {
                        _selectedObject = hit.collider.gameObject.transform.parent.gameObject;
                    }
                }
            }

            if (ZhuzhiusControls.LeftMouse && _previousMouse && _selectedObject != null)
            {
                var mousePosition = Mouse.current.position.ReadValue();
                var worldPosition = ZhuzhiusVariables.MainCamera.ScreenToWorldPoint(mousePosition);
                worldPosition.z = 0;
                _selectedObject.transform.position = worldPosition;
            }

            if (!ZhuzhiusControls.LeftMouse && _previousMouse)
            {
                _previousMouse = false;
                _selectedObject = null;
            }
        }
        #endregion

        #region Minecraft Mode
        public static void MinecraftMode()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Notifications.NotificationManager.instance?.SendError("You are not host!");
                return;
            }

            if (ZhuzhiusControls.LeftMouse)
            {
                var tilemap = GameManager.Instance.tilemap;
                var mouseWorldPos = ZhuzhiusVariables.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var tilePosition = tilemap.WorldToCell(mouseWorldPos);
                var PlacetilePosition = Vector3Int.FloorToInt(tilePosition);
                var paramaters = new object[] { tilePosition.x, tilePosition.y, 1, 1, new string[] { "SpecialTiles/QuestionCoin" } };
                var options = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.AddToRoomCache
                };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTileBatch, paramaters, SendOptions.SendReliable, options);
            }

            if (ZhuzhiusControls.RightMouse)
            {
                var tilemap = GameManager.Instance.tilemap;
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var tilePosition = tilemap.WorldToCell(mouseWorldPos);
                var PlacetilePosition = Vector3Int.FloorToInt(tilePosition);
                var paramaters = new object[] { tilePosition.x, tilePosition.y, 1, 1, new string[] { "" } };
                var options = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.AddToRoomCache
                };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTileBatch, paramaters, SendOptions.SendReliable, options);
            }
        }
        #endregion

        #region Interact Tile
        public static void InteractTile()
        {
            if (ZhuzhiusControls.LeftMouse)
            {
                var tilemap = GameManager.Instance.tilemap;
                var mouseWorldPos = ZhuzhiusVariables.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var tilePosition = tilemap.WorldToCell(mouseWorldPos);
                var GetTilePosition = Vector3Int.FloorToInt(tilePosition);
                var tile = GameManager.Instance.tilemap.GetTile(GetTilePosition);
                var interactableTile = tile as InteractableTile;
                interactableTile.Interact(GameManager.Instance.localPlayer.GetComponent<PlayerController>(), InteractableTile.InteractionDirection.Up, Utils.TilemapToWorldPosition(GetTilePosition, null));
            }
        }
        #endregion

        #region Crash Rooms
        public static void CrashRooms()
        {
            if (PhotonNetwork.NetworkClientState != ClientState.Joined)
            {
                return;
            }

            _crashing = true;
            if (PhotonNetwork.IsMasterClient)
            {
                if (!_asd)
                {
                    SetLobbyName("FckedByArtemius466");
                    var sigma = false;

                    Utils.GetCustomProperty<bool>(Enums.NetRoomProperties.GameStarted, out sigma);

                    if (sigma)
                    {
                        PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/1-Up", new Vector3(0, 0, 0), Quaternion.identity);
                    }
                    else
                    {
                        ZhuzhiusVariables.instance.StartCoroutine(StartAndCrashAfter1Sec());
                    }

                    if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                    {
                        _asd = true;
                    }
                }
                else
                {
                    if (PhotonNetwork.NetworkClientState == ClientState.Joined)
                    {
                        PhotonNetwork.LeaveRoom(false);

                        var currentRoom = PhotonNetwork.CurrentRoom;
                        var hashtable = new ExitGames.Client.Photon.Hashtable();
                        var gameStarted = Enums.NetRoomProperties.GameStarted;
                        hashtable[gameStarted] = false;
                        currentRoom.SetCustomProperties(hashtable, null, null);

                        PhotonNetwork.DestroyAll();

                        SceneManager.LoadScene("MainMenu");
                    }
                }
            }
            else
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            }
        }
        #endregion

        #region Start And Crash After 1 Sec
        private static IEnumerator StartAndCrashAfter1Sec()
        {
            var currentRoom = PhotonNetwork.CurrentRoom;
            var hashtable = new ExitGames.Client.Photon.Hashtable();
            var gameStarted = Enums.NetRoomProperties.GameStarted;
            hashtable[gameStarted] = true;
            currentRoom.SetCustomProperties(hashtable, null, null);
            var raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };
            PhotonNetwork.RaiseEvent(1, null, raiseEventOptions, SendOptions.SendReliable);
            yield return new WaitForSecondsRealtime(1f);

            PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/1-Up", new Vector3(0, 0, 0), Quaternion.identity);
        }
        #endregion

        #region Set Ping
        public static void SetPing(string text)
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            var ping = int.Parse(text);
            _currentPing = ping;
            _changePing = true;
        }
        #endregion

        #region Set Lobby Name
        public static void SetLobbyName(string text)
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            var currentRoom = PhotonNetwork.CurrentRoom;
            var hashtable = new ExitGames.Client.Photon.Hashtable();
            var hostName = Enums.NetRoomProperties.HostName;
            hashtable[hostName] = text;
            currentRoom.SetCustomProperties(hashtable, null, null);
        }
        #endregion

        #region Room Antiban
        public static void RoomAntiban()
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            var currentRoom = PhotonNetwork.CurrentRoom;
            var hashtable = new ExitGames.Client.Photon.Hashtable();
            var bans = Enums.NetRoomProperties.Bans;

            NameIdPair[] pu;

            Utils.GetCustomProperty<NameIdPair[]>(Enums.NetRoomProperties.Bans, out pu, currentRoom.CustomProperties);

            var flag = false;

            foreach (var pair in pu)
            {
                flag = (pair.name == PhotonNetwork.LocalPlayer.NickName || pair.userId == PhotonNetwork.LocalPlayer.UserId);
            }

            if (flag)
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    SetMasterSelf();
                }

                hashtable[bans] = null;
                currentRoom.SetCustomProperties(hashtable, null, null);
            }
        }
        #endregion

        #region Set Debug Player
        public static void SetDebugPlayer(bool state)
        {
            var hashtable = new ExitGames.Client.Photon.Hashtable();
            var status = Enums.NetPlayerProperties.Status;
            hashtable[status] = state;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
        }
        #endregion

        #region Set Ping Disable
        public static void SetPingDisable(string text)
        {
            _changePing = false;
        }
        #endregion

        #region Set Ping Enable
        public static void SetPingEnable(string text)
        {
            if (!PhotonNetwork.InRoom)
            {
                Notifications.NotificationManager.instance?.SendError("You are not in room!");
                return;
            }

            var ping = int.Parse(text);
            _currentPing = ping;
            _changePing = true;
        }
        #endregion

        #region Fortnite Mode
        public static void FortniteMode()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            // try
            // {
            //     foreach (var bro in GameObject.FindObjectsOfType(MovingPowerup))
            //     {
            //         PhotonNetwork.Destroy(bro.photonView);
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Debug.LogError($"Error in Fortnite mode: {ex.Message}");
            // }

            foreach (var bro in GameManager.Instance.players)
            {
                if (bro.state != Enums.PowerupState.FireFlower)
                {
                    var p = SpawnPrefabInPlayer(bro.gameObject, "Prefabs/Powerup/FireFlower");
                    bro.photonView.RPC("Powerup", RpcTarget.All, new object[] { p });
                }
            }

            var fireballMovers = UnityEngine.Object.FindObjectsOfType<FireballMover>();
            var fireballGameObjects = new List<GameObject>();
            foreach (var mover in fireballMovers)
            {
                var tilemap = GameManager.Instance.tilemap;
                var tilePosition = tilemap.WorldToCell(mover.transform.position);
                var paramaters = new object[] { tilePosition.x, tilePosition.y, 1, 1, new string[] { "SpecialTiles/BrownBrick" } };
                var options = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.AddToRoomCache
                };
                GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTileBatch, paramaters, SendOptions.SendReliable, options);
                PhotonNetwork.Destroy(mover.gameObject);
            }
        }
        #endregion

        #region Update Ping
        public static IEnumerator UpdatePing()
        {
            for (; ; )
            {
                yield return new WaitForSecondsRealtime(2f);
                if (!_changePing)
                {
                    if (PhotonNetwork.InRoom)
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                        {
                            { Enums.NetPlayerProperties.Ping, PhotonNetwork.GetPing() }
                        }, null, null);
                    }
                }
                else
                {
                    if (PhotonNetwork.InRoom)
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                        {
                            { Enums.NetPlayerProperties.Ping, _currentPing }
                        }, null, null);
                    }
                }
            }
            yield break;
        }
        #endregion

        #region Revive On Enter Enable
        public static void ReviveOnEnterEnable()
        {
            //GameManager.Instance.nonSpectatingPlayers.Add(PhotonNetwork.LocalPlayer);
            //GameManager.Instance.SpectationManager.Spectating = false;

            //GameManager.Instance.localPlayer = PhotonNetwork.Instantiate("Prefabs/" + Utils.GetCharacterData(null).prefab, GameManager.Instance.spawnpoint, Quaternion.identity, 0, null);
            //GameManager.Instance.localPlayer.GetComponent<Rigidbody2D>().isKinematic = true;
        }
        #endregion

        #region Revive On Enter Disable
        public static void ReviveOnEnterDisable()
        {
            //reviveOnEnter = false;
        }
        #endregion

        #region Play Sound Explode
        public static void PlaySoundExplode()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Enemy_Bobomb_Explode });
        }
        #endregion

        #region Play Sound Player
        public static void PlaySoundPlayer()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Player_Voice_Selected });
        }
        #endregion

        #region Play Sound UI Quit
        public static void PlaySoundUI_Quit()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Quit });
        }
        #endregion

        #region Play Sound UI 1UP
        public static void PlaySoundUI_1UP()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Powerup_Sound_1UP });
        }
        #endregion

        #region Play Sound UI Error
        public static void PlaySoundUI_Error()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Error });
        }
        #endregion

        #region Play Sound Death
        public static void PlaySoundDeath()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Player_Sound_Death });
        }
        #endregion

        #region Play Sound Start Game
        public static void PlaySoundStartGame()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_StartGame });
        }
        #endregion

        #region Play Player Disconnect
        public static void PlayPlayerDisconnect()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_PlayerDisconnect });
        }
        #endregion

        #region Play Pause
        public static void PlayPause()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Pause });
        }
        #endregion

        #region Play Player Connect
        public static void PlayPlayerConnect()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_PlayerConnect });
        }
        #endregion

        #region Play Match Win
        public static void PlayMatchWin()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Match_Win });
        }
        #endregion

        #region Play Match Lose
        public static void PlayMatchLose()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Match_Win });
        }
        #endregion

        #region Random Point Of Players
        private static Vector3 RandomPointOfPlayers()
        {
            var output = Vector3.zero;
            var players = GameManager.Instance.players;

            output = players[UnityEngine.Random.Range(0, players.Count)].transform.position;

            output.x += UnityEngine.Random.Range(-10, 10);
            output.y += UnityEngine.Random.Range(-10, 10);

            return output;
        }
        #endregion
    }
}
