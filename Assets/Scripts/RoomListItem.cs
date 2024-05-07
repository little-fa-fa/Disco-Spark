using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public Button joinRoomButton;

    private string roomName;

    // Setup the room item with room details
    public void SetUp(RoomInfo roomInfo)
    {
        roomName = roomInfo.Name;
        roomNameText.text = roomName;
        playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        joinRoomButton.interactable = roomInfo.PlayerCount < roomInfo.MaxPlayers;
        joinRoomButton.onClick.RemoveAllListeners();
        joinRoomButton.onClick.AddListener(OnJoinRoom);
    }

    // Called when the join button is clicked
    public void OnJoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
    }
}