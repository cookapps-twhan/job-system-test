
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class JobParallelForTransformEnemyManager : EnemyManager {
    
    protected void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
        prefab = Resources.Load<GameObject>("JobParallelForTransformEnemy");

        enemiesTr = new Transform[enemies.Length];
        transformArr = new TransformAccessArray(enemiesTr);
        speeds = new NativeArray<float>(enemies.Length, Allocator.Persistent);
        isCollisions = new NativeArray<bool>(enemies.Length, Allocator.Persistent);
        enemiesArraySizeChanged += OnEnemiesArraySizeChanged;
        enemiesModified += OnEnemiesModified;
    }

    private void OnEnemiesModified(bool isAdded, Enemy enemy) {
        if (isAdded) {
            enemiesTr[enemy.Index] = enemy.CachedTr;
            transformArr[enemy.Index] = enemy.CachedTr;
        } else {
            enemiesTr[enemy.Index] = null;
            transformArr[enemy.Index] = null;
        }

    }

    private void OnEnemiesArraySizeChanged() {
        jobHandle.Complete();
        jobScheduled = false;
        transformArr.Dispose();
        speeds.Dispose();
        isCollisions.Dispose();
        enemiesTr = new Transform[enemies.Length];
        transformArr = new TransformAccessArray(enemiesTr);
        speeds = new NativeArray<float>(enemies.Length, Allocator.Persistent);
        isCollisions = new NativeArray<bool>(enemies.Length, Allocator.Persistent);
    }

    private void OnDestroy() {
        instance = null;
        jobHandle.Complete();
        transformArr.Dispose();
        speeds.Dispose();
        isCollisions.Dispose();
        enemiesArraySizeChanged -= OnEnemiesArraySizeChanged;
    }

    private GameObject prefab = null;
    protected override GameObject GetEnemyPrefab() => prefab;
    
    private Transform[] enemiesTr;
    private TransformAccessArray transformArr;
    private NativeArray<float> speeds;
    private NativeArray<bool> isCollisions;
    private JobHandle jobHandle;
    private bool jobScheduled;
    private MoveAndCheckJob moveAndCheckJob;

    protected override void Update() {
        base.Update();
        MoveEnemies();
    }

    private void MoveEnemies() {
        for (var i = 0; i < enemies.Length; i++) {
            var isEmpty = ReferenceEquals(enemies[i], null);
            if (isEmpty)
                continue;
            speeds[i] = enemies[i].Speed;
        }
        moveAndCheckJob = new MoveAndCheckJob() {
            targetPosition = GameManager.Instance.Player.CachedTr.position,
            speeds = speeds,
            deltaTime = Time.deltaTime,
            isCollisions = isCollisions
        };

        jobHandle = moveAndCheckJob.Schedule(transformArr);
        jobScheduled = true;
    }

    private void LateUpdate() {
        if (!jobScheduled)
            return;
        jobHandle.Complete();
        for (var i = 0; i < enemies.Length; i++) {
            var hasEnemy = !ReferenceEquals(enemies[i], null);
            if (!hasEnemy)
                continue;
            var isCollision = moveAndCheckJob.isCollisions[i];
            if (!isCollision)
                continue;
        
            Instance.ReturnToPool(enemies[i]);
        }
    }

    [BurstCompile]
    private struct MoveAndCheckJob : IJobParallelForTransform {
        public float3 targetPosition;
        public NativeArray<float> speeds;
        public float deltaTime;
        public NativeArray<bool> isCollisions;

        void IJobParallelForTransform.Execute(int index, TransformAccess transform) {
            if (!transform.isValid)
                return;
            float3 position = (float3)transform.position;
            var direction = targetPosition - position;
            direction = math.normalize(direction);
            direction *= speeds[index] * deltaTime;
            position += direction;
            var distance = math.distancesq(targetPosition, position);
            isCollisions[index] = distance < 0.25f;
            transform.position = position;
        }
    }
}