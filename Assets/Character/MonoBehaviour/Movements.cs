using System;
using UnityEngine;

namespace Character.MonoBehaviour
{
    public class Movements : UnityEngine.MonoBehaviour
    {
        public float horizontalDisplacement = 0.1f;
        public int gravity = 50;
        public int gravityFactor = 4;
        public int maxFallGravityMultiplier = 100;
        public Camera cameraSelected;
        public float dirDrawRay = 1f;

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
        private bool _isGrounded;
        private float _cumulativeFallingSpeed;
        private float _m_gravity;
        private float _m_maxFallVelocity;

        public void Awake()
        {
            _isGrounded = false;
            _m_gravity = Convert.ToSingle(gravity / Math.Pow(10, gravityFactor));
            _m_maxFallVelocity = _m_gravity * maxFallGravityMultiplier;

            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _box = GetComponent<BoxCollider2D>();
            
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

        public void FixedUpdate()
        {
            Falling();
            DetectGround();
            if (_isGrounded) _nextMovement = Vector2.zero;
            _prevPosition = _rigidbody2D.position;
            _currentPosition = _prevPosition + _nextMovement;

            _rigidbody2D.MovePosition(_currentPosition);
            _nextMovement = Vector2.zero;
        }

        private void ResetGravity()
        {
            _m_gravity = Convert.ToSingle(gravity / Math.Pow(10, gravityFactor));
            _m_maxFallVelocity = _m_gravity * maxFallGravityMultiplier;
            _cumulativeFallingSpeed = 0;
        }

        private void Falling()
        {
            // Vemos is estamos parado en el suelo
            if (_isGrounded) return;
            
            // sumamos a la velocidad de caida acumulativa y nos fijamos que no sea mayor a la permitida
            if (! ((_cumulativeFallingSpeed + _m_gravity) > _m_maxFallVelocity))
            {
                _cumulativeFallingSpeed += _m_gravity;
            }
            
            _fallingVelocity = new Vector2(0, -_cumulativeFallingSpeed);
            
            Move(_fallingVelocity);
        }

        private void DetectGround()
        {
            // border cast ray
            var ray1 = _rigidbody2D.position + _box.offset;
            ray1.y -= _box.bounds.extents.y;
            
            Debug.DrawRay(ray1, _nextMovement, Color.red);
            var hit = Physics2D.Raycast(
                ray1,
                _nextMovement,
                _nextMovement.y, 
                LayerMask.GetMask("Ground")
                );
            
            // La reposicion del personaje por el choque contra un ground deberia estar encapsulada en otra parte
            
            if (hit.collider)
            {
                Debug.Log(_nextMovement);
                _isGrounded = true;
                var _point = hit.point;
                // Busca poner el rigid body sobre el punto de colision
                _point.y += _box.bounds.extents.y;
                // Busca centrar el frame de manera horizontal
                _point.x -= _box.offset.x;
                _rigidbody2D.position = _point;
            }
        }

        private void Move(Vector2 movement)
        {
            _nextMovement += movement;
        }
    }
}

