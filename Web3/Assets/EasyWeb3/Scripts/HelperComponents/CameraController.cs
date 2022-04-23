
using UnityEngine;

public class CameraController : GameSystem
{
    [Header("General")]
    public Transform target;

    [Header("Controls")]
    public float turnSpeed = 150.0F;
    public float autoFocusTime = 0.5F;
    public float idleTurnAngleLimit = 45.0F;
    public float strafeAngle = 45.0F;
    public float maxZoom = 4.0F;

    [Header("Offsetting")]
    public Vector3 targetOffset = new Vector3(0,0.8F,0);         // offset of target's true position
    public Vector3 followOffset = new Vector3(0,0.6F,-2.15F);    // where are we relative to target
    public Vector3 rotationOffset = new Vector3(20.0F, 0, 0);    // where do we look relative to the target

    [Header("Collision")]
    public LayerMask collisionLayer;              // objects that collision respond to
    public float collisionPadding = 1000.0F;      // how much padding around clip plane? (higher values for less clipping)
    public float collisionBump = 0.25F;           // how much additional offsetting? (higher values for less clipping)

    [Header("Input")]
    public float minimumInspectAngle = -90.0F;    // min X angle when left click inspecting
    public float maximumInspectAngle = 45.0F;     // max X angle when left click inspecting
    public float mouseClickTurnSpeed = 750.0F;
    public float verticalMouseTurnSpeed = 325.0F;
    public float zoomSpeed = 2.0F;

    // Components
    private PlayerController m_Player;
    private Camera m_Camera;

    // Inputs
    private bool m_RightClick;
    private bool m_LeftClick;
    private bool m_VerticalStart;
    private bool m_HorizontalStart;
    private float m_HorizontalRaw;
    private float m_Horizontal;
    private float m_VerticalRaw;
    private float m_Vertical;
    private float m_MouseX;
    private float m_MouseY;
    private float m_Scroll;

    // Interaction
    private bool m_UIInteraction;
    // the params below used for TurnTargetTowardCameraDirection()
    private bool m_AutoFocus;   // control param to determine if TurnTargetTowardCameraDirection() should ever execute
    private bool m_AutoFocusStarted;
    private float m_CameraAngleStart;
    private Quaternion m_PlayerAngleStart;
    private Quaternion m_PlayerAngleGoal;
    private float m_Timer;

    // Vectors
    private Vector3 m_TargetPos;
    private Vector3 m_LookPos;
    private Vector3 m_FollowOffset;
    private Vector2 m_MouseRotationOffset;
    private Quaternion m_TargetRotation;
    private Quaternion m_MouseRotation;

    // Control
    private float m_StrafeLockAngle;
    private float m_StrafeAngle;
    private float m_Zoom;
    private float m_TargetZoom;

    private PlayerController player {
        get {
            if (!target) {
                LogWarning("No target found.");
                return null;
            }
            if (!m_Player) {
                m_Player = target.GetComponent<PlayerController>();
            }
            if (!m_Player) {
                LogWarning("Target must have PlayerController attached");
            }
            return m_Player;
        }
    }

#region Unity Functions
    private void LateUpdate() {
        if (!TargetHasIntegrity()) return;
        PollInput();

        if (m_RightClick && m_Horizontal != 0) {
            StrafeDrivePlayer();
        } else if (m_VerticalRaw != 0) {
            DrivePlayer();
        } else if ((m_RightClick || m_HorizontalRaw != 0) && !m_LeftClick && m_VerticalRaw == 0) {
            TurnTargetWithCameraDirection();
        } else if (m_LeftClick && !m_RightClick && !m_UIInteraction) {
            RotateAroundTargetWithClick();
        } else if (!m_LeftClick && !m_RightClick && m_AutoFocus && m_HorizontalRaw == 0) {
            TurnTargetTowardCameraDirection();
        }

        if (m_RightClick) {
           m_MouseRotationOffset.x -= m_MouseY * verticalMouseTurnSpeed * Time.deltaTime;
           m_MouseRotationOffset.x = Mathf.Clamp(m_MouseRotationOffset.x, minimumInspectAngle, maximumInspectAngle);
        }

        Zoom();
        ApplyCameraPosition();
        ApplyCameraRotation();
        DetectCollision();
    }

