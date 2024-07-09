using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public abstract class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance => instance;
    protected static EnemyManager instance;
    
    protected Enemy[] enemies = new Enemy[16384];
    protected event Action<bool, Enemy> enemiesModified;
    protected event Action enemiesArraySizeChanged;
    private int enemyIndex = 0;
    private Queue<Enemy> enemyPool = new ();
    [SerializeField] private Transform pool;
    [SerializeField] private TMP_Text enemyCount;
    [SerializeField] private int emitter = 2;
    [SerializeField] private Transform enemyNode;
    [SerializeField] private Slider slider;
    private IntReactiveProperty countRxProp;
    private int targetCount;

    protected abstract GameObject GetEnemyPrefab();
    
    protected virtual void Start() {
        countRxProp = new IntReactiveProperty(0);
        countRxProp.SubscribeWithState(enemyCount, (count, text) => text.text = $"{count}/{targetCount}");
        targetCount = 10000;
        slider.value = targetCount * 0.01f;
        slider.onValueChanged.AsObservable().ThrottleFrame(5).Subscribe(targetCount => {
            this.targetCount = (int)targetCount * 100;
            if (this.targetCount > enemies.Length) {
                var currLength = enemies.Length;
                while (currLength < targetCount) {
                    currLength *= 2;
                }

                Array.Resize(ref enemies, currLength);
                enemiesArraySizeChanged?.Invoke();
            }
        });
    }

    protected virtual void Update() {
        for (int i = 0; i < emitter; i++) {
            if (countRxProp.Value >= targetCount)
                break;

            CreateEnemy();
        }
    }
    
    private void CreateEnemy() {
        if (!enemyPool.TryDequeue(out var enemy)) {
            var enemyGo = Instantiate(GetEnemyPrefab());
            enemy = enemyGo.GetComponent<Enemy>();
        }
        enemy.CachedTr.SetParent(enemyNode);
        enemy.CachedTr.position = GetInitPosition();
        countRxProp.Value++;
        while (true) {
            enemyIndex++;
            if (enemyIndex >= 16384) {
                enemyIndex = 0;
            }
            if (ReferenceEquals(enemies[enemyIndex], null)) {
                enemy.Index = enemyIndex;
                enemies[enemyIndex] = enemy;
                enemiesModified?.Invoke(true, enemy);
                break;
            }
        }
    }
    
    public void ReturnToPool(Enemy enemy) {
        if (targetCount < countRxProp.Value) { // targetCount 보다 많으면 풀에 넣고
            enemy.CachedTr.SetParent(pool);
            enemyPool.Enqueue(enemy);
            enemies[enemy.Index] = null;
            enemiesModified?.Invoke(false, enemy);
            countRxProp.Value--;
        } else { // 아니면 초기위치로 보내서 다시 움직이도록
            enemy.CachedTr.position = GetInitPosition();
        }
    }
    
    private static Vector3 GetInitPosition() {
        var initPosition = Vector3.left * Random.Range(10f, 12f);
        initPosition = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * initPosition;
        return initPosition;
    }
}
