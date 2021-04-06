# Introduction

Hello, I am a game developer and this is a minigame project developed with Unity. After I quited my last job, I wanted to write a demo game mainly for my work portfolio. So I started this project.

I was playing Hollow Knight when I was thinking what this game should be about, so I was inspired by Hollw Knight and try to write a Metroidvania game. The main ideas of this game are:

* My target development time is 2 - 3 months.
* My target deploying platform is mobile device (because it is the most familiar platform of mine), and I want really simple user control.

So I designed the system of tap, hold and release with assigned commands. I originally only want to input by 1 finger. But then I realised that I was actually making a side-scrolling game and I should use landscape orientation. You actually hold the device by 2 hands. Limiting the user to use only a finger seems a bit weird. Hence, I add the left hand input to move the camera. I originally think that it is just a dispensable function, but when I tried to play the game, I found that it is actually quite useful (how lucky).

During the development of the game, I find out some problems of the gameplay:

* User may not be easily familier with the command input because the command mapping with tap, hold and release keep changing for different missions.
* Some commands are quite elementary (e.g. jump, air jump, turn) and it is hard to exclude them from the command mapping. So the combination of command mapping is not as free as expected. For this reason, it is also hard to add new command or design new mission. But as this game is designed as a minigame and I have no plan to expand it, it would not be a big problem.

Turn out the first version of this game is released after almost 4 months... and that version have not included any audio stuffs (audio is supported from v1.0.1). Currently only android version is available (.apk). iOS version is not available because I haven't joined the Apple Developer Program, and I do not have Apple mobile device for testing...

Hope you would enjoy this game!

Tony Lam

# Standards

* [Code Standard](./Metroidvania/Assets/Documents/HihiFramework/CodeStandard.md)
* [Unity Project File Standard](./Metroidvania/Assets/Documents/HihiFramework/UnityProjectFileStandard.md)

# Insufficiency of this project

* The image assets are not managed very well (e.g. The pixels per unit of the sprites are different). (Drawing those images have already used up all my strength...)
* No connection with any server/API (I do not have backend development skill...). For this reason, I do not use Asset Bundle and use Resources folder instead for the dynamic assets.
* Not much optimization has been done. Since the game is not high demanding, it should not be a critical problem.
* I think the scripts of character model / animation / collision / action by user input are not written very well. They are a bit messy and not so flexiable for changes.

# Latest Version

* Version : 1.0.1
* Tag : v1.0.1
* [Download APK](https://drive.google.com/file/d/1LZWuNwGWp4ZS66Y2K0BdFdZVpvHpiPb-)
* [Release Notes](./ReleaseNotes.md)