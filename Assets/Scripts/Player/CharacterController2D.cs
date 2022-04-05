using System;
using UnityEngine;
using UnityEngine.Events;
//using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour {

    private float m_JumpForce = 800f;                          // Amount of force added when the player jumps.
    private float minimumJumpForceMultiplier = 0.5f;           // Minimum amount player will jump if they tap button instead of holding
    private bool isHoldingJump = false;
    private float variableJumpForce = 0f;
    private float totalJumpForce = 0f;
    private float timeSinceLastJumpHold = 0f;

    [NonSerialized] public float sprintMultiplier = 1.75f;
    [Range(0, .3f)] private float m_MovementSmoothing = .05f;                                    // How much to smooth out the movement // 
    private bool m_AirControl = true;                                           // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

    const float k_GroundedRadius = 0.1f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    private Rigidbody m_Rigidbody;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    private void Awake() {
        m_Rigidbody = GetComponent<Rigidbody>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }

    private void FixedUpdate() //FixedUpdate
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i].gameObject != gameObject) {
                m_Grounded = true;
                if (!wasGrounded && m_Rigidbody.velocity.y <= 0)
                    OnLandEvent.Invoke();
            }
        }

        //Allow the jump height to vary based on how long button is held
        if (isHoldingJump && Time.realtimeSinceStartup - timeSinceLastJumpHold >= 0.05f) {
            AddJumpHeight();
            timeSinceLastJumpHold = Time.realtimeSinceStartup;
        }
    }


    public void Move(float move, bool isSprinting, bool jump, bool flipDirection, string direction) {

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl) {

            // If sprinting
            if (isSprinting) {
                // increase the speed by the sprint multiplier
                move *= sprintMultiplier;
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody.velocity.y);

            // And then smoothing it out and applying it to the character
            m_Rigidbody.velocity = Vector3.SmoothDamp(m_Rigidbody.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            if (flipDirection) {
                if ((direction.Equals("right") && !m_FacingRight) || (direction.Equals("left") && m_FacingRight))
                    Flip();
            }

        }
        // If the player should jump
        if (m_Grounded && jump) {
            StartJump();
        }
    }


    private void Flip() {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        transform.Rotate(0f, 180f, 0f);
    }

    public void StartJump() {
        isHoldingJump = true;
        //m_Grounded = false;
        variableJumpForce = m_JumpForce * minimumJumpForceMultiplier;
        totalJumpForce = variableJumpForce;
        m_Rigidbody.AddForce(new Vector2(0f, variableJumpForce * m_Rigidbody.mass / Time.timeScale));
    }

    public void ReleaseJump() {
        isHoldingJump = false;
    }


    public void AddJumpHeight() {
        
        variableJumpForce = m_JumpForce * 0.1f;
        totalJumpForce += variableJumpForce;

        //Debug.Log(variableJumpForce + " | " + totalJumpForce);
        if (totalJumpForce >= m_JumpForce) {
            variableJumpForce = 0f;
            isHoldingJump = false;
        }

        if (isHoldingJump)
            m_Rigidbody.AddForce(new Vector2(0f, variableJumpForce * m_Rigidbody.mass));
    }




}
