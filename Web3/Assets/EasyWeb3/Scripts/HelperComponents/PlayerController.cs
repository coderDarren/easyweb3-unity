using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : GameSystem
{

    [Header("Forces")]
    public float runSpeed = 6.0F;
    public float walkSpeed = 2.0F;
    public float jumpForce = 4.0F;
    public float gravity = 0.15F;

    [Header("Grounding")]
    [Range(4,24)]
    public int groundCheckDensity = 12;
    public float groundCheckRadius = 0.3F;
    public float groundCheckDist = 1.2F;
    public float groundCheckBias = 0.1F;
    public LayerMask groundLayer;

    // Components
    private CharacterController m_Character;
    private Vector3 m_MoveDirection;
    private Quaternion m_TargetRotation;

    // Inputs
    private bool m_Update;
    private bool m_InputIsFrozen;
    private bool m_RightClick;
    private bool m_Jump;
    private bool m_Shift;
    private float m_HorizontalRaw;
    private float m_Horizontal;
    private float m_VerticalRaw;
    private float m_Vertical;

    // Physics
    private bool m_Grounded;
    private float m_GroundBiasTimer;
    private float m_Gravity;


    private CharacterController character {
        get {
            if (!m_Character) {
                m_Character = GetComponent<CharacterController>();
            }
            return m_Character;
        }
    }

    public float runInput { get {return m_Vertical;} }
    public bool grounded { get { return m_Grounded; } }

#region Unity Functions
    private void Start() {
        m_TargetRotation = transform.rotation;
        m_Update = true;
        m_InputIsFrozen = false;
    }

    private void Update() {
        if (!m_Update) return;
        PollInput();
        CheckGrounded();
        Jump();
        Move();
        Turn();
    }
#endregion

#region Public Functions
    public void FreezeInput() {
        m_InputIsFrozen = true;
    }

    public void FreeInput() {
        m_InputIsFrozen = false;
    }

    public void Rotate(float _angle) {
        m_TargetRotation *= Quaternion.Euler(0, _angle, 0);
    }

    public void SetRotation(Quaternion _rot) {
        m_TargetRotation = _rot;
    }

    public void SetRotationImmediate(Quaternion _rot) {
        transform.rotation = _rot;
        m_TargetRotation = transform.rotation;
    }
#endregion

#region Private Functions
    private void DecayInput() {
        m_Vertical = Mathf.Lerp(m_Vertical, 0, 5*Time.deltaTime);
        m_VerticalRaw = Mathf.Lerp(m_VerticalRaw, 0, 5*Time.deltaTime);
        m_Horizontal = Mathf.Lerp(m_Horizontal, 0, 5*Time.deltaTime);
        m_HorizontalRaw = Mathf.Lerp(m_HorizontalRaw, 0, 5*Time.deltaTime);
        m_Shift = false;
    }

    private void PollInput() {
        if (m_InputIsFrozen) {
            DecayInput();
            return;
        }

        m_RightClick = Input.GetMouseButton(1);
        m_Jump = Input.GetButton("Jump");
        m_Shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        m_HorizontalRaw = Input.GetAxisRaw("Horizontal");
        m_Horizontal = Input.GetAxis("Horizontal");
        m_VerticalRaw = Input.GetAxisRaw("Vertical");
        m_Vertical = Input.GetAxis("Vertical");
    }

    private void Jump() {
        if (m_Grounded && m_Jump) {
            m_MoveDirection.y = jumpForce + 0.01f;
        } else if (!m_Grounded) {
            m_MoveDirection.y -= gravity;
        }
    }

    private void Move() {
        Vector3 _dir = m_RightClick && m_VerticalRaw == 0 && m_HorizontalRaw != 0 ? transform.forward : transform.forward * m_VerticalRaw;
        float _speed = m_VerticalRaw < 0 || m_Shift ? walkSpeed : (0.1F + runSpeed);
        m_MoveDirection = _dir * _speed + Vector3.up * m_MoveDirection.y;
        character.Move(m_MoveDirection * Time.deltaTime);
    }

    private void Turn() {
        transform.rotation = Quaternion.Lerp(transform.rotation, m_TargetRotation, 25 * Time.deltaTime);
    }

    private void CheckGrounded() {
        RaycastHit _hitInfo;
        Vector3 _from = transform.position + character.center;
        int _hits = 0;
        bool _hit = false;

        // check center
        if (Physics.Raycast(_from, Vector3.down, out _hitInfo, groundCheckDist, groundLayer)) {
            _hit = true;
            _hits++;
            if (debug) {
                Debug.DrawLine(_from, _from + Vector3.down * groundCheckDist, Color.green);
            }
        }

        // check radius
        // !! TODO
        // Make this a function
        for (int i = 0; i < groundCheckDensity; i++) {
            float _angle = (360.0F / groundCheckDensity) * i;
            _from = transform.position + character.center + (Quaternion.Euler(0, _angle, 0) * Vector3.forward) * groundCheckRadius;
            if (Physics.Raycast(_from, Vector3.down, out _hitInfo, groundCheckDist, groundLayer)) {
                _hit = true;
                _hits++;
            }
            if (debug) {
                Debug.DrawLine(_from, _from + Vector3.down * groundCheckDist, Color.green);
            }
        }

        for (int i = 0; i < groundCheckDensity; i++) {
            float _angle = (360.0F / groundCheckDensity) * i;
            _from = transform.position + character.center + (Quaternion.Euler(0, _angle, 0) * Vector3.forward) * (groundCheckRadius/2.0f);
            if (Physics.Raycast(_from, Vector3.down, out _hitInfo, groundCheckDist, groundLayer)) {
                _hit = true;
                _hits++;
            }
            if (debug) {
                Debug.DrawLine(_from, _from + Vector3.down * groundCheckDist, Color.green);
            }
        }

        for (int i = 0; i < groundCheckDensity; i++) {
            float _angle = (360.0F / groundCheckDensity) * i;
            _from = transform.position + character.center + (Quaternion.Euler(0, _angle, 0) * Vector3.forward) * (groundCheckRadius/5.0f);
            if (Physics.Raycast(_from, Vector3.down, out _hitInfo, groundCheckDist, groundLayer)) {
                _hit = true;
                _hits++;
            }
            if (debug) {
                Debug.DrawLine(_from, _from + Vector3.down * groundCheckDist, Color.green);
            }
        }

        // handle hit
        // calculate forward and right vectors
        if (_hit) {
            m_Grounded = true;
            m_GroundBiasTimer = 0;
        } else {
            // Don't "unground" until bias is reached
            if (m_Grounded) {
                m_GroundBiasTimer += Time.deltaTime;
                if (m_GroundBiasTimer > groundCheckBias) {
                    m_Grounded = false;
                }
            }
        }
        
        if (debug) {
            Debug.DrawLine(transform.position, transform.position + transform.forward * 1.0f, Color.blue);
            Debug.DrawLine(transform.position, transform.position + transform.right * 1.0f, Color.red);
        }
    }
#endregion
}