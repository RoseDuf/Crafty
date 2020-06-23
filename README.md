# Crafty

- [Crafty](#crafty)
  - [Getting Started](#getting-started)
    - [Unity Version](#unity-version)
    - [Install Packages](#install-packages)
    - [Run](#run)
  - [General Guidelines](#general-guidelines)

3D golf-like inspired by Kirby's Dream Course. Hit creature to collect stars. The last remaining creature becomes the whole. Hitting creatures also allows you steal there abilities. If you hit spikes or enemies, you lose a heart. If you fall into the abyss, you lose a life. If you lose all lives, you need to restart all the holes for that course (set of levels).  
There are 2 gauges when you shoot the ball: strength and spin. If you press a button at the moment you bounce off a surface, you get an extra “umpf” to your bounce which makes you go higher.  
The level ends when you run out of lives or put your ball into the hole.  

## Getting Started

### Unity Version
Make sure to install Unity version `2019.4.0f1`.

### Install Packages
Open Unity Editor and open `Window > Package Manager` from the top toolbar. In the Package Manager editor window, search for and install:
- `Universal RP`
- `Shader Graph`

### Run
Before your first run, go to `File > AutoPreloadScene > Load Preload Scene On Play`. A scene selection dialog should pop up. Select `_Scenes/Preload.unity`.

Hit the play button in the editor.


## General Guidelines

- If you want to test something new in a scene, create a new scene with your name. Ideally, each one of us should have our own scenes to work on.

- If you want to develop or experiment with a new feature (that being shaders, models, scenes, textures, etc.), add it to the `Sandbox/Local/` folder.

- Create a new branch for each new feature that you work on.

- If you aren't comfortable merging your branch into master or are having problems merging, please ask for help before doing anything crazy.

- See `unity_style_guide.md` for info about code style.

- See `unity_project_structure.md` for info about folder and scene hierarchy structure.