<p align="center">
    <img src="../master/External/logo.png" alt="Luna multiplayer logo"/>
    <a href="https://www.youtube.com/watch?v=rmJL_c-EJK8"><img src="https://img.youtube.com/vi/rmJL_c-EJK8/0.jpg" alt="Video" height="187" width="250"/></a>    
    <a href="https://www.youtube.com/watch?v=gf6xyLnpnoM"><img src="https://img.youtube.com/vi/gf6xyLnpnoM/0.jpg" alt="Video" height="187" width="250"/></a>
</p>

<p align="center">
    <a href="https://paypal.me/gavazquez"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg?style=flat&logo=paypal" alt="PayPal"/></a>
    <a href="https://discord.gg/wKVMhWQ"><img src="https://img.shields.io/discord/378456662392045571.svg?style=flat&logo=discord&label=discord" alt="Chat on discord"/></a>
    <a href="../../releases"><img src="https://img.shields.io/github/release/lunamultiplayer/lunamultiplayer.svg?style=flat&logo=github&logoColor=white" alt="Latest release" /></a>
    <a href="../../releases"><img src="https://img.shields.io/github/downloads/lunamultiplayer/lunamultiplayer/total.svg?style=flat&logo=github&logoColor=white" alt="Total downloads" /></a>
</p>

<p align="center">
    <a href="https://forum.kerbalspaceprogram.com/index.php?/topic/168271-131-luna-multiplayer-lmp-alpha"><img src="https://img.shields.io/badge/KSP%20Forum-Post-4265f4.svg?style=flat" alt="KSP forum post"/></a>
    <a href="https://github.com/LunaMultiplayer/LunaMultiplayerUpdater"><img src="https://img.shields.io/badge/Automatic-Updater-4265f4.svg?style=flat" alt="Latest build updater"/></a>
</p>

---

<p align="center">
  <a href="../../releases/latest"><img src="../master/External/downloadIcon.png" alt="Download" height="85" width="300"/></a>
  <a href="../../wiki"><img src="../master/External/documentationIcon.png" alt="Documentation" height="85" width="353"/></a>
</p>

---

# Luna Multiplayer Mod (LMP)

*Multiplayer mod for [Kerbal Space Program (KSP)](https://kerbalspaceprogram.com)*

### Main features:

- [x] Clean and optimized code, based on systems and windows which makes it easier to read and modify.
- [x] Multi threaded.
- [x] [NTP](https://en.wikipedia.org/wiki/Network_Time_Protocol) protocol to sync the time between clients and the server.
- [x] [UDP](https://en.wikipedia.org/wiki/User_Datagram_Protocol) based using the [Lidgren](https://github.com/lidgren/lidgren-network-gen3) library for reliable UDP message handling.
- [x] [Interpolation](http://www.gabrielgambetta.com/entity-interpolation.html) so the vessels won't jump when there are bad network conditions.
- [x] Multilanguage.
- [x] [Nat-punchtrough](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/Master-server) feature so a server doesn't need to open ports on it's router.
- [x] Servers displayed within the mod.
- [x] Settings saved as XML.
- [x] [UPnP](https://en.wikipedia.org/wiki/Universal_Plug_and_Play) support for servers and [master servers](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/Master-server)
- [x] Better creation of network messages so they are easier to modify and serialize.
- [x] Every network message is cached in order to reduce the garbage collector spikes.
- [x] Based on tasks instead of threads.
- [x] Supports career and science modes (funds, science, strategies, etc are shared between all players).
- [x] Cached [QuickLZ](http://www.quicklz.com) for fast compression without generating garbage.
- [ ] Support for groups/companies inside career and science modes.
---
### How to use and configuration:

Please visit [this page](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/Create-a-server) in the wiki

---
### Troubleshooting:

Please visit [this page](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/Troubleshooting) in the wiki to solve the most common issues with LMP

---
### Servers:

You can check [how many servers are up](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/Master-server-status) and running either in [Release](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/How-to-get-the-latest-version-of-LMP) or in [Nightly](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/How-to-get-nightly-builds) versions through our [master servers](https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/Master-server)

---
### Contributing:

Consider [donating trough paypal](https://paypal.me/gavazquez) if you like this project. 
It will encourage us to do future releases, fix bugs and add new features :star:

Please write the code as you were going to leave it, return after 1 year and you'd have to understand what you wrote.  
It's **very** important that the code is clean and documented so in case someone leaves, another programmer could take and maintain it. Bear in mind that **nobody** likes to take a project where it's code looks like a dumpster.

There's also a test project in case you want to add tests to your code.
