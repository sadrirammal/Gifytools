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

## Configuring SSH Key login
1. ssh-keygen -t ed25519 -f ~/.ssh/github_deploy_key
2. cat github_deploy_key.pub >> ~/.ssh/authorized_keys
3. chmod 700 ~/.ssh
4. chmod 600 ~/.ssh/authorized_key

### Disable SSH Password
**DUDE IF YOU DON'T HAVE THE SSH KEY SETUP DONE THIS WILL LOCK YOU OUT OF YOU DROPLET**
1. nano /etc/ssh/sshd_config
2. add this
3. PasswordAuthentication no
UsePAM yes
PubkeyAuthentication yes
4. sudo systemctl reload ssh

### Install and Enable fail to ban
SSH gets bruteforced all the fucking time to the point that it actually uses 3-5% CPU.
Follow these steps to ban the bots:

1. sudo apt install fail2ban -y
2. sudo systemctl enable fail2ban
3. sudo systemctl start fail2ban
4. sudo systemctl status fail2ban
5. sudo fail2ban-client status sshd


### Service contents:

  GNU nano 8.1                            /etc/systemd/system/Gifytools.service                                     
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
Environment=FileSystemSettings__FFmpegPath=/usr/bin/ffmpeg
Environment=FileSystemSettings__VideoInputPath=/opt/gifytools/videoInput
Environment=FileSystemSettings__GifOutputPath=/opt/gifytools/gifOutput
Environment=FileSystemSettings__Fonts__0__Name=DejaVu Sans
Environment=FileSystemSettings__Fonts__0__Path=/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf
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

### Automatically deleting videos and gifs
DELETED
Simply added hangfire job to delete 7 day old shit


## Backend setup
In the Backend we have a rather simple asp.net 8 API. It uses Postgres and ef. After having done a load test of our 9 USD per month server we know we should be able to have 5 workers running at the same time. 
We will need a process queue that will trigger the ffmpeg convert jobs. 

### Setting up EF
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0">
    
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.2" />

program.cs:

    var appDbConnectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(appDbConnectionString));


## Frontend Setup
1. ng add ngx-bootstrap
2. ng add bootstrap (don't forget to add js and css in package.json)
3. 


    
