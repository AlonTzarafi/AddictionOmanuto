using UnityEngine;
using UnityEngine.UI;

public class DopamineCatcherGame : MonoBehaviour
{
    public static DopamineCatcherGame Instance;

    private int score = 0;

    private float initialSpawnInterval = 0.24f;
    private float spawnInterval;
    private float minSpawnInterval = 0.08f;
    private float maxSpawnInterval = 1000000f;

    private float spawnIntervalIncreaseRate = 0.04f; // Rate at which spawnInterval increases when not collecting
    private float spawnIntervalDecreaseAmount = 0.04f; // Amount to decrease spawnInterval when collecting dopamine

    private float timeSinceLastSpawn = 0f;
    private float timeSinceLastCatch = 0f;

    private int maxDopamineOnScreen = 200;
    private int dopamineCount = 0;

    private int startingDopamine = 5;

    private Text scoreText;

    // Reference to player
    private GameObject player;

    // Reference to Restart Button
    private Button restartButton;

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<DopamineCatcherGame>();
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Font is at Assets/Resources/SuperPixel.ttf
        // var fontResourcePath = "SuperPixelaa";
        var font = Resources.Load<Font>("PixelGameFont");
        // var ttt = Resources.Load("fdsaglkds");


        // Initialize spawnInterval
        spawnInterval = initialSpawnInterval;

        // Set up camera
        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.backgroundColor = Color.cyan;
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5;

        // Create UI Canvas for score and restart button
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Create Score Text
        GameObject scoreTextObject = new GameObject("ScoreText");
        scoreTextObject.transform.SetParent(canvasObject.transform);
        scoreText = scoreTextObject.AddComponent<Text>();
        scoreText.font = font;
        scoreText.fontSize = 22;
        scoreText.alignment = TextAnchor.UpperLeft;
        scoreText.color = Color.black;
        RectTransform rectTransform = scoreText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        // Make width 200
        rectTransform.sizeDelta = new Vector2(200, 30);
        

        UpdateScoreText();

        // Create Restart Button
        GameObject restartButtonObject = new GameObject("RestartButton");
        restartButtonObject.transform.SetParent(canvasObject.transform);
        restartButton = restartButtonObject.AddComponent<Button>();
        Image buttonImage = restartButtonObject.AddComponent<Image>();
        buttonImage.color = Color.white; // Set button color

