using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Vampire
{
    public class LevelManager : MonoBehaviour
    {
        private LevelBlueprint levelBlueprint;

        [SerializeField] private Character playerCharacter;
        [SerializeField] private EntityManager entityManager;
        [SerializeField] private AbilityManager abilityManager;
        [SerializeField] private AbilitySelectionDialog abilitySelectionDialog;
        [SerializeField] private InfiniteBackground infiniteBackground;
        [SerializeField] private Inventory inventory;
        [SerializeField] private StatsManager statsManager;
        [SerializeField] private GameOverDialog gameOverDialog;
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private TilemapBackgroundGenerator tilemapBackgroundGenerator;

        private float levelTime = 0;
        private float timeSinceLastMonsterSpawned;
        private float timeSinceLastChestSpawned;
        private bool miniBossSpawned = false;
        private bool finalBossSpawned = false;

        private bool isInitialized = false;

        public void Init(LevelBlueprint levelBlueprint)
        {
            this.levelBlueprint = levelBlueprint;
            levelTime = 0;

            var gameStateManager = FindObjectOfType<GameStateManager>();
            CharacterStatBlueprint playerStats = gameStateManager.PlayerStats;
            List<UpgradeStateSaveData> upgradeStates = gameStateManager.GetUpgradeStatesSafe();

            Debug.Log($"[LevelManager] Loaded PlayerStats: Attack={playerStats.attackPower}, ExtraProjectiles={playerStats.extraProjectiles}");

            entityManager.Init(this.levelBlueprint, playerCharacter, inventory, statsManager, infiniteBackground, abilitySelectionDialog);
            abilityManager.Init(this.levelBlueprint, entityManager, playerCharacter, playerStats);
            abilitySelectionDialog.Init(abilityManager, entityManager, playerCharacter);
            playerCharacter.Init(entityManager, abilityManager, statsManager, playerStats);
            playerCharacter.OnDeath.AddListener(GameOver);

            entityManager.SpawnGemsAroundPlayer(this.levelBlueprint.initialExpGemCount, this.levelBlueprint.initialExpGemType);
            entityManager.SpawnChest(levelBlueprint.chestBlueprint);
            infiniteBackground.Init(this.levelBlueprint.backgroundTexture, playerCharacter.transform);
            tilemapBackgroundGenerator.Init(levelBlueprint.backgroundTiles, playerCharacter.transform);
            inventory.Init();

            isInitialized = true; // ✅ 이제 초기화 완료
            levelBlueprint.Initialize();
        }

        IEnumerator Start()
        {
            if (CrossSceneData.LevelBlueprint == null)
            {
                Debug.LogError("[LevelManager] LevelBlueprint is null! Cannot initialize level.");
                yield break;
            }

            // ✅ GameStateManager 초기화 완료 대기
            var gameStateManager = FindObjectOfType<GameStateManager>();
            while (gameStateManager == null || !gameStateManager.IsInitialized)
            {
                Debug.Log("[LevelManager] Waiting for GameStateManager to initialize...");
                yield return null; // 다음 프레임까지 대기
                gameStateManager = FindObjectOfType<GameStateManager>();
            }

            Debug.Log("[LevelManager] GameStateManager is ready. Proceeding with Init().");
            Init(CrossSceneData.LevelBlueprint);
        }

        void Update()
        {
            if (!isInitialized) return; // ✅ 초기화 전이면 아무것도 하지 않음
            levelTime += Time.deltaTime;
            gameTimer.SetTime(levelTime);

            HandleMonsterSpawning();
            HandleBossSpawning();
            HandleChestSpawning();
        }

        private void HandleMonsterSpawning()
        {
            if (levelTime < levelBlueprint.levelTime)
            {
                timeSinceLastMonsterSpawned += Time.deltaTime;
                float spawnRate = levelBlueprint.monsterSpawnTable.GetSpawnRate(levelTime / levelBlueprint.levelTime);
                float monsterSpawnDelay = spawnRate > 0 ? 1.0f / spawnRate : float.PositiveInfinity;

                if (timeSinceLastMonsterSpawned >= monsterSpawnDelay)
                {
                    (int monsterIndex, float hpMultiplier) = levelBlueprint.monsterSpawnTable.SelectMonsterWithHPMultiplier(levelTime / levelBlueprint.levelTime);
                    (int poolIndex, int blueprintIndex) = levelBlueprint.MonsterIndexMap[monsterIndex];
                    MonsterBlueprint monsterBlueprint = levelBlueprint.monsters[poolIndex].monsterBlueprints[blueprintIndex];
                    entityManager.SpawnMonsterRandomPosition(poolIndex, monsterBlueprint, monsterBlueprint.hp * hpMultiplier);

                    timeSinceLastMonsterSpawned = Mathf.Repeat(timeSinceLastMonsterSpawned, monsterSpawnDelay);
                }
            }
        }

        private void HandleBossSpawning()
        {
            if (!miniBossSpawned && levelTime > levelBlueprint.miniBosses[0].spawnTime)
            {
                miniBossSpawned = true;
                entityManager.SpawnMonsterRandomPosition(levelBlueprint.monsters.Length, levelBlueprint.miniBosses[0].bossBlueprint);
            }

            if (!finalBossSpawned && levelTime > levelBlueprint.levelTime)
            {
                finalBossSpawned = true;
                Monster finalBoss = entityManager.SpawnMonsterRandomPosition(levelBlueprint.monsters.Length, levelBlueprint.finalBoss.bossBlueprint);
                finalBoss.OnKilled.AddListener(LevelPassed);
            }
        }

        private void HandleChestSpawning()
        {
            timeSinceLastChestSpawned += Time.deltaTime;
            if (timeSinceLastChestSpawned >= levelBlueprint.chestSpawnDelay)
            {
                for (int i = 0; i < levelBlueprint.chestSpawnAmount; i++)
                {
                    entityManager.SpawnChest(levelBlueprint.chestBlueprint);
                }
                timeSinceLastChestSpawned = Mathf.Repeat(timeSinceLastChestSpawned, levelBlueprint.chestSpawnDelay);
            }
        }

        public void GameOver()
        {
            Time.timeScale = 0;

            // 코인 저장
            int coinCount = PlayerPrefs.GetInt("Coins");
            PlayerPrefs.SetInt("Coins", coinCount + statsManager.CoinsGained);

            var gameStateManager = FindObjectOfType<GameStateManager>();
            gameStateManager?.SaveGame();

            // ✅ 스탯 초기화
            gameStateManager?.ResetCharacterStats();

            gameOverDialog.Open(false, statsManager);
        }

        public void LevelPassed(Monster finalBossKilled)
        {
            Time.timeScale = 0;

            int coinCount = PlayerPrefs.GetInt("Coins");
            PlayerPrefs.SetInt("Coins", coinCount + statsManager.CoinsGained);

            var gameStateManager = FindObjectOfType<GameStateManager>();
            gameStateManager?.SaveGame();

            // ✅ 스탯 초기화
            gameStateManager?.ResetCharacterStats();

            gameOverDialog.Open(true, statsManager);
        }

        public void Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(0);
        }
    }
}
