using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private Joystick joystick;
    [SerializeField] private float speed;
    private Transform cachedTr;
    public Transform CachedTr => cachedTr;
    
    void Awake() {
        cachedTr = transform;
    }
    
    void FixedUpdate() {
        Vector3 direction = Vector3.up * joystick.Vertical + Vector3.right * joystick.Horizontal;
        cachedTr.Translate(speed * Time.fixedDeltaTime * direction);
    }
}
