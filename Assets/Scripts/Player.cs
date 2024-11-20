using UnityEngine;
using Photon.Pun;
using UnityEngine.Windows.Speech; // Import for Unity's built-in speech recognition
using System.Collections.Generic;
using System.Linq;
using Photon.Voice.Unity;
using UnityEngine.UI;

public class Player : MonoBehaviourPun, IPunObservable
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float maxJumpForce = 3f; 
    public float chargeRate = 5f;   
    private bool isChargingJump = false; 
    private float currentCharge = 0f; 

    public Image chargeBar;  
    public float deceleration = 0.1f;

    private Recorder photonVoiceRecorder;
    private bool isMoving = false;
    private Vector3 initialPosition;

    private float jumpChargeStartTime;
    private float minJumpForce = 2f;

    //Test Volume
    public Text volumeText;
    public Text latencyText;

    public float gravityScale = 1f;         
    public float ascentGravityScale = 0.8f; 
    public float descentGravityScale = 1.5f;

    public List<Prop> props;
    void Start()
    {
        if (photonView.IsMine)
        {
            GameObject photonVoiceNetwork = GameObject.Find("PhotonVoiceNetwork");
            if (photonVoiceNetwork != null)
            {
                photonVoiceRecorder = photonVoiceNetwork.GetComponentInChildren<Recorder>();
                if (photonVoiceRecorder != null)
                {
                    photonVoiceRecorder.TransmitEnabled = true;
                }
                else
                {
                    Debug.LogError("Recorder component not found in PhotonVoiceNetwork");
                }
            }
            else
            {
                Debug.LogError("PhotonVoiceNetwork GameObject not found");
            }
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 1, -10);

            initialPosition = transform.position;

            Collider2D playerCollider = GetComponent<Collider2D>();

            // Ignore collisions with other players
            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                if (player != this) // Ignore self
                {
                    Physics2D.IgnoreCollision(playerCollider, player.GetComponent<Collider2D>());
                }
            }

        }
 
        // Find the existing UI Text component for displaying volume
        volumeText = GameObject.Find("VolumeText").GetComponent<Text>();
        latencyText = GameObject.Find("LatencyText").GetComponent<Text>();
        if (volumeText != null)
        {
            UnityEngine.Debug.Log("VolumeText UI component found");
        }
        else
        {
            UnityEngine.Debug.LogError("VolumeText UI component not found");
        }

        if (latencyText != null)
        {
            UnityEngine.Debug.Log("latencyText UI component found");
        }
        else
        {
            UnityEngine.Debug.LogError("latencyText UI component not found");
        }
    }

    void Update()
    {
        if (photonView.IsMine)  // Example: push-to-talk
        {
            latencyText.text  = $"Ping: {PhotonNetwork.GetPing()} ms";
            if (isMoving)
            {
                float currentVolume = GetCurrentMicrophoneVolume();
                int db = (int)(20 * Mathf.Log10(currentVolume)+40);
                rb.velocity = new Vector2(currentVolume, rb.velocity.y);
                
                volumeText.text = "Volume: " + (db).ToString(); // Update the volume text

            }
            else if (rb.velocity.x != 0)
            {
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, deceleration * Time.deltaTime), rb.velocity.y);
            }

            if (Microphone.IsRecording(null) && !isMoving)
            {
                isMoving = true;
            }
            else if (!Microphone.IsRecording(null) && isMoving)
            {
                isMoving = false;
            }
            HandleMouseInput();
            if (chargeBar != null)
            {
                
                chargeBar.fillAmount = currentCharge / maxJumpForce;
            }
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // 左键开始蓄力
        {
            isChargingJump = true;
            currentCharge = 0f; // 重置蓄力值
        }

        if (Input.GetMouseButtonUp(0) && isChargingJump) // 左键释放跳跃
        {
            Jump();
            
        }

        if (Input.GetMouseButtonDown(1) && isChargingJump) // 右键取消蓄力
        {
            isChargingJump = false;
            currentCharge = 0f; // 重置蓄力值
        }

        if (isChargingJump)
        {
            currentCharge += chargeRate * Time.deltaTime;
            currentCharge = Mathf.Clamp(currentCharge, 0f, maxJumpForce); // 限制最大值
        }
    }
    public void Kill()
    {
        if (photonView.IsMine)
        {
            Vector3 respawnPosition = GameManager.sharedSavePoint;

            if (respawnPosition != Vector3.zero)
            {
                transform.position = respawnPosition;
            }
            else
            {
                transform.position = initialPosition;
            }
            for (int i = props.Count - 1; i >= 0; i--) 
            {
                props[i].Drop();
                props.RemoveAt(i);
            }
        }
    }

    private Transform FindClosestRespawnPoint()
    {
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        Transform closestRespawnPoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject respawnPoint in respawnPoints)
        {
            float distance = Vector3.Distance(transform.position, respawnPoint.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRespawnPoint = respawnPoint.transform;
            }
        }

        return closestRespawnPoint;
    }
    private float GetCurrentMicrophoneVolume()
    {
        return photonVoiceRecorder.LevelMeter.CurrentAvgAmp * 70;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (rb.velocity.y > 0.1f) 
        {
            rb.gravityScale = ascentGravityScale;
        }
        else if (rb.velocity.y < -0.1f)
        {
            rb.gravityScale = descentGravityScale;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }

        // Process movement and jumping here if flags are used
    }

    //void Walk()
    //{
    //    if (photonView.IsMine)
    //    {
    //        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    //    }
    //}

    void Jump()
    {

        if (photonView.IsMine && Mathf.Abs(rb.velocity.y) < 0.001f)
        {
            rb.AddForce(new Vector2(0, currentCharge), ForceMode2D.Impulse);
            isChargingJump = false;
            currentCharge = 0f;
        }
    }

    public void Stop()
    {
        if (photonView.IsMine)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(rb.position);
            stream.SendNext(rb.velocity);
        }
        else
        {
            // Network player, receive data
            rb.position = (Vector2)stream.ReceiveNext();
            rb.velocity = (Vector2)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void ForceLeaveRoom()
    {
        Debug.Log("Forced to leave room");
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
        
    }
}
