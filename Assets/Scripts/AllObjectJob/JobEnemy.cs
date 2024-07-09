using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

public class JobEnemy : Enemy {
    protected override void Awake() {
        base.Awake();
        position = new NativeArray<float3>(1, Allocator.Persistent);
        isCollision = new NativeArray<bool>(1, Allocator.Persistent);
    }

    private JobHandle jobHandle;
    private MoveAndCheckJob moveAndCheckJob;
    private bool isUpdated;
    NativeArray<float3> position;
    NativeArray<bool> isCollision;

    private void OnDestroy() {
        position.Dispose();
        isCollision.Dispose();
    }

    private void Update() {
        isUpdated = true;
        position[0] = cachedTr.position;
        moveAndCheckJob = new MoveAndCheckJob(
            position,
            GameManager.Instance.Player.CachedTr.position,
            speed,
            Time.fixedDeltaTime,
            isCollision);
        
        jobHandle = moveAndCheckJob.Schedule();
    }

    private void LateUpdate() {
        if (!isUpdated)
            return;
    
        jobHandle.Complete();
        cachedTr.position = position[0];
        if (!isCollision[0]) {
            return;
        }
        
        EnemyManager.Instance.ReturnToPool(this);
    }

    [BurstCompile]
    private struct MoveAndCheckJob : IJob {
        private NativeArray<float3> position;
        private float3 targetPosition;
        private float speed;
        private float deltaTime;
        private NativeArray<bool> isCollision;

        public MoveAndCheckJob(NativeArray<float3> position, Vector3 targetPosition, float speed, float deltaTime, NativeArray<bool> isCollision) {
            this.position = position;
            this.targetPosition = targetPosition;
            this.speed = speed;
            this.deltaTime = deltaTime;
            this.isCollision = isCollision;
        }
        
        void IJob.Execute() {
            var direction = targetPosition - position[0];
            direction = math.normalize(direction);
            direction *= speed * deltaTime;
            position[0] += direction;
            var distance = math.distancesq(targetPosition, position[0]);
            isCollision[0] = distance < 0.25f;
        }
    }
}

