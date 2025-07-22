using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements; // Ads 패키지 없으면 삭제
using TMPro;                      // TMP 텍스트용

namespace Vampire
{
#if UNITY_ADS
    public class FarmManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
#else
    public class FarmManager : MonoBehaviour
#endif
    {
        [Header("Config")]
        [SerializeField] CharacterBlueprint[] allBlueprints;
        [SerializeField] GameObject farmCharPrefab;
        [SerializeField] Transform farmArea;
        [SerializeField] float maxOfflineHours = 8f;

        [Header("UI")]
        [SerializeField] TMP_Text coinText;
        [SerializeField] TMP_Text pendingText;
        [SerializeField] TMP_Text boosterText;
        [SerializeField] UnityEngine.UI.Button adButton;

        readonly List<FarmSlot> slots = new();

        double boosterMult = 1.0;
        float boosterRemain = 0f;

        void Start()
        {
            foreach (var bp in allBlueprints)
            {
                if (!bp.owned) continue;
                var obj = Instantiate(farmCharPrefab, RandPos(), Quaternion.identity, farmArea);
                var movement = obj.GetComponent<FarmCharacterMovement>();
                if (movement != null)
                    movement.Init(bp);
                else
                    Debug.LogWarning("FarmCharacterMovement가 프리팹에 붙어있지 않음");

                slots.Add(new FarmSlot
                {
                    bp = bp,
                    lastCollectedTime = Time.realtimeSinceStartup
                });
            }

            HandleOfflineReward();

            if (adButton) adButton.onClick.AddListener(ShowRewardAd);

#if UNITY_ADS
            Advertisement.Initialize("YOUR_GAME_ID", false);
            Advertisement.Load("Rewarded_Android", this);
#endif
        }

        void Update()
        {
            TickRealtime();
            RefreshUI();
        }

        void OnApplicationQuit()
        {
            long now = DateTime.UtcNow.Ticks;
            PlayerPrefs.SetString("LAST_QUIT", now.ToString());
        }


        // ---------- 생산 ----------
        void TickRealtime()
        {
            if (boosterRemain > 0)
            {
                boosterRemain -= Time.unscaledDeltaTime;
                if (boosterRemain <= 0) { boosterMult = 1; boosterRemain = 0; }
            }

            foreach (var s in slots)
            {
                if (Time.realtimeSinceStartup - s.lastCollectedTime >= 1f)
                {
                    double add = s.bp.farmProductionPerSecond * boosterMult;

                    // ✅ CoinManager 사용
                    CoinManager.Instance.AddCoins(Mathf.FloorToInt((float)add));

                    s.lastCollectedTime += 1f;
                }
            }
        }

        // ---------- 오프라인 ----------

        void HandleOfflineReward()
        {
            if (!PlayerPrefs.HasKey("LAST_QUIT")) return;

            long savedTicks;
            if (!long.TryParse(PlayerPrefs.GetString("LAST_QUIT"), out savedTicks)) return;

            DateTime lastQuit = new DateTime(savedTicks);
            TimeSpan elapsed = DateTime.UtcNow - lastQuit;

        
            double capSec = maxOfflineHours * 3600;
            double seconds = Math.Min(elapsed.TotalSeconds, capSec);

            double earn = 0;
            foreach (var s in slots)
                earn += seconds * s.bp.farmProductionPerSecond;

            int currentCoins = PlayerPrefs.GetInt("Coins", 0);
            PlayerPrefs.SetInt("Coins", currentCoins + Mathf.FloorToInt((float)earn));
        }

        // ---------- 광고 ----------
        void ShowRewardAd()
        {
#if UNITY_ADS
            Advertisement.Show("Rewarded_Android", this);
#else
            GrantBooster(); // Ads 미설치 상태 테스트용
#endif
        }

#if UNITY_ADS
        public void OnUnityAdsShowComplete(string id, UnityAdsShowCompletionState st)
        {
            if (st == UnityAdsShowCompletionState.COMPLETED) GrantBooster();
            Advertisement.Load(id, this);
        }
        public void OnUnityAdsAdLoaded(string id) { }
        public void OnUnityAdsFailedToLoad(string id, UnityAdsLoadError err, string msg) { }
        public void OnUnityAdsShowFailure(string id, UnityAdsShowError err, string msg) { }
        public void OnUnityAdsShowStart(string id) { }
        public void OnUnityAdsShowClick(string id) { }
#endif

        void GrantBooster()
        {
            boosterMult = 2;
            boosterRemain = 600f;
        }

        // ---------- UI ----------
        void RefreshUI()
        {
            // ✅ 코인 표시도 CoinManager가 자동 갱신
            coinText.text = $"💰 {CoinManager.Instance.GetCoins()}";
            pendingText.text = $"x{boosterMult} PRODUCE";
            boosterText.text = boosterMult == 1 ? ""
                        : $"TIME: {Mathf.CeilToInt(boosterRemain)}s";
        }

        Vector3 RandPos() =>
            new(UnityEngine.Random.Range(-5f, 5f),
                UnityEngine.Random.Range(-3f, 3f), 0);
    }
}
