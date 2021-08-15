using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            //var joystick = new vJoy();
            InitializeComponent();
            XInput.XInputEnable(true);
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
                        
                        if (result != 0)
                        {
                            str += $"Controller {i}: is disconnected\n";

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
                    await this.Dispatcher.InvokeAsync(() => { this.datadump.Text = str; });
                    await Task.Delay(100);
                }
            });
        }
    }

}
