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

    //Disabled player when use pause menu
    private GameObject playerInstance;

    void Start()
    {
        PauseMenu.SetActive(false);

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
                playerController.enabled = !isPaused;
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



    // Optionally handle failed attempt to leave a room
}