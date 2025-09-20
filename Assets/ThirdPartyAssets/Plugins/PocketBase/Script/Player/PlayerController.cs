using UnityEngine;
using Cinemachine;

using System.Collections.Generic;

using Utilities;
using Timer = Utilities.Timer;





namespace Plateformer


{
    public class PlayerController : MonoBehaviour

    {
        [Header("Reference")]
        [SerializeField] Animator animator;
        [SerializeField] CinemachineFreeLook freeLookCam;

        [SerializeField] InputReader input;

        [SerializeField] Rigidbody rb;

        [SerializeField] GroundChecker groundChecker;

        [Header("Setting")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float rotationSpeed = 15f;
        [SerializeField] float smoothTime = 0.2f;

        [Header("Jump Setting")]

        [SerializeField] float jumpforce = 10f;
        [SerializeField] float jumpCooldown = 0.3f;

        [SerializeField] float gravityMultiplier = 1f;

        const float ZeroF = 0f;

        Transform mainCam;

        float currentSpeed;

        float velocity;


        static readonly int Speed = Animator.StringToHash("Speed");

        

        

        Vector3 movement;

        List<Timer> timers;
        CountdownTimer jumpCooldownTimer;

        public bool canMove = true ;

     


        void Start()
        {
            input.EnablePlayerActions();
        }

        void OnEnable()
        {
            input.Jump += OnJump;
        }

        void OnDisable()
        {
            input.Jump -= OnJump;
        }


        void OnJump(bool performed)
        {
          
            if (performed && groundChecker.isGrounded && !jumpCooldownTimer.IsRunning)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpforce, rb.linearVelocity.z);
                jumpCooldownTimer.Start();
                animator.SetTrigger("Jump");
            }
            else if (performed)
            {
                Debug.Log("Saut bloqué!");
            }
        }
         void Awake()
        {
            mainCam = Camera.main.transform;
            freeLookCam.Follow = transform;
            freeLookCam.LookAt = transform;
            freeLookCam.OnTargetObjectWarped(transform, positionDelta: transform.position - freeLookCam.transform.position - Vector3.forward);
            rb.freezeRotation = true;
            rb.useGravity = false;
            canMove = true;
   
            jumpCooldownTimer = new CountdownTimer(jumpCooldown); 
            timers = new List<Timer>(1) { jumpCooldownTimer };
        }

 void Update()
{
    movement = new Vector3(input.Direction.x, 0f, input.Direction.y);  
    HandleTimers();
    UpdateAnimator();
}
        private void HandleTimers()
        {
            if (timers != null)
            {
                foreach (var timer in timers)
                {
                    timer.Tick(Time.deltaTime);
                }
            }
        }

        void FixedUpdate()
        {
            HandleMouvement();
            ApplyCustomGravity();
        }


        private void UpdateAnimator()
        {
            animator.SetFloat(Speed, currentSpeed);
         
        }

        private void HandleMouvement()
        {
            if (!canMove)
            {
               
                return;
            }

            var adjustDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

            
            if (adjustDirection.magnitude > ZeroF)
            {
                HandleRotation(adjustDirection);
                HandleHorizontalMovement(adjustDirection);
                SmoothSpeed(adjustDirection.magnitude);
            }
            else
            {
                SmoothSpeed(ZeroF);
                rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
            }
        }

        void HandleHorizontalMovement(Vector3 adjustDirection)
        {
            Vector3 velocity = adjustDirection * moveSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
            
        }
        void HandleRotation(Vector3 adjustDirection)
        {
            var targetRotation = Quaternion.LookRotation(adjustDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.LookAt(transform.position + adjustDirection);
        }

        void SmoothSpeed(float value)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
            
        }

        void ApplyCustomGravity()
        {
            if (!groundChecker.isGrounded)
            {
                rb.linearVelocity += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
            }
        }
    }


}
