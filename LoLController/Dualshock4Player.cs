using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Popups;
using System.Windows.Input;
using System.Reflection.Metadata;
using System.Numerics;
using Windows.Web.UI.Interop;
using Windows.Gaming.Input;
using Newtonsoft.Json;
using Windows.UI.Composition;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Controls;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls.Primitives;

namespace LoLController

{
    public class PlayerDescription
    {
        private List<Run> Runs = new List<Run>();

        public void FillTextBlock(TextBlock holder)
        {
            holder.Inlines.Clear();
            foreach (var run in Runs) holder.Inlines.Add(run);
        }

        public PlayerDescription AddRun(string text, params Action<Run>[] modifiers)
        {
            var run = new Run() { Text = text };
            foreach (var mod in modifiers) mod(run);

            Runs.Add(run);
            return this;
        }

        public PlayerDescription(string text) 
        {
            AddRun(text);
        }
    }

    public class PlayerHolder<T>
    {
        private Dictionary<T, Func<ILeagueOfLegendsPlayer>> RegisteredPlayers = new Dictionary<T, Func<ILeagueOfLegendsPlayer>>();

        public ILeagueOfLegendsPlayer SelectedPlayer { get; private set; }

        public void AddPlayer(T key, Func<ILeagueOfLegendsPlayer> player)
        {
            RegisteredPlayers[key] = player;
        }

        public void RemovePlayer(T key)
        {
            RegisteredPlayers.Remove(key);
        }

        public void SelectPlayer(T key, ControlsConfig config)
        {
            SelectedPlayer = RegisteredPlayers.ContainsKey(key) ? RegisteredPlayers[key]() : new DisabledPlayer();
            SelectedPlayer.UpdateConfig(config);
        }
    }

    public class ControlsConfig
    {
        public float RightStickSensitivity = 30f;
        public float RightStickMapSensitivity = 5f;

        public ushort ShopBind = 25; // P
        public ushort BaseBind = 48; // B 
        public ushort StopBind = 31; // S
        public ushort AttackChampionsOnlyBind = 41; // `

        public ushort SummonerSpell1Bind = 32; // D
        public ushort SummonerSpell2Bind = 33; // F

        public ushort LockCameraBind = 21; // Y
        public ushort AttackMoveBind = 30; // A

        public ushort QAbilityBind = 16; // Q
        public ushort WAbilityBind = 17; // W
        public ushort EAbilityBind = 18; // E
        public ushort RAbilityBind = 19; // R

        public ushort TabBind = 15; // Tab
        public ushort EmoteWheelBind = 20; // T
        public ushort PingsBind = 34; // G
        public ushort DanceBind = 4; // 3

        public ushort Item1Bind = 2;
        public ushort Item2Bind = 3;
        public ushort Item3Bind = 4;
        public ushort Item4Bind = 5;
        public ushort Item5Bind = 6;
        public ushort Item6Bind = 7;
        public ushort Item7Bind = 8;

        public ControlsConfig Copy() // good code quality
        {
            return new ControlsConfig()
            {
                RightStickSensitivity = this.RightStickSensitivity,
                RightStickMapSensitivity = this.RightStickMapSensitivity,

                ShopBind = this.ShopBind,
                BaseBind = this.BaseBind,
                StopBind = this.StopBind,
                AttackChampionsOnlyBind = this.AttackChampionsOnlyBind,

                SummonerSpell1Bind = this.SummonerSpell1Bind,
                SummonerSpell2Bind = this.SummonerSpell2Bind,

                LockCameraBind = this.LockCameraBind,
                AttackMoveBind = this.AttackMoveBind,

                QAbilityBind = this.QAbilityBind,
                WAbilityBind = this.WAbilityBind,
                EAbilityBind = this.EAbilityBind,
                RAbilityBind = this.RAbilityBind,

                TabBind = this.TabBind,
                EmoteWheelBind = this.EmoteWheelBind,
                PingsBind = this.PingsBind,
                DanceBind = this.DanceBind,

                Item1Bind = this.Item1Bind,
                Item2Bind = this.Item2Bind,
                Item3Bind = this.Item3Bind,
                Item4Bind = this.Item4Bind,
                Item5Bind = this.Item5Bind,
                Item6Bind = this.Item6Bind,
                Item7Bind = this.Item7Bind
            };
        }

        [JsonIgnore] public ushort[] ItemBinds => new ushort[] { Item1Bind, Item2Bind, Item3Bind, Item4Bind, Item5Bind, Item6Bind, Item7Bind }; // 1 2 3 4 5 6 7
    }

