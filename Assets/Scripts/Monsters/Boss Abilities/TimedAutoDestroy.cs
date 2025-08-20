using UnityEngine;

namespace Vampire
{
    public class TimedAutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.6f;
        private void OnEnable() { Destroy(gameObject, lifetime); }
    }
}
