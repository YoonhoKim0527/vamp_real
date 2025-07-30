using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;

namespace Vampire
{
    [RequireComponent(typeof(RectTransform))]
    public class TouchJoystick : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float joystickRadius = 100f;
        [SerializeField] private RectTransform joystick;          // 핸들
        [SerializeField] private RectTransform joystickBounds;    // 배경
        [SerializeField] private UnityEvent<Vector2> onJoystickMoved;
        [SerializeField] private UnityEvent onStartTouch;
        [SerializeField] private UnityEvent onEndTouch;

        [InputControl(layout = "Vector2")]
        [SerializeField] private string m_ControlPath;
        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        private bool beingTouched = false;
        private Vector2 initialTouchPosition;

        void Start()
        {
            joystick.gameObject.SetActive(false);
            joystickBounds.gameObject.SetActive(false);
        }

        void Update()
        {
            if (beingTouched && Time.timeScale > 0)
            {
                Vector2 currentTouch = Input.mousePosition;
                UpdateTouch(currentTouch);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Time.timeScale <= 0) return;

            beingTouched = true;
            initialTouchPosition = eventData.position;

            joystickBounds.position = initialTouchPosition;
            joystick.position = initialTouchPosition;

            joystickBounds.sizeDelta = Vector2.one * joystickRadius * 2f;

            joystick.gameObject.SetActive(true);
            joystickBounds.gameObject.SetActive(true);

            onStartTouch?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            EndTouch();
        }

        private void UpdateTouch(Vector2 screenPosition)
        {
            Vector2 delta = screenPosition - initialTouchPosition;
            Vector2 direction = delta.normalized;

            float distance = Mathf.Min(delta.magnitude, joystickRadius);
            Vector2 handlePosition = initialTouchPosition + direction * distance;

            joystick.position = handlePosition;

            onJoystickMoved?.Invoke(direction);
        }

        private void EndTouch()
        {
            joystick.gameObject.SetActive(false);
            joystickBounds.gameObject.SetActive(false);

            onJoystickMoved?.Invoke(Vector2.zero);
            onEndTouch?.Invoke();

            beingTouched = false;
        }
    }
}
