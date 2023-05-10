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
using System.Diagnostics;
using Windows.System.Diagnostics;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Popups;
using Windows.ApplicationModel.Core;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using Windows.Storage;
using Microsoft.Toolkit.Uwp.Helpers;

namespace LoLController
{
    public sealed partial class MainPage : Page
    {
        public static PlayerHolder<int> PlayerHolder = new PlayerHolder<int>();

        public static ControlsConfig CurrentSavedConfig { get; private set; }
        public static ControlsConfig EditingConfig { get; private set; }
        private const string ConfigFilePath = @"config.json";

        private const string TestFilePath = @"test.txt";

        private static StorageFolder StorageFolder;



        public static event Action<ControllerState> OnControllerRead = (s) => { };

        private const string LeagueOfLegendsProcessName = "League of Legends.exe";
        private const string LoLLauncherProcessName = "LeagueClientUx.exe";

        private bool GameIsRunning = false;

        private static DiagnosticAccessStatus diagnosticAccessStatus = DiagnosticAccessStatus.Denied;

        private Dictionary<int, Action<ControllerState>> ControllerStateDisplayers = new Dictionary<int, Action<ControllerState>>();

        private const string DefaultControllerLabel = "--Disconnected--";
        private List<RawGameController> GameControllers = new List<RawGameController>();

        public int CurrentControllerId
        {
            get
            {
                return _currentControllerId = GameControllers.Count == 0 ? 0 : _currentControllerId % GameControllers.Count;
            }
            set
            {
                _currentControllerId = GameControllers.Count == 0 ? 0 : value % GameControllers.Count;
            }
        }
        private int _currentControllerId = 0;

        public RawGameController CurrentController => GameControllers.Count == 0 ? null : GameControllers[CurrentControllerId];

        private async void Alert(object obj) // god's greatest debugging warrior
        {
            await new MessageDialog(obj.ToString()).ShowAsync();
        }

        private async Task LoadConfigFile()
        {
            if (!(await StorageFolder.FileExistsAsync(ConfigFilePath))) await StorageFolder.CreateFileAsync(ConfigFilePath);
            string content = await StorageFolder.ReadTextFromFileAsync(ConfigFilePath);
            if (content == "") content = "-"; // apparently "" is parsable and is equal to null, so i need an error here

            ControlsConfig c;
            try
            {
                c = JsonConvert.DeserializeObject<ControlsConfig>(content);
            }
            catch
            {
                c = new ControlsConfig();
                var str = JsonConvert.SerializeObject(c, Formatting.Indented);
                await StorageFolder.WriteTextToFileAsync(str, ConfigFilePath);
            }

            CurrentSavedConfig = c;
            EditingConfig = c;
            PlayerHolder.SelectedPlayer.UpdateConfig(CurrentSavedConfig);
        }

        private async void SaveConfig(object sender, RoutedEventArgs e)
        {
            CurrentSavedConfig = EditingConfig.Copy();
            PlayerHolder.SelectedPlayer.UpdateConfig(CurrentSavedConfig);
            await SaveConfigFile();
        }

        private async Task SaveConfigFile()
        {
            var str = JsonConvert.SerializeObject(CurrentSavedConfig, Formatting.Indented);
            await StorageFolder.WriteTextToFileAsync(str, ConfigFilePath);
        }

        private async void ResetConfig(object sender, RoutedEventArgs e)
        {
            EditingConfig = new ControlsConfig();
            CurrentSavedConfig = EditingConfig.Copy();
            await SaveConfigFile();
            LoadConfig(null, null);
        }

        private void LoadConfigVisuals(ControlsConfig config)
        {
            var fields = typeof(ControlsConfig).GetFields();

            foreach(var field in fields)
            {
                var element = this.FindName(field.Name) as TextBox;
                if (element is null) continue;

                var obj = field.GetValue(config);
                var str = obj is ushort ? ((VirtualKey)KeyTable.GetVirtualKey((ushort)obj)).ToString() : Convert.ToInt32(obj).ToString();
                element.Text = str;
            }
        }

        private async void LoadConfig(object sender, RoutedEventArgs e)
        {
            await LoadConfigFile();
            LoadConfigVisuals(CurrentSavedConfig);
        }






        

        private void RegisterPlayers()
        {
            PlayerHolder.AddPlayer(0, () => new DisabledPlayer());
            PlayerHolder.AddPlayer(1, () => new Dualshock4Player());
        }

