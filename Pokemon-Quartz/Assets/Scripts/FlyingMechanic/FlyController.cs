using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Test implemention for controlling a pokemon while flying.
public class FlyController : MonoBehaviour
{
    [SerializeField] GameObject ground;

    // Handles physics and movement of pokemon.
    public float forwardSpeed = 25f, hoverSpeed = 5f;
    private float activeForwardSpeed, activeHoverSpeed;
    private float forwardAcceleration = 2.5f, hoverAcceleration = 2f;

    // Handles the camera movement of our plane.
    public float lookRateSpeed = 90f;
    private Vector2 lookInput, screenCenter, mouseDistance;

    private float rollInput;
    public float rollSpeed = 90f, rollAcceleration = 3.5f;

    // Start is called before the first frame update
    void Start()
    {
        screenCenter.x = Screen.width * .5f;
        screenCenter.y = Screen.height * .5f;

        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        lookInput.x = Input.mousePosition.x;
        lookInput.y = Input.mousePosition.y;

        mouseDistance.x = (lookInput.x - screenCenter.x) / screenCenter.x;
        mouseDistance.y = (lookInput.y - screenCenter.y) / screenCenter.y;

        // Limits turn feature.
        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

        rollInput = Mathf.Lerp(rollInput, (-1)*Input.GetAxisRaw("Horizontal"), rollAcceleration * Time.deltaTime);

        //Rotate pokemon based on mouse movement.
        transform.Rotate(-mouseDistance.y * lookRateSpeed * Time.deltaTime, mouseDistance.x * lookRateSpeed * Time.deltaTime, rollInput * rollSpeed *Time.deltaTime, Space.Self);

        // Handle acceleration and movement.
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, Input.GetAxisRaw("Vertical") * forwardSpeed, forwardAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, Input.GetAxisRaw("Hover") * hoverSpeed, hoverAcceleration * Time.deltaTime);

        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;
        transform.position += (transform.up * activeHoverSpeed * Time.deltaTime);

        //(transform.right * activeStrafeSpeed * Time.deltaTime)
    }
}
