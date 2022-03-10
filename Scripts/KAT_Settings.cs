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

namespace KillFrenzy.AvatarTextTools.Settings
{
	public static class KatSettings
	{
		public const string ParamTextVisible = "KAT_Visible";
		public const string ParamTextPointer = "KAT_Pointer";
		public const string ParamTextSyncPrefix = "KAT_CharSync";

		public const string ParamKeyboardPrefix = "KAT_Keyboard";
		public const string ParamKeyboardPressed = "KAT_KeyboardPressed";
		public const string ParamKeyboardShift = "KAT_KeyboardShift";
		public const string ParamKeyboardProximity = "KAT_KeyboardProximity";
		public const string ParamKeyboardProximityExit = "KAT_KeyboardProximityExit";
		public const string ParamKeyboardHighlight = "KAT_KeyboardHighlight";
		public const string ParamKeyboardPressedClear = ParamKeyboardPressed + "Clear";
		public const string ParamKeyboardPressedBackspace = ParamKeyboardPressed + "Bksp";
		public const string ParamKeyboardPressedShiftL = ParamKeyboardPressed + "ShiftL";
		public const string ParamKeyboardPressedShiftR = ParamKeyboardPressed + "ShiftR";

		public const int KeyboardKeyBackspace = 97;
		public const int KeyboardKeyClear = 96;
		public const int KeyboardKeyShiftL = 98;
		public const int KeyboardKeyShiftR = 99;
		public const float KeyboardBackspaceMode = (float)KeyboardKeyBackspace / 127f;

		public const int KeyboardKeysCount = 96;

		public const string CharacterAnimationFolder = "KAT_CharAnimations/";
		public const string CharacterAnimationClipNamePrefix = "Char";
		public const int TextLength = 128;
		public const int SyncParamsSize = 4;
		public const int PointerCount = TextLength / SyncParamsSize;
		public const int PointerAltSyncOffset = 100;

		public const string GeneratedOutputFolderName = "KAT_GeneratedOutput";

		public const string TextAttachmentPointName = "KAT_Text_AttachmentPoint";
		public const string KeyboardAttachmentPointName = "KAT_Keyboard_AttachmentPoint";
	}

	public static class KatAttachmentPoint
	{
		public const int None = 0;
		public const int Head = 1;
		public const int Chest = 2;
	}
}

#endif
