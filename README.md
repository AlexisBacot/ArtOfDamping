# ArtOfDamping
Unity code used for the article "The Art of Damping" at https://www.alexisbacot.com/blog/the-art-of-damping

It includes two scripts:
- ToolDamper.cs: with all the damping functions
- SampleDamper2DGraph.cs: Script for the sample scene to control the goal with arrow key or mouse

One scene:
- SampleDamper2DGraph: Scene used to display two goal & follower points in red & blue

How to Use:
- Download Unity 2019 LTS (it's free)
- Clone / Download this repository
- Start Unity & select your local repository folder as your unity project
- In Unity: There is only one scene right now so the SampleDamper2DGraph scene should be your current scene automatically
- In Unity: Just click Play to test! Use the up & down arrow keys to move the goal point slowly, or the mouse to move it instantly
- In Unity: Select the *** 2D Graph *** GameObject to change the damping function used and its parameters
- In Unity: FEEL the damping, change the parameters, play with each function to understand what it does intuitively :)

Most of the code is just a port to C# from Daniel Holden original article at http://theorangeduck.com/page/spring-roll-call#damper
