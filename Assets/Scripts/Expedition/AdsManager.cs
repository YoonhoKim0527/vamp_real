using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace Vampire
{
    public class AdsManager : MonoBehaviour, IUnityAdsShowListener, IUnityAdsLoadListener
    {
        public static AdsManager Instance;

        [SerializeField] string androidAdUnitId = "Rewarded_Android";
        [SerializeField] string iosAdUnitId = "Rewarded_iOS";
        [SerializeField] string gameId = "1234567"; // 실제 Game ID 입력
        [SerializeField] bool testMode = true;
        [SerializeField] bool forceRewardOnEditor = true;

        string adUnitId;
        Action onRewarded;

        bool adReady = false; // ✅ 광고 준비 상태 직접 관리

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID
            adUnitId = androidAdUnitId;
#elif UNITY_IOS
            adUnitId = iosAdUnitId;
#endif

            Advertisement.Initialize(gameId, testMode);
        }

        void Start()
        {
            Advertisement.Load(adUnitId, this);
        }

        public void ShowRewardAd(Action onSuccess)
        {
#if UNITY_EDITOR
            if (forceRewardOnEditor)
            {
                Debug.Log("[AdsManager] UnityEditor 환경 - 광고 없이 보상 지급");
                onSuccess?.Invoke();
                return;
            }
#endif

            if (adReady)
            {
                onRewarded = onSuccess;
                Advertisement.Show(adUnitId, this);
                adReady = false; // 다시 로딩 필요
            }
            else
            {
                Debug.LogWarning("[AdsManager] 광고 준비 안됨 - 보상 지급 안함");
            }
        }

        // ✅ 광고 로드 성공 시 호출
        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId == adUnitId)
            {
                adReady = true;
                Debug.Log($"[AdsManager] 광고 로드 완료: {placementId}");
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"[AdsManager] 광고 로드 실패: {placementId} - {error}: {message}");
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showState)
        {
            if (placementId == adUnitId && showState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("[AdsManager] 광고 시청 완료 - 보상 지급");
                onRewarded?.Invoke();
                onRewarded = null;
            }

            // 다음 광고 다시 로드
            Advertisement.Load(adUnitId, this);
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"[AdsManager] 광고 표시 실패: {placementId} - {error}: {message}");

#if UNITY_EDITOR
            if (forceRewardOnEditor)
            {
                Debug.Log("[AdsManager] 광고 표시 실패 - 테스트 모드에서 보상 강제 지급");
                onRewarded?.Invoke();
                onRewarded = null;
            }
#endif
        }

        public void OnUnityAdsShowStart(string placementId) { }
        public void OnUnityAdsShowClick(string placementId) { }

        // ✅ 외부에서 광고 가능 여부 체크할 수 있도록 추가
        public bool IsAdReady()
        {
            return adReady;
        }
    }
}
