using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using System.Collections;
using System.Numerics;

namespace LoLController
{
    public class ControllerState
    {
        public ulong TimestampUlong { get; private set; }
        public DateTime Timestamp => DateTime.FromBinary((long)TimestampUlong);

        public IReadOnlyList<bool> Buttons { get; private set; } = new List<bool>();
        public IReadOnlyList<GameControllerSwitchPosition> Switches { get; private set; } = new List<GameControllerSwitchPosition>();
        public IReadOnlyList<double> Axis { get; private set; } = new List<double>();

        public ControllerState(ulong stamp, IEnumerable<bool> buttons, IEnumerable<GameControllerSwitchPosition> switches, IEnumerable<double> axis)
        {
            TimestampUlong = stamp;
            Buttons = buttons.ToList();
            Switches = switches.ToList();
            Axis = axis.ToList();
        }

        public static ControllerState GetFrom(RawGameController controller)
        {
            var buttons = new bool[controller.ButtonCount];
            var switches = new GameControllerSwitchPosition[controller.SwitchCount];
            var axis = new double[controller.AxisCount];
            var stamp = controller.GetCurrentReading(buttons, switches, axis);

            return new ControllerState(stamp, buttons, switches, axis);
        }

        public bool CanBeDualshock4()
        {
            return Buttons.Count == 14 && Switches.Count == 1 && Axis.Count == 6;
        }

        public Dualshock4State ToDualshock4State()
        {
            if (!CanBeDualshock4()) throw new Exception();

            return new Dualshock4State(this, Buttons[0], Buttons[1], Buttons[2], Buttons[3], Buttons[4], Buttons[5], Buttons[6], Buttons[7], Buttons[8], Buttons[9], Buttons[10], Buttons[11], Buttons[12], Buttons[13],
                                       Switches[0],
                                       new Vector2((float)Axis[0], (float)Axis[1]), new Vector2((float)Axis[2], (float)Axis[5]), Axis[3], Axis[4]);
        }
    }

    public class Dualshock4State
    {
        public ControllerState State { get; private set; }

        public bool Square { get; private set; }
        public bool Cross { get; private set; }
        public bool Circle { get; private set; }
        public bool Triangle { get; private set; }
        public bool L1 { get; private set; }
        public bool R1 { get; private set; }
        public bool L2 { get; private set; }
        public bool R2 { get; private set; }
        public bool Share { get; private set; }
        public bool Options { get; private set; }
        public bool LeftStickClick { get; private set; }
        public bool RightStickClick { get; private set; }
        public bool MiddleButtonClick { get; private set; }
        public bool TouchPadPress { get; private set; }

        public GameControllerSwitchPosition DPad { get; private set; }

        public Vector2 LeftStick { get; private set; }
        public Vector2 RightStick { get; private set; }
        public double L2Weight { get; private set; }
        public double R2Weight { get; private set; }

        public Dualshock4State(ControllerState state, bool square, bool cross, bool circle, bool triangle, bool l1, bool r1, bool l2, bool r2, bool share, bool options, bool leftStickClick, bool rightStickClick, bool middleButtonClick, bool touchPadPress, GameControllerSwitchPosition dPad, Vector2 leftStick, Vector2 rightStick, double l2Weight, double r2Weight)
        {
            State = state;
            Square = square;
            Cross = cross;
            Circle = circle;
            Triangle = triangle;
            L1 = l1;
            R1 = r1;
            L2 = l2;
            R2 = r2;
            Share = share;
            Options = options;
            LeftStickClick = leftStickClick;
            RightStickClick = rightStickClick;
            MiddleButtonClick = middleButtonClick;
            TouchPadPress = touchPadPress;
            DPad = dPad;
            LeftStick = leftStick;
            RightStick = rightStick;
            L2Weight = l2Weight;
            R2Weight = r2Weight;
        }


    }
}
