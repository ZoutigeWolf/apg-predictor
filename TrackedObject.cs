using System;
using MelonLoader;
using UnityEngine;

namespace APG_Predictor
{
    [RegisterTypeInIl2Cpp]
    public class TrackedObject : MonoBehaviour
    {
        public TrackedObject(IntPtr ptr) : base(ptr)
        {
        }

        public GameObject Player { get; set; }

        public Vector3 PredictedPosition { get; private set; }

        public Vector3 LastFramePos { get; private set; }

        public void OnUpdate()
        {
            try
            {
                PredictedPosition = GetPredictedPosition();

                LastFramePos = transform.position;
            }
            catch
            {
            }
        }

        private Vector3 GetPredictedPosition()
        {
            if (LastFramePos != null && Player != null)
            {
                float speed = Vector3.Distance(transform.position, LastFramePos) / Time.deltaTime;
                Vector3 dir = (transform.position - LastFramePos).normalized * speed;

                float distance = Vector3.Distance(Player.transform.position, transform.position);
                var bulletTravelTime = distance / 70f;

                Vector3 predictedPos = dir * bulletTravelTime + transform.position;

                return predictedPos;
            }

            return Vector3.zero;
        }
    }
}