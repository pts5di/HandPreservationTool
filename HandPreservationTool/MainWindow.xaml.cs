using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using vJoyInterfaceWrap;

namespace HandPreservationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static void WriteTrace(string str)
        {
            Trace.WriteLine($"HPT_DEBUG: {str}");
        }

        public MainWindow()
        {

            // initialize UI
            InitializeComponent();
            this.datadump.Text = "big BOY";

            Task.Run(async () =>
            {
                var joystick = new vJoy();

                if (!joystick.vJoyEnabled())
                {
                    WriteTrace("vJoy driver not enabled: Failed getting vJoy attributes.");
                    return;
                }
                else
                {
                    WriteTrace($"Vendor: {joystick.GetvJoyManufacturerString()}\nProduct :{joystick.GetvJoyProductString()}\nVersion Number:{joystick.GetvJoySerialNumberString()}");
                }

                // initial state
                var possibleIds = new uint[11] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                var enabledIds = possibleIds.Where(id => joystick.isVJDExists(id)).ToArray();
                var chosenVJoyId = enabledIds[0];
                uint chosenXboxId = uint.MaxValue;

                // initialize XInput
                XInput.XInputEnable(true);

                // initialize VJoy
                VjdStat status = joystick.GetVJDStatus(chosenVJoyId);
                joystick.AcquireVJD(chosenVJoyId);

                var initialBias = new vJoy.JoystickState();

                while (true)
                {
                    var str = "";
                    for (uint i = 0; i <= 3; i++)
                    {
                        var newState = new XInput.XInputState();
                        var result = XInput.XInputGetState(i, ref newState);
                        
                        if (result != 0)
                        {
                            str += $"Controller {i}: is disconnected\n";
                        }
                        else if (chosenXboxId == i)
                        {
                            str += $"Controller {i} has been chosen\n";
                        } 
                        else if (chosenXboxId == uint.MaxValue)
                        {
                            // if the xbox controller hasn't been selected,
                            // just pick the first one forever (for now -- we will
                            // eventually want to add a dropdown to let the user pick)
                            chosenXboxId = i;
                            initialBias.AxisX = newState.Gamepad.sThumbLX;
                            initialBias.AxisY = newState.Gamepad.sThumbLY;
                            initialBias.AxisXRot = newState.Gamepad.sThumbRX;
                            initialBias.AxisYRot = newState.Gamepad.sThumbRY;
                        }
                        foreach (XInput.XInputGamepadButton button in Enum.GetValues(typeof(XInput.XInputGamepadButton)))
                        {
                            if ((newState.Gamepad.wButtons & button) > 0)
                            {
                                var buttonName = Enum.GetName(typeof(XInput.XInputGamepadButton), button);
                                str += $"Controller {i}: {buttonName} is pressed\n";
                            }
                        }

                        str += $"Controller {i}: Left Stick: {newState.Gamepad.sThumbLX}X, {newState.Gamepad.sThumbLY}Y\n";
                        str += $"Controller {i}: Right Stick: {newState.Gamepad.sThumbRX}X, {newState.Gamepad.sThumbRY}Y\n";
                    }

                    var chosenXboxState = new XInput.XInputState();
                    XInput.XInputGetState(chosenXboxId, ref chosenXboxState);
                    var xPad = chosenXboxState.Gamepad;
                    var xPadButtons = (uint)xPad.wButtons;

                    uint aButton = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.A)) > 0 ? 0x1 << 0 : 0);
                    uint bButton = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.B)) > 0 ? 0x1 << 1 : 0);
                    uint xButton = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.X)) > 0 ? 0x1 << 2 : 0);
                    uint yButton = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.Y)) > 0 ? 0x1 << 3 : 0);
                    uint lBumper = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.LeftShoulder)) > 0 ? 0x1 << 4 : 0);
                    uint rBumper = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.RightShoulder)) > 0 ? 0x1 << 5 : 0);
                    uint select = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.Back)) > 0 ? 0x1 << 6 : 0);
                    uint start = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.Start)) > 0 ? 0x1 << 7 : 0);
                    uint lStick = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.LeftThumb)) > 0 ? 0x1 << 8 : 0);
                    uint rStick = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.RightThumb)) > 0 ? 0x1 << 9 : 0);
                    uint guide = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.Guide)) > 0 ? 0x1 << 10 : 0);
                    uint dPadUp = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.DPadUp)) > 0 ? 0x1 << 11 : 0);
                    uint dPadDown = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.DPadDown)) > 0 ? 0x1 << 12 : 0);
                    uint dPadLeft = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.DPadLeft)) > 0 ? 0x1 << 13 : 0);
                    uint dPadRight = (uint)((xPadButtons & ((uint)XInput.XInputGamepadButton.DPadRight)) > 0 ? 0x1 << 14 : 0);

                    uint buttons = aButton | bButton | xButton | yButton | lBumper | rBumper | select | start | lStick | rStick | guide | dPadUp | dPadDown | dPadLeft | dPadRight;


                    var vjoyInput = new vJoy.JoystickState()
                    {
                        bDevice = (byte)chosenVJoyId,
                        AxisX = ((int)xPad.sThumbLX) - initialBias.AxisX,
                        AxisY = ((int)-xPad.sThumbLY) - initialBias.AxisY,
                        AxisXRot = xPad.sThumbRX - initialBias.AxisXRot,
                        AxisYRot = xPad.sThumbRY - initialBias.AxisYRot,
                        AxisZ = xPad.bRightTrigger,
                        AxisZRot = xPad.bLeftTrigger,
                        Buttons = buttons,
                    };

                    joystick.UpdateVJD(chosenVJoyId, ref vjoyInput);

                    await this.Dispatcher.InvokeAsync(() => { this.datadump.Text = str; });
                    await Task.Delay(10);
                }
            });
        }
    }

}
