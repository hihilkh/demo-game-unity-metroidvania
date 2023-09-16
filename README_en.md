# Introduction

Hello, I am a game developer and this is a minigame project developed with Unity. After I quited my last job, I wanted to write a demo game mainly for my work portfolio. So I started this project.

# About the Game

## Demo Video

https://github.com/hihilkh/demo-game-unity-metroidvania/assets/35343910/a86c46c9-e8ea-4839-a405-06b1c9b1ed9a

## Basic Information

* Title : Find A Way Out
* Story : You find youself being trapped in a cave and cannot go out. Also, it seems you do not quite remember anything at the past. Choose your skills wisely to search the cave, figure out the mystery of the cave and find a way to go out of it.
* Platform : Android (ARMv7)
* Minimum API Level : Android 6.0
* Recommended Aspect Ratio : 16:9
* [Download](https://github.com/hihilkh/demo-game-unity-metroidvania/releases/latest)
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

* Development Period : 2020/11 ~ 2021/04
* Unity Version : 2019.4.18f1
* [Code Standard](./Metroidvania/Assets/Documents/HihiFramework/CodeStandard.md)
* [Unity Project File Standard](./Metroidvania/Assets/Documents/HihiFramework/UnityProjectFileStandard.md)

# What Is Good About This Project

* I thought of and implemented the whole game by myself, such as the game system, story, map design and the image assets (excluding audio assets). I think it is a good experience for both programming and game planning.
* In this game, depending on the combination of the skills, the character can do a variety of actions. I made an effort to do the implementation of those actions and also to do the level design and gimmick design base on the actions.
* Within the development, I kept reminding myself to write code that is easy to maintain and try my best to seperate the responsibility of the codes.

# Insufficiency Of This Project

* User may not be easily familier with the command input because the command mapping with tap, hold and release keep changing for different missions.
* Some commands are quite elementary (e.g. jump, air jump, turn) and it is hard to exclude them from the command mapping. So the combination of command mapping is not as free as expected. For this reason, it is also hard to add new command or design new mission. But as this game is designed as a minigame and I have no plan to expand it, it would not be a big problem.
* The game is primarily designed for 16:9 aspect ratio and I have not done a full testing for other aspect ratio. It may have problem while playing with devices with aspect ratio other than 16:9.
* The image assets are not managed very well (e.g. The `Pixels per unit` of the sprites are different). (Drawing those images have already used up all my strength...)
* No connection with any server and no usage of API call (I do not have backend development skill...). For this reason, I do not use `Asset Bundle` and use `Resources` folder instead of dynamic assets.
* Not much optimization has been done.
* The scripts about the character (e.g. model / AI / animation / collision / action by user input) are not well designed. As the game became more complicated, the scripts became too large and messy and and not so flexiable for further changes.