using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Preview.Injection;

namespace LoLController
{
    public class Input
    {
        private InputInjector Injector;
        public Input()
        {
            Injector = InputInjector.TryCreate();
        }

        public void ClickRMB()
        {
            var down = new InjectedInputMouseInfo { MouseOptions = InjectedInputMouseOptions.RightDown };
            var up = new InjectedInputMouseInfo { MouseOptions = InjectedInputMouseOptions.RightUp };
            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { down, up });
        }
        public void ClickLMB()
        {
            var down = new InjectedInputMouseInfo { MouseOptions = InjectedInputMouseOptions.LeftDown };
            var up = new InjectedInputMouseInfo { MouseOptions = InjectedInputMouseOptions.LeftUp };
            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { down, up });
        }
        public async void DoubleClickLMB()
        {
            ClickLMB();
            await Task.Delay(5);
            ClickLMB();
        }
        public void ClickLMBDown()
        {
            var down = new InjectedInputMouseInfo { MouseOptions = InjectedInputMouseOptions.LeftDown };
            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { down });
        }
        public void ClickLMBUp()
        {
            var up = new InjectedInputMouseInfo { MouseOptions = InjectedInputMouseOptions.LeftUp };
            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { up });
        }

        public void MouseScroll(int direction)
        {
            var dir = Math.Sign(direction);
            var reset = new InjectedInputMouseInfo() { MouseOptions = InjectedInputMouseOptions.Wheel, MouseData = 0 };

            uint scroll = 0;
            unchecked
            {
                scroll = (uint)direction;
            }

            var mouseScroll = new InjectedInputMouseInfo() { MouseOptions = InjectedInputMouseOptions.Wheel, MouseData = scroll };
            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { mouseScroll, reset });
        }

        public void SendKeyStroke(ushort scanCode)
        {
            var keyDown = new InjectedInputKeyboardInfo();
            keyDown.ScanCode = scanCode;
            keyDown.KeyOptions = InjectedInputKeyOptions.ScanCode;

            var keyUp = new InjectedInputKeyboardInfo();
            keyUp.ScanCode = scanCode;
            keyUp.KeyOptions = InjectedInputKeyOptions.ScanCode | InjectedInputKeyOptions.KeyUp;

            Injector.InjectKeyboardInput(new List<InjectedInputKeyboardInfo> { keyDown, keyUp });
        }

        public void SendKeyStrokes(params ushort[] scanCode)
        {
            var keyDowns = scanCode.Select(s => new InjectedInputKeyboardInfo { ScanCode = s, KeyOptions = InjectedInputKeyOptions.ScanCode });
            var keyUps = scanCode.Select(s => new InjectedInputKeyboardInfo { ScanCode = s, KeyOptions = InjectedInputKeyOptions.ScanCode | InjectedInputKeyOptions.KeyUp });

            var list = new List<InjectedInputKeyboardInfo>();
            list.AddRange(keyDowns);
            list.AddRange(keyUps);

            Injector.InjectKeyboardInput(list);
        }

        public void SendKeyStrokeDown(ushort scanCode)
        {
            var keyDown = new InjectedInputKeyboardInfo();
            keyDown.ScanCode = scanCode;
            keyDown.KeyOptions = InjectedInputKeyOptions.ScanCode;
            Injector.InjectKeyboardInput(new List<InjectedInputKeyboardInfo> { keyDown });
        }

        public void SendKeyStrokeUp(ushort scanCode)
        {
            var keyUp = new InjectedInputKeyboardInfo();
            keyUp.ScanCode = scanCode;
            keyUp.KeyOptions = InjectedInputKeyOptions.ScanCode | InjectedInputKeyOptions.KeyUp;
            Injector.InjectKeyboardInput(new List<InjectedInputKeyboardInfo> { keyUp });
        }

        public void ResetMouse()
        {
            var reset = new InjectedInputMouseInfo { DeltaX = 32678, DeltaY = 32678, MouseOptions = InjectedInputMouseOptions.Absolute };
            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { reset });
        }

        public void ResetAndMoveMouse(int dx, int dy)
        {
            var reset = new InjectedInputMouseInfo { DeltaX = 32678, DeltaY = 32678, MouseOptions = InjectedInputMouseOptions.Absolute };
            var move = new InjectedInputMouseInfo { DeltaX = dx, DeltaY = dy };

            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { reset, move });
        }

        public void MoveMouse(int dx, int dy)
        {
            var move = new InjectedInputMouseInfo { DeltaX = dx, DeltaY = dy };
            Injector.InjectMouseInput(new List<InjectedInputMouseInfo> { move });
        }
    }
}
