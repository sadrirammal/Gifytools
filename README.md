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
```
# Keep root, but keys only (no passwords)
PermitRootLogin prohibit-password
PubkeyAuthentication yes
PasswordAuthentication no
KbdInteractiveAuthentication no
ChallengeResponseAuthentication no
UsePAM yes

# Make brute force harder
LoginGraceTime 30s
MaxAuthTries 3
MaxSessions 10

# Optional: disable X11 on a server
X11Forwarding no

# (Optional) restrict SSH to specific users
# AllowUsers root
```
2. sudo sshd -t        # syntax check; no output = OK
3. sudo systemctl reload ssh


### Install and Enable Fail2Ban
SSH gets bruteforced all the fucking time to the point that it actually uses 3-5% CPU.
Follow these steps to ban the bots:

1. sudo apt install fail2ban -y
2. sudo systemctl enable fail2ban
3. sudo systemctl start fail2ban
4. sudo systemctl status fail2ban
5. sudo fail2ban-client status sshd

#### Configuring Fail2Ban
nano /etc/fail2ban/jail.d/sshd.local
```
[DEFAULT]
bantime = 12h
findtime = 10m
maxretry = 4
# (Optional, but great) exponential backoff for repeat offenders
bantime.increment = true
bantime.factor    = 2
bantime.formula   = bantime * (1 + factor ** (failures - 1))
bantime.overalljails = true
bantime.rndtime   = 10m

[sshd]
enabled  = true
backend  = systemd
port     = 22
logpath  = %(sshd_log)s
```
sudo systemctl restart fail2ban

##### Further config of Fail2Ban
If you checkout logs you will notice that we are permanently getting port scanned by botnets. To prevent them from even getting basic info we will ban them.

1. Create a custom Filter
sudo nano /etc/fail2ban/filter.d/ufw-scan.conf

```
[Definition]
failregex = \[UFW BLOCK\].*SRC=<HOST>
ignoreregex =
```

2. sudo nano /etc/fail2ban/jail.d/ufw-scan.local
```
[ufw-scan]
enabled  = true
filter   = ufw-scan
logpath  = /var/log/ufw.log
backend  = auto
maxretry = 1
findtime = 600
bantime  = 1d
banaction = ufw
```

3. sudo ufw logging on
4. sudo systemctl restart fail2ban
5. sudo fail2ban-client status ufw-scan # This is to verify that the mf's are getting blocked.


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
1. npm install
2. ng s -o


    
