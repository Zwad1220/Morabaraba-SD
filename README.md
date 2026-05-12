# Morabaraba Web Platform - Group 4  
### Live Demo  
The deployed browser version of the game can be accessed here:  
https://morabaraba-platform-prototype-group-4.netlify.app/  

## Overview 
This project is a browser-playable implementation of the traditional African strategy board game **Morabaraba** developed using **Unity WebGL**. The platform allows players to play Morabaraba directly in a web browser, while supporting additional game features on top of the base game such as, AI opponents, undo/redo functionality, move highlighting, and a full rule enforcement.  
The project was developed as part of software development group assignment focusing on software architecture, testing, deployment, teamwork, and project management.
### Group Members
- Ziyad Khan – 2869715
- Vuyisa Msipa – 2798787
- Jayaveer Harilal – 2110071
#### Technologies used
| Technology | Purpose |
| ----------- | ----------- |
| Unity | Core game engine |
| C# | Gameplay scripting |
| UnityWebGL | Browser deployment |
| Netlify | Browser hosting |
| HTML5 | Website structure |
| CSS3 | Website styling |
| JavaScript | Web functions |
| GitHub | Version control |  
## Features Implemented  
**Core Gameplay**
- Full Morabaraba gameplay
- Placement phase
- Movement phase
- Flying phase
- Mill detection
- Capture system
- Win conditions
- Draw conditions
- Turn management
  
**Additional Features**
  - AI opponents with multiple difficulties
  - Undo and redo functionality
  - Valid move highlighting
  - Win and draw screens
  - Piece counters
  - Browser-playable WebGL deployment
 
  ## Folder Structure
  ### ***Marabaraba***  
  Contains the main Unity project.  
    
  **Inside the *assets* folder**
  - Game C# scripts
  - Unity Scenes
  - Prefabs
  - Materials
  - UI assets
  - Unity Test framework scripts

### ***Morabaraba Website Library***  
Contains the exported Unity WebGL build and website files. 

**Includes:**
- HTML files
- CSS styling
- JavaScript integration files
- Unity WebGL build files

**TemplateData**  
Contains:
- Unity WebGL canvas styling
- Loader assets
- Website styling resources used by the Unity WebGL export

## Running the Project  
### Running the Unity Project  
1. Open Unity Hub
2. Add the *Marabaraba* project folder
3. Open the project using the recommended Unity version
4. Open the main scene
5. Press play in the Unity Editor
