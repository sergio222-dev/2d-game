using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms;

namespace Character
{
    public class Movements : UnityEngine.MonoBehaviour
    {
        public float horizontalDisplacement = 0.1f;
        public int gravity = 50;
        public int gravityFactor = 4;
        public int maxFallGravityMultiplier = 100;
        public Camera cameraSelected;
        public float dirDrawRay = 1f;
        [FormerlySerializedAs("GroundDetectDistance")] public float groundDetectDistance = 0.1f;

        // Objects
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _box;
        
        // Accumulators
        private Vector2 _velocity;
        private Vector2 _fallingVelocity;
        private Vector2 _direction;
        private Vector2 _nextMovement = Vector2.zero;
        private Vector2 _prevPosition;
        private Vector2 _currentPosition;
        private RaycastHit2D[] _hitsBuffer = new RaycastHit2D[3];
        private bool _isGrounded;
        private float _cumulativeFallingSpeed;
        private float _m_gravity;
        private float _m_maxFallVelocity;
        private ContactFilter2D _filter;

        public void Awake()
        {

            
        }
        
        void Start()
        {
            
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _box = GetComponent<BoxCollider2D>();

            _isGrounded = false;
            _m_gravity = Convert.ToSingle(gravity / Math.Pow(10, gravityFactor));
            _m_maxFallVelocity = _m_gravity * maxFallGravityMultiplier;

            _filter = new ContactFilter2D {layerMask = LayerMask.GetMask("Ground"), useLayerMask = true};

            _currentPosition = _prevPosition = _rigidbody2D.position;
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.D))
            {
                Move(new Vector2(horizontalDisplacement, 0));
                _spriteRenderer.flipX = false;
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                Move(new Vector2(-horizontalDisplacement, 0));
                _spriteRenderer.flipX = true;
            }

            if (Input.GetKey(KeyCode.Mouse1))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = cameraSelected.ScreenToWorldPoint(mousePosition);
                _rigidbody2D.position = worldPosition;
                ResetGravity();
                _isGrounded = false;
            }
            
            if (Input.GetKey(KeyCode.Mouse0))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = cameraSelected.ScreenToWorldPoint(mousePosition);
                _rigidbody2D.position = worldPosition;
            }
            
        }

        // Se ejecuta 50 veces por segundo
        public void FixedUpdate()
        {
            CheckIfTouchTheGround();
            if (!_isGrounded)
            {
                AccelerationByFalling();
            }
            
            _prevPosition = _rigidbody2D.position;
            _currentPosition = _prevPosition + _nextMovement;
            
            if (!_isGrounded) CheckAndFixIfCollisionWithGround();
            
            _rigidbody2D.MovePosition(_currentPosition);
            _nextMovement = Vector2.zero;
        }

        private void ResetGravity()
        {
            _m_gravity = Convert.ToSingle(gravity / Math.Pow(10, gravityFactor));
            _m_maxFallVelocity = _m_gravity * maxFallGravityMultiplier;
            _cumulativeFallingSpeed = 0;
        }

        /**
         * Calcula los origenes de los 3 rayos desde abajo del personaje
         */
        private Vector2[] CalculateOriginRayFromBottom()
        {
            // el tamaño del collider
            var size = _box.size;
            // array para guardar los origins position de los rayos
            var originArray = new Vector2[3];
            // escalado
            var scale = transform.localScale;

            //calcular el origen de los tres rayos
            var position = _currentPosition;
            var offset = _box.offset;
            var rayCastOrigin = position + (offset * scale);
            var rayCastFromBottom = rayCastOrigin + size.y * 0.5f * scale.y  * Vector2.down;

            originArray[0] = rayCastFromBottom + size.x * scale.x * 0.5f * Vector2.left;
            originArray[1] = rayCastFromBottom;
            originArray[2] = rayCastFromBottom + size.x * scale.x * 0.5f * Vector2.right;

            return originArray;
        }

        private void CheckIfTouchTheGround()
        {
            var rayOrigins = CalculateOriginRayFromBottom();
            var distanceRay = groundDetectDistance * 2f;
            var count = 0;
            foreach (var t in rayOrigins)
            {
                count += Physics2D.Raycast(t, Vector2.down, _filter, _hitsBuffer, distanceRay);
            }

            _isGrounded = (count > 0);
            if (_isGrounded)
            {
                _nextMovement.y = 0f;
                ResetGravity();
            }
        }

        private void CheckAndFixIfCollisionWithGround()
        {
            var rayOrigins = CalculateOriginRayFromBottom();
            var distanceRay = _nextMovement.y;
            
            var count = 0;
            for (var i = 0; i < rayOrigins.Length; i++)
            {
                count += Physics2D.Raycast(rayOrigins[i], Vector2.down, _filter, _hitsBuffer, distanceRay);
            }

            if (count == 0) return;

            // si hubo colision reposicionar al personaje sobre el suelo
            var point = CalculateMostYPoint(_hitsBuffer);

            // reposicionamiento
            var localScale = transform.localScale;
            point += _box.size.y * localScale.y * 0.5f * Vector2.up;
            var positionX = _currentPosition.x;
            var positionY = point.y + _box.offset.y * localScale.y + groundDetectDistance;
            var newPosition = new Vector2(positionX, positionY);
            _currentPosition = newPosition;
            ResetGravity();
        }

        private void AccelerationByFalling()
        {
            // sumamos a la velocidad de caida acumulativa y nos fijamos que no sea mayor a la permitida
            if (! ((_cumulativeFallingSpeed + _m_gravity) > _m_maxFallVelocity))
            {
                _cumulativeFallingSpeed += _m_gravity;
            }
            
            _fallingVelocity = new Vector2(0, -_cumulativeFallingSpeed);
            
            Move(_fallingVelocity);
        }

        private Vector2 CalculateMostYPoint(RaycastHit2D[] hits)
        {
            var hitsFiltered = hits.Where((h => h.collider != null));
            var hitReduced = hitsFiltered.Aggregate((acc, hit) => acc.point.y > hit.point.y ? acc : hit);
            return hitReduced.point;
        }

        private void Move(Vector2 movement)
        {
            _nextMovement += movement;
        }
    }
}