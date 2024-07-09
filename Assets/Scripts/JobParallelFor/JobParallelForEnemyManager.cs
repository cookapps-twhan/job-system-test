
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class JobParallelForEnemyManager : EnemyManager {
    
    protected void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
        prefab = Resources.Load<GameObject>("JobParallelForEnemy");
        
        hasObjects = new NativeArray<bool>(enemies.Length, Allocator.Persistent);
        positions = new NativeArray<float3>(enemies.Length, Allocator.Persistent);
        speeds = new NativeArray<float>(enemies.Length, Allocator.Persistent);
        isCollisions = new NativeArray<bool>(enemies.Length, Allocator.Persistent);
        
        enemiesArraySizeChanged += OnEnemiesArraySizeChanged;
    }

    private void OnEnemiesArraySizeChanged() {
        hasObjects.Dispose();
        positions.Dispose();
        speeds.Dispose();
        isCollisions.Dispose();
        hasObjects = new NativeArray<bool>(enemies.Length, Allocator.Persistent);
        positions = new NativeArray<float3>(enemies.Length, Allocator.Persistent);
        speeds = new NativeArray<float>(enemies.Length, Allocator.Persistent);
        isCollisions = new NativeArray<bool>(enemies.Length, Allocator.Persistent);
    }

    private void OnDestroy() {
        instance = null;
        hasObjects.Dispose();
        positions.Dispose();
        speeds.Dispose();
        isCollisions.Dispose();
        enemiesArraySizeChanged -= OnEnemiesArraySizeChanged;
    }

    private GameObject prefab = null;
    protected override GameObject GetEnemyPrefab() => prefab;

    private NativeArray<bool> hasObjects;
    private NativeArray<float3> positions;
    private NativeArray<float> speeds;
    private NativeArray<bool> isCollisions;
    private MoveAndCheckJob moveAndCheckJob;
    private JobHandle jobHandle;
    private bool jobScheduled;

    protected override void Update() {
        base.Update();
        MoveEnemies();
    }
    
    private void MoveEnemies() {
        for (var i = 0; i < enemies.Length; i++) {
            hasObjects[i] = !ReferenceEquals(enemies[i], null);
            if (!hasObjects[i])
                continue;
            positions[i] = enemies[i].CachedTr.position;
            speeds[i] = enemies[i].Speed;
        }
        moveAndCheckJob = new MoveAndCheckJob(
            hasObjects,
            positions,
            GameManager.Instance.Player.CachedTr.position,
            speeds,
            Time.deltaTime,
            isCollisions);

        jobHandle = moveAndCheckJob.Schedule(enemies.Length, 64);
    }

    private void LateUpdate() {
        jobHandle.Complete();
        for (var i = 0; i < enemies.Length; i++) {
            var hasEnemy = !ReferenceEquals(enemies[i], null);
            if (!hasEnemy)
                continue;
            enemies[i].CachedTr.position = moveAndCheckJob.positions[i];
            var isCollision = moveAndCheckJob.isCollisions[i];
            if (!isCollision)
                continue;
        
            Instance.ReturnToPool(enemies[i]);
        }
    }

    [BurstCompile]
    private struct MoveAndCheckJob : IJobParallelFor {
        [ReadOnly] public NativeArray<bool> hasObjects;
        public NativeArray<float3> positions;
        private float3 targetPosition;
        [ReadOnly] private NativeArray<float> speeds;
        private float deltaTime;
        public NativeArray<bool> isCollisions;

        public MoveAndCheckJob(NativeArray<bool> hasObjects, NativeArray<float3> positions, Vector3 targetPosition, NativeArray<float> speeds, float deltaTime, NativeArray<bool> isCollisions) {
            this.hasObjects = hasObjects;
            this.positions = positions;
            this.targetPosition = targetPosition;
            this.speeds = speeds;
            this.deltaTime = deltaTime;
            this.isCollisions = isCollisions;
        }

        void IJobParallelFor.Execute(int index) {
            if (!hasObjects[index])
                return;
            var direction = targetPosition - positions[index];
            direction = math.normalize(direction);
            direction *= speeds[index] * deltaTime;
            positions[index] += direction;
            var distance = math.distancesq(targetPosition, positions[index]);
            isCollisions[index] = distance < 0.25f;
        }
    }
}