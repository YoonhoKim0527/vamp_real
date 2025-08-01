using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class GachaChestController : MonoBehaviour
    {
        [Header("Chest References")]
        [SerializeField] private Transform chestTransform;
        [SerializeField] private ParticleSystem glowParticles;

        [Header("Shake Settings")]
        [SerializeField] private float shakeDuration = 0.6f;
        [SerializeField] private float shakeMagnitude = 15f;
        [SerializeField] private int shakeFrequency = 10;

        public IEnumerator PlayChestSequence()
        {
            ResetGlowImmediately();                     // 🔒 파티클 초기화 (비활성화)
            yield return ShakeChest();                 // 🎯 상자 흔들림
            PlayGlowParticles();                       // 🌟 이제 팍 터뜨리기!
        }

        private IEnumerator ShakeChest()
        {
            float interval = shakeDuration / shakeFrequency;
            int direction = 1;
            Quaternion originalRotation = chestTransform.rotation;

            for (int i = 0; i < shakeFrequency; i++)
            {
                float angle = direction * shakeMagnitude;
                chestTransform.rotation = Quaternion.Euler(0, 0, angle);
                direction *= -1;

                yield return new WaitForSeconds(interval);
            }

            chestTransform.rotation = originalRotation;
        }

        private void PlayGlowParticles()
        {
            if (glowParticles != null)
            {
                glowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                glowParticles.Play();
            }
        }

        public void ResetGlowImmediately()
        {
            if (glowParticles != null)
            {
                glowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                glowParticles.Clear(); // 💡 재생 흔적도 전부 제거
            }
        }
    }
}
