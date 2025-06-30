namespace Vampire
{
    public static class CrossSceneData
    {
        public static CharacterBlueprint CharacterBlueprint { get; set; }
        public static bool ExtraProjectile = false;

        public static void Reset()
        {
            ExtraProjectile = false;
        }
    }
}
