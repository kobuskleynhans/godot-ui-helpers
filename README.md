# Sprite2D UI Component using Godot and C#

This is a guide to use a C# script in [Godot](https://godotengine.org/) for creating dynamic UI components using 2D Sprites. 

![screenshot](https://raw.githubusercontent.com/kobuskleynhans/godot-ui-helpers/godot-4.1/addons/godot-ui-helpers/shots/star-sample.png)

## Quick Start

1.  After importing the asset, if you do not already have a C# Solution you can create one under `Project -> Tools -> C# -> Create C# Solution`.

2. Then click `Build` which will build the .Net Project.

3. Load one of the sample scenes under `addons/godot-ui-helpers/scenes`and run it.

4. Change the `distance` variable in the inspector and other settings to see the effects.

## Sample Scenes

It's a good idea to look at the sample scenes to get a feel for how the script works.

In the `/scene` directory, you'll find sample scenes that show different usage scenarios for the script. You can use these as starting points or references for your own use cases.

Please run these scenes and experiment with changing the `distance` variable inside the inspector on runtime, so you can see how they affect the present UI elements. Inside the inspector, by manipulating the properties of the script, you are able to see the immediate changes in the scene.
| Battery Sample  | Signal Sample | Folder Sample |
| --- | --- | --- |
| `/scenes/battery-sample.tscn` | `/scenes/signal-sample.tscn` | `/scenes/folder-sample.tscn` |
| <img src="https://raw.githubusercontent.com/kobuskleynhans/godot-ui-helpers/godot-4.1/addons/godot-ui-helpers/shots/battery-sample.png"  width="200" height="200"> | <img src="https://raw.githubusercontent.com/kobuskleynhans/godot-ui-helpers/godot-4.1/addons/godot-ui-helpers/shots/signal-sample.png"  width="200" height="200"> | <img src="https://raw.githubusercontent.com/kobuskleynhans/godot-ui-helpers/godot-4.1/addons/godot-ui-helpers/shots/folder-sample.png"  width="200" height="200"> |

| Progress Sample | Hydrogen Sample | 
| --- | --- |
| `/scenes/progress-sample.tscn` | `/scenes/hydrogen-sample.tscn` |
| <img src="https://raw.githubusercontent.com/kobuskleynhans/godot-ui-helpers/godot-4.1/addons/godot-ui-helpers/shots/progress-sample.png"  width="200" height="200"> | <img src="https://raw.githubusercontent.com/kobuskleynhans/godot-ui-helpers/godot-4.1/addons/godot-ui-helpers/shots/hydrogen-sample.png"  width="200" height="200"> 

## How to Use

To use this script, you follow these steps:

1. Attach the `sprite2d_ui` C# script to a Node2D object in your scene.

2. Set the Texture for base, pip and end as needed. You can use the provided sample textures or create your own.

3. All configurations for the UI component are in Godot's Inspector. You can tweak many visual and behavioral properties for example colors, textures, position, rotation, and scaling of the sprite UI elements and even how they will change over distance. 

4. The real power of this script lies in its `distance` property. By varying it from 0 to 100, you will create a wide array of effects. For example, a progress bar that fills up, or an element that slides or snaps into place.

5. With the `Mode` dropdown, you can control how the player sees the change in `distance`. It can either `Fill`,  where each pip is rendered with an offset of `_pipSpacing` or `Snap`, where elements snap from 0 to 100 instantly in their respective position with only one pip visible at a time, or `Slide`, where the pip smoothly slides from 0 to 100.

6. A mirror feature is available with the `_mirror` flag. If set to true, it generates a mirrored component around the Y axis which can then be further fine tuned in the inspector.

7. The `minPips` and `maxPips` variables control the number of elements ("pips") visible at the minimum and maximum distance, respectively.

8. If you want to include an optional sprite at the end of your sequence, you can use the `_end` sprite and its related settings.
