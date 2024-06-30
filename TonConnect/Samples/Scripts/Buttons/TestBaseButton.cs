using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UnitonConnect.Core.Demo
{
    public abstract class TestBaseButton : MonoBehaviour
    {
        [SerializeField, Space] protected Button _target;

        private void OnEnable()
        {
            _target.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _target.onClick.RemoveListener(OnClick);
        }

        public abstract void OnClick();

        public void SetListener(UnityAction action)
        {
            _target.onClick.AddListener(action);
        }

        public void RemoveListeners()
        {
            _target.onClick.RemoveAllListeners();
        }
    }
}