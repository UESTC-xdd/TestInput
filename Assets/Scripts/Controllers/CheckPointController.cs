using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    public bool IsCurCheckPoint { get; set; }
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsCurCheckPoint)
        {
            if (collision.gameObject.tag == "Player")
            {
                PlayerState curPlayerState = collision.GetComponent<PlayerState>();
                spriteRenderer.sprite = curPlayerState.CheckPointSprite;
                curPlayerState.CurSpawnPointPos = transform.position;

                AudioMgr.Instance.PlaySEClipOnce(AudioMgr.Instance.SE_CheckPoint);

                IsCurCheckPoint = true;
            }
        }

    }
}