    private void OnEnable() {
        m_Camera = GetComponent<Camera>();
        m_AutoFocus = true;
        m_TargetZoom = 1;
        m_Zoom = m_TargetZoom;
    }
#endregion

#region Private Functions
    private void OnUIHandleInteractionStart(int _id) {
        m_UIInteraction = true;
    }

    private void OnUIHandleInteractionStop(int _id) {
        m_UIInteraction = false;
    }

    private bool TargetHasIntegrity() {
        if (target == null) {
            LogWarning("Could not find target. Please assign in the inspector..");
            return false;
        }
        return true;
    }
    
    private void PollInput() {
        m_RightClick = Input.GetMouseButton(1);
        m_LeftClick = Input.GetMouseButton(0);
        m_HorizontalRaw = Input.GetAxisRaw("Horizontal");
        m_Horizontal = Input.GetAxis("Horizontal");
        m_VerticalRaw = Input.GetAxisRaw("Vertical");
        m_Vertical = Input.GetAxis("Vertical");
        m_MouseX = Input.GetAxis("Mouse X");
        m_MouseY = Input.GetAxis("Mouse Y");
        m_Scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetMouseButtonUp(1)) {
            m_AutoFocus = true;
        } else if (Input.GetMouseButtonUp(0)) {
            m_AutoFocus = false;
        }

        if (m_HorizontalRaw != 0 && !m_HorizontalStart) {
            m_AutoFocusStarted = false;
            m_HorizontalStart = true;
            m_AutoFocus = true;
            m_Timer = 0;
        } else if (m_HorizontalRaw == 0) {
            m_HorizontalStart = false;
        }
        
