using System;
using System.Runtime.InteropServices;

namespace HandPreservationTool
{
    public static class XInput
    {
        [DllImport("XINPUT1_4.DLL")]
        public static extern void XInputEnable(bool enabled);

        [DllImport("XINPUT1_4.DLL")]
        public static extern XInputResult XInputGetState(uint dwUserIndex, ref XInputState pState);

        [StructLayout(LayoutKind.Sequential)]
        public struct XInputState
        {
            public uint dwPacketNumber;
            public XInputGamepad Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XInputGamepad
        {
            public XInputGamepadButton wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        [Flags]
        public enum XInputResult : uint
        {
            Success = 0,
            ErrorDeviceNotConnected = 0x48F,
        }

        [Flags]
        public enum XInputGamepadButton : ushort
        {
            DPadUp = 0x1,
            DPadDown = 0x2,
            DPadLeft = 0x4,
            DPadRight = 0x8,
            Start = 0x10,
            Back = 0x20,
            LeftThumb = 0x40,
            RightThumb = 0x80,
            LeftShoulder = 0x100,
            RightShoulder = 0x200,
            Guide = 0x400,
            A = 0x1000,
            B = 0x2000,
            X = 0x4000,
            Y = 0x8000,
        }
    }

}
