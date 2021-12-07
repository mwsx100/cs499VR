# cs499VR
Semester project for CS499

in Assets folder, there is a folder called Prefabs, in Prefabs there is the VRRig set up with some preset configurations. You can drag that into any scene so you don't have to build a VRRig object from scratch every time.

# Project Aspect:
We have created an immersive VR experience for people to explore their wildest imaginations. In our open environment, we allow our users to engage in a gamifying painting experience. With GraffitiVR, we did not just want to create an immersive experience for users to paint, but we wanted to give users the ability to really distress. One of the most famous Graffiti artists, Banksy, considers Graffiti art not only a form of creative expression, but a form of therapy. In this experience we let the user go on a journey of their dreams helping them be expressive and have a relaxing time.

# Inspiration:
GraffitiVR was inspired by a love for creative expression. As tech enthusiasts we were huge fans of Microsoft Paint. We loved the simplicity of the world’s most popular application. It allowed us to make those small drawings but for some it was a versatile tool to produce creative work. At the time of its retirement, Microsoft reported that it had over 100 million monthly active users. We want to bring the same success to VR by extending it with the robust power of three-dimensional experiences.

# Our Capabilities:
As mentioned, GraffitiVR isn’t just a Virtual Reality game, but it is a form of therapy, for that reason, we really implemented a lot of unique traits to make it very therapeutic. Users have access to creative tools such as Spray Paint, a Paint Gun, Brushes, Markers and a Whiteboard. We also wanted a unique place for users to express their creativity while giving them a feeling of bliss, which is why we purposefully made the setting at a school, and after 16 weeks, we can all agree that we feel like zombies, hence, why we are zombies, so not only do users spray the walls, they can even fight off the zombies. Since we are emphasizing therapy, we added traits similar to a Rage Room, where users can vandalize the objects and get their rage out, hence why we categorized this application as a therapeutic game.
- Open Environment: we have an open world in the setting of a school. A relatable environment for student users. We allow them to express and have fun in a therapeutic experience. In the classroom itself, there are whiteboard boards, lockers, desks, markers, and projectors. Another twist is having zoombies that add an engaging experience to the game.
- Tools: Spray Paint, Paint Gun, Brushes, Markers. Most of our tools are similar to the ones you would find in a school setting. However, to add the vandalizing gaming twist we added a graffiti spray paint tool. If bored, our users are able to spray the area and have fun.
- Gesture Motion Tracking: we can display hands for drawing. Additionally, picking up tools. And the most challenging part was the accurate motion tracking to result in precise drawing.

Most of our features were achieved from the existing goal of creating a painting application. In our original abstract, we had promised a paint escape experience. A three-dimensional painting application. While developing the application, we ended up expanding it into a game and made a more robust experience. The only functionality that is missing from our original idea is the multiplayer experience due to time constraints and technical complexity.

# Instructions to play the game:
GraffitiVR is a hybrid gaming experience. At its core it is a painting experience. It allows you to draw on the whiteboards and express your creative expression. Though, to give it a gaming experience we have an open environment with spray paint guns, a mock classroom, and zombies.
1. To load application:
- Download zip file and unzip the folder.
- Ensure you have Unity version 2020.3.23f for optimal compatibility.
- Create a new 3D project. When complete, open directory of new project. You should see your Assets folder, Packages folder, etc. Copy and paste every file from your downloaded folder over to the directory folder of your new project. Choose to replace any files in destination. You may be forced to skip select files, that is okay.
- Search and load scene titled ‘VR’.
- Switch platform to android and select Oculus Quest as run device (File->Build Settings).
- Build and run on your Oculus Quest.

2. To engage in rayline painting experience:
- Make sure your controllers are already on before you play the application.
- Simply approach any whiteboard within some feet, point your hand at it, and press down on the trigger to begin painting. The harder you press, the thicker the lines.

3. To engage in a spray painting experience:
- Grab either a spray paint can, or spray gun.
- Go to any paintable object (most things that are not complex/tiled textures)
- Spray paint on the object like you would in real life!

# Application Design:
- The user will be loaded into the main environment.
- The user will be able to move around with the left controller and look around with the right controller and the headset.
- The user will have the ability to pick up either spray paint or paint gun, which is located near their load area.
- If the user moves to a different area in the environment they can use a marker with a whiteboard to draw, or the raylines that project from their hands.

# Limitations:
- Due to only having the Oculus Quest 2, we were unable to test for other virtual reality headsets. For now, the game is only certified for the Oculus Quest 2 headset.
- Though, the Unity XR plugin should allow us to port it to other headsets with minimal overhead in the future.
- The game has no persistent storage. As a result, all data is deleted after the session ends. In the future, we can improve it by having unique user authentication and a database attached to each session.

# Future Expansion:
- We plan on adding a multiplayer experience so people can collaborate. It fits well within the metaverse expansion plans.
- Multiple open environment themes that people can select for their use instead of a classroom.
- We also plan on adding hand gesture tracking so people can use their hands instead of controllers.

# Note:
Due to technical difficulties of merging branches in Unity, we will be sending you the project in two parts. You can think of the 1st part as the ‘main’ part, and the second as further development. The instructions for running them each separately should be the same, with the exception of different scene names. One version includes spray painting in a mock classroom. In addition, you may use the left trigger to draw on either of the two whiteboards in the scene. The second version will be in a different environment. This version will utilize the right trigger to draw and has the updated version of our pointToDraw script which implements the rayline drawing.  We wrote pointToDraw from scratch and it has a lot of neat features which include: smooth line drawing that is a result of various interpolation techniques and Chaiken’s corner cutting algorithm, multiple different brush styles to choose from which include:  a square print, different circles of varying smoothness,  dotted line, and 2 spray methods. To change the brush style, one has to go into the script into the Interpolate method and change the drawing method used there. We also have the bucket object which works in conjunction with pointToDraw and allows the user to aim their raycast at the bucket object and squeeze the trigger to change your color. We encourage you to try both versions.

# To Run in Command Line: 
1. Open terminal/command prompt
2. Open desired directory
4. git pull https://github.com/mwsx100/cs499VR.git
5. Open Unity Application
6. Go to file to open app
