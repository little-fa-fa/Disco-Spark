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
    public float maxJumpForce = 6f;  // Maximum jump force
    public float deceleration = 0.1f;

    private Recorder photonVoiceRecorder;
    private bool isMoving = false;
    private Vector3 initialPosition;

    private bool isChargingJump = false;
    private float jumpChargeStartTime;
    private float minJumpForce = 2f;

    //Test Volume
    public Text volumeText;

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
        }
 
        // Find the existing UI Text component for displaying volume
        volumeText = GameObject.Find("VolumeText").GetComponent<Text>();
        if (volumeText != null)
        {
            UnityEngine.Debug.Log("VolumeText UI component found");
        }
        else
        {
            UnityEngine.Debug.LogError("VolumeText UI component not found");
        }
    }

    void Update()
    {
        if (photonView.IsMine)  // Example: push-to-talk
        {
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

        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))  // Start charging on button press
        {
            isChargingJump = true;
            jumpChargeStartTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0) && isChargingJump)  // Release and jump
        {
            isChargingJump = false;
            Jump();  // Call the modified Jump method
        }
    }
    public void Kill()
    {
        if (photonView.IsMine)
        {
            Transform closestRespawnPoint = FindClosestRespawnPoint();
            if (closestRespawnPoint != null)
            {
                transform.position = closestRespawnPoint.position;
            }
            else
            {
                transform.position = initialPosition;
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
            float chargeDuration = Time.time - jumpChargeStartTime;
            float jumpForce = Mathf.Clamp(chargeDuration * maxJumpForce, minJumpForce, maxJumpForce);
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
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
}
