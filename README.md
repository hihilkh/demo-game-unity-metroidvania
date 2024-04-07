[![en](https://img.shields.io/badge/lang-en-red.svg)](./README.md)
[![jp](https://img.shields.io/badge/lang-jp-green.svg)](./README.jp.md)

# Demo Video

https://github.com/hihilkh/demo-game-unity-metroidvania/assets/35343910/a86c46c9-e8ea-4839-a405-06b1c9b1ed9a

# Introduction

* Title : Find A Way Out
* Genre : Metroidvania
* Platform : Android

# Story

One day, you wake up and find that you are trapped in a cave and cannot go out. What's worse, it seems you do not remember anything at the past. You are going to explore the cave, figure out the mystery and find a way to escape from the cave.

# Game System

The cave is separated into a few areas and first you need to select which area to explore.

The character will automatically walk horizontally. What you have to do is to select the skills to use (out of total 5 skills) to control the character in order to walk through the area. You can perform different actions by the same skill, depending on the input gesture (`Tap`, `Hold` or `Release`) and location (`on ground` or `in the air`).

You can check your progress within the area selection scene (e.g., if there is any items not yet been collected). Also, there are 3 endings in total. Please try to get all the items and endings. Hope you would enjoy the game!

# About the Development

* Development Period : 2020/11 ~ 2021/04
* Development Environment : Unity2019.4.18f1 / C#
* More About Source Code : [here](./SourceCodeGuide.md)

# About the Release

* [Download APK File](https://github.com/hihilkh/demo-game-unity-metroidvania/releases/latest)
* Platform : Android (ARMv7)
* Minimum API Level : Android 6.0
* Recommended Aspect Ratio : 16:9
* [Release Notes](./ReleaseNotes.md)

# Self Evaluation

### What I Have Learned

* In this project, I have done all the designs and implementations of the whole game, including the game system, story, map design and the image assets (excluding audio assets). I think it is a good experience for both programming and game planning.
* Depending on the combination of the skills, the character can do quite a lot of things. I made an effort to do the implementation of those actions and also to do the level design and gimmick design based on the actions.
* Within the development, I kept reminding myself to write code that is easy to maintain and try my best to separate the responsibility of the codes. For example,
	* I used ScriptableObject to store character and enemies status. 
	* I have separated the player scripts into model, controller, camera, audio, different attacks, StateMachineBehaviour of animations, etc.
	* I have made my own game framework for some generic features, such as logging, audios, localization, game configuration.

### Insufficiency

* Users may not be easily familier with the control because the skill table (the selected skills) may keep changing in different areas of the cave.
* Some skills (e.g., jump, air jump, turn) are quite elementary and it is hard to exclude them from the skill table. So the combination of skill table is not as free as expected. For this reason, it is also hard to add new skills or design new areas. Fortunately, the story and contents of this game is decided and I have no plan to expand it, so it is not a big problem.
* The game is primarily designed for 16:9 aspect ratio and I have not done a full testing for other aspect ratio. It may have problem while playing with devices with aspect ratio other than 16:9.
* The image assets are not managed very well (e.g., the pixels per unit of the sprites are not consistent). (Drawing the images has already exhausted all of my strength...)
* Asset bundle or addressable are not used. Only the Resources folder is used for simplicity sake.
* Not much optimization has been done.
* The scripts about the character (e.g., model / AI / animation / collision / action by user input) are not well designed. As the game became more complicated, the scripts became too large and messy and not so flexiable for further changes.