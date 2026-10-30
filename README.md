Leslie's VR Project

A Unity 6 virtual reality project for Meta Quest, built during the Create with VR course. You start in a main menu and choose between two rooms to explore:

Create Room: a cosy living room with a live wall clock, hats you can hang on hooks or wear, a tennis racket and ball, and a TV remote.
Canvas Room: a sci-fi storage room and main room joined by a glass door you open by its handle, with two sockets that only accept the right objects.

Both rooms have realistic hands that move with your fingers, teleport mats, and a short step-by-step guide.

How to run
Open the VR Room-Inema folder in Unity Hub with Unity 6000.6.2f1.
In File > Build Profiles, switch to Android and check that the scene list starts with Main Menu, followed by Leslie Room and VR Room.
Turn on Developer Mode for your Quest in the Meta Horizon app, connect it by USB and allow USB debugging in the headset.
Click Build And Run. The app opens on the main menu.

To try it in the editor without a headset, open the Main Menu scene, add the XR Interaction Simulator prefab and press Play.

Controls
Grab and move objects: grip
Press UI buttons: point and pull the trigger, or poke with your index finger
Teleport: point at a blue mat and click the thumbstick
Turn: thumbstick left or right
Return to the start point: B
Show the guide again: Y
Go back to the main menu: the Main Menu button
Things to try
Canvas Room: grab the glass door's handle and swing it open. Then put Tube A on the left pad and the Mug on the right pad of the big box under the shelf. Swap them and they won't snap in.
Create Room: check the clock above the mirror against your real time. Hang a hat on a hook or put one on your head.
Squeeze grip and trigger slowly and watch the fingers move.
Scripts

All custom scripts are in Assets/Scripts:

WallClock.cs: turns the clock hands using the system time.
HandPoseAnimator.cs: animates the hand models from grip and trigger.
HingeDoorGrab.cs: swings the physics door around its hinge toward your hand.
SceneGuide.cs: builds the main menu and the room guides, and loads scenes.
