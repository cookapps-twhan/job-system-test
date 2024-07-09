
using UnityEngine;

public class JobEnemyManager : EnemyManager {
    protected void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
        prefab = Resources.Load<GameObject>("JobEnemy");
    }

    private void OnDestroy() {
        instance = null;
    }

    private GameObject prefab = null;
    protected override GameObject GetEnemyPrefab() => prefab;
}
