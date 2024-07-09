
using UnityEngine;

public class UpdateEnemyManager : EnemyManager {
    
    protected void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        prefab = Resources.Load<GameObject>("UpdateEnemy");
        instance = this;
    }

    private void OnDestroy() {
        instance = null;
    }

    private GameObject prefab = null;
    protected override GameObject GetEnemyPrefab() => prefab;
}