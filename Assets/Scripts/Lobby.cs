using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Lobby : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomNameInputField;
    public GameObject roomListContent;
    public GameObject roomListItemPrefab;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Photon Master Server and joined the lobby.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected from Photon Services. Reason: {cause}");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined lobby.");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                GameObject listItem = Instantiate(roomListItemPrefab, roomListContent.transform);
                listItem.GetComponent<RoomListItem>().SetUp(room);
            }
        }
    }

    public void OnCreateRoomButton()
    {
        string roomName = roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Room name cannot be empty.");
            return;
        }
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}");
    }

    public void OnJoinRoomButton(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room successfully, now loading play scene.");
        PhotonNetwork.LoadLevel("SampleScene");
    }
}