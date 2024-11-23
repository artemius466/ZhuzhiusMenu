using ExitGames.Client.Photon;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Zhuzhius.Buttons;
using HarmonyLib;
using static System.Net.Mime.MediaTypeNames;

namespace Zhuzhius
{
    public static class Functions
    {
        public static PhotonView GetPhotonViewByPlayer(Player player)
        {
            foreach (PhotonView view in GameObject.FindObjectsOfType<PhotonView>())
            {
                if (view.OwnerActorNr == player.ActorNumber)
                {
                    return view;
                }
            }

            return null;
        }

        public static GameObject GetClick()
        {
            if (ZhuzhiusControls.leftMouse)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 worldPosition = ZhuzhiusVariables.mainCamera.ScreenToWorldPoint(mousePosition);

                worldPosition.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
                return hit.collider.gameObject;
            }
            else
            {
                return null;
            }
        }

        public static void Tracers()
        {
            float lineWidth = 0.05f;
            foreach (PlayerController player in GameManager.Instance.players)
            {
                if (player != GameManager.Instance.localPlayer)
                {
                    try
                    {
                        //UnityEngine.Object.Destroy(line);
                        GameObject line = new GameObject("Line");
                        LineRenderer liner = line.AddComponent<LineRenderer>();
                        UnityEngine.Color thecolor = GuiManager.enabledColor;
                        UnityEngine.Color thecolor2 = GuiManager.textColor;

                        thecolor.a -= 100;

                        liner.startColor = thecolor; liner.endColor = thecolor2; liner.startWidth = lineWidth-0.03f; liner.endWidth = lineWidth; liner.positionCount = 2; liner.useWorldSpace = true;
                        liner.SetPosition(0, GameManager.Instance.localPlayer.transform.position);
                        liner.SetPosition(1, player.transform.position);


                        ZhuzhiusVariables.instance.StartCoroutine(DeleteLine(line));
                    }
                    catch (Exception e)
                    {
                        foreach (var line1 in GameObject.FindObjectsOfType(typeof(LineRenderer)))
                        {
                            GameObject.Destroy(line1);
                        }
                    }
                }
            }
        }

