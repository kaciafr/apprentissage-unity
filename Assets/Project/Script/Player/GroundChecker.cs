using UnityEngine;

namespace Plateformer
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] float checkDistance = 1.2f;
        [SerializeField] LayerMask groundLayer = -1;
        [SerializeField] float checkRadius = 0.4f;
        
        public bool isGrounded { get; private set; }

        void FixedUpdate()
        {
            Vector3 sphereCenter = transform.position - Vector3.up * 0.1f;
            isGrounded = Physics.CheckSphere(sphereCenter, checkRadius, groundLayer);
            
            // Debug visuel
            Debug.DrawRay(transform.position, Vector3.down * checkDistance, isGrounded ? Color.green : Color.red);
        }
    }
}