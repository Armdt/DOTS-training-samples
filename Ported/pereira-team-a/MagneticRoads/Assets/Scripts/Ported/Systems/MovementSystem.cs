﻿using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MovementSystem : JobComponentSystem
{
    private EntityQuery query;
    protected override void OnCreate()
    {
        query = GetEntityQuery(new EntityQueryDesc
        {
            All = new []{ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Rotation>(), ComponentType.ReadOnly<SplineData>()},
            None = new []{ComponentType.ReadOnly<FindTarget>() }
        });
    }

    struct InfoForMoveAndRotation
    {
        public float3 splinePoint;
        public Quaternion rotation;
    }

    [BurstCompile]
    struct MoveJob : IJobForEach<Translation, Rotation, SplineData>
    {
        public float deltaTime;
        public void Execute(ref Translation translation,ref Rotation rotation, ref SplineData trackSpline)
        {
            translation.Value += math.normalize(trackSpline.TargetPosition - translation.Value) * deltaTime * 2f;

            //float dist = math.distance(trackSpline.TargetPosition, trackSpline.StartPosition);
            //float trail = math.distance(trackSpline.TargetPosition, translation.Value);

            //var moveDisplacement = (deltaTime * 2f) / dist;
            //var t = Mathf.Clamp01(trail / dist + moveDisplacement);
            //float3 up;
            //float3 point = float3.zero;
            //float3 splinePoint = Extrude(point, trackSpline, t, out up);

            //up *= splineSide;

            //translation.Value = splinePoint + math.normalize(up) * .06f;

            //float3 moveDir = trackSpline.TargetPosition - trackSpline.StartPosition;
            //float splineDirection = 1;
            //lastPosition = position;
            //if (moveDir.sqrMagnitude > 0.0001f && up.sqrMagnitude > 0.0001f)
            //{
            //    rotation.Value = quaternion.LookRotation(moveDir * splineDirection, up);
            //}
        }
    }

    public static float3 Extrude(float3 point,SplineData info, float t, out float3 up)// Vector2 point, float t, out Vector3 tangent, out Vector3 up)
    {
        float3 sample1 = Evaluate(t, info);
        float3 sample2;

        float flipper = 1f;
        if (t + .01f < 1f)
        {
            sample2 = Evaluate(t + .01f, info);
        }
        else
        {
            sample2 = Evaluate(t - .01f, info);
            flipper = -1f;
        }

        var tangent = math.normalize(sample2 - sample1) * flipper;
        tangent = math.normalize(tangent);

        // each spline uses one out of three possible twisting methods:
        quaternion fromTo = quaternion.identity;// Quaternion.identity;
        //if (twistMode == 0)
        //{
        //    // method 1 - rotate startNormal around our current tangent
        //    float angle = Vector3.SignedAngle(startNormal, endNormal, tangent);
        //    fromTo = Quaternion.AngleAxis(angle, tangent);
        //}
        //else if (twistMode == 1)
        //{
            // method 2 - rotate startNormal toward endNormal
            fromTo = Quaternion.FromToRotation(info.spline.StartNormal,info.spline.EndNormal);
        //}
        //else if (twistMode == 2)
        //{
        //    // method 3 - rotate startNormal by "startOrientation-to-endOrientation" rotation
        //    Quaternion startRotation = Quaternion.LookRotation(startTangent, startNormal);
        //    Quaternion endRotation = Quaternion.LookRotation(endTangent * -1, endNormal);
        //    fromTo = endRotation * Quaternion.Inverse(startRotation);
        //}
        // other twisting methods can be added, but they need to
        // respect the relationship between startNormal and endNormal.
        // for example: if startNormal and endNormal are equal, the road
        // can twist 0 or 360 degrees, but NOT 180.

        float smoothT = Mathf.SmoothStep(0f, 1f, t * 1.02f - .01f);
        up = math.mul(math.slerp(quaternion.identity, fromTo, smoothT),info.spline.StartNormal);
        //up = Quaternion.Slerp(quaternion.identity, fromTo, smoothT) * info.spline.StartNormal;
        float3 right = math.cross(tangent, up);

        return sample1 + right * point.x + up * point.y;
    }

    public static float3 Evaluate(float t, SplineData spline)
    {
        t = Mathf.Clamp01(t);
        return spline.StartPosition * (1f - t) * (1f - t) * (1f - t) + 3f * spline.spline.Anchor1 * (1f - t) * (1f - t) * t + 3f * spline.spline.Anchor2 * (1f - t) * t * t + spline.TargetPosition * t * t * t;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //1. get the direction
        //2. move to the Position
        var job = new MoveJob
        {
            deltaTime = Time.deltaTime
        };
        return job.Schedule(query, inputDeps);
    }
}
