using UnityEngine;

namespace Chara
{
    public class Movements : MonoBehaviour
    {
        public float playerSpeed;
        private Rigidbody2D _rb2d;

        // Start is called before the first frame update
        void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.D))
            {
                _rb2d.AddForce(new Vector2(playerSpeed,0));
            }
            if (Input.GetKey(KeyCode.A))
            {
                _rb2d.AddForce(new Vector2(-playerSpeed,0));
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
    }
}

