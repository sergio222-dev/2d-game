using System;
using UnityEngine;

namespace Character
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
        public float gravity = 50f;
        public float jumpForce;
        private bool _isGrounded;
        private Animator _animator;

        // Start is called before the first frame update
        private void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();

            var position = _rb2d.position;
            _currentPosition = position;
            _prevPosition = position;
        }
        
        void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _isGrounded = true;
        }

        // Se ejecuta 50 veces por segundo
        void FixedUpdate()
        {
            float move = Input.GetAxis("Horizontal");

            if (move < 0)
            {
                _spriteRenderer.flipX = true;
            } else if (move > 0)
            {
                _spriteRenderer.flipX = false;
            }
            
            _rb2d.velocity = new Vector2(playerSpeed * move, _rb2d.velocity.y);

            RaycastHit2D hitInfo;
            hitInfo = Physics2D.Raycast(transform.position - new Vector3(0, _spriteRenderer.bounds.extents.y + 0.01f, 0), Vector2.down, 0.1f);

            if (hitInfo)
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }
            
            if (Input.GetKey(KeyCode.Space) && _isGrounded)
            {
                _rb2d.AddForce(Vector2.up);
            }
            
            Debug.DrawRay(transform.position - new Vector3(0, _spriteRenderer.bounds.extents.y + 0.01f, 0), Vector2.down * 0.1f, new Color(1f, 0.05f, 0.05f), 0.4f);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.D))
            {
                Move(new Vector2(playerSpeed, 0) * Time.deltaTime);
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                Move(new Vector2(-playerSpeed, 0) * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Jump();
            }
        }

        private void Jump()
        {
            _rb2d.velocity = Vector2.up;
            _rb2d.AddForce(Vector2.up);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                _rb2d.velocity = Vector2.zero;
            }
        }

        private void Deceleration()
        {
            var deceleration = 0f;

            if (_rb2d.velocity.x > 0)
            {
                deceleration = _rb2d.velocity.x - (friction * Time.deltaTime);
                if (deceleration < 0)
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
    }
}