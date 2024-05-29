using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Prop
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            FinishPoint finishPoint = collision.gameObject.GetComponent<FinishPoint>();
            finishPoint.Unlock();
            Destroy(this.gameObject);
        }
    }
}