        private static IEnumerator DeleteLine(GameObject line)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            UnityEngine.Object.Destroy(line);
            
        }

        public static void OpenCategory(int category)
        {
            Buttons.Buttons.category = category;
        }

        public static void SpeedUp()
        {
            Time.timeScale *= 2;
        }

        public static void SlowDown()
        {
            Time.timeScale /= 2;
        }

        public static void StaticPlayer()
        {
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().normalGravity = 0f;
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().body.velocity = Vector3.zero;
        }
        public static void NormalPlayer()
        {
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().normalGravity = 3.25f;
        }

        public static void AirJump()
        {
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().bounce = true;
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().onGround = true;
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().koyoteTime = 0.06f;
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().propeller = false;
            GameManager.Instance.localPlayer.GetComponent<PlayerController>().startedSliding = false;
            //GameManager.Instance.localPlayer
            //UnityEngine.Debug.Log("YOOOO");
        }

        // Overpowered
        public static bool returnHost = false;

        public static void ReturnHostEnable()
        {
            returnHost = true;
        }

        public static void ReturnHostDisable()
        {
            returnHost = false;
        }

        public static void SetMasterSelf()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                if (!ZhuzhiusVariables.SetOldMaster)
                {
                    ZhuzhiusVariables.OldMaster = PhotonNetwork.MasterClient;
                    ZhuzhiusVariables.SetOldMaster = true;
                }

                Player target = PhotonNetwork.LocalPlayer;
                PhotonNetwork.SetMasterClient(target);

                Notifications.NotificationManager.instance.SendNotification("You are now host!");
            }
            else
            {
                Notifications.NotificationManager.instance.SendError("You are already host!");
            }
        }

        public static IEnumerator ReturnMaster()
        {
            yield return new WaitForSecondsRealtime(0.2f);
            PhotonNetwork.SetMasterClient(ZhuzhiusVariables.OldMaster);
            ZhuzhiusVariables.SetOldMaster = false;
            ZhuzhiusVariables.OldMaster = null;
        }

        public static void KillAll()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (Player sigma in PhotonNetwork.PlayerListOthers)
                {
                    GetPhotonViewByPlayer(sigma).RPC("Death", RpcTarget.All, false, false);
                }
            } else
            {
                Notifications.NotificationManager.instance.SendError("You are not host!");
            }
        }

        public static void KillGun()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject target = GetClick();

                if (target != null)
                {
                    if (PhotonNetwork.IsMasterClient) target.GetPhotonView().RPC("Death", RpcTarget.All, false, false);
                    else Notifications.NotificationManager.instance.SendError("You are not host!");
                }
            }
        }

        public static void InstantWin()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(19, PhotonNetwork.LocalPlayer, NetworkUtils.EventAll, SendOptions.SendReliable);
            }
            else Notifications.NotificationManager.instance.SendError("You are not host!");
        }

        public static void InstantWinGun()
        {
            GameObject target = GetClick();

            if (target != null)
            {
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.RaiseEvent(19, target.GetPhotonView().Owner, NetworkUtils.EventAll, SendOptions.SendReliable);
                else Notifications.NotificationManager.instance.SendError("You are not host!");
            }
        }

        private static void RespawnStar(Vector3 pos)
        {
            PhotonNetwork.InstantiateRoomObject("Prefabs/BigStar", pos, Quaternion.identity);
        }

        public static void SpawnStar()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                RespawnStar(GameManager.Instance.localPlayer.transform.position + (GameManager.Instance.localPlayer.GetComponent<PlayerController>().facingRight ? Vector3.right : Vector3.left) + new Vector3(0, 0.2f, 0));
            }
            else Notifications.NotificationManager.instance.SendError("You are not host!");
        }

        public static void KickAll()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/1-Up", new Vector3(0, 0, 0), Quaternion.identity);
            }
            else Notifications.NotificationManager.instance.SendError("You are not host!");
        }

        public static void DestroyAll()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();
            }
            else Notifications.NotificationManager.instance.SendError("You are not host!");
        }

        private static Vector3 RandomPointOfPlayers()
        {
            Vector3 output = Vector3.zero;
            var players = GameManager.Instance.players;

            output = players[UnityEngine.Random.Range(0, players.Count)].transform.position;

            output.x += UnityEngine.Random.Range(-10, 10);
            output.y += UnityEngine.Random.Range(-10, 10);

            return output;
        }

        public static void RandomInstantiateFireball()
        {
            if (PhotonNetwork.IsMasterClient) PhotonNetwork.InstantiateRoomObject("Prefabs/Fireball", RandomPointOfPlayers(), Quaternion.identity);
            else Notifications.NotificationManager.instance.SendError("You are not host!");
        }

        public static void RandomInstantiateIceball()
        {
            if (PhotonNetwork.IsMasterClient) PhotonNetwork.InstantiateRoomObject("Prefabs/IceBall", RandomPointOfPlayers(), Quaternion.identity);
        }

        public static void RandomInstantiateShit()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/MegaMushroom", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/FireFlower", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/IceFlower", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/PropellerMushroom", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Powerup/Star", RandomPointOfPlayers(), Quaternion.identity);
            }
        }

        public static void RandomInstantiateEnemies()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/BlueKoopa", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Bobomb", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/BulletBill", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Goomba", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Koopa", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/PiranhaPlant", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/RedKoopa", RandomPointOfPlayers(), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject("Prefabs/Enemy/Spiny", RandomPointOfPlayers(), Quaternion.identity);
            }
        }

        public static void RandomInstantiateCoin()
        {
            if (PhotonNetwork.IsMasterClient) PhotonNetwork.InstantiateRoomObject("Prefabs/LooseCoin", RandomPointOfPlayers(), Quaternion.identity);
            else Notifications.NotificationManager.instance.SendError("You are not host!");
        }

        public static int SpawnPrefabInPlayer(GameObject player, string prefab)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var obj = PhotonNetwork.InstantiateRoomObject(prefab, player.transform.position, Quaternion.identity);

                return obj.GetPhotonView().ViewID;
            }
            return -1;
        }

        public static void FreezeAll()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (PlayerController player in GameManager.Instance.players)
                {
                    Vector3 targetPos = player.transform.position;
                    targetPos.x -= 1;
                    targetPos.y += 1;
                    Vector3 coolPos = targetPos;

                    for (float i = 0.1f; i <= 1f; i += 0.1f)
                    {
                        coolPos = targetPos;
                        PhotonNetwork.InstantiateRoomObject("Prefabs/IceBall", coolPos, Quaternion.identity);
                        targetPos.x += i;
                        Debug.Log(i);
                    }
                }
            }
        }

        public static bool previousMouse = false;
        public static GameObject selectedObject;

        public static void DragObjects()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (ZhuzhiusControls.leftMouse && !previousMouse)
                {
                    previousMouse = true;
                    Vector2 mousePosition = Mouse.current.position.ReadValue();
                    Vector3 worldPosition = ZhuzhiusVariables.mainCamera.ScreenToWorldPoint(mousePosition);
                    worldPosition.z = 0;
                    RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
                    if (hit.collider != null)
                    {

                        selectedObject = hit.collider.gameObject;
                        if (selectedObject.name == "Hitbox")
                        {
                            selectedObject = hit.collider.gameObject.transform.parent.gameObject;
                        }
                        Debug.Log(selectedObject);
                    }
                }
                if (ZhuzhiusControls.leftMouse && previousMouse && selectedObject != null)
                {
                    Vector2 mousePosition = Mouse.current.position.ReadValue();
                    Vector3 worldPosition = ZhuzhiusVariables.mainCamera.ScreenToWorldPoint(mousePosition);
                    worldPosition.z = 0;
                    selectedObject.transform.position = worldPosition;
                }
                if (!ZhuzhiusControls.leftMouse && previousMouse)
                {
                    previousMouse = false;
                    selectedObject = null;
                }
            } else
            {
                Notifications.NotificationManager.instance.SendError("You are not host!");
                var thisBtn = Buttons.Buttons.GetButtonByname("Drag Objects [SS] [<color=green>HOST</color>]");

                Buttons.Buttons.buttons[thisBtn.Key] = false;
            }
        }

        public static void MinecraftMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (ZhuzhiusControls.leftMouse)
                {
                    Tilemap tilemap = GameManager.Instance.tilemap;
                    Vector3 mouseWorldPos = ZhuzhiusVariables.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
                    Vector3Int PlacetilePosition = Vector3Int.FloorToInt(tilePosition);
                    object[] paramaters = { tilePosition.x, tilePosition.y, 1, 1, new string[] { "SpecialTiles/QuestionCoin" } };
                    RaiseEventOptions options = new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others,
                        CachingOption = EventCaching.AddToRoomCache
                    };
                    GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTileBatch, paramaters, SendOptions.SendReliable, options);
                }
                if (ZhuzhiusControls.rightMouse)
                {
                    Tilemap tilemap = GameManager.Instance.tilemap;
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
                    Vector3Int PlacetilePosition = Vector3Int.FloorToInt(tilePosition);
                    object[] paramaters = { tilePosition.x, tilePosition.y, 1, 1, new string[] { "" } };
                    RaiseEventOptions options = new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others,
                        CachingOption = EventCaching.AddToRoomCache
                    };
                    GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTileBatch, paramaters, SendOptions.SendReliable, options);
                }

            }
            else
            {
                Notifications.NotificationManager.instance.SendError("You are not host!");
                var thisBtn = Buttons.Buttons.GetButtonByname("Place Bricks [SS] [<color=green>HOST</color>]");

                Buttons.Buttons.buttons[thisBtn.Key] = false;
            }
        }

        public static void InteractTile()
        {
            if (ZhuzhiusControls.leftMouse)
            {
                Tilemap tilemap = GameManager.Instance.tilemap;
                Vector3 mouseWorldPos = ZhuzhiusVariables.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
                Vector3Int GetTilePosition = Vector3Int.FloorToInt(tilePosition);
                TileBase tile = GameManager.Instance.tilemap.GetTile(GetTilePosition);
                InteractableTile interactableTile = tile as InteractableTile;
                interactableTile.Interact(GameManager.Instance.localPlayer.GetComponent<PlayerController>(), InteractableTile.InteractionDirection.Up, Utils.TilemapToWorldPosition(GetTilePosition, null));
            }
        }

        public static int currentPing;
        public static bool changePing = false;
        public static bool changePingCoroutineStarted = false;

        public static void SetPing(string text)
        {
            int ping = int.Parse(text);
            if (PhotonNetwork.InRoom)
            {
                currentPing = ping;
                changePing = true;
            }
        }
        
        public static void SetLobbyName(string text)
        {
            if (PhotonNetwork.InRoom)
            {
                Room currentRoom = PhotonNetwork.CurrentRoom;
                ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
                object hostName = Enums.NetRoomProperties.HostName;
                object bans = Enums.NetRoomProperties.Bans;
                hashtable[hostName] = text;
                //hashtable[bans] = true;
                Debug.Log(currentRoom.CustomProperties[bans]);
                currentRoom.SetCustomProperties(hashtable, null, null);
            }
        }

        public static void RoomAntiban()
        {
            if (PhotonNetwork.InRoom)
            {
                Room currentRoom = PhotonNetwork.CurrentRoom;
                ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
                object bans = Enums.NetRoomProperties.Bans;

                NameIdPair[] pu;

                Utils.GetCustomProperty<NameIdPair[]>(Enums.NetRoomProperties.Bans, out pu, currentRoom.CustomProperties);

                bool flag = false;

                foreach (NameIdPair pair in pu)
                {
                    flag = (pair.name == PhotonNetwork.LocalPlayer.NickName || pair.userId == PhotonNetwork.LocalPlayer.UserId);
                }
                Debug.Log(flag);

                if (flag)
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        SetMasterSelf();
                    }


                    hashtable[bans] = null;
                    //hashtable[bans] = true;
                    Debug.Log(currentRoom.CustomProperties[bans]);
                    currentRoom.SetCustomProperties(hashtable, null, null);
                }

            }
        }

        public static void SetDebugPlayer(bool state)
        {
            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
            object status = Enums.NetPlayerProperties.Status;
            hashtable[status] = state;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
        }

        public static void SetPingDisable(string text)
        {
            changePing = false;
        }

        public static void SetPingEnable(string text)
        {
            int ping = int.Parse(text);
            if (PhotonNetwork.InRoom)
            {
                currentPing = ping;
                changePing = true;
            }
            else
            {
                Notifications.NotificationManager.instance.SendError("You are not in room!");
            }
        }

        public static void FortniteMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (GameObject bro in GameObject.FindObjectsOfType(typeof(MovingPowerup)))
                {
                    PhotonNetwork.Destroy(bro);
                }

                foreach (PlayerController bro in GameManager.Instance.players)
                {
                    if (bro.state != Enums.PowerupState.FireFlower)
                    {
                        var p = SpawnPrefabInPlayer(bro.gameObject, "Prefabs/Powerup/FireFlower");
                        bro.photonView.RPC("Powerup", RpcTarget.All, new object[]  { p });
                    }
                }

                FireballMover[] fireballMovers = UnityEngine.Object.FindObjectsOfType<FireballMover>();
                List<GameObject> fireballGameObjects = new List<GameObject>();
                foreach (FireballMover mover in fireballMovers)
                {
                    Tilemap tilemap = GameManager.Instance.tilemap;
                    Vector3Int tilePosition = tilemap.WorldToCell(mover.transform.position);
                    object[] paramaters = { tilePosition.x, tilePosition.y, 1, 1, new string[] { "SpecialTiles/BrownBrick" } };
                    RaiseEventOptions options = new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others,
                        CachingOption = EventCaching.AddToRoomCache
                    };
                    GameManager.Instance.SendAndExecuteEvent(Enums.NetEventIds.SetTileBatch, paramaters, SendOptions.SendReliable, options);
                    PhotonNetwork.Destroy(mover.gameObject);
                }
            }
        }

        public static IEnumerator UpdatePing()
        {
            for (; ; )
            {
                yield return new WaitForSecondsRealtime(2f);
                if (!changePing)
                {
                    if (PhotonNetwork.InRoom)
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
                        {
                            Enums.NetPlayerProperties.Ping,
                            PhotonNetwork.GetPing()
                        } }, null, null);
                    }
                } else
                {
                    if (PhotonNetwork.InRoom)
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
                        {
                            Enums.NetPlayerProperties.Ping,
                            currentPing
                        } }, null, null);
                    }
                }
            }
            yield break;
        }

        public static bool reviveOnEnter = false;

        public static void ReviveOnEnterEnable()
        {
            //reviveOnEnter = true;

            //GameManager.Instance.nonSpectatingPlayers.Add(PhotonNetwork.LocalPlayer);
            //GameManager.Instance.SpectationManager.Spectating = false;

            //GameManager.Instance.localPlayer = PhotonNetwork.Instantiate("Prefabs/" + Utils.GetCharacterData(null).prefab, GameManager.Instance.spawnpoint, Quaternion.identity, 0, null);
            //GameManager.Instance.localPlayer.GetComponent<Rigidbody2D>().isKinematic = true;
        }
        
        public static void ReviveOnEnterDisable()
        {
            //reviveOnEnter = false;
        }

        public static void PlaySoundExplode()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Enemy_Bobomb_Explode });
        }
        public static void PlaySoundPlayer()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Player_Voice_Selected });
        }
        public static void PlaySoundUI_Quit()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Quit });
        }
        public static void PlaySoundUI_1UP()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Powerup_Sound_1UP });
        }
        public static void PlaySoundUI_Error()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Error });
        }

        public static void PlaySoundDeath()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.Player_Sound_Death });
        }
        public static void PlaySoundStartGame()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_StartGame });
        }
        public static void PlayPlayerDisconnect()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_PlayerDisconnect });
        }
        public static void PlayPause()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Pause });
        }
        public static void PlayPlayerConnect()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_PlayerConnect });
        }
        public static void PlayMatchWin()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Match_Win });
        } 
        public static void PlayMatchLose()
        {
            GameManager.Instance.localPlayer.GetPhotonView().RPC("PlaySound", RpcTarget.All, new object[] { Enums.Sounds.UI_Match_Win });
        }
    }
}
