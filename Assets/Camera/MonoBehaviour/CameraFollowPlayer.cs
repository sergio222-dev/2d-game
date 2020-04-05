using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public GameObject follow;
    public Vector2 minCameraPos;
    public Vector2 maxCameraPos;
    public float smoothTime;

    private Vector2 velocity;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var position = follow.transform.position;
        /**
         * Suaviza el movimiento de la posicion de transform.position.x,
         * hacia follow.transform.position.x,
         * que lo gestione utilizando esta variable velocity.x
         * en el periodo de tiempo de smoothTime que indica el tiempo de smooth de la camara, esto se define en unity
         */
        var posX = Mathf.SmoothDamp(transform.position.x, follow.transform.position.x, ref velocity.x, smoothTime);
        var posY = Mathf.SmoothDamp(transform.position.y, follow.transform.position.y, ref velocity.y, smoothTime);

        // Clamps the given value between the given minimum float and maximum float values.  Returns the given value if it is within the min and max range.
        transform.position = new Vector3(
            Mathf.Clamp(posX, minCameraPos.x, maxCameraPos.x),
            Mathf.Clamp(posY, minCameraPos.y, maxCameraPos.y),
            transform.position.z);
    }
}
