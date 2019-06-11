using UnityEngine;

namespace Tengio
{
    public abstract class PoolElement : MonoBehaviour
    {

        public abstract void Initialize();
        public abstract void Activate();
        public abstract void Deactivate();
        public abstract bool IsAvailable();
    }
}
