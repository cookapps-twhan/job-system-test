using UnityEngine;

public class Enemy : MonoBehaviour {
    protected Transform cachedTr;
    public Transform CachedTr => cachedTr;

    [SerializeField] protected float speed;
    // [SerializeField] protected SpriteRenderer spriteRenderer;
    public float Speed => speed;
    public int Index;
    
    protected virtual void Awake() {
        cachedTr = transform;
    }

    // private void LateUpdate() {
    //     var position = cachedTr.position;
    //     var insideView = GameManager.Instance.screenRect.Contains(position);
    //     if (spriteRenderer.enabled != insideView) {
    //         spriteRenderer.enabled = insideView;
    //     }
    // }
}

