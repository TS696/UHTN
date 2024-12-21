using Sandbox.Common;
using UHTN.DomainAssets;
using UnityEngine;

namespace Sandbox.Sample_OpenDoor_DomainAsset
{
    [RequireComponent(typeof(DomainAssetAgent))]
    public class Sample_OpenDoor_DomainAsset : MonoBehaviour
    {
        public enum KeyType
        {
            Red,
            Blue,
            Yellow
        }

        [SerializeField]
        private Agent _agent;

        public Agent Agent => _agent;

        [SerializeField]
        private GameObject _door;

        public GameObject Door => _door;

        [SerializeField]
        private GameObject _redKey;

        [SerializeField]
        private GameObject _blueKey;

        [SerializeField]
        private GameObject _yellowKey;

        public GameObject GetKey(KeyType keyType)
        {
            switch (keyType)
            {
                case KeyType.Red:
                    return _redKey;
                case KeyType.Blue:
                    return _blueKey;
                case KeyType.Yellow:
                    return _yellowKey;
                default:
                    return null;
            }
        }

        private DomainAssetAgent _domainAssetAgent;

        void Awake()
        {
            _domainAssetAgent = GetComponent<DomainAssetAgent>();
        }

        void Start()
        {
            _domainAssetAgent.Prepare(this);
            _domainAssetAgent.Run();
        }

        private void OnGUI()
        {
            var pos = new Vector2(5, 5);
            var size = new Vector2(100, 20);
            var rect = new Rect(pos, size);

            var hasRed = GUI.Toggle(rect, _redKey.activeInHierarchy, "Red Key");
            rect.y += 30;
            var hasBlue = GUI.Toggle(rect, _blueKey.activeInHierarchy, "Blue Key");
            rect.y += 30;
            var hasYellow = GUI.Toggle(rect, _yellowKey.activeInHierarchy, "Yellow Key");

            if (hasRed != _redKey.activeInHierarchy)
            {
                _redKey.SetActive(hasRed);
            }

            if (hasBlue != _blueKey.activeInHierarchy)
            {
                _blueKey.SetActive(hasBlue);
            }

            if (hasYellow != _yellowKey.activeInHierarchy)
            {
                _yellowKey.SetActive(hasYellow);
            }
        }
    }
}
