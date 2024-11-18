# Networks Project - HyperStrike
The following project was created during the Networks & Online Games course at CITM UPC university.

## Github Link
Link to Github repository: https://github.com/Historn/NetworksProject

## Latest Github Release
Link to latest Github release: https://github.com/Historn/NetworksProject/releases/tag/v0.0.1

## Showcase Demo Video
Link to the v0.0.1 video: https://youtu.be/P6krXWUM1QQ

## How to Run the Project
1. Clone the following repository into your local PC: https://github.com/Historn/NetworksProject
2. Using Unity Hub, add the Unity project from the repository into the hub.
3. Once the project is added into the hub, open it.

## Scene to Run
Assets/Scenes/MainMenu

## List of Contributions
- UDP Client-Server initial structure by Arnau Jiménez.
- Player(s) prefab instanciation and movement by Adrià Pons.
- Scene(s) set-up and configuration by Joel Chaves.
- UI set-up and buttons workflow with NetworksManager by Rylan Graham.

## List of Reported Bugs
- Physics are laggy due to currently being using Rigidbodies components in each of the players.
- If joining as a Client without previously opening the server as a Host, Client will be sended into the "PitchScene" and infinite players will be spawned.