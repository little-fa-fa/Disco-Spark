using System.Collections;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public float pickUpRange = 2f;
    public float shrinkSpeed = 2f;
    public float followSpeed = 5f;
    public Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);

    private bool isPickedUp = false;
    private Transform playerTransform;

    void Update()
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

    private void CheckForPickUp()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickUpRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                playerTransform = collider.transform;
                isPickedUp = true;
                StartCoroutine(Shrink());
                break;
            }
        }
    }

    private IEnumerator Shrink()
    {
        while (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * shrinkSpeed);
            yield return null;
        }
    }

    private void FollowPlayer()
    {
        if (playerTransform != null)
        {
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, Time.deltaTime * followSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
    }
}