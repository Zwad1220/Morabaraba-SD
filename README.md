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

## Running the Website Version  
1. Open the deployed Netlify Link (https://morabaraba-platform-prototype-group-4.netlify.app/)  
   OR
2. Open the *Morabaraba Website Library*
3. Launch using a local web server or Live Server Extension
4. Open in supported web browser

### Controls 
| Action | Input |
| ----------- | ----------- |
| Place piece | Left click |
| Select piece | Left click |
| Move piece | Left click |
| Capture piece | Left click |
| Undo move | Undo button |
| Redo move | Redo button |
| Restart game (after completed game) | Restart button |
| Restart/Quit game (during game) | Refresh/Close page |  
### AI Difficulty Levels  
| Difficulty | Behaviour |
| ----------- | ----------- |
| Easy | Random legal moves |
| Medium | Attemps mills and blocks opponent |
| Hard | Prioritises strategic movement and placements |

### Testing  
Testing was performed using:
- Unity Test Framework
- Manual gameplay testing
- Integration testing
- AI behaviour testing
- Browser compatibility testing
- Rule validation testing
 
**Test coverage included**
- Placement validation
- Movement validation
- Flying phase
- Mill detection
- Capture rules
- Undo/redo functionality
- Draw conditions
- AI behaviour
- WebGL compatibility  
### Deployment  
The project was deployed using:
- Unity WebGL build target
- Netlify hosting platform
- Browser-based deployment pipeline

The system is designed to run directly in modern web browsers without requiring additional downloads or installations.  

### Known Limitations  
- WebGL performance may vary depending on hardware or browser
- Multiplayer Networking is not implemented
- Game logic does not work on mobile

### Future Improvements  
Potential future improvements include:
- Online multiplayer support and live chat
- Improved AI using algorithms
- Additional board themes
- Additional traditional games
- Cultural history of games
- Authentification

### System Requirements
| Requirement | Minimum |
| ----------- | ----------- |
| Modern Web Browser | Chrome, Firefox, Edge (recommendations) |
| RAM | 4GB |
| Internet connection for web version | Stable broadband connection |
| Screen resolution | 1280x720 recommended |  

Compatible with:
- Windows
- macOS
- Linux

## Acknowledgements  
This project was inspired by the traditional African board game ***Morabaraba*** and developed for academic purposes as part of a software development project.