    public interface ILeagueOfLegendsPlayer
    {
        PlayerDescription GetDescription();
        void UpdateConfig(ControlsConfig config);
        void OnTick(ControllerState state);
    }

    public class DisabledPlayer : ILeagueOfLegendsPlayer
    {
        public PlayerDescription GetDescription() => new PlayerDescription(
            "This player will not perform any actions.");
        public void OnTick(ControllerState state) { }
        public void UpdateConfig(ControlsConfig config) { }
    }

    public class Dualshock4Player : ILeagueOfLegendsPlayer
    {
        public ControlsConfig Config = new ControlsConfig();


        public PlayerDescription GetDescription() => new PlayerDescription("")
            .AddRun("This is a player created to play League with PS4 controller.\n\n")
            .AddRun("                             Controls\n\n", r => r.FontWeight = FontWeights.ExtraBold)
            .AddRun("Right stick:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" moving cursor.\n")
            .AddRun("Right stick click:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" hold Left Mouse Button and slow down cursor movement.\n")
            .AddRun("Left stick:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" moving your character. Works best with locked camera.\n")
            .AddRun("Left stick click:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" stop character movement.\n\n")
            .AddRun("Square:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" Q Ability.\n")
            .AddRun("Triangle:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" W Ability.\n")
            .AddRun("Circle:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" E Ability.\n")
            .AddRun("Cross:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" R Ability.\n\n")
            .AddRun("D-Pad Up:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" lock/unlock camera.\n")
            .AddRun("D-Pad Right:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" stop character and reset camera.\n")
            .AddRun("D-Pad Down:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" bring up emote wheel.\n")
            .AddRun("D-Pad Left:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" bring up pings wheel.\n\n")
            .AddRun("Touch pad press:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" open up Tab menu.\n")
            .AddRun("Options:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" teleport to base.\n")
            .AddRun("Share:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" open the shop.\n\n")
            .AddRun("L1:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" use the item in the first slot.\n")
            .AddRun("R2:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" while holding L1, press to select next item slot. If pressed simultaniously with any ability button, puts a level point into that ability. If neither L1 nor abilities are being held, performs RMB and attack move.\n")
            .AddRun("R1:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" use the item in the fourth slot (usually ward).\n")
            .AddRun("L2:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" hold LMB. Hold to modify some buttons' purpose.\n\n")
            .AddRun("While holding L2, some of the controls are altered to do different stuff. Here is the list of altered controls:\n\n")
            .AddRun("L2 + Left stick:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" scroll mouse wheel.\n\n")
            .AddRun("L2 + Square:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" first summoner's spell.\n")
            .AddRun("L2 + Triangle:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" second summoner's spell.\n\n")
            .AddRun("L2 + D-Pad Down:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" perform dance command.\n")
            .AddRun("L2 + D-Pad Left:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" attack champions only.\n\n")
            .AddRun("L2 + Share:", r => r.FontWeight = FontWeights.Bold)
            .AddRun(" double-click LMB, thus buying an item in the shop.\n")
        ;

        private const string LoLWindowName = "League of Legends (TM) Client";

        private WindowInfo LoLWindow;
        private Input Injector;

        public Dualshock4Player()
        {
            Injector = new Input();
        }

        public void UpdateConfig(ControlsConfig config)
        {
            Config = config;
        }

        public void OnTick(ControllerState state)
        {
            if (!state.CanBeDualshock4()) return;

            //var window = WindowInfo.GetActiveWindow();
            //if (window is null || window.Title != LoLWindowName) return;
            var screen = WindowInfo.GetCurrentDisplaySize();
            var window = new WindowInfo(IntPtr.Zero, LoLWindowName, new RECT() { Top = 0, Left = 0, Bottom = (int)screen.Height, Right = (int)screen.Width });

            LoLWindow = window;
            ControlTick(state.ToDualshock4State());
        }

        private int LeftStickMoveCounter = 0;
        private Vector2 LeftStickPreviousActive = new Vector2();
        private int SelectedItem = 0;

        private Dictionary<string, int> Delays = new Dictionary<string, int>();


        private Dualshock4State PreviousState;
        private void ControlTick(Dualshock4State state)
        {
            var screenCenter = new Vector2(LoLWindow.Rectangle.Right - LoLWindow.Rectangle.Left, LoLWindow.Rectangle.Bottom - LoLWindow.Rectangle.Top);
            var cursorPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;

            var screenWidth = LoLWindow.Rectangle.Right - LoLWindow.Rectangle.Left;
            var screenHeight = LoLWindow.Rectangle.Bottom - LoLWindow.Rectangle.Top;

            var leftStickMagnitude = FixedStickCoordinates(state.LeftStick).Length();
            var rightStickMagnitude = FixedStickCoordinates(state.RightStick).Length();

            var modifierActive = state.L2;
            var normalMode = !modifierActive;

            var usingAbilities = state.Square || state.Triangle || state.Circle || state.Cross;

            // Utilities
            HandleSingleClick(state, s => DPadIs(s, GameControllerSwitchPosition.Right), () => StopCommand());
            HandleSingleClick(state, s => DPadIs(s, GameControllerSwitchPosition.Up), () => Injector.SendKeyStroke(Config.LockCameraBind));
            HandleSingleClick(state, s => s.LeftStickClick, () => Injector.SendKeyStroke(Config.StopBind));

            HandleHolding(state, s => s.TouchPadPress, () => Injector.SendKeyStrokeDown(Config.TabBind), () => Injector.SendKeyStrokeUp(Config.TabBind));

            HandleHolding(state, s => s.L1,
            () =>
            {
                SelectedItem = 0;
            },
            () =>
            {
                var bind = Config.ItemBinds[SelectedItem];
                Injector.SendKeyStroke(bind);
                SelectedItem = 0;
            });

            HandleHolding(state, s => s.L2, () => Injector.ClickLMBDown(), () => Injector.ClickLMBUp());

            HandleSingleClick(state, s => s.R2, async () =>
            {
                SelectedItem = (SelectedItem + 1) % 7;

                if (state.L1) return;

                Injector.ClickRMB();

                if (usingAbilities) return;

                await Task.Delay(10);
                Injector.SendKeyStroke(Config.AttackMoveBind);
            }); 
            
            HandleSingleClick(state, s => s.R1, () =>
            {
                var bind = Config.ItemBinds[3]; // ward
                Injector.SendKeyStroke(bind);
            });

            HandleHolding(state, s => s.RightStickClick, () => Injector.ClickLMBDown(), () => Injector.ClickLMBUp());

            if (true) // Normal Mode
            {
                // *Calm* buttons
                HandleSingleClick(state, s => s.Share && normalMode, () => Injector.SendKeyStroke(Config.ShopBind));
                HandleSingleClick(state, s => s.Options && normalMode, () => Injector.SendKeyStroke(Config.BaseBind));

                // Abilities
                HandleHolding(state, s => s.Square && normalMode, () => { if (state.R2) Injector.SendKeyStrokes(29, Config.QAbilityBind); else Injector.SendKeyStrokeDown(Config.QAbilityBind); }, () => Injector.SendKeyStrokeUp(Config.QAbilityBind));
                HandleHolding(state, s => s.Triangle && normalMode, () => { if (state.R2) Injector.SendKeyStrokes(29, Config.WAbilityBind); else Injector.SendKeyStrokeDown(Config.WAbilityBind); }, () => Injector.SendKeyStrokeUp(Config.WAbilityBind));
                HandleHolding(state, s => s.Circle && normalMode, () => { if (state.R2) Injector.SendKeyStrokes(29, Config.EAbilityBind); else Injector.SendKeyStrokeDown(Config.EAbilityBind); }, () => Injector.SendKeyStrokeUp(Config.EAbilityBind));
                HandleHolding(state, s => s.Cross && normalMode, () => { if (state.R2) Injector.SendKeyStrokes(29, Config.RAbilityBind); else Injector.SendKeyStrokeDown(Config.RAbilityBind); }, () => Injector.SendKeyStrokeUp(Config.RAbilityBind));

                // Utilities (these are in normal mode, because you need Right Stick to aim)
                HandleHolding(state, s => DPadIs(s, GameControllerSwitchPosition.Left) && normalMode, () => Injector.SendKeyStrokeDown(Config.PingsBind), () => Injector.SendKeyStrokeUp(Config.PingsBind));
                HandleHolding(state, s => DPadIs(s, GameControllerSwitchPosition.Down) && normalMode, () => Injector.SendKeyStrokeDown(Config.EmoteWheelBind), () => Injector.SendKeyStrokeUp(Config.EmoteWheelBind));
            }

            if(true) // Modified mode
            {
                // Summoners
                HandleSingleClick(state, s => s.Square && modifierActive, () => Injector.SendKeyStroke(Config.SummonerSpell1Bind));
                HandleSingleClick(state, s => s.Triangle && modifierActive, () => Injector.SendKeyStroke(Config.SummonerSpell2Bind));

                // Utilities
                HandleHolding(state, s => DPadIs(s, GameControllerSwitchPosition.Left) && modifierActive, () => Injector.SendKeyStrokeDown(Config.AttackChampionsOnlyBind), () => Injector.SendKeyStrokeUp(Config.AttackChampionsOnlyBind)); // if you for some godforsaken reason do not have it set to toggle mode
                HandleSingleClick(state, s => DPadIs(s, GameControllerSwitchPosition.Down) && modifierActive, () => Injector.SendKeyStrokes(29, Config.DanceBind));

                // *Calm* buttons
                HandleHolding(state, s => s.Share && modifierActive, async () => { Injector.ClickLMB(); await Task.Delay(5); Injector.ClickLMBDown(); }, () => Injector.ClickLMBUp());
            }











            // Aiming and scrolling
            var rightStick = FixedStickCoordinates(state.RightStick);
            if(rightStickMagnitude > 0.1f)
            {
                if(state.L2)
                {
                    var dir = -Math.Sign(rightStick.Y);
                    Injector.MouseScroll(dir * 10);
                }
                else
                {
                    var sensitivity = state.RightStickClick ? Config.RightStickMapSensitivity : Config.RightStickSensitivity;
                    Injector.MoveMouse((int)(rightStick.X * sensitivity), (int)(rightStick.Y * sensitivity));
                }
            }

            // Movement and aiming
            var leftStick = FixedStickCoordinates(state.LeftStick);
            var leftStickNormalized = Vector2.Normalize(leftStick);
            if (leftStickMagnitude > 0.1f)
            {
                var delta = Vector2.Distance(LeftStickPreviousActive, leftStick);
                var usePreviousStick = delta < 0.01f;

                LeftStickMoveCounter++;
                var maxDistance = screenHeight / 4;

                var analyzedStick = usePreviousStick ? LeftStickPreviousActive : leftStick;
                var newPosition = GetStickMovement(analyzedStick, analyzedStick.Length(), maxDistance);

                var newXPosition = newPosition.X;
                var newYPosition = newPosition.Y;

                Injector.ResetAndMoveMouse((int)newXPosition, (int)newYPosition);

                if (LeftStickMoveCounter % 20 == 0 && !state.LeftStickClick && !usingAbilities)
                    Injector.ClickRMB();

                if (!usePreviousStick) LeftStickPreviousActive = leftStick;
            }




            PreviousState = state;
        }

        private async void StopCommand()
        {
            Injector.SendKeyStrokes(Config.StopBind, Config.LockCameraBind); await Task.Delay(5); Injector.SendKeyStroke(Config.LockCameraBind); Injector.ResetMouse();
        }

        private Vector2 GetStickMovement(Vector2 direction, float magnitude, float maxDistance)
        {
            var x = (float)(direction.X * Math.Pow(magnitude * 2, 2) * maxDistance);
            var y = (float)(direction.Y * Math.Pow(magnitude * 2, 2) * maxDistance);

            return new Vector2(x, y);
        }

        private Vector2 FixedStickCoordinates(Vector2 stick) => new Vector2(stick.X - 0.5f, stick.Y - 0.5f);

        private static bool DPadIs(Dualshock4State state, params GameControllerSwitchPosition[] pos) => state.DPad == pos[0];//pos.Any(p => state.DPad == p);

        private void HandleSingleClick(Dualshock4State currentState, Func<Dualshock4State, bool> checkButtonClick, Action action)
        {
            var previousButton = PreviousState is null ? false : checkButtonClick(PreviousState);
            var currentButton = checkButtonClick(currentState);

            if (previousButton == currentButton || !currentButton) return;

            action();
        }

        private void HandleHolding(Dualshock4State currentState, Func<Dualshock4State, bool> checkButtonClick, Action actionStart, Action actionEnd)
        {
            var previousButton = PreviousState is null ? false : checkButtonClick(PreviousState);
            var currentButton = checkButtonClick(currentState);

            if(!previousButton && currentButton)
            {
                actionStart();
            }
            if(previousButton && !currentButton)
            {
                actionEnd();
            }
        }

        private void HandleHolding(Dualshock4State currentState, Func<Dualshock4State, bool> checkButtonClick, Action action, int ticksDelay, string id)
        {
            var previousButton = PreviousState is null ? false : checkButtonClick(PreviousState);
            var currentButton = checkButtonClick(currentState);

            if (!Delays.ContainsKey(id)) Delays[id] = 0;
            if (previousButton && currentButton) Delays[id]++;
            else Delays[id] = 0;

            if (Delays[id] >= ticksDelay)
            {
                action();
                Delays[id] = 0;
            }
        }




        
    }
}