        public MainPage()
        {
            this.InitializeComponent();
            RegisterPlayers();

            StorageFolder = ApplicationData.Current.LocalFolder;

            LoadConfig(null, null);

            AskForDiagnosticRights();

            RawGameController.RawGameControllerAdded += (o, e) => { RefreshGameControllers(); };
            RawGameController.RawGameControllerRemoved += (o, e) => { RefreshGameControllers(); };

            ControllerStateDisplayers[0] = DefaultStateDisplayer;
            ControllerStateDisplayers[1] = Dualshock4StateDisplayer;

            OnControllerRead += DisplayControllerState;
            OnControllerRead += SelectedPlayerUpdate;

            DisplayMethodChanged(null, null);
            ControllerStateUpdate();
            ProcessScanner();
        }

        private void SelectedPlayerUpdate(ControllerState state)
        {
            if(GameIsRunning)
                PlayerHolder.SelectedPlayer.OnTick(state);
        }

        private async void AskForDiagnosticRights()
        {
            DiagnosticAccessStatus _diagnosticAccessStatus = await AppDiagnosticInfo.RequestAccessAsync();
            diagnosticAccessStatus = _diagnosticAccessStatus;
        }

        private void ControllerSelectionRight(object sender, RoutedEventArgs e)
        {
            CurrentControllerId++;
            UpdateControllerUI();
        }
        private void ControllerSelectionLeft(object sender, RoutedEventArgs e)
        {
            CurrentControllerId--;
            UpdateControllerUI();
        }
        private void RefreshGameControllers()
        {
            GameControllers = RawGameController.RawGameControllers.ToList();
            UpdateControllerUI();
        }


        private async void UpdateControllerUI()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                SelectedControllerLabel.Text = CurrentController?.DisplayName ?? DefaultControllerLabel;

                ButtonsStackPanel.Children.Clear();
                SwitchesStackPanel.Children.Clear();
                AxisStackPanel.Children.Clear();

                if (CurrentController is null) return;