        // Set button position and size
        RectTransform buttonRect = restartButtonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 1);
        buttonRect.anchoredPosition = new Vector2(-10, -10);
        buttonRect.sizeDelta = new Vector2(100, 30);

        // Add Text to the button
        GameObject buttonTextObject = new GameObject("ButtonText");
        buttonTextObject.transform.SetParent(restartButtonObject.transform);
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.text = "Restart";
        buttonText.font = font;
        buttonText.fontSize = 22;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.black;

        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.sizeDelta = buttonRect.sizeDelta;
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;

        // Add OnClick listener to restart button
        restartButton.onClick.AddListener(RestartGame);

        // Create player
        CreatePlayer();

        // Start with 5 dopamine on screen
        for (int i = 0; i < startingDopamine; i++)
        {
            SpawnDopamine();
        }
    }

    void Update()
    {
        timeSinceLastCatch += Time.deltaTime;

        // Increase spawnInterval over time when not collecting dopamine
        spawnInterval = Mathf.Min(maxSpawnInterval, spawnInterval + spawnIntervalIncreaseRate * Time.deltaTime);
        // spawnInterval = Mathf.Min(maxSpawnInterval, spawnInterval + spawnIntervalIncreaseRate * timeSinceLastCatch);

        if (dopamineCount < maxDopamineOnScreen)
        {
            timeSinceLastSpawn += Time.deltaTime;

            if (timeSinceLastSpawn >= spawnInterval)
            {
                SpawnDopamine();
                timeSinceLastSpawn = 0f;
            }
        }
    }

    void SpawnDopamine()
    {
        GameObject dopamine = new GameObject("Dopamine");
        dopamine.tag = "Dopamine";
        SpriteRenderer dopamineRenderer = dopamine.AddComponent<SpriteRenderer>();
        dopamineRenderer.sprite = SpriteCreator.CreateSprite(Color.yellow);
        dopamineRenderer.sortingOrder = 0;

        float xPosition = Random.Range(-7f, 7f);
        float yPosition = Random.Range(-7f, 7f);
        dopamine.transform.position = new Vector3(xPosition, yPosition, 0);

        // Add Rigidbody2D to dopamine
        Rigidbody2D dopamineRb = dopamine.AddComponent<Rigidbody2D>();
        dopamineRb.gravityScale = 0;
        dopamineRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add CircleCollider2D to dopamine
        CircleCollider2D dopamineCollider = dopamine.AddComponent<CircleCollider2D>();
        dopamineCollider.isTrigger = true;

        // Add Dopamine script
        dopamine.AddComponent<Dopamine>();

        dopamineCount++;
    }

    public void DecreaseDopamineCount()
    {
        dopamineCount--;
    }

    public void IncreaseScore()
    {
        score++;
        UpdateScoreText();
        ResetCatchTimer();

        // Decrease spawnInterval when collecting dopamine
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecreaseAmount);

        CheckGameOver();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    public void ResetCatchTimer()
    {
        timeSinceLastCatch = 0f;
    }

    public void CheckGameOver()
    {
        if (score >= 50)
        {
            // Find the player and destroy it
            if (player != null)
            {
                Destroy(player);
                player = null;
                Debug.Log("Player collected 50 dopamine and has disappeared.");
            }
        }
    }

    public void RestartGame()
    {
        // Reset variables
        score = 0;
        UpdateScoreText();

        spawnInterval = initialSpawnInterval;

        timeSinceLastSpawn = 0f;
        timeSinceLastCatch = 0f;

        // Destroy all dopamine molecules
        GameObject[] dopamines = GameObject.FindGameObjectsWithTag("Dopamine");
        foreach (GameObject dopamine in dopamines)
        {
            Destroy(dopamine);
        }
        dopamineCount = 0;

        // Destroy player if it exists
        if (player != null)
        {
            Destroy(player);
        }

        // Recreate player
        CreatePlayer();

        // Start with startingDopamine dopamine molecules
        for (int i = 0; i < startingDopamine; i++)
        {
            SpawnDopamine();
        }
    }

    void CreatePlayer()
    {
        // Create player
        player = new GameObject("Player");
        SpriteRenderer playerRenderer = player.AddComponent<SpriteRenderer>();
        playerRenderer.sprite = SpriteCreator.CreateSprite(Color.blue);
        playerRenderer.sortingOrder = 1; // Ensure player is rendered above other objects
        player.transform.position = new Vector3(0, 0, 0);

        // Add Rigidbody2D to player
        Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0;
        playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add BoxCollider2D to player
        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();
        playerCollider.isTrigger = false;

        // Add player movement script
        player.AddComponent<PlayerController>();
    }
}

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float moveVertical = Input.GetAxis("Vertical"); // W/S or Up/Down arrows
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb.velocity = movement * speed;

        // Clamp player's position within screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, -7f, 7f);
        float clampedY = Mathf.Clamp(transform.position.y, -7f, 7f);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}

public class Dopamine : MonoBehaviour
{
    void Update()
    {
        // Destroy dopamine if it goes out of bounds
        if (transform.position.x < -8f || transform.position.x > 8f || transform.position.y < -8f || transform.position.y > 8f)
        {
            DestroySelf();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            // Increase score
            DopamineCatcherGame.Instance.IncreaseScore();
            DopamineCatcherGame.Instance.ResetCatchTimer();
            DopamineCatcherGame.Instance.CheckGameOver();
            // Destroy the dopamine molecule
            DestroySelf();
        }
    }

    void DestroySelf()
    {
        DopamineCatcherGame.Instance.DecreaseDopamineCount();
        Destroy(gameObject);
    }
}

public static class SpriteCreator
{
    public static Sprite CreateSprite(Color color)
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
}
