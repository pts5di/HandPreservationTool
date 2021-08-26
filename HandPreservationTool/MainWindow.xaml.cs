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
                        }
                        foreach (XInput.XInputGamepadButton button in Enum.GetValues(typeof(XInput.XInputGamepadButton)))
                        {
                            if ((newState.Gamepad.wButtons & button) > 0)
                            {
                                var buttonName = Enum.GetName(typeof(XInput.XInputGamepadButton), button);
                                str += $"Controller {i}: {buttonName} is pressed\n";
                            }
                        }
                    }

                    var chosenXboxState = new XInput.XInputState();
                    XInput.XInputGetState(chosenXboxId, ref chosenXboxState);
                    var xPad = chosenXboxState.Gamepad;
                    var xPadButtons = (uint)xPad.wButtons;

                    uint aButton = (uint)((xPadButtons & (0x1 << 12)) > 0 ? 0x1 << 0 : 0);
                    uint bButton = (uint)((xPadButtons & (0x1 << 13)) > 0 ? 0x1 << 1 : 0);

                    uint buttons = aButton | bButton;


                    var vjoyInput = new vJoy.JoystickState()
                    {
                        bDevice = (byte)chosenVJoyId,
                        AxisX = xPad.sThumbLX,
                        AxisY = -xPad.sThumbLY,
                        AxisXRot = xPad.sThumbRX,
                        AxisYRot = xPad.sThumbRY,
                        AxisZ = xPad.bRightTrigger,
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
