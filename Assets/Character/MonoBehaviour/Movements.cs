using System;
using UnityEngine;

namespace Chara
{
    public class Movements : MonoBehaviour
    {
        public float playerSpeed;
        public float friction;
        private Rigidbody2D _rb2d;
        private SpriteRenderer _spriteRenderer;
        private Vector2 _xMovement; //next movement
        private Vector2 _prevPosition;
        private Vector2 _currentPosition;
        private Vector2 _velocity;

        // Start is called before the first frame update
        void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            var position = _rb2d.position;
            _currentPosition = position;
            _prevPosition = position;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.D))
            {
                Move(new Vector2(playerSpeed, 0) * Time.deltaTime);
                _spriteRenderer.flipX = false;
            }
            if (Input.GetKey(KeyCode.A))
            {
                Move(new Vector2(-playerSpeed, 0) * Time.deltaTime);
                _spriteRenderer.flipX = true;
            }
            
            // Deceleration();

            // if (Input.GetKey(KeyCode.D))
            // {
            //     _spriteRenderer.flipX = false;
            //     _rb2d.AddForce(new Vector2(playerSpeed,0));
            // }
            // if (Input.GetKey(KeyCode.A))
            // {
            //     _rb2d.AddForce(new Vector2(-playerSpeed,0));
            //     _spriteRenderer.flipX = true;
            // }
            // if (Input.GetKey(KeyCode.W))
            // {
            //     _rb2d.AddForce(new Vector2(0,playerSpeed));
            // }
            //
            // if (_rb2d.velocity.x > 5)
            // {
            //     _rb2d.velocity = new Vector2(5,_rb2d.velocity.y);
            // }
            //
            // if (_rb2d.velocity.x < -5)
            // {
            //     _rb2d.velocity = new Vector2(-5, _rb2d.velocity.y);
            // }
        }

        private void Deceleration()
        {
            var deceleration = 0f;

            if(_rb2d.velocity.x > 0)
            {
                deceleration = _rb2d.velocity.x - (friction * Time.deltaTime);
                if(deceleration < 0)
                {
                    deceleration = 0;
                }
                _rb2d.velocity = new Vector2(deceleration, _rb2d.velocity.y);
            }

            if (_rb2d.velocity.x < 0)
            {
                deceleration = _rb2d.velocity.x + (friction * Time.deltaTime);
                if (deceleration > 0)
                {
                    deceleration = 0;
                }
                _rb2d.velocity = new Vector2(deceleration, _rb2d.velocity.y);
            }
        }

        private void Move(Vector2 movement)
        {
            _xMovement += movement;
        }

        // Se ejecuta 50 veces por segundo
        private void FixedUpdate()
        {
            _prevPosition = _rb2d.position;
            _currentPosition = _prevPosition + _xMovement;
            _velocity = (_currentPosition - _prevPosition) / Time.deltaTime;
            // Debug.Log(_velocity.ToString());
            
            _rb2d.MovePosition(_currentPosition);
            _xMovement = Vector2.zero;
        }
    }
}

