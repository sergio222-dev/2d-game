﻿using System;
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
        public bool debugRayGround = false;
        public bool debugRayFalling = false;
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
                Move(new Vector2(horizontalDisplacement * Time.deltaTime, 0));
                _spriteRenderer.flipX = false;
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                Move(new Vector2(-horizontalDisplacement * Time.deltaTime, 0));
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
            /**
             * Se chequea que el personaje este en el suelo
             */
            CheckIfTouchTheGround();
            if (!_isGrounded)
            {
                /**
                 * Si el personaje no esta en el suelo se aplica una acceleracion horizontal al nextMovement (gravedad)
                 */
                AccelerationByFalling();
            }
            
            /**
             * Calculamos la nueva posicion en base a la actual y el next movement
             */
            _prevPosition = _rigidbody2D.position;
            _currentPosition = _prevPosition + _nextMovement;
            
            /**
             * Si el personaje no sigue en el suelo se checkea si colisionara con un piso y se corrige su posicion(currentPosition)
             */
            if (!_isGrounded) CheckAndFixIfCollisionWithGround();
            
            /**
             * se actualiza la posicion del personaje
             */
            _rigidbody2D.MovePosition(_currentPosition);
            /**
             * se borra la velocidad actual para recalcular en el siguiente frame
             */
            _nextMovement = Vector2.zero;
        }

        /**
         * Se encarga de reiniciar la aceleracion por gravedad la gravedad
         */
        private void ResetGravity()
        {
            _m_gravity = Convert.ToSingle(gravity / Math.Pow(10, gravityFactor));
            _m_maxFallVelocity = _m_gravity * maxFallGravityMultiplier;
            _cumulativeFallingSpeed = 0;
        }

        /**
         * Calcula los origenes de los 3 rayos desde la parte inferior de la caja de colisiones del personaje
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
            /**
             * Aqui se toma la posicion del personaje y se le suma el offset( corrimiento de la caja de colisiones
             * con respecto al sprite) y se multiplica por el escalado (es necesario ya que la caja de colisiones
             * usa el eje de ordenadas local al sprite)
             */
            var rayCastOrigin = position + (offset * scale);
            
            /**
             * Se le suma la mita del tamaño de la caja de colisiones, se multiplica para el escalado y el Vector.down
             * es para que el resultado sea negativo (0,-1)
             */
            var rayCastFromBottom = rayCastOrigin + size.y * 0.5f * scale.y  * Vector2.down;

            originArray[0] = rayCastFromBottom + size.x * scale.x * 0.5f * Vector2.left;
            originArray[1] = rayCastFromBottom;
            originArray[2] = rayCastFromBottom + size.x * scale.x * 0.5f * Vector2.right;

            return originArray;
        }

        /**
         * Este metodo revisa si el personaje esta tocando el suelo
         */
        private void CheckIfTouchTheGround()
        {
            var rayOrigins = CalculateOriginRayFromBottom();
            // multiplicamos por 2 para que sea mas preciso la deteccion del suelo
            var distanceRay = groundDetectDistance * 2f;
            var count = 0;
            foreach (var t in rayOrigins)
            {
                /**
                 * Se lanzan los rayos desde los origines, todas las colisiones se guardan en _hitsBuffer y son filtrados
                 * por _filter
                 */
                if (debugRayGround) Debug.DrawRay(t, Vector2.down * distanceRay, Color.red);
                count += Physics2D.Raycast(t, Vector2.down, _filter, _hitsBuffer, distanceRay);
            }

            // En caso de haber al menos una colision se considera que esta en el suelo y se resetea la acceleracion
            // por gravedad
            _isGrounded = (count > 0);
            if (_isGrounded)
            {
                ResetGravity();
            }
        }

        /***
         * Este metodo se encarga de revisar si el personaje chocara con el suelo en el siguiente frame y en caso de
         * ser asi corregira su posicion
         */
        private void CheckAndFixIfCollisionWithGround()
        {
            var rayOrigins = CalculateOriginRayFromBottom();
            var distanceRay = _nextMovement.y;
            
            var count = 0;
            for (var i = 0; i < rayOrigins.Length; i++)
            {
                if(debugRayFalling) Debug.DrawRay(rayOrigins[i], Vector2.down * distanceRay, Color.yellow);
                count += Physics2D.Raycast(rayOrigins[i], Vector2.down, _filter, _hitsBuffer, distanceRay);
            }

            // En caso de no haber ninguna colision se saldra de este metodo y se continuara normalmente
            if (count == 0) return;

            // si hubo colision se busca la coordenada con la Y que este mas arriba para hubicar al personaje
            var point = CalculateMostYPoint(_hitsBuffer);

            // reposicionamiento
            var localScale = transform.localScale;
            /**
             * Al punto de colision se le suma la mitad del tamaño de la caja de colisiones, el vector.up es para que
             * sea positivo (0,1)
             */
            point += _box.size.y * localScale.y * 0.5f * Vector2.up;
            /**
             * Se calculan las nuevas posiciones
             */
            var positionX = _currentPosition.x;
            /**
             * a la nueva posicion se le resta el corrimiento de la caja (offset) y se lo multiplica por el escalado
             * ya que la caja tiene coordenadas locales, se le suma la distancia al suelo para que en un siguiente
             * frame el metodo de deteccion del suelo pueda detectarlo facilmente
             */
            var positionY = point.y - _box.offset.y * localScale.y + groundDetectDistance;
            var newPosition = new Vector2(positionX, positionY);
            /**
             * Es importante que la nueva posicion sea asignara a _currentPosition ya que el esta se encarga de actualizar
             * la posicion del rigidbody
             */
            _currentPosition = newPosition;
            // Resetamos la gravedad para no tener mas aceleracion
            ResetGravity();
        }

        /**
         * Ese metodo se encarga de acelerar verticalmente(hacia abajo) al personaje
         */
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

        /**
         * Este metodo devuele la colision con la Y mas elevada
         */
        private Vector2 CalculateMostYPoint(RaycastHit2D[] hits)
        {
            var hitsFiltered = hits.Where((h => h.collider != null));
            var hitReduced = hitsFiltered.Aggregate((acc, hit) => acc.point.y > hit.point.y ? acc : hit);
            return hitReduced.point;
        }

        /*
         * actualiza el proximo movimiento del personaje
         */
        private void Move(Vector2 movement)
        {
            _nextMovement += movement;
        }
    }
}