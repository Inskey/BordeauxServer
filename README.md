#Bordeaux Remote Console
A lightweight solution to accessing your Minecraft Server's console with ease.  
Allows managing of multiple Minecraft Servers on the same machine.

##How to use it  
###Installation  
Installing BordeauxRC is streamlined in order to make the process as simple as possible.

####Windows
Simply upload the BordeauxRCServer.exe to the Windows server and start the Bordeaux server once in order to generate the default settings.  
Then create a new text file inside the newly created BordeauxRCServer directory.

See the section below titled **Config** for more info on what to put in this text file.

####Linux/OSX
For Linux/OSX Based systems, installation is slightly more tricky, as there is an extra step involved.  
In order to use Bordeaux on a Unix based server, you will first need to install Mono. You can find info on how to do this [here](http://www.mono-project.com/docs/getting-started/install/)

Once Mono is installed on your server, you can run the BordeauxRCServer.exe file by running the following command in your terminal: `mono BordeauxRCServer.exe`

Once you have run the server once close it, then create a new text file inside the newly created BordeauxRCServer directory.

See the section below titled **Config** for more info on what to put in this text file.

###Config
Config files for the Bordeaux server are handled as individual text files (`.txt` extention) in the BordeauxRCServer directory created when you first run the server.  
You will need to make a separate text file for each Minecraft Server you want Bordeaux to manage, and each text file will need 4 lines:

* First line: Server name - This must be unique for each server, and will function as the login username for this server  
* Second Line: Server password - This password will be required in order to login to this server, note that the Bordeaux server will hash your password the first time it reads your config file, so the password is *not* stored in plaintext
* Third Line: Absolute directory to the server jar file - Note that this is the **absolute** path, meaning you will need to specify the drive letter on Windows, or start from the root directory `/` on Linux/OSX  
* Fourth Line: Additional Java args - Any additional args you would like Bordeaux to start your server with, such as min/max memory allocation. These are specified as you would specify them in a `start.bat` or `start.sh` file

###Usage
Once you have properly configured your server, using it is simple.  
Just open up BordeauxRCClient.exe on your own PC (If you are running Linux/OSX you can use Mono as detailed in the Linux/OSX installation instructions)  
Once opened, a dialog will pop up asking for `Host`, `User` and `Pass`.

`Host` is the IP address or DNS of the Bordeaux server, the same one you would use to SSH or Remote Desktop into the server machine.  
`User` is the username you specified for the specific minecraft server you wish to access  
`Pass` is the password you specified for the specific minecraft server you wish to access

###Bugs/Issues
If you encounter any bugs or have any issues setting up Bordeaux on your system or just have a suggestion, please don't hesitate to submit an Issue ticket here on GitHub.

It's the best way to contact the Bordeaux devs regarding bugs and support.