﻿
using BattleTech;
using UnityEngine;

namespace CBTBehaviors {

    class MechEmergencyShutdownSequence : MultiSequence {
        
        private EmergencyShutdownState state;
        private float timeInCurrentState;
        private const float TimeToRise = 1f;
        private const float TimeToShutdown = 1f;

        private enum EmergencyShutdownState {
            None,
            Rising,
            ShuttingDown,
            Finished
        }

        public MechEmergencyShutdownSequence(Mech mech) : base(mech.Combat) {
            this.OwningMech = mech;
            this.setState(EmergencyShutdownState.Rising);
            //this.SetState();
        }

        private Mech OwningMech { get; set; }

        //private void SetState() {
        //    Mod.Log.Info($"Mech {this.OwningMech.DisplayName}_{this.OwningMech.GetPilot().Name} shuts down from overheating");
        //    this.OwningMech.IsShutDown = true;
        //    this.OwningMech.DumpAllEvasivePips();
        //}

        public override void OnAdded() {
            base.OnAdded();
            if (this.OwningMech.GameRep != null) {
                Mod.Log.Info("Sending floatie notification.");

                string text = string.Format("MechOverheatSequence_{0}_{1}", base.RootSequenceGUID, base.SequenceGUID);
                
                AudioEventManager.CreateVOQueue(text, -1f, null, null);
                AudioEventManager.QueueVOEvent(text, VOEvents.Mech_Overheat_Shutdown, this.OwningMech);
                AudioEventManager.StartVOQueue(1f);
                
                this.OwningMech.GameRep.PlayVFX(8, this.OwningMech.Combat.Constants.VFXNames.heat_heatShutdown, true, Vector3.zero, false, -1f);
               
                //base.Combat.MessageCenter.PublishMessage(new FloatieMessage(this.OwningMech.GUID, this.OwningMech.GUID, 
                //    "EMERGENCY SHUTDOWN!", FloatieMessage.MessageNature.Debuff));
                
                WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_overheat_alarm_3, WwiseManager.GlobalAudioObject, null, null);
                if (this.OwningMech.team.LocalPlayerControlsTeam) {
                    AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_overheating", null, null);
                }

            }
        }

        private void setState(EmergencyShutdownState newState) {
            Mod.Log.Info($"MESS - Setting state to: {newState} for actor: {CombatantHelper.LogLabel(this.OwningMech)}");
            if (this.state == newState) {
                return;
            }
            this.state = newState;
            this.timeInCurrentState = 0f;
            if (newState == EmergencyShutdownState.ShuttingDown) {
                if (this.OwningMech.GameRep != null) {
                    this.OwningMech.GameRep.PlayShutdownAnim();
                }
                this.OwningMech.CancelCreatedEffects();
                return;
            }
            if (newState != EmergencyShutdownState.Finished) {
                return;
            }

            Mech.heatLogger.Log("Mech " + this.OwningMech.DisplayName + " shuts down from overheating");
            this.OwningMech.IsShutDown = true;
            this.OwningMech.DumpAllEvasivePips();
        }

        private void UpdateState() {
            this.timeInCurrentState += Time.deltaTime;
            EmergencyShutdownState shutdownState = this.state;
            if (shutdownState != EmergencyShutdownState.Rising) {
                if (shutdownState != EmergencyShutdownState.ShuttingDown) {
                    return;
                }
                if (this.timeInCurrentState > TimeToShutdown) {
                    this.setState(EmergencyShutdownState.Finished);
                }
            } else if (this.timeInCurrentState > TimeToRise) {
                this.setState(EmergencyShutdownState.ShuttingDown);
                return;
            }
        }

        public override void OnUpdate() {
            this.UpdateState();
            //base.OnUpdate();
        }

        public override void OnSuspend() {
            base.OnSuspend();
        }

        public override void OnResume() {
            base.OnResume();
        }

        public override void OnComplete() {
            base.OnComplete();
        }

        public override bool IsValidMultiSequenceChild {
            get {
                return true;
            }
        }

        public override bool IsParallelInterruptable {
            get {
                return false;
            }
        }

        public override bool IsCancelable {
            get {
                return false;
            }
        }

        public override bool IsComplete {
            get {
                return this.state == EmergencyShutdownState.Finished;
            }
        }

        public override int Size() {
            return 0;
        }

        public override bool ShouldSave() {
            return false;
        }
    }
}
