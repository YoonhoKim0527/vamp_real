using UnityEngine;

namespace Vampire
{
    public class WatchAdPanel : MonoBehaviour
    {
        // 🔘 버튼 A: 보스에게 받는 데미지만 2배
        public void OnClick_DamageBoostOnly()
        {
            AdsManager.Instance.ShowRewardAd(() =>
            {
                BoostManager.Instance.ActivateBoost(BoostType.Damage, 2f, 10f);
            });
        }

        // 🔘 버튼 B: 캐릭터 발사 속도만 2배
        public void OnClick_AttackSpeedBoostOnly()
        {
            AdsManager.Instance.ShowRewardAd(() =>
            {
                BoostManager.Instance.ActivateBoost(BoostType.AttackSpeed, 2f, 10f);
            });
        }
        public void OnClick_CoinBoostOnly()
        {
            AdsManager.Instance.ShowRewardAd(() =>
            {
                BoostManager.Instance.ActivateBoost(BoostType.Coin, 2f, 10f); // 10초 동안 코인 2배
            });
        }
    }
}