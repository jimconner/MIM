namespace MIM
{
    using System;
    using System.Collections.Generic;
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using Boardgame.BoardPiece;
    using Boardgame.Data;
    using Boardgame.Ui;
    using DataKeys;
    using Fidelity.Singleton;
    using Fidelity.Localization;
    using HarmonyLib;
    using Prototyping;
    using TMPro;
    using System.Linq;


    internal static class ModPatcher
    {
        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(ActionSelect), "UpdateHoverTile"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(ActionSelect_UpdateHoverTile_Postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(BaseGrabbableMiniature), "DecideWhatObjectToGrab"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(BaseGrabbableMiniature_DecideWhatObjectToGrab_Postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(GrabbedPieceHudInstantiator), "AddPieceStatsToViewModel"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(GrabbedPieceHudInstantiator_AddPieceStatsToViewModel_Postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(GrabbedPieceHudInstantiator), "AddMana"),
                prefix: new HarmonyMethod(typeof(ModPatcher), nameof(GrabbedPieceHudInstantiator_AddMana_Prefix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(ActionSelect), "ClearOutline"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(ActionSelect_ClearOutline_Postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(Grabbable), "OnGrabbed"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(Grabbable_OnGrabbed_Postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(GrabbedPieceHudInstantiator), "AddMoveRange"),
                prefix: new HarmonyMethod(typeof(ModPatcher), nameof(GrabbedPieceHudInstantiator_AddMoveRange_Prefix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(PieceStatsView), "UpdateView"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(PieceStatsView_UpdateView_Postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(GrabbedPieceHudInstantiator), "AddAbilities"),
                prefix: new HarmonyMethod(typeof(ModPatcher), nameof(GrabbedPieceHudInstantiator_AddAbilities_Prefix)));
        }

        private static void ActionSelect_UpdateHoverTile_Postfix(ref ActionSelect __instance, ref BoardModel ___boardModel, ref Ability ___ability, ref IntPoint2D hoverTile)
        {
            if (___ability == null)
            {
                VisibilityCalculator visibilityCalculator = new VisibilityCalculator();
                TileRect viewerTile = new TileRect(hoverTile);
                visibilityCalculator.SetupAndCalculate(___boardModel.tileSet, viewerTile, new List<TileRect>(), 100, false);
                MoveSet visibleTiles = new MoveSet(viewerTile);
                visibilityCalculator.ForEachVisible(delegate(IntPoint2D visibleTile, int distance)
                {
                    visibleTiles.AddMove(visibleTile, distance);
                });
                TileHighlightController.TileHighLight.UpdateHighlights(visibleTiles, TileHighlight.HighlightType.Hostile, null);
                TileHighlightController.PlayerPieceTurnHighlight.UpdateHighlights(__instance.MovesInRange, TileHighlight.HighlightType.Friendly, null);
                return;
            }

            if (turretAbilities.Contains(___ability.abilityKey))
            {
                VisibilityCalculator visibilityCalculator2 = new VisibilityCalculator();
                TileRect viewerTile2 = new TileRect(hoverTile);
                MIM.Logger.Msg($"MIM: ViewerTile {viewerTile2}");
                if (viewerTile2.Equals(new TileRect(-1, -1)))
                {
                    viewerTile2 = new TileRect(15, 15, 1);
                }

                visibilityCalculator2.SetupAndCalculate(___boardModel.tileSet, viewerTile2, new List<TileRect>(), 100, false);
                MoveSet visibleTiles = new MoveSet(viewerTile2);
                visibilityCalculator2.ForEachVisible(delegate (IntPoint2D visibleTile, int distance)
                {
                    visibleTiles.AddMove(visibleTile, distance);
                });
                TileHighlightController.PlayerPieceTurnHighlight.UpdateHighlights(visibleTiles, TileHighlight.HighlightType.Hostile, null);
            }
        }

        // Token: 0x04000001 RID: 1
        private static List<AbilityKey> turretAbilities = new List<AbilityKey>
        {
            AbilityKey.RepeatingBallista,
            AbilityKey.TheBehemoth,
            AbilityKey.HealingWard,
            AbilityKey.DetectEnemies,
            AbilityKey.Lure,
            AbilityKey.Torch,
            AbilityKey.CallCompanion,
            AbilityKey.SummonElemental,
        };

        private static void BaseGrabbableMiniature_DecideWhatObjectToGrab_Postfix(ref BaseGrabbableMiniature __instance, ref GameContext ___gameContext, ref Grabbable __result)
        {
            Piece myPiece = __instance.MyPiece;
            VisibilityCalculator visibilityCalculator = new VisibilityCalculator();
            TileRect viewerTile = new TileRect(myPiece.gridPos.min);
            visibilityCalculator.SetupAndCalculate(___gameContext.boardModel.tileSet, viewerTile, new List<TileRect>(), 100, false);
            TileHighlightController.TileHighLight.Clear();
            MoveSet visibleTiles = new MoveSet(myPiece.gridPos);
            visibilityCalculator.ForEachVisible(delegate (IntPoint2D visibleTile, int distance)
            {
                visibleTiles.AddMove(visibleTile, distance);
            });
            TileHighlightController.TileHighLight.UpdateHighlights(visibleTiles, TileHighlight.HighlightType.Hostile, null);
            MoveSet movesInRange = ___gameContext.boardModel.GetMovesInRange(myPiece.gridPos, myPiece.GetMoveRange() * (myPiece.IsPlayer() ? 1 : 2), myPiece);
            TileHighlightController.PlayerPieceTurnHighlight.UpdateHighlights(movesInRange, TileHighlight.HighlightType.Friendly, null);
        }

        private static void GrabbedPieceHudInstantiator_AddPieceStatsToViewModel_Postfix(ref GrabbedPieceHudInstantiator __instance, ref PieceStatsViewModel ___pieceStatsViewModel, ref PieceAndTurnController ___pieceAndTurnController)
        {
            Piece myPiece = __instance.MyPiece;
            if (myPiece.IsPlayer())
            {
                ___pieceStatsViewModel.pieceStats.Add("Cards: " + myPiece.inventory.Items.Count.ToString() + "/" + ((myPiece.characterClass == CharacterClass.Hunter || myPiece.characterClass == CharacterClass.Sorcerer || myPiece.characterClass == CharacterClass.Warlock) ? 11 : 10).ToString());
                ___pieceStatsViewModel.abilities.Clear();
                using (List<Inventory.Item>.Enumerator enumerator = myPiece.inventory.Items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Ability ability;
                        if (AbilityFactory.TryGetAbility(enumerator.Current.abilityKey, out ability) && !string.IsNullOrEmpty(ability.titleLocalizedKey))
                        {
                            Inventory.Item item = enumerator.Current;
                            CardConfigData cardconfig;
                            new GameDataAPI().FindCardConfig(MotherbrainGlobalVars.CurrentConfig, ability.abilityKey, out cardconfig);
                            int num = (cardconfig != null) ? cardconfig.SellValue : 0;
                            if (num != 0)
                            {
                                ___pieceStatsViewModel.abilities.Add(Singleton<Locale>.Instance.GetString(ability.titleLocalizedKey) + "(" + num.ToString() + ")");
                            }
                        }
                    }
                }
            }
        }

        private static bool GrabbedPieceHudInstantiator_AddMana_Prefix()
        {
            return false;
        }

        private static void ActionSelect_ClearOutline_Postfix(ActionSelect.OutlineType type)
        {
            TileHighlightController.TileHighLight.Clear();
        }

        private static void Grabbable_OnGrabbed_Postfix(ref Grabbable __instance, HandId handId, bool grabFlag)
        {
            if (!grabFlag)
            {
                TileHighlightController.TileHighLight.Clear();
                TileHighlightController.PlayerPieceTurnHighlight.Clear();
            }
        }

        private static bool GrabbedPieceHudInstantiator_AddMoveRange_Prefix(ref GrabbedPieceHudInstantiator __instance, ref PieceStatsViewModel ___pieceStatsViewModel)
        {
            if (__instance.MyPiece.IsPlayer())
            {
                ___pieceStatsViewModel.pieceStats.Add("Movement: " + __instance.MyPiece.GetMoveRange().ToString());
                return false;
            }

            return true;
        }

        private static void PieceStatsView_UpdateView_Postfix(ref PieceStatsView __instance, ref TextMeshPro ___statsText)
        {
            ___statsText.fontSize = 4f;
        }

        private static float ProbabilityForAbility(AbilityKey abilityKey)
        {
            Ability ability;
            if (AbilityFactory.TryGetAbility(abilityKey, out ability))
            {
                return ability.probability * 100f;
            }

            return 100f;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x0000249C File Offset: 0x0000069C
        private static float ProbabilityForAbilities(List<AbilityKey> abilityKeys)
        {
            float num = 0f;
            for (int i = 0; i < abilityKeys.Count; i++)
            {
                num += ProbabilityForAbility(abilityKeys[i]);
            }

            return num / (float)abilityKeys.Count;
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000024D8 File Offset: 0x000006D8
        private static float ProbabilityForAbilityAttribute(ref PieceConfigData pieceConfig, Ability.AbilityAttribute abilityAttribute)
        {
            return ProbabilityForAbilities((from x in pieceConfig.Abilities
                                                              where AbilityFactory.AbilityHasAttribute(x, abilityAttribute)
                                                              select x).ToList<AbilityKey>());
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002514 File Offset: 0x00000714
        private static float ProbabilityForBehavior(ref PieceConfigData pieceConfig, Behaviour behaviour)
        {
            if (behaviour <= Behaviour.RangedAttackHighPrio)
            {
                switch (behaviour)
                {
                    case Behaviour.RangedSpellCaster:
                        return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.RangedAttack);
                    case Behaviour.CastOnTeam:
                        return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.CastSpellOnAllies);
                    case Behaviour.Charging:
                    case (Behaviour)8:
                    case Behaviour.SlimeFusion:
                        break;
                    case Behaviour.LeechMelee:
                        return ProbabilityForAbility(AbilityKey.LeechMelee);
                    case Behaviour.EarthShatter:
                        return ProbabilityForAbility(AbilityKey.EarthShatter);
                    case Behaviour.SpawnPiece:
                        return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.SpawnPiece);
                    case Behaviour.SpawnBuildUp:
                        return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.SpawnPiece);
                    case Behaviour.AbilityBuildUp:
                        return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.Buildup);
                    default:
                        if (behaviour == Behaviour.VerminFrenzy)
                        {
                            return ProbabilityForAbility(AbilityKey.VerminFrenzy);
                        }
                        if (behaviour == Behaviour.RangedAttackHighPrio)
                        {
                            return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.RangedAttack);
                        }

                        break;
                }
            }
            else if (behaviour <= Behaviour.PikeAttack)
            {
                if (behaviour == Behaviour.RootWall)
                {
                    return ProbabilityForAbility(AbilityKey.RootWall);
                }

                if (behaviour == Behaviour.PikeAttack)
                {
                    return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.RangedAttack);
                }
            }
            else
            {
                if (behaviour == Behaviour.KeepDistance)
                {
                    return ProbabilityForAbilityAttribute(ref pieceConfig, Ability.AbilityAttribute.RangedAttack);
                }

                if (behaviour == Behaviour.StationaryAbilityBehaviour)
                {
                    return ProbabilityForAbilities(pieceConfig.Abilities.Cast<AbilityKey>().ToList());
                }
            }

            return 100f;
        }

        // Token: 0x06000013 RID: 19 RVA: 0x000025F4 File Offset: 0x000007F4
        private static bool GrabbedPieceHudInstantiator_AddAbilities_Prefix(ref GrabbedPieceHudInstantiator __instance, ref PieceStatsViewModel ___pieceStatsViewModel, ref PieceConfigData ___pieceConfig)
        {
            Piece myPiece = __instance.MyPiece;
            int num = 0;
            if (myPiece.effectSink.TryGetStat(Stats.Type.HealthPotion, out num, -1) && myPiece.GetPieceConfig().Abilities.Contains(AbilityKey.EnemyHeal))
            {
                ___pieceStatsViewModel.pieceStats.Add("Health potions: " + num.ToString());
            }
            float num2 = myPiece.GetPieceConfig().ChanceOfDeathPanic * 100f;
            if (num2 > 0f)
            {
                ___pieceStatsViewModel.pieceStats.Add("Death panic: " + num2.ToString("00") + "%");
            }
            float num3 = myPiece.GetPieceConfig().ChanceOfFirePanic * 100f;
            if (num3 > 0f)
            {
                ___pieceStatsViewModel.pieceStats.Add("Fire panic: " + num3.ToString("00") + "%");
            }
            int num4 = (int)(((double)myPiece.GetPieceConfig().BerserkBelowHealth - 0.01) * (double)((float)myPiece.GetMaxHealth(0)));
            if (num4 > 0)
            {
                ___pieceStatsViewModel.pieceStats.Add("Berserk at: " + num4.ToString());
            }

            ___pieceStatsViewModel.abilities.Clear();
            List<AbilityKey> abilities = ___pieceConfig.Abilities.Cast<AbilityKey>().ToList();
            int count = abilities.Count;
            if (count == 0)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                Ability ability;
                if (AbilityFactory.TryGetAbility(abilities[i], out ability) && !string.IsNullOrEmpty(ability.titleLocalizedKey))
                {
                    string @string = Singleton<Locale>.Instance.GetString(ability.titleLocalizedKey);
                    int cooldownForAbility = myPiece.GetCooldownForAbility(abilities[i]);
                    if (cooldownForAbility > 0)
                    {
                        ___pieceStatsViewModel.abilities.Add(@string + "(" + cooldownForAbility.ToString() + ")");
                    }
                    else
                    {
                        ___pieceStatsViewModel.abilities.Add(@string);
                    }
                }
            }

            Behaviour[] array = new Behaviour[38];
            Behaviour[] array2 = array;
            List<Behaviour> list = new List<Behaviour>(___pieceConfig.Behaviours);
            List<Behaviour> list2 = new List<Behaviour>();
            for (int j = 0; j < array2.Length; j++)
            {
                if (list.Contains(array2[j]))
                {
                    list2.Add(array2[j]);
                }
            }

            if (list2.Count > 0)
            {
                string text = "Behaviors: " + list2[0].ToString();
                float num5 = ProbabilityForBehavior(ref ___pieceConfig, list2[0]);
                if (num5 != 100f)
                {
                    text = text + "(" + num5.ToString("00") + "%)";
                }

                for (int k = 1; k < list2.Count; k++)
                {
                    text = text + "," + list2[k].ToString();
                    num5 = ProbabilityForBehavior(ref ___pieceConfig, list2[k]);
                    if (num5 != 100f)
                    {
                        text = text + "(" + num5.ToString("00") + "%)";
                    }
                }

                ___pieceStatsViewModel.pieceStats.Add(text);
            }

            return false;
        }
    }
}
