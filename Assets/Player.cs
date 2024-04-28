using UnityEngine;
using UnityEngine.Windows.Speech; // Import for Unity's built-in speech recognition
using System.Collections.Generic;
using System.Linq;

public class VoiceControlledPlayer2D : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> actions = new Dictionary<string, System.Action>();

    void Start()
    {
        actions.Add("walk", Walk);
        actions.Add("jump", Jump);
        actions.Add("stop", Stop);

        // Initialize the keyword recognizer and provide the list of commands
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    void FixedUpdate()
    {
        // Process movement and jumping here if flags are used
    }

    void Walk()
    {
        // Move the player horizontally
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        // Add a vertical force for the jump if the player is not already in the air
        if (Mathf.Abs(rb.velocity.y) < 0.001f)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    void Stop()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}