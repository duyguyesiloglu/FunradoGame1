using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private TimerController timerController;
    private CharacterController characterController;
    private EnemyController enemyManager;
    private Animator friendlyCharacter;
    private bool isPlayerMoving;
    private int keyCount = 0;
    private int collectableCount = 0;
    private int totalCollectables;
    private CameraController cameraController;

    public float playerSpeed = 0.5f;
    public float xSpeed = 2f;
    public float max = 4.6f;
    public int playerLevel = 1;
    public FloatingJoystick floatingJoystick;
    public TextMeshProUGUI keyCountText;
    public TextMeshProUGUI wellDoneText;
    public TextMeshProUGUI playerLevelText;
    public GameObject gameOverCanvas;
    public GameObject winPanel;
    public GameObject pauseCanvas;
    public Button muteButton;
    public Button unmuteButton;

    void Start()
    {
        timerController = FindObjectOfType<TimerController>();
        characterController = GetComponent<CharacterController>();
        enemyManager = FindObjectOfType<EnemyController>();
        friendlyCharacter = GetComponent<Animator>();
        cameraController = FindObjectOfType<CameraController>();

        totalCollectables = GameObject.FindGameObjectsWithTag("Collectable").Length;

        timerController.StartTimer();
        isPlayerMoving = true;

        UpdateKeyCountUI();
        UpdatePlayerLevelUI();
        wellDoneText.gameObject.SetActive(false);
        gameOverCanvas.SetActive(false);
        winPanel.SetActive(false);
        pauseCanvas.SetActive(false);

        muteButton.onClick.AddListener(MuteAudio);
        unmuteButton.onClick.AddListener(UnmuteAudio);

        unmuteButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isPlayerMoving) return;

        float touchX = floatingJoystick.Horizontal;
        float touchY = floatingJoystick.Vertical;
        float newXValue = transform.position.x + xSpeed * touchX * Time.deltaTime;
        newXValue = Mathf.Clamp(newXValue, -max, max);

        Vector3 move = new Vector3(newXValue - transform.position.x, 0, touchY * playerSpeed * Time.deltaTime);
        characterController.Move(move);

        UpdateAnimation(touchX, touchY);
    }

    private void UpdateAnimation(float touchX, float touchY)
    {
        if (friendlyCharacter.GetBool("isPlayerDead")) return;

        if (Mathf.Abs(touchX) < 0.1f && Mathf.Abs(touchY) < 0.1f)
        {
            friendlyCharacter.SetBool("isRunning", false);
        }
        else
        {
            friendlyCharacter.SetBool("isRunning", true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            HandleFinishTrigger();
        }
        else if (other.CompareTag("Enemy"))
        {
            HandleEnemyTrigger(other);
        }
        else if (other.CompareTag("BlueKey") || other.CompareTag("RedKey"))
        {
            CollectKey(other.gameObject);
        }
        else if (other.CompareTag("BlueGate") || other.CompareTag("RedGate"))
        {
            TryOpenGate(other.gameObject);
        }
        else if (other.CompareTag("Collectable"))
        {
            CollectCollectable(other.gameObject);
        }
    }

    private void HandleFinishTrigger()
    {
        if (enemyManager.AreAllEnemiesDead() && collectableCount == totalCollectables)
        {
            isPlayerMoving = false;
        }
    }

    private void HandleEnemyTrigger(Collider enemyCollider)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (enemy.level > playerLevel)
            {
                StartDeathSequence();
            }
            else
            {
                StartCoroutine(DefeatEnemySequence(enemy));
            }
        }
    }

    private IEnumerator DefeatEnemySequence(Enemy enemy)
    {
        isPlayerMoving = false;
        friendlyCharacter.SetTrigger("Punch");
        cameraController.CloseUp(enemy.transform.position);

        yield return new WaitForSeconds(2.0f);

        Destroy(enemy.gameObject);
        cameraController.ResetCamera();
        isPlayerMoving = true;
    }

    private void DestroyEnemy(Enemy enemy)
    {
        enemy.PlayDeathAnimation();
        StartCoroutine(DestroyAfterAnimation(enemy));
        enemyManager.EnemyDefeated(enemy);
        StartCoroutine(ShowWellDoneMessage());
    }

    private IEnumerator DestroyAfterAnimation(Enemy enemy)
    {
        yield return new WaitForSeconds(1.0f);
        Destroy(enemy.gameObject);
    }

    private void CollectKey(GameObject key)
    {
        keyCount++;
        UpdateKeyCountUI();
        Destroy(key);
    }

    private void TryOpenGate(GameObject gate)
    {
        if (gate.CompareTag("RedGate") && collectableCount > 0)
        {
            ShowWinPanel();
        }
        else
        {
            if (keyCount > 0)
            {
                keyCount--;
                OpenGate(gate);
            }

            UpdateKeyCountUI();
        }
    }

    private void OpenGate(GameObject gate)
    {
        gate.GetComponent<Collider>().enabled = false;
        StartCoroutine(OscillateGate(gate));
    }

    private IEnumerator OscillateGate(GameObject gate)
    {
        HingeJoint hinge = gate.GetComponent<HingeJoint>();
        if (hinge == null) yield break;

        JointSpring spring = hinge.spring;
        spring.spring = 10;
        spring.damper = 5;
        spring.targetPosition = 90;
        hinge.spring = spring;
        hinge.useSpring = true;

        yield return new WaitForSeconds(2.0f);

        spring.targetPosition = -90;
        hinge.spring = spring;
        hinge.useSpring = true;

        yield return new WaitForSeconds(2.0f);

        hinge.useSpring = false;
    }

    private void CollectCollectable(GameObject collectable)
    {
        Destroy(collectable);
        collectableCount++;
        playerLevel++;
        UpdatePlayerLevelUI();
    }

    private void UpdateKeyCountUI()
    {
        keyCountText.text = "Keys: " + keyCount;
    }

    private void UpdatePlayerLevelUI()
    {
        playerLevelText.text = "Level: " + playerLevel;
    }

    private void StartDeathSequence()
    {
        isPlayerMoving = false;
        friendlyCharacter.SetTrigger("Punch");
        friendlyCharacter.SetBool("isPlayerDead", true);
        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        yield return new WaitForSeconds(friendlyCharacter.GetCurrentAnimatorStateInfo(0).length);
        gameOverCanvas.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator ShowWellDoneMessage()
    {
        wellDoneText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        wellDoneText.gameObject.SetActive(false);
    }

    private void ShowWinPanel()
    {
        winPanel.SetActive(true);
        isPlayerMoving = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MuteAudio()
    {
        AudioListener.volume = 0;
        muteButton.gameObject.SetActive(false);
        unmuteButton.gameObject.SetActive(true);
    }

    public void UnmuteAudio()
    {
        AudioListener.volume = 1;
        muteButton.gameObject.SetActive(true);
        unmuteButton.gameObject.SetActive(false);
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("Level2");
    }
}
