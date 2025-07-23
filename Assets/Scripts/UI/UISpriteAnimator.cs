using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnimator : MonoBehaviour
{
    [Header("UI Image")]
    [SerializeField] private Image targetImage; // 애니메이션 적용할 UI 이미지

    [Header("Animation Frames")]
    [SerializeField] private Sprite[] frames; // 순서대로 넣을 Sprite들

    [Header("Settings")]
    [SerializeField] private float frameRate = 0.1f; // 한 프레임당 지속 시간 (초)
    [SerializeField] private bool loop = true;       // 반복 여부

    private int currentFrame = 0;
    private float timer = 0f;

    private void Update()
    {
        if (frames == null || frames.Length == 0 || targetImage == null)
            return;

        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1; // 마지막 프레임 고정
                    enabled = false; // 스크립트 비활성화
                }
            }

            targetImage.sprite = frames[currentFrame];
        }
    }

    public void ResetAnimation()
    {
        currentFrame = 0;
        timer = 0f;
        targetImage.sprite = frames[currentFrame];
        enabled = true;
    }
}
