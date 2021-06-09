using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    public Sprite CheckPointSprite;
    public float DeadLinePosY;

    public Vector3 CurSpawnPointPos { get; set; }

    public bool IsDead { get; set; }

    public UnityAction OnPlayerDead;

    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        CurSpawnPointPos = transform.position;

        OnPlayerDead -= PlayerDead;
        OnPlayerDead += PlayerDead;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < DeadLinePosY && !IsDead)
        {
            IsDead = true;
            OnPlayerDead();
        }
    }

    public void Respawn()
    {
        transform.position = CurSpawnPointPos;
        IsDead = false;
        AudioMgr.Instance.BGMSource.Play();
        controller.SetInputEnable(true);
    }

    public void PlayerDead()
    {
        AudioMgr.Instance.BGMSource.Stop();
        AudioMgr.Instance.PlaySEClipOnce(AudioMgr.Instance.SE_Dead);
        controller.SetInputEnable(false);
        StartCoroutine(PlayerDeadWaitCou());

    }

    IEnumerator PlayerDeadWaitCou()
    {
        yield return new WaitUntil(() => !AudioMgr.Instance.SESource.isPlaying);
        yield return new WaitForSeconds(0.5f);
        Respawn();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Camera mainCamera = Camera.main;
        Gizmos.DrawLine(new Vector3(mainCamera.transform.position.x - mainCamera.orthographicSize, DeadLinePosY, 0), new Vector3(mainCamera.transform.position.x + mainCamera.orthographicSize, DeadLinePosY, 0));
    }
}
