using System;
using System.Collections.Generic;
using System.Linq;
using Atoms;
using Misc.Collision;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Playables;


//Playercontroller expansion of Tarodev's base. Added custom collision, sliding and slopes.
namespace Player {
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// Right now it only contains movement and jumping, but it should be pretty easy to expand... I may even do it myself
    /// if there's enough interest. You can play and compete for best times here: https://tarodev.itch.io/
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/GqeHHnhHpz
    /// </summary>
    public class PlayerController : MonoBehaviour, IPlayerController {
        // Public for external hooks
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public Vector3 RawMovement { get; private set; }
        public bool Grounded => _groundCollision.Collision.HasCollision;

        private Vector3 _lastPosition;
        private Vector2 _currentVerticalVelocity;
        private float _inputVelocity;
        private Vector2 _currentDirection = Vector2.right;
        private Vector2 _currentSpeed = Vector2.zero;
        private Direction _playerDirection = Direction.Right;

        [SerializeField] private Vector2Variable _playerDirectionVariable;

        [SerializeField] private CustomCollisionVariable _groundCollision;
        [SerializeField] private FloatVariable _groundAngle;

        private IPlayerMovement _currentMovement;
        private IPlayerMovement _playerMovementWalking;
        private IPlayerMovement _playerMovementSliding;
        private PlayerJump _playerJump;

        enum Direction
        {
            Left = -1,
            Right = 1
        }

        // This is horrible, but for some reason colliders are not fully established when update starts...
        private bool _active;

        void Awake()
        {
            Invoke(nameof(Activate), 0.5f);
            _playerMovementWalking = GetComponent<PlayerWalking>();
            _playerMovementSliding = GetComponent<PlayerSliding>();
            _playerJump = GetComponent<PlayerJump>();
            _currentMovement = _playerMovementWalking;
        }
        void Activate() =>  _active = true;
        
        private void Update() {
            if(!_active) return;
            
            // Calculate velocity
            Velocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;

            GatherInput();
            CalculateMovement();
            CalculateMovementDirection();

            CalculateJumpApex(); // Affects fall speed, so calculate before gravity
            CalculateGravity(); // Vertical movement
            CalculateJump(); // Possibly overrides vertical

            CalculateSlide();
            CalculateVerticalCollision();
            CalculateHorizontalCollision();
                
            MoveCharacter(); // Actually perform the axis movement

        }

        [SerializeField] private int _horizontalCollisionRays = 3;
        [SerializeField] private float _horizontalCollisionOffset = 0.1f;

        private void CalculateHorizontalCollision()
        {
            Vector2 pos = transform.position;
            pos.y += (_characterBounds.center.y + _horizontalCollisionOffset) - _characterBounds.extents.y;
            var offset = new Vector2(0.0f, (_characterBounds.size.y - _horizontalCollisionOffset) / (_horizontalCollisionRays - 1));



            var distance = _characterBounds.extents.x;
            for (int i = 0; i < _horizontalCollisionRays; i++)
            {
                var rightCollision = CollisionHelper.CalculateHorizontalCollision(pos + (offset * i), Vector2.right, distance, _groundLayer);
                
                if (rightCollision.HasCollision)
                {
                    if (_currentSpeed.x > 0)
                        _currentSpeed.x = 0;
                    break;
                }
                
                var leftCollision = CollisionHelper.CalculateHorizontalCollision(pos + (offset * i), Vector2.left, distance, _groundLayer);

                if (leftCollision.HasCollision)
                {
                    if (_currentSpeed.x < 0)
                        _currentSpeed.x = 0;
                    break;
                }

            }
        }


        private void CalculateJump()
        {
            _currentVerticalVelocity = _playerJump.CalculateJump(_currentVerticalVelocity);
        }

        private void CalculateMovement()
        {
            var movement = _currentMovement.CalculateMovement(Input);
            _currentDirection = movement.normalized;
            _currentSpeed = movement;
        }

        private void CalculateMovementDirection()
        {
            if (Mathf.Abs(_currentDirection.x) > 0.01f)
            { 
                _playerDirectionVariable.SetValue(_currentDirection);
            } 
        }

        #region Gather Input

        private void GatherInput()
        {
            Input = new FrameInput {
                JumpDown = InputHandler.Jump.Started, // try with value Atom
                JumpUp = InputHandler.Jump.Canceled,
                X = InputHandler.Movement.Performed.x
            };
        }

        #endregion

        #region Collisions

