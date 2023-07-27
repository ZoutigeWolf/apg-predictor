using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;
using UnityEngine;

using Player.Cameras;

namespace APG_Predictor
{
    public class PredictorMod : MelonMod
    {
        int ownerID = 0;
        GameObject player = null;

        float range = 50f;

        CameraControlBase camera;

        List<TrackedObject> trackedPlayers = new List<TrackedObject>();

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            SetupTrackers();
            UpdateTrackers();

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                range += 5f;
                MelonLogger.Msg($"Detection Range: {range}");
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                range -= 5f;
                MelonLogger.Msg($"Detection range: {range}");
            }

            if (ownerID == 0 || player == null || camera == null)
            {
                ownerID = GetOwnerId();
                player = GetPlayerObject();
                camera = GetCamera();

                return;
            }

            TrackedObject enemy = GetClosestEnemy();

            if (enemy == null)
            {
                return;
            }

            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

            if (distance > 0.75f && distance <= range)
            {
                Vector3 predictedPos = enemy.PredictedPosition;

                if (predictedPos != null && predictedPos != Vector3.zero)
                {
                    Vector3 dir = (predictedPos - camera.transform.position).normalized;

                    if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        if (camera != null)
                        {
                            Vector3 rotation = Quaternion.LookRotation(dir).eulerAngles;
                            
                            if (rotation.x > 90f)
                            {
                                rotation.x -= 360f;
                            }

                            typeof(CameraControlBase).GetProperty("targetEuler").SetValue(camera, rotation, null);
                        }
                    }
                }
            }
        }

        void SetupTrackers()
        {
            if (player == null)
            {
                return;
            }

            trackedPlayers.Clear();

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (obj.Equals(player))
                {
                    continue;
                }

                TrackedObject tracker = obj.GetComponent<TrackedObject>();

                if (tracker == null)
                {
                    tracker = obj.AddComponent<TrackedObject>();
                }

                tracker.Player = player;

                trackedPlayers.Add(tracker);
            }
        }

        void CheckTrackers()
        {
            for (int i = 0; i < trackedPlayers.Count; i++)
            {
                if (trackedPlayers[i] == null)
                {
                    trackedPlayers.RemoveAt(i);
                }
            }
        }

        void UpdateTrackers()
        {
            foreach (TrackedObject tracker in trackedPlayers)
            {
                tracker.OnUpdate();
            }
        }

        int GetOwnerId()
        {
            GameObject clients = GameObject.Find("ClientObjects");

            if (clients == null)
            {
                return 0;
            }

            foreach (var child in GameObject.Find("ClientObjects").transform)
            {
                GameObject client = child.Cast<Transform>().gameObject;

                AoTNetworking.Clients.MirrorClientObject mirror = client.GetComponent<AoTNetworking.Clients.MirrorClientObject>();

                if (mirror.IsOwner)
                {
                    return (int)mirror.Id;
                }
            }

            return 0;
        }

        GameObject GetPlayerObject()
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.GetComponent<AoTNetworking.Players.MirrorNetworkedPlayer>().NetworkSyncOwnerId == ownerID)
                {
                    return player;
                }
            }

            return null;
        }

        CameraControlBase GetCamera()
        {
            GameObject playerCam = GameObject.Find("player_camera(Clone)");

            if (playerCam == null)
            {
                return null;
            }

            return playerCam.GetComponent<Player.Cameras.CameraControlBase>();
        }

        TrackedObject GetClosestEnemy()
        {
            TrackedObject closestEnemy = null;
            float currentDistance = 10000000000000f;

            foreach (TrackedObject enemy in trackedPlayers)
            {
                if (enemy.GetComponent<AoTNetworking.Players.MirrorNetworkedPlayer>().SyncOwnerId == ownerID)
                {
                    continue;
                }

                Vector3 diff = enemy.transform.position - player.transform.position;
                float dist = diff.magnitude;

                if (dist < currentDistance)
                {
                    closestEnemy = enemy;
                    currentDistance = dist;
                }
            }

            return closestEnemy;
        }
    }
}
