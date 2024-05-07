using UnityEngine;
using Photon.Pun;
using UnityEngine.Windows.Speech; // Import for Unity's built-in speech recognition
using System.Collections.Generic;
using System.Linq;

public class Player : MonoBehaviourPun, IPunObservable
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> actions = new Dictionary<string, System.Action>();

    void Start()
    {
        if (photonView.IsMine)
        {
            actions.Add("walk", Walk);
            actions.Add("jump", Jump);
            actions.Add("stop", Stop);

            // Initialize the keyword recognizer and provide the list of commands
            keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
            keywordRecognizer.Start();
        }
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        if (photonView.IsMine)
        {
            Debug.Log(speech.text);
            actions[speech.text].Invoke();
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // Process movement and jumping here if flags are used
    }

    void Walk()
    {
        if (photonView.IsMine)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
    }

    void Jump()
    {
        if (photonView.IsMine && Mathf.Abs(rb.velocity.y) < 0.001f)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    void Stop()
    {
        if (photonView.IsMine)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void OnDestroy()
    {
        if (photonView.IsMine && keywordRecognizer != null)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
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