using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TestPhycsic : MonoBehaviour
{
    public float moveSpeed = 5f; // The movement speed of the character
    public float jumpForce = 7f; // The force applied when the character jumps
    public float friction = 6f; // The friction factor (higher values mean more friction)
    public float acceleration = 10f; // The acceleration factor

    [SerializeField] private float gravity = .2f;
    [SerializeField] private float floorDrag = .2f;
    [SerializeField] private float airDrag = .05f;


    private CharacterController controller;
    private bool isGrounded;

    [SerializeField] private float maxMovementSpeed = 2;
    [SerializeField] private float test = 2;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private float drag;
    public Vector3 velocity = Vector3.zero;
    private float yVelocity = 0;

    [SerializeField] private LayerMask groundMask;

    private Vector3 GroundCheckPosition => transform.position - (controller.height / 2 * Vector3.up);

    private void Update()
    {
        // Check if the character is grounded
        isGrounded = controller.isGrounded;

        // Process movement input
        float horizontalInput = Mathf.Round(Input.GetAxis("Horizontal"));
        float verticalInput = Mathf.Round(Input.GetAxis("Vertical"));

        test = Mathf.Round(horizontalInput);

        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;

        velocity += direction * moveSpeed;

        if (velocity.magnitude > maxMovementSpeed)
        {
            velocity -= velocity.normalized * (velocity.magnitude - maxMovementSpeed);
        }

        if (!CheckGrounded())
        {
            yVelocity += -gravity;
            drag = airDrag;
        }
        else
        {
            yVelocity = 0;
            velocity.y = 0;
            drag = floorDrag;
        }

        if (Input.GetKeyDown(KeyCode.Space) && CheckGrounded())
        {
            yVelocity += jumpForce;
        }

        if (direction.magnitude < 1)
        {
            velocity -= velocity * drag;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            velocity += new Vector3(5,8,5);
        }
        print(CheckGrounded());

        controller.Move((velocity + (Vector3.up * yVelocity)) * Time.deltaTime);
    }

    private bool CheckGrounded()
    {
        // Perform a raycast downward to check for ground collision
        return Physics.Raycast(GroundCheckPosition, Vector3.down, .1f, groundMask);
    }
}