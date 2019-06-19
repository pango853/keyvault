KeyVault is an easy & free to use, offline pasword manager. It is implemented with .NET Framework.


# How-To

## Configuration

First change **KeyVault.exe.config** as below.

```KeyVault.exe.config
	<appSettings>
		<add key="SALT" value="----FILL-IN-WITH-YOUR-OWN-Cryptographic-Salt-HERE-AND-NEVER-FORGET-IT!!!----"/>
		<add key="DB_PATH" value="----Specify-filename-or-path-for-the-database-like-C:\MY-SECRET-PLACE\keyvault.db----"/>
	</appSettings>
```

## Add a password
Start KeyVault.exe and run a command like `/add MyGitHubAccount https://github.com/login MyUserName MyPassword`.
Confirm in the add dialog and press OK to store it.

## Get a password
Just type in part of the name or URL to search for the record.
And they press Enter to copy the URL, username and password one by one.


# TODO
- [ ] Show a list and let user to choose when more than one matches are found.
- [ ] Allow minimizing.
- [ ] Show tooltips.
- [ ] **URGENT** Check .NET Framework compatibility
- [ ] **URGENT** Package missing in release 0.2.0
