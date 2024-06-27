# Description

This Unity project was made in June 2024 to **evaluate the capabilities and the current state of Unity's Netcode for Entities networking library**, especially regarding its use for massively multiplayer online games.

It was made by [Felix Beinssen](https://github.com/Chafficui) for his bachelor thesis in computer science during an internship at [CipSoft](https://www.cipsoft.com/), an independent developer and operator of online games from Regensburg, Germany.

**The project is a tech demo. It is neither a polished game nor a perfect example.** It contains some representative aspects of gameplay in massively multiplayer games: Avatars in a 3D scene, moving around in 2D on a plane, being able to shoot and being hit, colliding with other avatars, colliding with an obstacle and colliding with the area's fences.

From the project a stand-alone game server running headless in a console and a stand-alone game-client with a graphical user interface can be built, both for Windows and Linux. Both use IL2CPP, not mono. The game server accepts network connections from game clients. The game client should be run on multiple PCs and each game client process can run several logical clients, using the (amazing) ThinClient feature of Netcode for Entities.

# Load Test Results

The project was used to conduct load tests trying to evaluate the maximum amount of connected clients ("concurrent users"). **Running on off-the-shelf hardware the game server was able to host 700+ concurrent users at 30 game server updates per second without visible stuttering.** With higher amounts, for example with 1000+ concurrent users, the network traffic became the bottleneck, resulting in visible stuttering of the avatars. Network traffic being the limiting bottleneck is fully expectable without extensive optimizations for network traffic as usually required for MMOs. Bottlenecks for CPU, GPU or RAM did not appear in the conducted load tests.

# Current Opinion

During our evaluation Netcode for Entities did run stable and fast and supported several hundred concurrent users. It seems to be possible to build massively multiplayer online games with it. On the other hand its documentation is below our expectations. Especially such an amazing feature like ThinClients is not really documented at all.

Unity's Entity Component System (ECS) adds both performance and complexity to a project. One must make an informed decision whether that is a good deal for the own project. On the other hand its documentation also is below our expectiations. In addition ECS in its current state in June 2024 does lack basic features compared to GameObjects, for example animation, sound or user interface. This lack requires hybrid solutions, adding even more complexity and reducing performance benefits.

# Requirements

This project was made with Unity Pro version 6000.0.5f1 and Visual Studio Professional 2022 on Windows 10.

It uses Netcode for Entities (1.2.1), Burst (1.8.15), Collections (2.4.1), Entities (1.2.1), Entities Graphics (1.2.1), Input System (1.8.2), Mathematics (1.3.2), Unity Physics (1.2.1) and Unity Transport (2.1.0). It uses IL2CPP, not mono.

**The project is provided "as is", without warranty of any kind or support.**

# Build Instructions

The project can be built using the standard Unity 6 Build Profiles for Windows, Linux, Windows Server and Linux Server.

# Game Client Command Line Parameters

The game client can be started with Unity's built-in command line parameters `-batchmode` and `-nographics` which is helpful for load tests. In addition it can be started with the following custom command line parameters:
* `-ip <address>`: Connect to game server at address.
* `-port <number>`: Connect to game server at port number.
* `-thinClients <amount>`: Start amount of thin clients within one game client process. Thin clients will send randomized input.
* `-loadtest`: Limit target frame rate to 10 frames per second (to reduce CPU load and in doing so to increase amount of game client processes on one PC).

# Third Party Licenses

This project uses the following libraries and assets:

* [Graphy Ultimate FPS Counter](https://github.com/Tayx94/graphy) by Tayx94 licensed under MIT License
* [Capsule Carl](https://www.patreon.com/posts/99531832) by Kay Lousberg licensed under Creative Commons Zero License
* [KayKit Dungeon Remastered Pack](https://kaylousberg.itch.io/kaykit-dungeon-remastered) by Kay Lousberg  licensed under Creative Commons Zero License

# License

Except the libraries and assets mentioned above everything else in this repository is available under the following license:

```
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <https://unlicense.org>
```
