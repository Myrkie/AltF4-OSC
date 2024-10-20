# OSC Alt-F4 Application for VRChat

This is a simple OSC application designed to monitor a specific boolean value. When the boolean is set to `true`, the application will automatically close the VRChat process after a delay of 800 milliseconds (via Alt+F4). 

This can be used for fun or gimmicky effects, such as a "log off suicide" in VRChat.

You must have basic understanding of unity animators if you plan to use this.

## Features
- Detects when a boolean parameter is set to `true`.
- Closes the VRChat process after a configured timeout `default 800ms`.

## Information
1. **Avatar Parameters**: The application trims avatar parameters. Use just the name of the parameter. For example, `/avatar/parameters/VRCEmote` becomes `VRCEmote`. This supports categorized animator parameters names.

2. **Timeout Logic**: Once the boolean is detected as `true`, VRChat will close after 800 milliseconds if "Enable Alt+F4" is checked. reset the boolean back to `false` in your animator after it has been set to `true` to avoid unintended behavior.

3. **OSCQuery**: This application utilizes OSCQuery this can be overwritten in required in the config.

## Resources Used
- [OSCQuery library](https://github.com/Natsumi-sama/OscQueryLibrary)
- [VRChat Dedicated OSC Library](https://github.com/Myrkie/VRChatOSCLib)
- [WPFDarkTheme](https://github.com/AngryCarrot789/WPFDarkTheme)

---

This structure clearly separates each part, making the README easier to follow for users.