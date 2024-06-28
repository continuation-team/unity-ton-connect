using UnityEngine;

namespace UnitonConnect.Core.Demo
{
    public abstract class TestBasePanel : MonoBehaviour
    {
        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
    }
}