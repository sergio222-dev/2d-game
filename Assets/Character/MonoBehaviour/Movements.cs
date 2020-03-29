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
        
        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _isGrounded = true;
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
            // Ver si el personaje esta el suelo
            _isGrounded = IsGround(groundDetectDistance);
            
            // Si no esta en el suelo, este empezara a caer
            if (!_isGrounded)
            {
                // Falling solo agrega la velocidad de caida al movimiento del personaje
                Falling();
                DetectCollisionWithGround();
            }
            
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

        /**
         * Agrega velocidad al movimiento del personaje
         */
        private void Falling()
        {
            // sumamos a la velocidad de caida acumulativa y nos fijamos que no sea mayor a la permitida
            if (! ((_cumulativeFallingSpeed + _m_gravity) > _m_maxFallVelocity))
            {
                _cumulativeFallingSpeed += _m_gravity;
            }
            
            _fallingVelocity = new Vector2(0, -_cumulativeFallingSpeed);
            
            Move(_fallingVelocity);
        }

        private void DetectCollisionWithGround()
        {
            var isCollision = IsGround(_nextMovement.y);
            // si no hubo colision salir del bucle
            if (!isCollision) return;
            
            // si hubo colision reposicionar al personaje sobre el suelo
            var point = CalculateMostYPoint(_hitsBuffer);
            
            // reposicionamiento
            Debug.Log("Point: " + point);
            var localScale = transform.localScale;
            point += _box.size.y * localScale.y * 0.5f * Vector2.up;

            var positionX = _rigidbody2D.position.x;
            var positionY = point.y + _box.offset.y * localScale.y + groundDetectDistance;
            Debug.Log("Posicion nueva X: " + positionX);
            Debug.Log("Posicion nueva Y: " + positionY);
            var newPosition = new Vector2(positionX, positionY);
            _rigidbody2D.MovePosition(newPosition);
            _nextMovement.y = 0f;
            ResetGravity();

        }

//        private void DetectGround()
//        {
//            if (_isGrounded) return;
//            
//            //calcular el origen de los tres rayos
//            var origins = CalculateOriginRay();
//            
//            foreach (var origin in origins)
//            {
//                Debug.DrawRay(
//                    origin,
//                    _nextMovement,
//                    Color.red);
//                
//                
//                var hit = Physics2D.Raycast(
//                    origin,
//                    Vector2.down,
//                    _nextMovement.y,
//                    LayerMask.GetMask("Ground")
//                );
//
//                if (!hit.collider) continue;
//                _isGrounded = true;
//                var point = hit.point;
//                Debug.Log("Punto de choque " + point);
//                // Busca poner el rigid body sobre el punto de colision
//                var localScale = transform.localScale;
//                point += _box.size.y * localScale.y * 0.5f * Vector2.up;
//               
//                Debug.Log("Punto corregido " + point);
//                _rigidbody2D.position = new Vector2(_rigidbody2D.position.x , point.y - _box.offset.y * localScale.y );
//                Debug.Log("Nueva posicion " + _rigidbody2D.position);
//                _nextMovement.y = 0f;
//                break;
//
//            }
//        }

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
            var position = _rigidbody2D.position;
            var offset = _box.offset;
            var rayCastOrigin = position + (offset * scale);
            var rayCastFromBottom = rayCastOrigin + size.y * 0.5f * scale.y  * Vector2.down;

            originArray[0] = rayCastFromBottom + size.x * scale.x * 0.5f * Vector2.left;
            originArray[1] = rayCastFromBottom;
            originArray[2] = rayCastFromBottom + size.x * scale.x * 0.5f * Vector2.right;

            return originArray;
        }
        
        /**
         * Castea los rayos desde abajo a una distancia fijada y un layer dado
         */
        private RaycastHit2D[] CastRaysFromBottom(float distance, LayerMask layer)
        {
            var rayHits = new RaycastHit2D[3];
            var rayOrigins = CalculateOriginRayFromBottom();
            for (var i = 0; i < rayHits.Length ; i++)
            {
                Debug.DrawRay(rayOrigins[i], _nextMovement, Color.red);
                var hit = Physics2D.Raycast(rayOrigins[i], Vector2.down, distance, layer);
                rayHits[i] = hit;
            }

            return rayHits;
        }

        /**
         * Detecta si el personaje esta tocando el suelo en sus 3 rayos desde la base.
         */
        private bool IsGround(float distance)
        {
            var hits = CastRaysFromBottom(distance, LayerMask.GetMask("Ground"));
            // Guarda los hits en un buffer
            _hitsBuffer = hits;
            return hits.Aggregate(false, (acc, hit) =>
            {
                var isHit = hit.collider != null;
                return (acc || isHit);
            });
        }

        private Vector2 CalculateMostYPoint(RaycastHit2D[] hits)
        {
            return hits.Aggregate(new Vector2(0, -Mathf.Infinity), (acc, hit) =>
            {
                return acc.y > hit.point.y ? acc : hit.point;
            });
        }

        private void Move(Vector2 movement)
        {
            _nextMovement += movement;
        }
    }
}