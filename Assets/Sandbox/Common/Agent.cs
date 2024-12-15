using UnityEngine;

namespace Sandbox.Common
{
    [RequireComponent(typeof(CharacterController))]
    public class Agent : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 3f;

        public bool IsBusy => _destination.HasValue;

        private CharacterController _characterController;
        private Vector3? _destination;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_destination.HasValue)
            {
                if (IsReached(_destination.Value))
                {
                    _destination = null;
                    return;
                }

                MoveTo(_destination.Value);
            }
        }

        public void SetDestination(Vector3? destination)
        {
            _destination = destination;
        }

        private bool IsReached(Vector3 to)
        {
            var direction = to - transform.position;
            direction.y = 0;
            return direction.magnitude < 1.2f;
        }

        private void MoveTo(Vector3 to)
        {
            var direction = to - transform.position;
            direction.y = 0;
            _characterController.Move(direction.normalized * _speed * Time.deltaTime);
        }
    }
}
