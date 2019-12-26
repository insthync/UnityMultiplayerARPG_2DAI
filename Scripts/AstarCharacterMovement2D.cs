﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace MultiplayerARPG
{
    public class AstarCharacterMovement2D : RigidBodyEntityMovement2D
    {
        private IAstarAI cacheAIPath;
        public IAstarAI CacheAIPath
        {
            get
            {
                if (cacheAIPath == null)
                    cacheAIPath = GetComponent<IAstarAI>();
                if (cacheAIPath == null)
                {
                    cacheAIPath = gameObject.AddComponent<AILerp>();
                    (cacheAIPath as AILerp).enableRotation = false;
                }
                return cacheAIPath;
            }
        }

        public override void EntityOnSetup()
        {
            base.EntityOnSetup();
            CacheNetTransform.onTeleport = (position, rotation) =>
            {
                CacheAIPath.Teleport(position);
            };
        }

        protected void Update()
        {
            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer)
            {
                (CacheAIPath as MonoBehaviour).enabled = false;
                return;
            }

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient)
            {
                (CacheAIPath as MonoBehaviour).enabled = false;
                return;
            }

            (CacheAIPath as MonoBehaviour).enabled = true;

            // Force set AILerp settings
            CacheAIPath.canMove = true;
            CacheAIPath.canSearch = true;
            CacheAIPath.maxSpeed = CacheEntity.GetMoveSpeed();
        }

        public override void EntityFixedUpdate()
        {
            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer)
                return;

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient)
                return;

            if (currentDestination.HasValue && CacheEntity.CanMove())
            {
                // Set destination to AI Path
                if (CacheAIPath.isStopped)
                    CacheAIPath.isStopped = false;
                CacheAIPath.destination = currentDestination.Value;
            }

            if (CacheAIPath.velocity.magnitude > 0)
                CacheEntity.SetDirection2D(CacheAIPath.velocity.normalized);

            CacheEntity.SetMovement(CacheAIPath.velocity.magnitude > 0 ? MovementState.Forward : MovementState.None);
        }

        public override void StopMove()
        {
            if (!CacheAIPath.isStopped)
                CacheAIPath.isStopped = true;
            base.StopMove();
        }
    }
}
