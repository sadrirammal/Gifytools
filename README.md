## This is going to be a Demo SaaS where I will show what I can do with minimal resources.

Problem GifyTools solves: When trying to upload 3D Print Timelapses to Social Media they usually end up beeing to big + don't have any text on it such as channel etc...
GifyTools is a simple software that converts the BambuuLab avi file to a gif. Removes Frames to reduce size (target size should be possible to make it postable on all platforms), compression, text overlay etc..

# High level Architecture:
Frontend: Angular

API: C#

Processing: Processing done via library and direct process call.

### Processing
-We will have workers that process the load. probably 80% CPU usage is fine, after that no more workers can be spinned up to ensure upload's still work

-Client Side video compression to have less processing in the backend

-Do not store videos delete them right after creating a gif

-Only store gifs for up to 24h and then delete them via Job.


# Setting up ubuntu server

## Prepping Ubuntu
1. Create a digital ocean ubuntu droplet on the lowest of the low tiers
2. sudo apt update && sudo apt upgrade -y
3. sudo apt install -y ffmpeg
4. which ffmpeg -> to get path of ffmpeg (put this into web config)

## Configuring Services 
2. Install .net runtime apt install -y aspnetcore-runtime-8.0
3. sudo nano /etc/systemd/system/Gifytools.service
4. systemctl start Gifytools
5. systemctl enable Gifytools

### Service contents:

[Unit]
Description=PlantMate

[Service]
ExecStart=/usr/bin/dotnet /opt/dotnet/Gifytools.dll
Restart=always
#Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-Gifytools
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target

### Setup certbot
1. sudo apt install certbot python3-certbot-nginx -y
(important you have to have set the dns records before doing this)
2. sudo certbot certonly --nginx -d api.gifytools.com
3. sudo certbot certonly --nginx -d www.gifytools.com
4. sudo certbot certonly --nginx -d gifytools.com 

## Configuring nginx
1. Install nginx by doing: apt install nginx -y
2. sudo nano /etc/nginx/sites-available/api
3. sudo nano /etc/nginx/sites-available/www
   
sudo ln -s /etc/nginx/sites-available/api /etc/nginx/sites-enabled/

sudo ln -s /etc/nginx/sites-available/www /etc/nginx/sites-enabled/

4. sudo nginx -t
6. sudo systemctl restart nginx