        [Header("COLLISION")] [SerializeField] private Bounds _characterBounds;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private int _detectorCount = 3;
        [SerializeField] private float _detectionRayLength = 0.1f;
        [SerializeField] [Range(0.1f, 0.3f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground

        private float _groundCollisionVerticalPosition;
        private Vector2 _groundNormal;
        private bool _hasSlopeCollision;
        

        
        private void OnDrawGizmos() {
            // Bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + _characterBounds.center, _characterBounds.size);

            if (!Application.isPlaying) return;

            // Draw the future position. Handy for visualizing gravity
            Gizmos.color = Color.red;
            var move = new Vector3(_currentSpeed.x, _currentVerticalVelocity.y) * Time.deltaTime;
            Gizmos.DrawWireCube(transform.position + move, _characterBounds.size);
        }

        #endregion


        #region Walk


        #endregion

        #region Gravity

        [Header("GRAVITY")] [SerializeField] private float _fallClamp = -40f;
        [SerializeField] private float _minFallSpeed = 80f;
        [SerializeField] private float _maxFallSpeed = 120f;
        private float _fallSpeed;

        private void CalculateJumpApex()
        {
            if (!_groundCollision.Collision.HasCollision)
            {
                // Gets stronger the closer to the top of the jump
                var apexPoint = Mathf.InverseLerp(10, 0, Mathf.Abs(_currentVerticalVelocity.y));
                _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, apexPoint);;
            }
        }
        
        private void CalculateGravity()
        {
            if (_groundCollision.Collision.HasCollision) return;

            // Add downward force while ascending if we ended the jump early
            
            var fallSpeed = _playerJump.EndedJumpEarly && _currentVerticalVelocity.y > 0 ? _fallSpeed * _playerJump.JumpEndEarlyGravityModifier : _fallSpeed;
            // Fall
            _currentVerticalVelocity.y -= fallSpeed * Time.deltaTime;

            // Clamp
            if (_currentVerticalVelocity.y < _fallClamp) _currentVerticalVelocity.y = _fallClamp;
        }

        #endregion

        #region Collision


        private float _yCollisionPosition;
        private float _slopeBoost;
        private float _previousSlopeBoost;

        private CustomCollision _previousFrontCollision = new CustomCollision();
        private Vector2 _previousFrontNormal;
        private float _lastSetAngle;
        private void CalculateVerticalCollision()
        {
            _playerJump.SetLandingThisFrame(false);
            
            var charPosition = transform.position;
            charPosition.y -= _characterBounds.extents.y - 0.5f;
            var groundCollision = CollisionHelper.CalculateVerticalCollision(charPosition, 1, _groundLayer, Color.gray);
            
            if (_groundCollision.Collision.HasCollision && !groundCollision.HasCollision)
                _playerJump.SetTimeLeftGrounded(Time.time); // Only trigger when first leaving
            else if (!_groundCollision.Collision.HasCollision && groundCollision.HasCollision)
            {
                _playerJump.SetCoyoteUsable(true); // Only trigger when first touching
                _playerJump.SetLandingThisFrame(true);
                _currentVerticalVelocity.y = 0;
            }

            _groundCollision.SetCollision(groundCollision);

            if (groundCollision.HasCollision)
            {
                
                _previousSlopeBoost = _slopeBoost;

                _yCollisionPosition = groundCollision.CollisionPoint.y + _characterBounds.extents.y - _characterBounds.center.y;

                var offset = _characterBounds.extents.x * 0.5f * _playerDirectionVariable.Value.x;

                var frontGroundCollision = CollisionHelper.CalculateVerticalCollision(charPosition + new Vector3(offset, 0, 0), 1.5f, _groundLayer, Color.cyan);
                var backGroundCollision = CollisionHelper.CalculateVerticalCollision(charPosition + new Vector3(-offset, 0, 0), 1.5f, _groundLayer, Color.red);

                _slopeBoost = CollisionHelper.SlopeCalculation(frontGroundCollision, backGroundCollision);

                var angle = Vector2.SignedAngle(_previousFrontCollision.CollisionNormal, frontGroundCollision.CollisionNormal) * _playerDirectionVariable.Value;
                if (angle.x < -0.1f && _previousFrontCollision.CollisionNormal != Vector2.up)
                {
                    Debug.Log("We're flying!");
                    _currentVerticalVelocity.y += _previousSlopeBoost * 40;
                }

                if (_lastSetAngle > 0.1f && !frontGroundCollision.HasCollision && _previousFrontCollision.HasCollision )
                {
                        // Fly here
                        Debug.Log("We're flying Here too!");
                        Debug.Log("We're flying Here too!");
                        _currentVerticalVelocity.y += _previousSlopeBoost * 40;
                }
                if (angle.x != 0)
                {
                    _lastSetAngle = angle.x;
                }
                Debug.Log(_lastSetAngle);

                

                _previousFrontCollision = frontGroundCollision;
                _groundAngle.SetValue(_slopeBoost * 0.1f * -_playerDirectionVariable.Value.x);

            }
            else
            {
                _lastSetAngle = 0;
            }
        }

        #endregion

        #region Move

        // We cast our bounds before moving to avoid future collisions
        private void MoveCharacter() {
            var pos = transform.position;

            RawMovement = _currentSpeed + _currentVerticalVelocity;
            
            var move = RawMovement * Time.deltaTime;
            
            pos += move;
            
            if (_groundCollision.Collision.HasCollision && _currentVerticalVelocity.y < 0.1f)
            {
                var newPos = pos;
                newPos.y = _yCollisionPosition;
                pos = newPos;
            }

            transform.position = pos;
        }

        #endregion
        

        #region Sliding

        [Header("SLIDING")] [SerializeField] private bool _enableSlide;
        private bool _isSliding;
        private Vector2 _currentSlideDirection;
        private float _currentSlideVelocity;
        private float _initialSlideVelocity;

        private void CalculateSlide()
        {
            if (InputHandler.Slide.Started)
            {
                SwitchToState(_playerMovementSliding);
            }

            if (InputHandler.Slide.Canceled)
            {
                SwitchToState(_playerMovementWalking);
            }
        }

        private void SwitchToState(IPlayerMovement playerMovement)
        {
            _currentMovement.Reset();

            _currentMovement = playerMovement;
            _currentMovement.SetSpeed(_currentSpeed);
        }

        #endregion
    }
}