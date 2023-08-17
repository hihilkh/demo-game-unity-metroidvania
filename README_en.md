# Introduction

Hello, I am a game developer and this is a minigame project developed with Unity. After I quited my last job, I wanted to write a demo game mainly for my work portfolio. So I started this project.

# About the Game

* [Download](https://github.com/hihilkh/demo-game-unity-metroidvania/releases/latest)
* Platform : Android (ARMv7)
* Minimum API Level : Android 6.0
* Recommended Aspect Ratio : 16:9
* [Release Notes](./ReleaseNotes.md)

## Development Progress

I was playing Hollow Knight when I was thinking what this game should be about, so I was inspired by Hollow Knight and try to write a Metroidvania game. 

The main ideas of this game are:

* The target development time is 2 - 3 months
* The target deploying platform is mobile device (because it is the most familiar platform of mine)
* Simple user control

So I designed the system of `Tap`, `Hold` and `Release` with assigned commands. I originally want to input by only 1 finger. But then I realised that I was actually making a side-scrolling game and I should use landscape orientation. You actually hold the device by 2 hands. Limiting the user to use only a finger seems a bit weird. Hence, I add the left hand input to move the camera. I originally think that it is just a dispensable function, but when I tried to play the game, I found that it is actually quite useful (how lucky).

Turn out the first version(v1.0.0) of this game is released after almost 4 months... and that version have not included any audio stuffs (audio is supported since v1.0.1). Currently only android version is available (.apk). iOS version is not available because I haven't joined the Apple Developer Program, and I do not have Apple mobile device for testing.

Hope you would enjoy the game!

# About Development

* Unity Version : 2019.4.18f1
* [Code Standard](./Metroidvania/Assets/Documents/HihiFramework/CodeStandard.md)
* [Unity Project File Standard](./Metroidvania/Assets/Documents/HihiFramework/UnityProjectFileStandard.md)

# Insufficiency of this project

* User may not be easily familier with the command input because the command mapping with tap, hold and release keep changing for different missions.
* Some commands are quite elementary (e.g. jump, air jump, turn) and it is hard to exclude them from the command mapping. So the combination of command mapping is not as free as expected. For this reason, it is also hard to add new command or design new mission. But as this game is designed as a minigame and I have no plan to expand it, it would not be a big problem.
* The game is primarily designed for 16:9 aspect ratio and I have not done a full testing for other aspect ratio. It may have problem while playing with devices with aspect ratio other than 16:9.
* The image assets are not managed very well (e.g. The `Pixels per unit` of the sprites are different). (Drawing those images have already used up all my strength...)
* No connection with any server and no usage of API call (I do not have backend development skill...). For this reason, I do not use `Asset Bundle` and use `Resources` folder instead of dynamic assets.
* Not much optimization has been done.
* The scripts about the character (e.g. model / animation / collision / action by user input) are not well designed. As the game became more complicated, the scripts became quite messy and not so flexiable for further changes.