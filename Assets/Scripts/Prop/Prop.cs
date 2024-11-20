using System.Collections;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public float pickUpRange = 2f;
    public float shrinkSpeed = 2f;
    public float followSpeed = 5f;
    public Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 originalScale;
    public Vector3 leftOffset = new Vector3(-1f, 0f, 0f);

    protected bool isPickedUp = false;
    protected Transform playerTransform;

    protected Vector3 initialPosition;
    protected GameManager gameManager;
    protected virtual void Start()
    {
        originalScale = transform.localScale;
        initialPosition = transform.position;
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Game Manager not found for props");
        }
    }

    protected virtual void Update()
    {
        if (!isPickedUp)
        {
            CheckForPickUp();
        }
        else
        {
            FollowPlayer();
        }
    }

    protected virtual void CheckForPickUp()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickUpRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                playerTransform = collider.transform;
                Player player = collider.GetComponent<Player>();
                player.props.Add(this);
                isPickedUp = true;
                StartCoroutine(Shrink());
                OnPickUp();
                break;
            }
        }
    }

    

    protected virtual IEnumerator Shrink()
    {
        while (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * shrinkSpeed);
            yield return null;
        }
    }

    protected virtual IEnumerator Shrunk()
    {
        while (transform.localScale != originalScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * shrinkSpeed);
            yield return null;
        }
    }

    protected virtual void FollowPlayer()
    {
        if (playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position + leftOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
        else
        {
            isPickedUp = false;
            StartCoroutine(Shrunk());
        }
    }

    protected virtual void OnPickUp()
    {
        // Override this method in derived classes to perform specific actions on pick up
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
    }

    public virtual void Drop()
    {
        Vector3 respawnPosition;
        if (GameManager.sharedSavePoint != Vector3.zero)
        {
            respawnPosition = GameManager.sharedSavePoint; 
        }
        else
        {
            respawnPosition = initialPosition; 
        }

        transform.position = respawnPosition;
        isPickedUp = false;
        StartCoroutine(Shrunk());
        Debug.Log($"Prop Dropped: {respawnPosition}");
    }
}
