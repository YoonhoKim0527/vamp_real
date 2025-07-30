using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BushPeekAnimation : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private RectTransform rabbit;
    [SerializeField] private RectTransform penguin;
    [SerializeField] private RectTransform monkey;

    [Header("Penguin Sprites")]
    [SerializeField] private Image penguinImage;
    [SerializeField] private Sprite penguinNormalSprite;
    [SerializeField] private Sprite penguinFlapSprite;
    [SerializeField] private float penguinFlapInterval = 0.3f;

    [Header("Rabbit Sprites")]
    [SerializeField] private Image rabbitImage;
    [SerializeField] private Sprite rabbitNormalSprite;
    [SerializeField] private Sprite rabbitAltSprite;
    [SerializeField] private float rabbitSwapInterval = 0.4f;

    [Header("Monkey Sprites")]
    [SerializeField] private Image monkeyImage;
    [SerializeField] private Sprite monkeyNormalSprite;
    [SerializeField] private Sprite monkeyHitSprite;

    [Header("Banana Animation")]
    [SerializeField] private GameObject banana;
    [SerializeField] private float bananaFallDuration = 1.0f;
    [SerializeField] private float bananaRotationSpeed = 360f;
    [SerializeField] private Vector2 bananaStartOffset = new Vector2(0f, 200f);

    [Header("Timing")]
    [SerializeField] private float peekDelay = 1.0f;
    [SerializeField] private float peekDuration = 1.0f;
    [SerializeField] private float stayDuration = 1.0f;

    private Vector2 rabbitStart = new Vector2(-1f, -1707f);
    private Vector2 rabbitTarget = new Vector2(-1f, -1442f);

    private Vector2 penguinStart = new Vector2(-827f, -1401f);
    private Vector2 penguinTarget = new Vector2(-608f, -1145f);

    private Vector2 monkeyStart = new Vector2(833f, -1329f);
    private Vector2 monkeyTarget = new Vector2(631f, -1105f);

    private Coroutine penguinFlapCoroutine;
    private Coroutine rabbitSwapCoroutine;

    private void Start()
    {
        rabbit.anchoredPosition = rabbitStart;
        penguin.anchoredPosition = penguinStart;
        monkey.anchoredPosition = monkeyStart;
        monkeyImage.sprite = monkeyNormalSprite;

        StartCoroutine(PeekSequence());
    }

    private IEnumerator PeekSequence()
    {
        while (true)
        {
            yield return StartCoroutine(Peek(rabbit, rabbitStart, rabbitTarget));
            rabbitSwapCoroutine = StartCoroutine(SwapRabbitSprites());

            yield return new WaitForSeconds(peekDelay);

            yield return StartCoroutine(Peek(penguin, penguinStart, penguinTarget));
            penguinFlapCoroutine = StartCoroutine(FlapPenguin());

            yield return new WaitForSeconds(peekDelay);

            yield return StartCoroutine(Peek(monkey, monkeyStart, monkeyTarget));
            yield return new WaitForSeconds(stayDuration);

            // 바나나 연출
            yield return StartCoroutine(AnimateBananaDrop());

            // 애니메이션 중지
            if (penguinFlapCoroutine != null)
            {
                StopCoroutine(penguinFlapCoroutine);
                penguinImage.sprite = penguinNormalSprite;
            }
            if (rabbitSwapCoroutine != null)
            {
                StopCoroutine(rabbitSwapCoroutine);
                rabbitImage.sprite = rabbitNormalSprite;
            }

            // 원래 위치로 복귀
            yield return StartCoroutine(Peek(monkey, monkeyTarget, monkeyStart));
            yield return StartCoroutine(Peek(penguin, penguinTarget, penguinStart));
            yield return StartCoroutine(Peek(rabbit, rabbitTarget, rabbitStart));
            yield return new WaitForSeconds(peekDelay);

            // 원숭이 표정 초기화
            monkeyImage.sprite = monkeyNormalSprite;
        }
    }

    private IEnumerator Peek(RectTransform character, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < peekDuration)
        {
            character.anchoredPosition = Vector2.Lerp(from, to, elapsed / peekDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        character.anchoredPosition = to;
    }

    private IEnumerator FlapPenguin()
    {
        while (true)
        {
            penguinImage.sprite = penguinFlapSprite;
            yield return new WaitForSeconds(penguinFlapInterval);
            penguinImage.sprite = penguinNormalSprite;
            yield return new WaitForSeconds(penguinFlapInterval);
        }
    }

    private IEnumerator SwapRabbitSprites()
    {
        while (true)
        {
            rabbitImage.sprite = rabbitAltSprite;
            yield return new WaitForSeconds(rabbitSwapInterval);
            rabbitImage.sprite = rabbitNormalSprite;
            yield return new WaitForSeconds(rabbitSwapInterval);
        }
    }

    private IEnumerator AnimateBananaDrop()
    {
        if (banana == null)
            yield break;

        RectTransform bananaRect = banana.GetComponent<RectTransform>();
        RectTransform monkeyRect = monkey;

        Vector2 monkeyPos = monkeyRect.anchoredPosition;
        Vector2 start = monkeyPos + bananaStartOffset;
        Vector2 offset = new Vector2(0f, 250f);
        Vector2 end = monkeyPos + offset;

        bananaRect.anchoredPosition = start;
        bananaRect.localRotation = Quaternion.identity;
        banana.SetActive(true);

        float elapsed = 0f;
        float rotation = 0f;

        while (elapsed < bananaFallDuration)
        {
            float t = elapsed / bananaFallDuration;
            bananaRect.anchoredPosition = Vector2.Lerp(start, end, t);
            rotation += bananaRotationSpeed * Time.deltaTime;
            bananaRect.localRotation = Quaternion.Euler(0f, 0f, rotation);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 위치 고정 + 회전 초기화
        bananaRect.anchoredPosition = end;
        bananaRect.localRotation = Quaternion.identity;

        // Monkey 맞은 sprite 전환
        monkeyImage.sprite = monkeyHitSprite;

        yield return new WaitForSeconds(0.5f); // 맞은 상태 유지 시간

        banana.SetActive(false);
    }
}
