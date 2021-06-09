using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.InputSystem;

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

    private Vector2 inputMove;

    private float jumpSpeed;

    private int Anim_IsCrouching;
    private int Anim_IsWalking;
    private int Anim_IsSprinting;
    private int Anim_IsLookingUp;
    private int Anim_IsUping;
    private int Anim_IsFalling;
    private int Anim_Horizontal;
    private int Anim_IsGrounded;

    private bool IsJumping;
    private bool IsSprinting;
    private bool IsCrouching;
    private bool IsWalking;
    public bool IsGrounded { get; set; }

    public bool IsFacingRight { get; set; }

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerRigid = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        IsFacingRight = true;

        InitInput();
        InitAnim();

        //jumpSpeed = Mathf.Sqrt(2 * playerRigid.gravityScale * -Physics2D.gravity.y * JumpHeight);
    }

    private void Update()
    {
        ReadInput();
        UpdateAnim();
        UpdateGroundedState();
    }

    private void FixedUpdate()
    {
        PlayerLocomotion();
    }

    private void InitInput()
    {
        //跳跃
        input.actions["Jump"].performed += ctx =>
        {
            PlayerJump();
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
        Anim_Horizontal = Animator.StringToHash("Horizontal");
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

        //蹲下
        if (inputMove.y < 0)
        {
            IsCrouching = true;
        }
        else if (inputMove.y >= 0)
        {
            IsCrouching = false;
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
                xSpeed += xDirect * Accelerate * Time.deltaTime;
                xSpeed = Mathf.Clamp(xSpeed, -SprintSpeed, SprintSpeed);
            }
            else if (!IsSprinting && inputMove.x != 0)
            {
                if (Mathf.Abs(xSpeed) < WalkSpeed)
                {
                    xSpeed += xDirect * Accelerate * Time.deltaTime;
                    xSpeed = Mathf.Clamp(xSpeed, -WalkSpeed, WalkSpeed);
                }
                else if (Mathf.Abs(xSpeed) > WalkSpeed)
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

            Debug.Log(xSpeed);
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
        if (!IsJumping && IsGrounded)
        {
            jumpSpeed = Mathf.Sqrt(2 * playerRigid.gravityScale * -Physics2D.gravity.y * JumpHeight);
            playerRigid.velocity = new Vector2(playerRigid.velocity.x, jumpSpeed);
            IsJumping = true;
            AudioMgr.Instance.PlaySEClipOnce(AudioMgr.Instance.SE_Jump);
        }
    }

    private void UpdateGroundedState()
    {
        if (Physics2D.BoxCast(GroundDetectPoint.position, new Vector2(GroundDetectWidth, GroundDetectHeight), 0, transform.right, Mathf.Infinity, GroundLayers))
        {
            //Debug.Log("在地上");
            IsGrounded = true;
        }
        else
        {
            //Debug.Log("不在地上");
            IsGrounded = false;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(GroundDetectPoint.position, new Vector3(GroundDetectWidth, GroundDetectHeight, 0));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer==LayerMask.NameToLayer("Ground"))
        {
            foreach (ContactPoint2D contactPoint in collision.contacts)
            {
                Vector2 hitPoint = contactPoint.point;
                if(hitPoint.y< GroundDetectPoint.position.y)
                {
                    IsJumping = false;
                }
            }

        }
    }

}
