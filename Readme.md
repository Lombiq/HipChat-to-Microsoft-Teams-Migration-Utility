# Lombiq HipChat to Microsoft Teams Migration Utility Readme



Utility to migrate Atlassian HipChat content to Microsoft Teams.

Note that this being a utility with just temporary use simplicity of implementation was favored against long-term maintainability. Note that the guide assumes you're using Windows but everything should work equally well under any OS supported by .NET Core.


## Usage

1. As a HipChat admin export your HipChat data from under you HipChat URL (e.g https://lombiq.hipchat.com), Group admin, Data export. Select to export every kind of data and the whole history. Use a password without any special characters or spaces.
2. Download the OpenSSL binaries if your system doesn't have them already. Recommended is the 1.0.2a (not any other version!) x64 zip from [here](https://bintray.com/vszakats/generic/openssl/1.0.2a) ([direct link to file](https://bintray.com/vszakats/generic/download_file?file_path=openssl-1.0.2a-win64-mingw.zip)). Unzip it, run *openssl.exe* and decrypt the export file with the following command: `aes-256-cbc -d -in C:\path\to\export\file.tar.gz.aes -out C:\export.tar.gz -pass pass:password`.
3. Use your favorite ZIP utility to extract the gz and tar so finally you'll end up with an unencrypted, unzipped export folder (this will contain folders like *rooms* and *users* and some further files like *rooms.json* and *users.json*). While this decrypt-unzip could be automated it's a yak shaving of epic proportions (but feel free to contribute it if you wish!) but you'll have to do it once any way.
4. Select the following permissions: Group.ReadWrite.All, User.Read.All


## Contribution and Feedback

The module's source is available in two public source repositories, automatically mirrored in both directions with [Git-hg Mirror](https://githgmirror.com):

- [https://bitbucket.org/Lombiq/hipchat-to-microsoft-teams-migration-utility/](https://bitbucket.org/Lombiq/hipchat-to-microsoft-teams-migration-utility/) (Mercurial repository)
- [https://github.com/Lombiq/Orchard-Training-Demo-Module](https://github.com/Lombiq/Orchard-Training-Demo-Module) (Git repository)

Bug reports, feature requests and comments are warmly welcome, **please do so via GitHub**.
Feel free to send pull requests too, no matter which source repository you choose for this purpose.

This project is developed by [Lombiq Technologies Ltd](https://lombiq.com/). Commercial-grade support is available through Lombiq.