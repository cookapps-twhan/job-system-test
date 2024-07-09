using System;
using Unity.Burst;
using UnityEngine;

public class UpdateEnemy : Enemy {
    private void Update() {
        var position = cachedTr.position;
        var playerPosition = GameManager.Instance.Player.CachedTr.position;
        var direction = playerPosition - position;
        direction = speed * Time.deltaTime * direction.normalized;
        position += direction;
        cachedTr.position = position;
        var distance = (playerPosition - position).sqrMagnitude;
        if (distance >= 0.25f)
            return;
        
        EnemyManager.Instance.ReturnToPool(this);
    }
}

