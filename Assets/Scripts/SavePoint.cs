using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.GetComponent<PhotonView>().IsMine)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.UpdateSavePoint(transform.position);
            }
        }
    }
}