        if (m_VerticalRaw != 0 && !m_VerticalStart) {
            m_AutoFocusStarted = false;
            m_VerticalStart = true;
            m_AutoFocus = true;
            m_Timer = 0;
        } else if (m_VerticalRaw == 0) {
            m_VerticalStart = false;
        }
    }

    /*
     * Rotate the player toward the direction of the camera while the player is strafing
     */
    private void StrafeDrivePlayer() {
        Log("StrafeDrivePlayer");
        
        m_StrafeLockAngle += m_MouseX * mouseClickTurnSpeed * Time.deltaTime;
        float _target = m_HorizontalRaw != 0 && m_VerticalRaw > 0 ? strafeAngle * m_HorizontalRaw :
                        m_HorizontalRaw != 0 && m_VerticalRaw < 0 ? -strafeAngle * m_HorizontalRaw :
                        m_HorizontalRaw > 0 && m_VerticalRaw == 0 ? 90 :
                        m_HorizontalRaw < 0 && m_VerticalRaw == 0 ? -90 : 0;

        if (m_VerticalRaw != 0) {
            m_StrafeAngle = Mathf.Lerp(m_StrafeAngle, _target, 5 * Time.deltaTime);
        } else {
            m_StrafeAngle = _target;
        }
        
        player.SetRotationImmediate(Quaternion.Euler(0, m_StrafeLockAngle + m_StrafeAngle, 0));
        m_MouseRotationOffset.y = -m_StrafeAngle;
    }

    /*
     * Rotate the player toward the direction of the camera while the player is running
     */
    private void DrivePlayer() {
        Log("DrivePlayer");

        m_StrafeLockAngle = transform.eulerAngles.y;
        m_StrafeAngle = 0;

        if (m_LeftClick) {
            RotateAroundTargetWithClick();
        } else if (m_MouseRotationOffset.y != 0) {
            m_MouseRotationOffset.y = 0;
            player.SetRotationImmediate(Quaternion.Euler(0, m_StrafeLockAngle, 0));
        }

        float _turnSpeed = m_HorizontalRaw != 0 ? m_HorizontalRaw * turnSpeed * Time.deltaTime :
                            m_RightClick ? m_MouseX * mouseClickTurnSpeed * Time.deltaTime : 0;
        player.Rotate(_turnSpeed);
    }

    /*
     * Rotate camera around player without rotating the target, and..
     * ..while the target is not moving
     */
    private void RotateAroundTargetWithClick() {
        Log("RotateAroundTargetWithClick");
        m_StrafeLockAngle = transform.eulerAngles.y;

        m_MouseRotationOffset.x -= m_MouseY * mouseClickTurnSpeed * Time.deltaTime;
        m_MouseRotationOffset.y += m_MouseX * mouseClickTurnSpeed * Time.deltaTime;
        m_MouseRotationOffset.x = Mathf.Clamp(m_MouseRotationOffset.x, minimumInspectAngle, maximumInspectAngle);
    }

    /*
     * While camera is being turned, turn the target while target is not moving
     */
    private void TurnTargetWithCameraDirection() {
        Log("TurnTargetWithCameraDirection"); 
        m_AutoFocusStarted = false;
        m_StrafeLockAngle = transform.eulerAngles.y;

        float _turnSpeed = m_HorizontalRaw != 0 ? m_HorizontalRaw * turnSpeed * Time.deltaTime :
                            m_RightClick ? m_MouseX * mouseClickTurnSpeed * Time.deltaTime : 0;

        m_MouseRotationOffset.y += _turnSpeed;
        m_MouseRotationOffset.y = ClampAngle(m_MouseRotationOffset.y, -180, 180);

        if (m_MouseRotationOffset.y <= -idleTurnAngleLimit) {
            m_MouseRotationOffset.y = ClampAngle(Mathf.Lerp(m_MouseRotationOffset.y, -idleTurnAngleLimit, 5 * Time.deltaTime), -180, 180);
        } else if (m_MouseRotationOffset.y >= idleTurnAngleLimit) {
            m_MouseRotationOffset.y = ClampAngle(Mathf.Lerp(m_MouseRotationOffset.y, idleTurnAngleLimit, 5 * Time.deltaTime), -180, 180);
        }

        if (Mathf.Abs(m_MouseRotationOffset.y) >= idleTurnAngleLimit) {
            player.Rotate(_turnSpeed);
        }
    }

    /*
     * While camera is static, turn the target to the direction of the camera
     */
    private void TurnTargetTowardCameraDirection() {
        Log("TurnTargetTowardCameraDirection");
        m_StrafeLockAngle = transform.eulerAngles.y;

        if (!m_AutoFocusStarted) {
            m_AutoFocusStarted = true;
            m_CameraAngleStart = transform.eulerAngles.y;
            m_PlayerAngleStart = Quaternion.Euler(0, target.eulerAngles.y, 0);
            m_PlayerAngleGoal = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            m_Timer = 0;
        } else {
            m_Timer += Time.deltaTime;
            player.SetRotation(Quaternion.Lerp(m_PlayerAngleStart, m_PlayerAngleGoal, m_Timer / autoFocusTime));
            m_MouseRotationOffset.y = m_CameraAngleStart - target.eulerAngles.y;
        }
    }

    private void Zoom() {
        m_TargetZoom = m_TargetZoom - m_Scroll * zoomSpeed;
        m_TargetZoom = Mathf.Clamp(m_TargetZoom, 1, maxZoom);
        m_Zoom = Mathf.Lerp(m_Zoom, m_TargetZoom, 4 * Time.deltaTime);
        m_FollowOffset = targetOffset + followOffset * m_Zoom;
    }

    /*
     * Look at the target
     */ 
    private void ApplyCameraRotation() {
        m_LookPos = target.position + targetOffset + target.right * m_FollowOffset.x + target.up * m_FollowOffset.y;
        m_TargetRotation = Quaternion.LookRotation(m_LookPos - transform.position, Vector3.up);
        m_TargetRotation *= Quaternion.Euler(rotationOffset);
        transform.rotation = m_TargetRotation;
    }

    /*
     * Find position in the world relative to the target
     */
    private void ApplyCameraPosition() {
        m_MouseRotation = Quaternion.Euler(m_MouseRotationOffset.x, m_MouseRotationOffset.y, 0);
        m_TargetPos = target.position + targetOffset + (target.rotation * m_MouseRotation) * m_FollowOffset;
        transform.position = m_TargetPos;
    }

    private float ClampAngle(float _a, float _min, float _max) 
    {
        while (_max < _min) _max += 360.0f;
        while (_a > _max) _a -= 360.0f;
        while (_a < _min) _a += 360.0f;
        
        if (_a > _max)
            {
            if (_a - (_max + _min) * 0.5f < 180.0f)
                return _max;
            else
                return _min;
            }
        else
            return _a;
    }

