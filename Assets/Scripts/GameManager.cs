using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Screen = UnityEngine.Device.Screen;

public class GameManager : MonoBehaviour {
    public static GameManager Instance => instance;
    private static GameManager instance;
    
    void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(instance);
        var y = Camera.main.orthographicSize;
        var x = y * Screen.width / Screen.height;
        screenRect = Rect.MinMaxRect(-x, -y, x, y);
        Application.targetFrameRate = 120;
        SceneManager.sceneLoaded += (scene, mode) => {
            var rootGOs = scene.GetRootGameObjects();
            foreach (var rootGO in rootGOs) {
                if (rootGO.name == "Player")
                    player = rootGO.GetComponent<Player>();
                var frameText = rootGO.transform.Find("Frame");
                if (frameText != null)
                    this.frameText = frameText.GetComponent<TMP_Text>();
            }
        };
    }

    [SerializeField] private Player player;
    public Player Player => player;
    [SerializeField] private TMP_Text frameText;

    public Rect screenRect;

    private float frameUpdateDuration = 0f;
    private float deltaTime;
    private int frameCount;
    void Update() {
        if (ReferenceEquals(frameText, null))
            return;

        deltaTime += Time.deltaTime;
        frameCount++;
        frameUpdateDuration -= Time.deltaTime;
        if (frameUpdateDuration > 0f)
            return;
        frameUpdateDuration = 0.5f;
        frameText.text = $"{frameCount / deltaTime:0.0}";
        frameCount = 0;
        deltaTime = 0;
    }
}