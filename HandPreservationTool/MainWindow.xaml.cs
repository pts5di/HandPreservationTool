using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HandPreservationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.datadump.Text = "big BOY";
            Task.Run(async () =>
            {
                while (true)
                {
                    var str = "";
                    for (uint i = 0; i <= 3; i++)
                    {
                        var newState = new XInput.XInputState();
                        var result = XInput.XInputGetState(i, ref newState);
                        
                        str += $"Controller {i}: A is {((newState.Gamepad.wButtons & XInput.XInputGamepadButton.A) > 0 ? "" : "not")} pressed\n";
                    }
                    await this.Dispatcher.InvokeAsync(() => { this.datadump.Text = str; });
                    await Task.Delay(100);
                }
            });
        }
    }

    public static class XInput
    {
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