                for (int i = 0; i < CurrentController.ButtonCount; i++) ButtonsStackPanel.Children.Add(new TextBlock { Margin = new Thickness(7) });
                for (int i = 0; i < CurrentController.SwitchCount; i++) SwitchesStackPanel.Children.Add(new TextBlock { Margin = new Thickness(7) });
                for (int i = 0; i < CurrentController.AxisCount; i++) AxisStackPanel.Children.Add(new TextBlock { Margin = new Thickness(7) });
            });
        }

        private async void ControllerStateUpdate()
        {
            while(true)
            {
                await Task.Delay(4);
                if (CurrentController is null) continue;

                var state = ControllerState.GetFrom(CurrentController);

                OnControllerRead(state);
            }
        }

        private void DisplayControllerState(ControllerState state)
        {
            var selectedDisplayer = ControllerStateDisplayModeSelector.SelectedIndex;
            if (!ControllerStateDisplayers.ContainsKey(selectedDisplayer)) DefaultStateDisplayer(state);
            else ControllerStateDisplayers[selectedDisplayer](state);
        }

        private async void PlayerChanged(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1);
            var index = SelectedPlayerComboBox.SelectedIndex;

            PlayerHolder.SelectPlayer(index, CurrentSavedConfig);

            PlayerHolder.SelectedPlayer.GetDescription().FillTextBlock(PlayerDescription);
        }

        #region Displaying

        private void DisplayMethodChanged(object o, RoutedEventArgs e)
        {
            var index = ControllerStateDisplayModeSelector.SelectedIndex;

            for (int i = 0; i < (DisplayHolder?.Children?.Count ?? 0); i++) if (i != index) DisplayHolder.Children[i].Visibility = Visibility.Collapsed; else DisplayHolder.Children[i].Visibility = Visibility.Visible;
        }
        private void DefaultStateDisplayer(ControllerState state)
        {
            if (ButtonsStackPanel.Children.Count != CurrentController.ButtonCount ||
                SwitchesStackPanel.Children.Count != CurrentController.SwitchCount ||
                AxisStackPanel.Children.Count != CurrentController.AxisCount) return;

            for (int i = 0; i < state.Buttons.Count; i++) (ButtonsStackPanel.Children[i] as TextBlock).Text = state.Buttons[i].ToString();
            for (int i = 0; i < state.Switches.Count; i++) (SwitchesStackPanel.Children[i] as TextBlock).Text = state.Switches[i].ToString();
            for (int i = 0; i < state.Axis.Count; i++) (AxisStackPanel.Children[i] as TextBlock).Text = state.Axis[i].ToString("N2");
        }
        private void Dualshock4StateDisplayer(ControllerState state)
        {
            if (!state.CanBeDualshock4()) return;
            var ds4state = state.ToDualshock4State();

            DS4_Square.Text = ds4state.Square.ToString();
            DS4_Cross.Text = ds4state.Cross.ToString();
            DS4_Circle.Text = ds4state.Circle.ToString();
            DS4_Triangle.Text = ds4state.Triangle.ToString();
            DS4_L1.Text = ds4state.L1.ToString();
            DS4_R1.Text = ds4state.R1.ToString();
            DS4_L2.Text = ds4state.L2.ToString();
            DS4_R2.Text = ds4state.R2.ToString();
            DS4_SHARE.Text = ds4state.Share.ToString();
            DS4_OPTIONS.Text = ds4state.Options.ToString();
            DS4_LeftStickClick.Text = ds4state.LeftStickClick.ToString();
            DS4_RightStickClick.Text = ds4state.RightStickClick.ToString();
            DS4_MiddleButton.Text = ds4state.MiddleButtonClick.ToString();
            DS4_TouchpadPress.Text = ds4state.TouchPadPress.ToString();

            DS4_DPadDirection.Text = ds4state.DPad.ToString();

            DS4_LeftStickX.Text = ds4state.LeftStick.X.ToString("N2");
            DS4_LeftStickY.Text = ds4state.LeftStick.Y.ToString("N2");
            DS4_RightStickX.Text = ds4state.RightStick.X.ToString("N2");
            DS4_RightStickY.Text = ds4state.RightStick.Y.ToString("N2");
            DS4_L2Weight.Text = ds4state.L2Weight.ToString("N2");
            DS4_R2Weight.Text = ds4state.R2Weight.ToString("N2");
        }


        #endregion Displaying
    
        private async void ProcessScanner()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                while (true)
                {
                    await Task.Delay(2000);

                    if (diagnosticAccessStatus != DiagnosticAccessStatus.Allowed) { LeagueOfLegendsStatusLabel.Foreground = CreateColor(184, 0, 0); LeagueOfLegendsStatusLabel.Text = "N/A"; continue; }

                    var lol = IsProcessRunning(LeagueOfLegendsProcessName);
                    var launcher = IsProcessRunning(LoLLauncherProcessName);

                    GameIsRunning = !(lol is null);

                    if (lol is null && launcher is null) { LeagueOfLegendsStatusLabel.Foreground = CreateColor(150, 150, 150); LeagueOfLegendsStatusLabel.Text = "Offline"; }
                    if (lol is null && !(launcher is null)) { LeagueOfLegendsStatusLabel.Foreground = CreateColor(252, 186, 3); LeagueOfLegendsStatusLabel.Text = "Launcher"; }
                    if (!(lol is null)) { LeagueOfLegendsStatusLabel.Foreground = CreateColor(3, 252, 65); LeagueOfLegendsStatusLabel.Text = "Running"; }
                }
            });
        }

        private async void BindAltered(object sender, KeyRoutedEventArgs args)
        {
            await Task.Delay(1); // Enough time for the app to type in key

            var tb = sender as TextBox;
            if (tb is null) return;

            var key = args.Key;
            tb.Text = key.ToString();

            AlterBind(tb.Name, key);
        }

        private async void AlterValue(object sender, RoutedEventArgs args)
        {
            await Task.Delay(1);
            var tb = sender as TextBox;
            if (tb is null) return;

            var text = "0" + tb.Text.Replace("_", "");
            var value = Convert.ToInt32(text);
            var fields = typeof(ControlsConfig).GetFields();

            var field = fields.ToList().Find(f => f.Name == tb.Name);
            if (field is null) return;

            field.SetValue(EditingConfig, value);
        }

        private void AlterBind(string textBoxName, VirtualKey key)
        {
            var fields = typeof(ControlsConfig).GetFields();
            var field = fields.ToList().Find(f => f.Name == textBoxName);
            if (field is null) return;

            var scanCode = KeyTable.GetScanKey((ushort)key);
            field.SetValue(EditingConfig, scanCode);
        }

        private SolidColorBrush CreateColor(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
        }

        private ProcessDiagnosticInfo IsProcessRunning(string name)
        {
            IReadOnlyList<ProcessDiagnosticInfo> processes = ProcessDiagnosticInfo.GetForProcesses();
            var p = processes.Where(x => x.ExecutableFileName == name).FirstOrDefault();

            return p;
        }
    }
}
