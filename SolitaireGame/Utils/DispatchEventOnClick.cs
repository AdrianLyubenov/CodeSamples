using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class DispatchEventOnClick : MonoBehaviour
    {
        public string eventName;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            EventManager.Broadcast(new GenericEvent(eventName));
        }
    }
