# About

This is an app which allows you to play League of Legends using a game controller, with possibility to (easily?) add more controllers and bindings. Currently supported controllers are:
- Dualshock 4, made by me
- To be continued...

# Requirements

To use the app you need to:

- Disable or reset Steam controller bindings completely. They WILL interfere with this app's bindings.
- You MUST allow the app access to diagnostic information. Otherwise the app WILL NOT run.
- Be responsible with your bindings - it you bind Alt + F4 on your E Q combo, it is not my fault.
- Be careful with the app overall. It is still running, even if League of Legends is in the background, so be careful not to misclick anything.

# How to use

The app is divided into few parts:

1. `Controller selector`
2. `Controller readings`
3. `Player selector`
4. `League of Legends status`
5. `Config and binds`

The only section you actually need to use is `Player selector` - select the preferred controller type and the version of controls, and you are ready to go. Note, that the controller won't work, until you have the game running, indicated by the app's `League of Legends status`.

`Controller selector` is used if you have multiple controllers connected to your PC. They aren't really labeled, so you'll need to find out which is which.

`Controller reading` is used to test your controller. By default it is set to raw readings, but you can change the display method to be more suitable for your controller.

`League of Legends status` displays whether your game or launcher are running.

`Config and binds` is self-explatory. Note that you can't bind your mouse buttons to anything due to some limits.

# How to install

to be added

# For developers

To develop the app, just clone the repository and open `.sln` file via Visual Studio. I've been developing with Visual Studio Community 2022 due to it being a UWP app and recommend doing the same, but you can try to use other IDEs or redactors.

Here I'll describe how to expand the app.

## Display methods

Display method is a thing, which displays which buttons are currently pressed, what positions are sticks in, etc. I have added only the Dualshock 4, because this is the only gamepad I have, but you can easily add more.

First, I'll recommend creating a state class for your controller, having `Dualshock4State` as an example (for example: `XBox360State`). You can go without it, but trust me, it will be useful.

Second, open the `MainPage.xaml` file and locate the `DisplayHolder` StackPanel. It has a comment for clarity. Add a new container as its last child and have all the needed interface there. IMPORTANT: for each controller there must be ONLY ONE container.

Third, add a new option to the `ControllerStateDisplayModeSelector` ComboBox. It has a comment above it as well. Add a new option to the end, having your controller's name as text.

Fourth, create a new method with signature `Action<ControllerState>` and have it assign all values to the textblocks. Do not worry about hiding other sections or showing yours - it is already handled. Example:
```cs
private void XBox360Displayer(ControllerState state)
{
  if(!state.CanBeXBox360()) return;
  var xboxState = state.ToXBox360State();
  
  X360_X.Text = xboxState.X ? "True" : "False";
  X360_Y.Text = xboxState.Y ? "True" : "False";
  ...
}
```

Last, locate the `MainPage()` constructor and add this newly created method to the `ControllerStateDisplayers` dictionary to its end. It is important to add the new elements to the end of the stackpanel/combobox/dictionary, because the app decides which method to use based on its index in these.

## New players

First, you need to create a new class, implementing the `ILeagueOfLegendsPlayer` interface. It has 3 methods, which are quite self-expalanatory. Use `Dualshock4Player` as a reference. The most important part is `OnTick()` - it is invoked every 4ms and provides the user's input. You can use the `Input` class to inject keyboard and mouse inputs. Again, use `Dualshock4Player` as a reference.

Second, add a new option to the `SelectedPlayerComboBox` in the `MainPage.xaml`. Add it in format of "Controller name (Author name)" ("XBox 360 (NoobSlayer)"), so that the options are differentiable.

Last, locate the `RegisterPlayers()` method in `MainPage.xaml.cs` and add your player.


