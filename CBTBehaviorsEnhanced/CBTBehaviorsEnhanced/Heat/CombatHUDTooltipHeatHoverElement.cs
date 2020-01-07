﻿using BattleTech;
using BattleTech.UI;
using CBTBehaviorsEnhanced.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using us.frostraptor.modUtils;

namespace CBTBehaviorsEnhanced.Heat {
    public class CombatHUDSidePanelHeatHoverElement : CombatHUDSidePanelHoverElement {

        private CombatHUD CombatHUD = null;

        private int CurrentHeat = 0;
        private int ProjectedHeat = 0;
        private int TempHeat = 0;

        public new void Init(CombatHUD HUD) {
            CombatHUD = HUD;

            this.Title = new Localize.Text("HEAT LEVEL");
            this.Description = new Localize.Text($"Heat: 0 / 0");
            this.WarningText = new Localize.Text("");

            base.Init(CombatHUD);
        }

        public void UpdateText(Mech displayedMech) {

            if (displayedMech.CurrentHeat != CurrentHeat || displayedMech.TempHeat != TempHeat ||
                CombatHUD.SelectionHandler.ProjectedHeatForState != ProjectedHeat) {
                Mod.Log.Debug($"Updating heat dialog for actor: {CombatantUtils.Label(displayedMech)}");

                Mod.Log.Debug($"  current values:  CurrentHeat: {CurrentHeat}  ProjectedHeat: {ProjectedHeat}  TempHeat: {TempHeat}");
                this.CurrentHeat = displayedMech.CurrentHeat;
                this.ProjectedHeat = CombatHUD.SelectionHandler.ProjectedHeatForState;
                this.TempHeat = displayedMech.TempHeat;

                int sinkableHeat = !displayedMech.HasAppliedHeatSinks ? displayedMech.AdjustedHeatsinkCapacity * -1 : 0;
                int futureHeat = Math.Max(0, CurrentHeat + TempHeat + ProjectedHeat + sinkableHeat);
                Mod.Log.Debug($"  currentHeat: {CurrentHeat}  tempHeat: {TempHeat}  projectedHeat: {ProjectedHeat}  sinkableHeat: {sinkableHeat}" +
                    $"  =  futureHeat: {futureHeat}");

                float heatCheck = displayedMech.HeatCheckMod(Mod.Config.Piloting.SkillMulti);
                float maxHeat = Mod.Config.Heat.Shutdown.Last().Key;
                StringBuilder descSB = new StringBuilder($"Heat: {futureHeat} / {maxHeat}");
                StringBuilder warningSB = new StringBuilder("");

                float threshold = 0f;
                // Check Ammo
                foreach (KeyValuePair<int, float> kvp in Mod.Config.Heat.Explosion) {
                    if (futureHeat >= kvp.Key) { threshold = kvp.Value; }
                }
                if (threshold != 0f && threshold != -1f) { 
                    Mod.Log.Debug($"Ammo Explosion Threshold: {threshold:P1} vs. d100+{heatCheck * 100f}");
                    descSB.Append($"\nAmmo Explosion on d100+{heatCheck * 100f} < {threshold:P1}");
                } else if (threshold == -1f) {
                    warningSB.Append("Guaranteed Ammo Explosion!");
                }
                threshold = 0f;

                // Check Injury
                foreach (KeyValuePair<int, float> kvp in Mod.Config.Heat.PilotInjury) {
                    if (futureHeat >= kvp.Key) { threshold = kvp.Value; }
                }
                if (threshold != 0f) {
                    Mod.Log.Debug($"Injury Threshold: {threshold:P1} vs. d100+{heatCheck * 100f}");
                    descSB.Append($"\nPilot Injury on d100+{heatCheck * 100f} < {threshold:P1}");
                }
                threshold = 0f;

                // Check System Failure
                foreach (KeyValuePair<int, float> kvp in Mod.Config.Heat.SystemFailures) {
                    if (futureHeat >= kvp.Key) { threshold = kvp.Value; }
                }
                if (threshold != 0f) {
                    Mod.Log.Debug($"System Failure Threshold: {threshold:P1} vs. d100+{heatCheck * 100f}");
                    descSB.Append($"\nSystem Failure on d100+{heatCheck * 100f} < {threshold:P1}");
                }
                threshold = 0f;

                // Check Shutdown
                foreach (KeyValuePair<int, float> kvp in Mod.Config.Heat.Shutdown) {
                    if (futureHeat >= kvp.Key) { threshold = kvp.Value; }
                }
                if (threshold != 0f && threshold != -1f) {
                    Mod.Log.Debug($"Shutdown Threshold: {threshold:P1} vs. d100+{heatCheck * 100f}");
                    descSB.Append($"\nShutdown on d100+{heatCheck * 100f} < {threshold:P1}");
                } else if (threshold == -1f) {
                    warningSB.Append("\nGuaranteed Shutdown!");
                }
                threshold = 0f;

                // Attack modifiers
                int modifier = 0;
                foreach (KeyValuePair<int, int> kvp in Mod.Config.Heat.Firing) {
                    if (futureHeat >= kvp.Key) { modifier = kvp.Value; }
                }
                if (modifier != 0) {
                    Mod.Log.Debug($"Attack Modifier: +{modifier}");
                    descSB.Append($"\nAttack Penalty: +{modifier}");
                }
                modifier = 0;

                // Movement modifier
                foreach (KeyValuePair<int, int> kvp in Mod.Config.Heat.Firing) {
                    if (futureHeat >= kvp.Key) { modifier = kvp.Value; }
                }
                if (modifier != 0) {
                    Mod.Log.Debug($"Movement Modifier: -{modifier * 30}m");
                    descSB.Append($"\nMovement Penalty: -{modifier * 30}m");
                }
                modifier = 0;

                base.SetTitleDescAndWarning("Heat", descSB.ToString(), warningSB.ToString());
                Mod.Log.Info($" Updated values: t:'{base.Title}' / d:'{base.Description}' / w:'{base.WarningText}'");
            }

        }

    }
}
