using UnityEngine;

namespace Chara
{
    public class Movements : MonoBehaviour
    {
        public float playerSpeed;
        public float friction;
        private Rigidbody2D _rb2d;
        private SpriteRenderer _spriteRenderer;

        // Start is called before the first frame update
        void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            Deceleration();

            if (Input.GetKey(KeyCode.D))
            {
                _spriteRenderer.flipX = false;
                _rb2d.AddForce(new Vector2(playerSpeed,0));
            }
            if (Input.GetKey(KeyCode.A))
            {
                _rb2d.AddForce(new Vector2(-playerSpeed,0));
                _spriteRenderer.flipX = true;
            }
            if (Input.GetKey(KeyCode.W))
            {
                _rb2d.AddForce(new Vector2(0,playerSpeed));
            }

            if (_rb2d.velocity.x > 5)
            {
                _rb2d.velocity = new Vector2(5,_rb2d.velocity.y);
            }

            if (_rb2d.velocity.x < -5)
            {
                _rb2d.velocity = new Vector2(-5, _rb2d.velocity.y);
            }
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
    }
}

