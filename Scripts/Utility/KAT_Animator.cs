/*
	Avatar Text for VRChat
	Copyright (C) 2022 KillFrenzy / Evan Tran

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;
using AnimatorController = UnityEditor.Animations.AnimatorController;

using KillFrenzy.AvatarTextTools.Settings;

namespace KillFrenzy.AvatarTextTools.Utility
{
	public static class KatAnimatorInstaller
	{
		public static bool InstallToAnimator(AnimatorController controller, bool writeDefaults = false)
		{
			AnimationClip[] animationChars = GetCharAnimations();
			AnimationClip animationDisable = Resources.Load<AnimationClip>(KatSettings.CharacterAnimationFolder + "KAT_Disable");
			AnimationClip animationEnable = Resources.Load<AnimationClip>(KatSettings.CharacterAnimationFolder + "KAT_Enable");
			if (animationChars == null || animationDisable == null || animationEnable == null) {
				Debug.Log("Failed: Resources/" + KatSettings.CharacterAnimationFolder + " is missing some animations.");
				return false;
			}

			// Add parameters
			controller.AddParameter(KatSettings.ParamTextVisible, AnimatorControllerParameterType.Bool);
			controller.AddParameter(KatSettings.ParamTextPointer, AnimatorControllerParameterType.Int);

			for (int i = 0; i < KatSettings.SyncParamsSize; i++) {
				controller.AddParameter(KatSettings.ParamTextSyncPrefix + i.ToString(), AnimatorControllerParameterType.Float);
			}

			// Add Layers
			controller.AddLayer(CreateToggleLayer(controller, animationDisable, animationEnable));
			for (int i = 0; i < KatSettings.SyncParamsSize; i++) {
				controller.AddLayer(CreateSyncLayer(controller, i, animationChars));
			}

			return true;
		}

		public static bool RemoveFromAnimator(AnimatorController controller) {
			// Remove Layers
			for (int i = controller.layers.Length - 1; i >= 0; i--) {
				AnimatorControllerLayer layer = controller.layers[i];
				if (
					layer.name.StartsWith(KatSettings.ParamTextVisible) ||
					layer.name.StartsWith(KatSettings.ParamTextSyncPrefix)
				) {
					controller.RemoveLayer(i);
				}
			}

			// Remove Parameters
			for (int i = controller.parameters.Length - 1; i >= 0; i--) {
				AnimatorControllerParameter parameter = controller.parameters[i];
				if (
					parameter.name.StartsWith(KatSettings.ParamTextVisible) ||
					parameter.name.StartsWith(KatSettings.ParamTextPointer) ||
					parameter.name.StartsWith(KatSettings.ParamTextSyncPrefix)
				) {
					controller.RemoveParameter(i);
				}
			}

			return true;
		}

		private static AnimationClip[] GetCharAnimations() {
			AnimationClip[] animationClips = Resources.LoadAll<AnimationClip>(KatSettings.CharacterAnimationFolder);
			AnimationClip[] orderedAnimationClips = new AnimationClip[KatSettings.TextLength + 1];

			// Order the animation clips so they match their index
			for (int i = 0; i < KatSettings.TextLength; i++) {
				orderedAnimationClips[i] = null;
				foreach (var animationClip in animationClips) {
					if (animationClip.name == KatSettings.CharacterAnimationClipNamePrefix + i.ToString()) {
						orderedAnimationClips[i] = animationClip;
						break;
					}
				}
				if (orderedAnimationClips[i] == null) {
					return null;
				}
			}

			// Add the clear animation clip to the end
			orderedAnimationClips[KatSettings.TextLength] = null;
			foreach (var animationClip in animationClips) {
				if (animationClip.name == KatSettings.CharacterAnimationClipNamePrefix + "Clear") {
					orderedAnimationClips[KatSettings.TextLength] = animationClip;
					break;
				}
			}
			if (orderedAnimationClips[KatSettings.TextLength] == null) {
				return null;
			}

			return orderedAnimationClips;
		}

		private static AnimatorControllerLayer CreateToggleLayer(AnimatorController controller, AnimationClip animationDisable, AnimationClip animationEnable)
		{
			AnimatorControllerLayer layer = new AnimatorControllerLayer();
			AnimatorStateMachine stateMachine;

			layer.name = KatSettings.ParamTextVisible;
			layer.defaultWeight = 1f;
			layer.stateMachine = stateMachine = new AnimatorStateMachine();
			layer.stateMachine.name = layer.name;
			AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(controller));

			// Hide KAT state
			AnimatorState stateDisabled = stateMachine.AddState("Disabled", new Vector3(300f, 200f, 0f));
			stateDisabled.motion = animationDisable;
			stateDisabled.writeDefaultValues = false;
			stateDisabled.speed = 0f;

			// Show KAT state
			AnimatorState stateEnabled = stateMachine.AddState("Enabled", new Vector3(300f, 300f, 0f));
			stateEnabled.motion = animationEnable;
			stateEnabled.writeDefaultValues = false;
			stateEnabled.speed = 0f;

			AnimatorStateTransition enableTransition = stateDisabled.AddExitTransition(false);
			enableTransition.destinationState = stateEnabled;
			enableTransition.AddCondition(AnimatorConditionMode.If, 0, KatSettings.ParamTextVisible);
			enableTransition.duration = 0;

			AnimatorStateTransition disabledTransition = stateEnabled.AddExitTransition(false);
			disabledTransition.destinationState = stateDisabled;
			disabledTransition.AddCondition(AnimatorConditionMode.IfNot, 0, KatSettings.ParamTextVisible);
			disabledTransition.duration = 0;

			return layer;
		}

		private static AnimatorControllerLayer CreateSyncLayer(AnimatorController controller, int logicId, AnimationClip[] animationChars)
		{
			AnimatorControllerLayer layer = new AnimatorControllerLayer();
			AnimatorStateMachine stateMachine;

			layer.name = KatSettings.ParamTextSyncPrefix + logicId.ToString();
			layer.defaultWeight = 1f;
			layer.stateMachine = stateMachine = new AnimatorStateMachine();
			layer.stateMachine.name = layer.name;
			AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(controller));

			// Default state
			AnimatorState stateDisabled = stateMachine.AddState("Disabled", new Vector3(300f, 200f, 0f));
			stateDisabled.writeDefaultValues = false;
			stateDisabled.speed = 0f;

			// Standby state - waits for updates to the pointer
			AnimatorState stateStandby = stateMachine.AddState("StandBy", new Vector3(300f, 500f, 0f));
			stateStandby.writeDefaultValues = false;
			stateStandby.speed = 0f;

			AnimatorStateTransition enabledTransition = stateDisabled.AddExitTransition(false);
			enabledTransition.destinationState = stateStandby;
			enabledTransition.AddCondition(AnimatorConditionMode.NotEqual, 0, KatSettings.ParamTextPointer);
			enabledTransition.duration = 0;

			AnimatorStateTransition disabledTransition = stateMachine.AddAnyStateTransition(stateDisabled);
			disabledTransition.AddCondition(AnimatorConditionMode.Equals, 0, KatSettings.ParamTextPointer);
			disabledTransition.duration = 0;

			// Create pointer animations states
			for (int i = 0; i < KatSettings.PointerCount; i++) {
				int pointerIndex = i + 1;
				int charIndex = KatSettings.SyncParamsSize * i + logicId;

				float pointerPosOffsetY = 500f + 50f * i;

				AnimatorState statePointerChar = stateMachine.AddState("Pointer" + pointerIndex.ToString() + " Char" + charIndex, new Vector3(800f, pointerPosOffsetY, 0f));
				statePointerChar.motion = animationChars[charIndex];
				statePointerChar.timeParameter = KatSettings.ParamTextSyncPrefix + logicId.ToString();
				statePointerChar.timeParameterActive = true;
				statePointerChar.writeDefaultValues = false;
				statePointerChar.speed = 0f;

				AnimatorStateTransition pointerStartTransition = stateStandby.AddExitTransition(false);
				pointerStartTransition.destinationState = statePointerChar;
				pointerStartTransition.AddCondition(AnimatorConditionMode.Equals, pointerIndex, KatSettings.ParamTextPointer);
				pointerStartTransition.duration = 0;

				AnimatorStateTransition pointerReturnTransition = statePointerChar.AddExitTransition(false);
				pointerReturnTransition.destinationState = stateStandby;
				pointerReturnTransition.AddCondition(AnimatorConditionMode.NotEqual, pointerIndex, KatSettings.ParamTextPointer);
				pointerReturnTransition.duration = 0;
			}

			// Create clear state
			AnimatorState stateClear = stateMachine.AddState("Clear Text", new Vector3(800f, 500f - 50f, 0f));
			stateClear.motion = animationChars[KatSettings.TextLength];
			stateClear.timeParameter = KatSettings.ParamTextSyncPrefix + logicId.ToString();
			stateClear.timeParameterActive = true;
			stateClear.writeDefaultValues = false;
			stateClear.speed = 0f;

			AnimatorStateTransition clearStartTransition = stateStandby.AddExitTransition(false);
			clearStartTransition.destinationState = stateClear;
			clearStartTransition.AddCondition(AnimatorConditionMode.Equals, 255, KatSettings.ParamTextPointer);
			clearStartTransition.duration = 0;

			AnimatorStateTransition clearReturnTransition = stateClear.AddExitTransition(false);
			clearReturnTransition.destinationState = stateStandby;
			clearReturnTransition.AddCondition(AnimatorConditionMode.NotEqual, 255, KatSettings.ParamTextPointer);
			clearReturnTransition.duration = 0;

			return layer;
		}
	}
}

#endif
