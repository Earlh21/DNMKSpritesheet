# DNMK Spritesheet

Creating custom shot data can be irritating to deal with, especially if you have to update it later. This is a GUI tool that allows you to create and edit shot types, then export the data to data, const, and spritesheet files for use in scripts.

![A screenshot of DNMK Spritesheet](https://i.postimg.cc/brGh3wrD/image.png)

## Sprites

You can import image files as sprites through Edit > New Sprite or Edit > Import Sprites and they will appear in the left pane.

## Shot Types

You can create shot types, both still and animated, by right clicking on a sprite. All shot types require a collision size and a delay color, which you can change in the topmost pane.

You can add animation frames (visible in the rightmost pane) by double clicking on a sprite while the animated shot type is selected. You can change the delay for any frame with the topmost pane and use the arrows to change order. You can delete them with the delete key.

## Delay Sprite

The delay sprite is the sprite drawn before the bullet is spawned. You can change it through right clicking a sprite.

## Exporting

Exporting gives you three files:

* A spritesheet containing all the sprites you have imported

* A shot data file containing attributes for each shot type, and the rectangle in the spritesheet for each shot type

* A const file that links the names you give to your shot types to the IDs used in the shot data file

You can export to a zip file with File > Export. More useful, however, is the Quick Export option. Quick Export will export the three required files to the directory your DNMKSpritesheet project file is located in by default. You can change where Quick Export exports to with Quick Export Options.

## Using the Exported Files

To use your generated shot types, you have to include the const file in your script.
If you don't know how to do that, you can use the following code if the const file is in the same directory as your script:

`#include "./<project name>Const.dnh"`

Replace <project name> with the name of your DNMKSpritesheet project file.

You can then refer to any shot type you created by its name in the GUI. As long as you don't change the name of the shot type you can make any other changes you want and you will never have to change your scripts.