#region Collision Detection
    private void DetectCollision() {
        // get clip points
        Vector3 _upperLeft = m_Camera.ScreenToWorldPoint(new Vector3(-collisionPadding, Screen.height+collisionPadding, m_Camera.nearClipPlane+0.0001f));
        Vector3 _upperRight = m_Camera.ScreenToWorldPoint(new Vector3(Screen.width+collisionPadding, Screen.height+collisionPadding, m_Camera.nearClipPlane+0.0001f));
        Vector3 _lowerLeft = m_Camera.ScreenToWorldPoint(new Vector3(-collisionPadding, -collisionPadding, m_Camera.nearClipPlane+0.0001f));
        Vector3 _lowerRight = m_Camera.ScreenToWorldPoint(new Vector3(Screen.width+collisionPadding, -collisionPadding, m_Camera.nearClipPlane+0.0001f));
        Vector3 _center = m_Camera.ScreenToWorldPoint(new Vector3(Screen.width/2.0f, Screen.height/2.0f, m_Camera.nearClipPlane+0.0001f));

        // draw debugging rays
        if (debug) {
            Debug.DrawLine(m_LookPos, _upperLeft, Color.green);
            Debug.DrawLine(m_LookPos, _upperRight, Color.green);
            Debug.DrawLine(m_LookPos, _lowerLeft, Color.green);
            Debug.DrawLine(m_LookPos, _lowerRight, Color.green);
            Debug.DrawLine(m_LookPos, _center, Color.green);
        }

        // check raycasts
        float _collisionOffset = 0;
        _collisionOffset = AggregateClipPointCollisionOffset(_center, _collisionOffset);
        _collisionOffset = AggregateClipPointCollisionOffset(_upperLeft, _collisionOffset);
        _collisionOffset = AggregateClipPointCollisionOffset(_upperRight, _collisionOffset);
        _collisionOffset = AggregateClipPointCollisionOffset(_lowerLeft, _collisionOffset);
        _collisionOffset = AggregateClipPointCollisionOffset(_lowerRight, _collisionOffset);
        if (_collisionOffset > 0)
            _collisionOffset += collisionBump;

        transform.position += transform.forward * _collisionOffset;
    }

    private float AggregateClipPointCollisionOffset(Vector3 _clip, float _maxDistance) {
        RaycastHit _hit;
        if (Physics.Raycast(m_LookPos, _clip - m_LookPos, out _hit, Vector3.Distance(_clip, m_LookPos), collisionLayer)) {
            if (debug) {
                Debug.DrawLine(_hit.point, _clip, Color.red);
            }
            float _distance = Vector3.Distance(_hit.point, _clip);
            if (_distance > _maxDistance) {
                _maxDistance = _distance;
            }
        }
        return _maxDistance;
    }
#endregion
#endregion
}