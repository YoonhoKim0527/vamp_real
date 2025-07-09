using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class FarmCharacterMovement : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Vector2 moveBoundsMin = new Vector2(-5f, -3f);
        [SerializeField] Vector2 moveBoundsMax = new Vector2(5f, 3f);

        CharacterBlueprint blueprint;
        float moveSpeed;
        float timer;
        float directionChangeInterval = 2f;
        Vector2 moveDirection;
        int currentFrame;
        float frameTimer;

        public void Init(CharacterBlueprint blueprint)
        {
            this.blueprint = blueprint;
            moveSpeed = blueprint.movespeed * 0.3f;

            spriteRenderer.sprite = blueprint.walkSpriteSequence[0];
        }

        void Start()
        {
            ChangeDirection();
        }

        void Update()
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, moveBoundsMin.x, moveBoundsMax.x);
            pos.y = Mathf.Clamp(pos.y, moveBoundsMin.y, moveBoundsMax.y);
            transform.position = pos;
            // 방향 전환
            timer += Time.deltaTime;
            if (timer >= directionChangeInterval)
            {
                ChangeDirection();
                timer = 0f;
            }

            // 걷기 애니메이션
            AnimateWalk();
        }

        void ChangeDirection()
        {
            moveDirection = Random.insideUnitCircle.normalized;

            // 좌우 반전
            if (moveDirection.x != 0)
                spriteRenderer.flipX = moveDirection.x < 0;
        }

        void AnimateWalk()
        {
            if (blueprint.walkSpriteSequence.Length == 0) return;

            frameTimer += Time.deltaTime;
            if (frameTimer >= blueprint.walkFrameTime)
            {
                currentFrame = (currentFrame + 1) % blueprint.walkSpriteSequence.Length;
                spriteRenderer.sprite = blueprint.walkSpriteSequence[currentFrame];
                frameTimer = 0f;
            }
        }
    }
}
