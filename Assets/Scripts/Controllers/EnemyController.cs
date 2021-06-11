using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyKind ThisEnemyKind;

    private void Start()
    {
        UIMgr.Instance.RespControlToggle.onValueChanged.AddListener(UpdateCol);
    }

    private void UpdateCol(bool enabled)
    {
        if(enabled)
        {
            Destroy(GetComponent<Collider2D>());
            gameObject.AddComponent(typeof(CircleCollider2D));
        }
        else
        {
            Destroy(GetComponent<Collider2D>());
            gameObject.AddComponent(typeof(BoxCollider2D));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerState curPlayerState = collision.gameObject.GetComponent<PlayerState>();
            curPlayerState.Respawn();
        }
    }
}

public enum EnemyKind
{
    Static,
    Moveable
}
