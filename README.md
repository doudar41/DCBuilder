Chaoskeep: Death Crusaders

This is the ongoing dungeon crawler game project. We are trying to create a retro looking game with Unity.

Party based RPG
Grid movement
Turn based combat

GameInstance script is non monobehavior, static and contains references to all singletons on scene, save/load logic and some functions of general use like dice rolling logic.   

Implemented systems 
Unity Tilemaps have functions for returning grid coordinates in Vector3 and Vector3Int data. 
Blocks are premade 3D gameObjects as a scriptable objects with attach script containing IBlock interface which is used to interact to Player Controller.

Player Controller is used for movement and interaction with environment with raycasting and checking IBlock interface. 
Main features: 
Grid based movement using Unity tilemaps. 
Raycast in direction of movement checking for interactable interface return bool allowing or deny movement
Checking movement target block for interactable interface and list of possible interactions which are party weight check, dialogue options, UI interaction options, portals, level exit, custom combat. 

Spells are part of scriptable objects. We decided to make all weapons and potions as spells because they share the same principles. 
Spell has a bunch of params which are dices used for calculations, damage or special effect, school of magic, outcome element etc. 
Spell container object may contain several spells, icon, indication if it apply to all party or enemies if it's a non combat spell, cost in shop, name of animation used.

Spellbook script manages most of a logic of area based spells and non combat spells. 
Interfaces IHero and IEnemy handles functions of applying spells on members of party and single enemies. 

Enemybase script shows possible usage of IEnemy interface for applying damage and spell effects and managing animations. 

Hero script is a biggest one and it saves a hero statistics and contains functions which return statistics with applied spells on a hero. 
Statistics are Attributes (Mainstats in a script), Skills and Depended stats (HP, MP, defence, accuracy, evasion).

Party script is responsable for keeping experience, money, food resources, track which hero is activated, has references to current dialogue options

Battle manager spawn enemies on a battle ground where player is teleported and sorting opponents using their initiative stat. It also initiates turns and enemy autoattacks and keep track of health and state of enemies and heroes to win and lose end.     

Dialogue/Quest system is based on scriptable objects and enum names (which is doubling each other and will be simplified later). 
Using scriptable object dialogue logic knew what next dialogue will be activated, reward player gets, which dialogues are repeatable which will be permanatelly deleted. 
Dialogues attached to one object have priority. For example, guards who deny passage have two dialogues attached deny priority 1 and allow priority 2.
When Party script has a dialogue Deny (priority 1) it will appear on screen and as repeateable dialogue it will appear till Party has a Allow (priority 2) dialogue attached which is one time dialogue and remove both dialogue options from Party and Guard object. 

We've built simplified starting level for testing quest/dialogue system.  
