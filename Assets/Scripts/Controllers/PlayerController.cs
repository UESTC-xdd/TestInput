using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public int JumpHeight;
    public float Accelerate;
    public float BrakeAccelerate;
    public int WalkSpeed;
    public int SprintSpeed;
    public int MaxDropSpeed;

    [Header("监测地面相关")]
    public Transform GroundDetectPoint;
    public float GroundDetectWidth;
    public float GroundDetectHeight;
    public LayerMask GroundLayers;

    private Rigidbody2D playerRigid;
    private PlayerInput input;
    private Animator playerAnim;
    private SpriteRenderer playerSpriteRenderer;

    private BoxCollider2D playerCol;
    private Vector2 standColOffset = new Vector2(0, 0);
    private Vector2 standColSize = new Vector2(0.875f, 1.25f);
    private Vector2 crouchColOffset = new Vector2(0, -0.1875f);
    private Vector2 crouchColSize = new Vector2(0.875f, 0.875f);
    private Vector2 RespColOffset = new Vector2(0, 0.125f);
    private Vector2 RespColSize = new Vector2(0.875f, 1);
    private Vector2 JumpRespColOffset = new Vector2(0, 0);
    private Vector2 JumpRespColSize = new Vector2(0.35f, 1.25f);
    private Vector2 GroundDetectOriginSize = new Vector2(0.75f, 0.3f);
    private Vector2 GroundDetectRespSize = new Vector2(0.3f, 0.3f);

    private Vector2 inputMove;

    private float jumpSpeed;

    private int Anim_IsCrouching;
    private int Anim_IsWalking;
    private int Anim_IsSprinting;
    private int Anim_IsLookingUp;
    private int Anim_IsUping;
    private int Anim_IsFalling;
    //private int Anim_Horizontal;
    private int Anim_IsGrounded;

    private bool IsJumping;
    private bool IsSprinting;
    private bool IsCrouching;
    private bool IsWalking;
    public bool IsGrounded { get; set; }

    public bool IsFacingRight { get; set; }

    public bool CanJump { get; set; }

    private Timer timer;//用于跳跃延迟判断落地
    private Timer timer2;//用于生成轨迹
    private Timer timer3;//用于跳跃延迟输入
    private Timer timer4;//用于跳跃输入宽容度
    private Timer timer5;//用于按空格输入痕迹显示
    private bool respEnabled;
    [Header("宽容度输入")]
    public float fallRespTime;
    public float JumpInputBufferTime;
    private bool IsJumpInputBuffered;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerRigid = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        playerCol = GetComponent<BoxCollider2D>();

        IsFacingRight = true;

        InitInput();
        InitAnim();

        jumpSpeed = Mathf.Sqrt(2 * playerRigid.gravityScale * -Physics2D.gravity.y * JumpHeight);
    }

    private void Update()
    {
        UpdateGroundedState();
        UpdateCanJumpState();
        ReadInput();
        UpdateAnim();

        //Update宽容度输入状态
        UpdateRespState();
        UpdateJumpInputBuffer();
        UpdateRespCol();
    }

    private void FixedUpdate()
    {
        PlayerLocomotion();
        UpdateTrail();
    }

    private void UpdateJumpInputBuffer()
    {
        if (CanJump)
        {
            if (IsJumpInputBuffered && timer4.IsCounting)
            {
                PlayerJump();
                IsJumpInputBuffered = false;
                timer4.Stop();
                return;
            }
        }
    }

    private void UpdateRespState()
    {
        respEnabled = UIMgr.Instance.RespControlToggle.isOn;
    }

    private void UpdateRespCol()
    {
        if (respEnabled)
        {
            //蹲下
            if (inputMove.y < 0)
            {
                IsCrouching = true;
                playerCol.offset = crouchColOffset;
                playerCol.size = crouchColSize;
            }
            else if (inputMove.y >= 0)
            {
                if (!IsGrounded && !IsJumping && IsSprinting && !IsCrouching && Mathf.Abs(playerRigid.velocity.x) >= SprintSpeed - 0.5f)
                {
                    playerCol.offset = RespColOffset;
                    playerCol.size = RespColSize;
                }
                else
                {
                    IsCrouching = false;
                    playerCol.offset = standColOffset;
                    playerCol.size = standColSize;
                }
            }

        }
        else
        {
            //蹲下
            if (inputMove.y < 0)
            {
                IsCrouching = true;
                playerCol.offset = crouchColOffset;
                playerCol.size = crouchColSize;
            }
            else if (inputMove.y >= 0)
            {
                IsCrouching = false;
                playerCol.offset = standColOffset;
                playerCol.size = standColSize;
            }
        }

        //跳跃时碰撞体变窄
        if(respEnabled)
        {
            if (IsJumping && playerRigid.velocity.y > 0)
            {
                UpdateGroundDetect(GroundDetectRespSize);
                playerCol.size = new Vector2(JumpRespColSize.x, playerCol.size.y);
            }
            else if (playerRigid.velocity.y <= 0)
            {
                if (GroundDetectWidth != GroundDetectOriginSize.x)
                    UpdateGroundDetect(GroundDetectOriginSize);

                if (playerCol.size.x != standColSize.x)
                    playerCol.size = new Vector2(standColSize.x, playerCol.size.y);
            }
        }
        else
        {
            if(GroundDetectWidth != GroundDetectOriginSize.x)
                UpdateGroundDetect(GroundDetectOriginSize);

            if(playerCol.size.x!= standColSize.x)
                playerCol.size = new Vector2(standColSize.x, playerCol.size.y);
        }

    }

    #region 轨迹逻辑
    private void UpdateTrail()
    {
        if (UIMgr.Instance.ShowTrailToggle.isOn)
        {
            if (playerRigid.velocity.magnitude > 0.1f)
            {
                if (timer2 == null)
                {
                    timer2 = Util.Instance.BeginTimer(0.1f);
                    timer2.OnCountStop -= GenerateOneTrail;
                    timer2.OnCountStop += GenerateOneTrail;
                }
                if (!timer2.IsCounting)
                {
                    timer2.ResetTimer(0.1f);
                }
            }
        }
        else
        {
            if (timer2 != null && timer2.IsCounting)
            {
                timer2.Stop();
            }
        }
    }

    private void GenerateOneTrail()
    {
        GameObject newTrail = PoolManager.instance.GetFromPool("Trail", transform.position);
        SpriteRenderer newTrailSprite = newTrail.GetComponent<SpriteRenderer>();
        newTrailSprite.sprite = playerSpriteRenderer.sprite;
        newTrailSprite.flipX = playerSpriteRenderer.flipX;
    }
    #endregion

    private void InitInput()
    {
        //跳跃
        input.actions["Jump"].performed += ctx =>
        {
            if (respEnabled)
            {
                if (CanJump)
                {
                    PlayerJump();
                }
                else
                {
                    if (IsJumping)
                    {
                        if (timer4 == null)
                        {
                            timer4 = Util.Instance.BeginTimer(JumpInputBufferTime);
                            timer4.OnCountStop += () =>
                              {
                                  IsJumpInputBuffered = false;
                              };
                        }
                        else
                        {
                            timer4.ResetTimer(JumpInputBufferTime);
                        }
                        IsJumpInputBuffered = true;
                    }
                }
            }
            else
            {
                if (CanJump)
                {
                    PlayerJump();
                }
            }
        };

        //加速
        input.actions["Sprint"].performed += ctx =>
         {
             IsSprinting = true;
         };

        input.actions["Sprint"].canceled += ctx =>
        {
            IsSprinting = false;
        };
    }

    private void InitAnim()
    {
        Anim_IsCrouching = Animator.StringToHash("IsCrouching");
        Anim_IsWalking = Animator.StringToHash("IsWalking");
        Anim_IsSprinting = Animator.StringToHash("IsSprinting");
        Anim_IsLookingUp = Animator.StringToHash("IsLookingUp");
        Anim_IsUping = Animator.StringToHash("IsUping");
        Anim_IsFalling = Animator.StringToHash("IsFalling");
        //Anim_Horizontal = Animator.StringToHash("Horizontal");
        Anim_IsGrounded = Animator.StringToHash("IsGrounded");
    }

    private void ReadInput()
    {
        inputMove = input.actions["Move"].ReadValue<Vector2>();
    }

    public void SetInputEnable(bool enabled)
    {
        input.enabled = enabled;
    }

    private void PlayerLocomotion()
    {
        //是否移动
        if (Mathf.Abs(inputMove.x) > 0.1f)
        {
            IsWalking = true;
        }
        else
        {
            IsWalking = false;
        }

        //朝向
        if (inputMove.x < 0)
        {
            IsFacingRight = false;
        }
        else if (inputMove.x > 0)
        {
            IsFacingRight = true;
        }

        //限制最高下降速度
        if (playerRigid.velocity.y < MaxDropSpeed)
        {
            playerRigid.velocity = new Vector2(playerRigid.velocity.x, MaxDropSpeed);
        }

        //没有移动地减速
        if (inputMove.x == 0 || (IsGrounded && IsCrouching)) //没移动
        {
            float xSpeed = playerRigid.velocity.x;
            if (xSpeed > 0)
            {
                xSpeed -= BrakeAccelerate * Time.deltaTime;
                xSpeed = Mathf.Clamp(xSpeed, 0, SprintSpeed);
            }
            else if (xSpeed < 0)
            {
                xSpeed += BrakeAccelerate * Time.deltaTime;
                xSpeed = Mathf.Clamp(xSpeed, -SprintSpeed, 0);
            }
            playerRigid.velocity = new Vector2(xSpeed, playerRigid.velocity.y);
        }
        else if (IsWalking) //水平移动
        {
            float xSpeed = playerRigid.velocity.x;
            float realXSpeed = Mathf.Abs(xSpeed);
            int xDirect = 1;

            if (inputMove.x > 0)
            {
                xDirect = 1;
            }
            else if (inputMove.x < 0)
            {
                xDirect = -1;
            }

            if (IsSprinting && inputMove.x != 0)
            {
                if (Mathf.Sign(xSpeed) != Mathf.Sign(inputMove.x))
                {
                    xSpeed += xDirect * (Accelerate + BrakeAccelerate) * Time.deltaTime;
                }
                else
                {
                    xSpeed += xDirect * Accelerate * Time.deltaTime;
                }
                xSpeed = Mathf.Clamp(xSpeed, -SprintSpeed, SprintSpeed);
            }
            else if (!IsSprinting && inputMove.x != 0)
            {
                if (realXSpeed <= WalkSpeed)
                {
                    if (Mathf.Sign(xSpeed) != Mathf.Sign(inputMove.x))
                    {
                        xSpeed += xDirect * (Accelerate + BrakeAccelerate) * Time.deltaTime;
                    }
                    else
                    {
                        xSpeed += xDirect * Accelerate * Time.deltaTime;
                    }
                    xSpeed = Mathf.Clamp(xSpeed, -WalkSpeed, WalkSpeed);
                }
                else if (realXSpeed > WalkSpeed)
                {
                    if (xSpeed < 0)
                    {
                        xSpeed += Accelerate * Time.deltaTime;
                    }
                    else if (xSpeed > 0)
                    {
                        xSpeed -= Accelerate * Time.deltaTime;
                    }
                }

            }

            playerRigid.velocity = new Vector2(xSpeed, playerRigid.velocity.y);
        }
    }

    private void UpdateAnim()
    {
        //朝向
        playerSpriteRenderer.flipX = !IsFacingRight;

        //是否移动
        playerAnim.SetBool(Anim_IsWalking, IsWalking);

        //是否冲刺
        playerAnim.SetBool(Anim_IsSprinting, IsSprinting);

        //是否朝上看
        if (inputMove.y > 0 && !IsWalking)
        {
            playerAnim.SetBool(Anim_IsLookingUp, true);
        }
        else
        {
            playerAnim.SetBool(Anim_IsLookingUp, false);
        }

        //上升落下
        if (!IsGrounded && playerRigid.velocity.y > 0 && !IsCrouching)
        {
            playerAnim.SetBool(Anim_IsUping, true);
            playerAnim.SetBool(Anim_IsFalling, false);
        }
        else if (!IsGrounded && playerRigid.velocity.y < 0 && !IsCrouching)
        {
            playerAnim.SetBool(Anim_IsUping, false);
            playerAnim.SetBool(Anim_IsFalling, true);
        }

        //在地面上不考虑上升下降
        if (IsGrounded)
        {
            playerAnim.SetBool(Anim_IsUping, false);
            playerAnim.SetBool(Anim_IsFalling, false);
        }

        //蹲下
        playerAnim.SetBool(Anim_IsCrouching, IsCrouching);

        //接触地面
        playerAnim.SetBool(Anim_IsGrounded, IsGrounded);
    }

    private void PlayerJump()
    {
        //jumpSpeed = Mathf.Sqrt(2 * playerRigid.gravityScale * -Physics2D.gravity.y * JumpHeight);
        playerRigid.velocity = new Vector2(playerRigid.velocity.x, jumpSpeed);
        IsJumping = true;
        AudioMgr.Instance.PlaySEClipOnce(AudioMgr.Instance.SE_Jump);
        timer = Util.Instance.BeginTimer(0.1f);
    }

    private void UpdateGroundedState()
    {
        if (Physics2D.BoxCast(GroundDetectPoint.position, new Vector2(GroundDetectWidth, GroundDetectHeight), 0, -transform.up, 0, GroundLayers))
        {
            //Debug.Log("在地上");
            IsGrounded = true;
        }
        //if (Physics2D.Raycast(GroundDetectPoint.position, -transform.up, GroundDetectHeight, GroundLayers))
        //{
        //    IsGrounded = true;
        //}
        else
        {
            //Debug.Log("不在地上");
            IsGrounded = false;
        }

    }

    private void UpdateCanJumpState()
    {
        if (respEnabled)
        {
            if (!IsJumping && !IsGrounded)
            {
                if (timer3 == null)
                {
                    timer3 = Util.Instance.BeginTimer(fallRespTime);
                }
                else
                {
                    if (timer3.IsCounting)
                    {
                        CanJump = true;
                    }
                    else
                    {
                        CanJump = false;
                    }
                }
            }
            else if (!IsJumping && IsGrounded)
            {
                CanJump = true;
                timer3 = null;
            }
            else
            {
                CanJump = false;
            }
        }
        else
        {
            if (!IsJumping && IsGrounded)
            {
                CanJump = true;
                timer3 = null;
            }
            else
            {
                CanJump = false;
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(GroundDetectPoint.position, new Vector3(GroundDetectWidth, GroundDetectHeight, 0));
        //Gizmos.DrawLine(GroundDetectPoint.position, GroundDetectPoint.position - transform.up * GroundDetectHeight);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (timer != null && !timer.IsCounting)
            {
                foreach (ContactPoint2D contactPoint in collision.contacts)
                {
                    Vector2 hitPoint = contactPoint.point;
                    if (hitPoint.y < transform.position.y - playerCol.size.y / 2 + 0.01f)
                    {
                        IsJumping = false;
                    }
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (timer != null && !timer.IsCounting)
            {
                foreach (ContactPoint2D contactPoint in collision.contacts)
                {
                    Vector2 hitPoint = contactPoint.point;
                    if (hitPoint.y < transform.position.y - playerCol.size.y / 2 + 0.01f)
                    {
                        IsJumping = false;
                    }
                }
            }

        }
    }

    private void UpdateGroundDetect(Vector2 groundDetectSize)
    {
        GroundDetectWidth = groundDetectSize.x;
        GroundDetectHeight = groundDetectSize.y;
    }
}
