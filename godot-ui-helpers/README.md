# Sprite2D UI Component using Godot and C#

This is a guide to use a C# script in [Godot](https://godotengine.org/) for creating dynamic UI components using 2D Sprites. 

## Sample Scenes

It's a good idea to look at the sample scenes to get a feel for how the script works.

In the `/scene` directory, you'll find sample scenes that show different usage scenarios for the script. You can use these as starting points or references for your own use cases.

Please run these scenes and experiment with changing the `distance` variable inside the inspector on runtime, so you can see how they affect the present UI elements. Inside the inspector, by manipulating the properties of the script, you are able to see the immediate changes in the scene.

## How to Use

To use this script, you follow these steps:

1. Attach the `sprite2d_ui` C# script to a Node2D object in your scene.

2. Set the Texture for base, pip and end as needed. You can use the provided sample textures or create your own.

3. All configurations for the UI component are in Godot's Inspector. You can tweak many visual and behavioral properties for example colors, textures, position, rotation, and scaling of the sprite UI elements. 

4. The real power of this script lies in its `distance` property. By varying it from 0 to 100, you will create a wide array of effects. For example, a progress bar that fills up, or an element that slides or snaps into place.

5. With the enum `Mode`, you can control how the player sees the change in `distance`. It can either `Fill`,  where each intermediate stage is visible until 100 is reached, `Snap`, where elements snap from 0 to 100 instantly, or `Slide`, where an element smoothly slides from 0 to 100.

6. A mirror feature is available with the `_mirror` flag. If set to true, it generates a mirrored component around the Y axis.

7. The `minPips` and `maxPips` variables control the number of elements ("pips") visible at the minimum and maximum distance, respectively.

8. If you want to include an optional sprite at the end of your sequence, you can use the `_end` sprite and its related settings.
