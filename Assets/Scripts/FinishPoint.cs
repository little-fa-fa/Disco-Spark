using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviourPunCallbacks
{
    private bool locked = true;

    public bool isPass = false;

    public List<string> level = new List<string>();

    // This is the range within which players need to be to trigger the next level
    public float detectionRange = 5f;

    public GameObject PassMenu;
    // Update is called once per frame
    void Update()
    {
        if (!locked)
        {
            CheckPlayersInRange();
        }
    }

    public void Unlock()
    {
        locked = false;
    }

    private void CheckPlayersInRange()
    {
        // Get all players in the room
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        bool allPlayersInRange = true;

        foreach (Photon.Realtime.Player player in players)
        {
            GameObject playerObject = GetPlayerObject(player);
            if (playerObject != null)
            {
                float distance = Vector3.Distance(transform.position, playerObject.transform.position);
                if (distance > detectionRange)
                {
                    allPlayersInRange = false;
                    break;
                }
            }
        }

        if (allPlayersInRange)
        {
            isPass = true;
            //GoToNextLevel();
            PassMenu.SetActive(true);
        }
    }

    private GameObject GetPlayerObject(Photon.Realtime.Player player)
    {
        // Find the player object in the scene
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in playerObjects)
        {
            if (obj.GetComponent<PhotonView>().Owner == player)
            {
                return obj;
            }
        }
        return null;
    }

    public void GoToNextLevel()
    {
        // Logic to load the next level
        if (PhotonNetwork.IsMasterClient)
        {
            PassMenu.SetActive(false);
            PhotonNetwork.LoadLevel(level[Mathf.Min(level.IndexOf(SceneManager.GetActiveScene().name) + 1, level.Count - 1)]); // Replace "NextLevel" with the actual next level name
        }
    }

    public bool IsPass()
    {
        return isPass;
    }
}
