namespace MIM
{
    using HarmonyLib;
    using MelonLoader;

    internal class PendingVoice
    {
        // Token: 0x04000001 RID: 1
        public static int playerId = -1;
    }

    internal class MIM : MelonMod
    {
        internal static readonly MelonLogger.Instance Logger = new MelonLogger.Instance("MIM");

        public override void OnApplicationStart()
        {
            Logger.Msg("MIM applying Harmony patches.");

            var harmony = new Harmony("com.conner.demeomods.MIM");
            ModPatcher.Patch(harmony);
        }
    }
}
