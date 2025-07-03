namespace Vampire
{
    public static class CrossSceneData
    {
        public static CharacterBlueprint CharacterBlueprint { get; set; }
        public static bool ExtraProjectile = false;
        public static bool ExtraDamage = false;
        public static bool ExtraHP = false;
        public static bool ExtraSpeed = false;


        public static void Reset()
        {
            ExtraProjectile = false;
            ExtraDamage = false;
            ExtraHP = false;
            ExtraSpeed = false;
        }
    }
}
