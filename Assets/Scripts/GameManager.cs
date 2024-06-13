using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Voice;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject spawnPoint;
    public VoiceFollowClient voiceClient;

    //find pauseMenu
    public GameObject PauseMenu;
    public bool isPaused = false;
    private bool isPass = false;

    //find passMenu
    public GameObject PassMenu;

    //Disabled player when use pause menu
    private GameObject playerInstance;
    // private bool reSpwan = false;

    void Start()
    {
        PauseMenu.SetActive(false);
        PassMenu.SetActive(false);

        if (PhotonNetwork.IsConnectedAndReady)
        {
            playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.transform.position, Quaternion.identity);
            voiceClient = FindAnyObjectByType<VoiceFollowClient>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenClosePauseMenu();
        }

        if (playerInstance != null)
        {
            var playerController = playerInstance.GetComponent<Player>();
            if (playerController != null)
            {
                FinishPoint finishPoint = GameObject.FindObjectOfType<FinishPoint>();
                isPass = finishPoint.IsPass();
                Debug.Log("isPause" + isPaused);
                Debug.Log("isPass" + isPass);
                if (isPass)
                {
                    playerController.enabled = false;
                }
                else
                {
                    playerController.enabled = true;
                    if (isPaused)
                    {
                        playerController.enabled = false;
                    }
                    else
                    {
                        playerController.enabled = true;
                    }
                }
   
                if (!playerController.enabled)
                {
                    playerController.Stop();
                }
            }
        }
    }

    // Call this method when the player chooses to leave the room
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
        voiceClient.Disconnect();

    }

    // Photon callback triggered after leaving the room
    public override void OnLeftRoom()
    {
        // Load the lobby or main menu scene
        SceneManager.LoadScene("Lobby");
        CleanUpResources();
    }

    private void CleanUpResources()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Example: Optionally destroy all networked objects if you're the room's host
            PhotonNetwork.DestroyAll();

        }
        else
        {
            // Handle client-specific clean-up, if any
        }

        // Additional clean-up logic here
        Debug.Log("Clean-up of network resources complete.");
    }

    public void OpenClosePauseMenu()
    {
        isPaused = !isPaused;
        PauseMenu.SetActive(isPaused);
    }

    public void UnStuck()
    {
        OpenClosePauseMenu();
        Player player = playerInstance.GetComponent<Player>();
        if (player != null && player.photonView.IsMine)
        {
            player.Kill();
        }
    }

    public void SpawnPlayer()
    {
        if (playerInstance == null)
        {
            if (playerInstance != null)
            {
                PhotonNetwork.Destroy(playerInstance);
                playerInstance = null;
            }
            CleanUpResources(); // Clean up resources here
            playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.transform.position, Quaternion.identity);
            voiceClient = FindAnyObjectByType<VoiceFollowClient>();
        }
        else
        {
            
            // Move the player back to the spawn point
            playerInstance.transform.position = spawnPoint.transform.position;
        }
    }

    public void TestSpawnPlayer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (!PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinOrCreateRoom("TestRoom", new Photon.Realtime.RoomOptions { MaxPlayers = 2 }, null);
        }
        else
        {
            SpawnPlayer();
        }
    }

}