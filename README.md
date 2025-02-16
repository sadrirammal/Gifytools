## This is going to be a Demo SaaS where I will show what I can do with minimal resources.

Problem GifyTools solves: When trying to upload 3D Print Timelapses to Social Media they usually end up beeing to big + don't have any text on it such as channel etc...
GifyTools is a simple software that converts the BambuuLab avi file to a gif. Removes Frames to reduce size (target size should be possible to make it postable on all platforms), compression, text overlay etc..

# High level Architecture:
Frontend: Angular

API: C#

Processing: Python or C++ workers that either access Database or get triggered by api.

### Processing
-We will have workers that process the load. probably 80% CPU usage is fine, after that no more workers can be spinned up to ensure upload's still work

-Client Side video compression to have less processing in the backend

-Use high performing processing language. check advantages.

-Do not store videos delete them right after creating a gif

-Only store gifs for up to 24h and then delete them via Job.


# Setting up ubuntu server
1. Create a digital ocean ubuntu droplet on the lowest of the low tiers
2. sudo apt update && sudo apt upgrade -y
3. sudo apt install -y ffmpeg
4. which ffmpeg -> to get path of ffmpeg (put this into web config)
5. 
