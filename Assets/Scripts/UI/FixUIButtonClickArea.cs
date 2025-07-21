using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    [RequireComponent(typeof(RectTransform))]
    public class FixUIButtonClickArea : MonoBehaviour
    {
        void Awake()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 lossyScale = rectTransform.lossyScale;

            // 부모 Scale을 역보정
            rectTransform.sizeDelta = new Vector2(
                rectTransform.sizeDelta.x * lossyScale.x,
                rectTransform.sizeDelta.y * lossyScale.y
            );

            // 부모 Scale을 1로 맞춰도 클릭 영역 유지
            rectTransform.localScale = Vector3.one;
        }
    }
}