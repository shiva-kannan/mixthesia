<p align="center">
  <br>
  <a href="https://www.etc.cmu.edu/projects/mixthesia/" style="font-size: 200px; text-decoration: none"><b>Mixthesia</b></a>
  <br><br>
  <img src="Media/mixthesia-logo.PNG" width="350">
</p>

## Project Structure
### Scenes Setup
Since the project was mostly about prototyping, the scenes are arranged according to the kind of prototypes they
fall under. And hence, the scenes present in the `Assets/Scenes` directory can be categorized into 3 types: 
1. Track Field : Volume, Panning and Automation (3 different sound tracks)
    1. [Prototype_AllIsFound](https://github.com/shiva-kannan/mixthesia/blob/895fb3d810caa175f4883e56ef43ec49b356421e/Assets/Scenes/Prototype_AllIsFound.unity)
    2. [Prototype_MajorLazer](https://github.com/shiva-kannan/mixthesia/blob/895fb3d810caa175f4883e56ef43ec49b356421e/Assets/Scenes/Prototype_MajorLazer.unity)
    3. [Prototype_VivaLaVida](https://github.com/shiva-kannan/mixthesia/blob/895fb3d810caa175f4883e56ef43ec49b356421e/Assets/Scenes/Prototype_VivaLaVida.unity) 
2. Reverb
    1. [Prototype_Reverb](https://github.com/shiva-kannan/mixthesia/blob/895fb3d810caa175f4883e56ef43ec49b356421e/Assets/Scenes/Prototype_Reverb.unity)
3. EQ + Audio Cube
    1. [Prototype_AudioCube](https://github.com/shiva-kannan/mixthesia/blob/895fb3d810caa175f4883e56ef43ec49b356421e/Assets/Scenes/Prototype_AudioCube.unity)
4. Intro Scene:
    1. [MainScene](https://github.com/shiva-kannan/mixthesia/blob/895fb3d810caa175f4883e56ef43ec49b356421e/Assets/Scenes/MainScene.unity)


### Scripts Setup
> ##### Track Field : Volume, Panning, and Automation
Star.cs | Randomly animates stars generated when recording automation curves.  
StarPopulator.cs | Populate stars on the recorded automation curve based on keyframes.  
TrackAutomation.cs | Handles recording and playing back automation data.  
TrackField.cs | Contains dimensions (middle axis, angle ranges, etc.) of the Track Field.  
TrackFieldAxisDrawer.cs | Displays the volume and panning axis when dragging a track.  


> ##### Reverb
ChurchPopulator.cs | Populate church model bits to create the church when reverb (xeno) is applied and reacts to the scaling of the church.  
TrackOrbCopied.cs | Defines behaviors of the floating track orb copy (only used in the reverb scene).  


> ##### EQ
AudioCube.cs | Contains dimensions (axis ranges, axis length, etc.) of the Audio Cube.  
FrequencyScaleDrawer.cs | Display the frequency scales along the y-axis of the Audio Cube.  
Tool_EQ_Filter.cs | Handles interaction between 3-band filters and pointer lasers and affect EQ parameters.  
Tool_EQ_Simple.cs | Handles adding/showing the simple EQ (stars and nebula).  
Tool_Reverb.cs | Handles guest interaction for reverb (xeno) and talks to individual track FMOD managers.  
Tool_Reverb_Opacity.cs | Handles guest interaction for reverb (xaeda) and talks to individual track FMOD managers.  
TrackFFTContainer.cs | Contains FFT data for a track.  


> ##### Misc.
AllTracksManager.cs | Stores references to individual FMOD managers of all tracks in the scene.  
Clickable.cs | Parent class of all clickable objects in the project (UI buttons, track orbs, tools, etc.)  
CPanel_Button.cs | Child class of Clickable.cs. Defines the behavior of UI buttons (Play/Pause, Stop, Mute/Unmute All, etc) on the song control panel.  
CPanel_Master.cs | Takes input from individual buttons and talks to the audio manager to trigger corresponding behaviors (Play/Pause, Stop, etc.).  
General_ColorPool.cs | Contains a pool of colors each track can randomly pick from.  
General_ShaderID.cs | Contains all IDs (string) of relevant properties of some shadergraph shaders.  
General_Utilities.cs | Contains some useful helper functions (mapping, db to linear convertion, etc.).  
Headphone.cs | Triggers the opening transition when guests put on the virtual headphone.  
MasterBusController.cs | Audio manager for song-level behaviors (Play/Pause, Stop, Mute/Unmute All, etc.).  
OpeningScene.cs | Animates the opening transition when guests put on the virtual headphone.  
Pointer.cs | Handles the interaction between the pointer laser and interactable objects.  
PrototypeSceneLoader.cs | Load different scenes.  
Timeline.cs | Display the timeline of the song and handles guest interaction with it.  
TimelineController.cs | Syncing all tracks to a given point in time.  
TrackFMODManager.cs | The FMOD manager for individual tracks. Contains relevant functions from the FMOD API.  
TrackOrb.cs | Defines all behaviors of a track orb.  
TrackTool.cs | Child class of Clickable.cs. Defines the how tools (reverb, simple EQ) are applied to a track.  
UI_Billboard.cs | Makes the object always look towards/away from the camera.  



### FMOD Setup
.. WIP

### Project Documentation Material

1. [Volume and Panning Prototype](https://www.etc.cmu.edu/projects/mixthesia/index.php/prototypes/volume-panning/)
2. [Reverb Prototype](https://www.etc.cmu.edu/projects/mixthesia/index.php/prototypes/reverb/)
3. [Automation and Timeline](https://www.etc.cmu.edu/projects/mixthesia/index.php/prototypes/automation-timeline/)
4. [EQ and Audio Cube](https://www.etc.cmu.edu/projects/mixthesia/index.php/prototypes/eq-audio-cube/)


### Tool Usage and Documentation

.. WIP

### Project Owners

[Sam Hu](https://www.samjhhu.com/)

[Shiva Kannan](https://shivakannan.me/)

[Pavan Paravasthu](https://www.pavanparavasthu.com/)

[Brandon Badger](https://bmbadger97.wixsite.com/portfolio)

[Isabel Yi]()
