# FaceFollower

While home from university I found my LEGO MindStorm set and decided top build
something with it. Recently having been on quiet a few video calls, and
wanting to build something useful, I came up with the idea to build a webcam
that keeps my face in frame. A FaceFollower.

## Facial Detection

I originally planed to use Azure's Computer Vision API for facial detection. But
the free version was to "slow" and I switched to [Emgu CV](http://www.emgu.com/wiki/index.php/Main_Page),
a C# wrapper for [OpenCV](https://opencv.org/). It's less accurate, but makes
more than up for it in speed.

## Robotic Camera Mount

The LEGO design went through several iterations, one with custom 3D-printed
gears, before I finally settled for the simpler design I have now.

## Controlling the Robot

To control the robot I used functions from [AForge.NET](http://aforgenet.com/)
robotics for LEGO library.

## More

A more detailed story of this project, as well as a demonstration video, can be
found on my blog.

[**http://blog.åsberg.net/facefollower/**](http://blog.xn--sberg-lra.net/facefollower/)