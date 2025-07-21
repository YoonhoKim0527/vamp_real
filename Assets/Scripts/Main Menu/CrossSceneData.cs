using System.Collections.Generic;

namespace Vampire
{
    public static class CrossSceneData
    {
        public static LevelBlueprint LevelBlueprint { get; set; }
        public static CharacterBlueprint CharacterBlueprint { get; set; }
        public static List<CharacterBlueprint> ExpeditionCharacters = new();
        public static bool ExtraProjectile = false;
        public static bool ExtraDamage = false;
        public static bool ExtraHP = false;
        public static bool ExtraSpeed = false;
        public static int BonusProjectile = 0;
        public static int BonusDamage = 0;
        public static int BonusHP = 0;
        public static int BonusSpeed = 0;


        public static void Reset()
        {
            ExtraProjectile = false;
            ExtraDamage = false;
            ExtraHP = false;
            ExtraSpeed = false;

            BonusProjectile = 0;
            BonusDamage = 0;
            BonusHP = 0;
            BonusSpeed = 0;
        }
    }
}
