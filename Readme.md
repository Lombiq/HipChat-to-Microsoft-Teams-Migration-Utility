# Lombiq HipChat to Microsoft Teams Migration Utility Readme



Utility to migrate Atlassian HipChat content to Microsoft Teams. You can use this instead of waiting for [official support](https://microsoftteams.uservoice.com/forums/555103-public/suggestions/16933120-importing-from-slack-hipchat-flowdock-basecamp). We're testing this utility at [Lombiq Technologies](https://lombiq.com) (a web development company working with Microsoft technologies) with a 4GB+ HipChat export package containing more than 200k messages.

Currently the app can import rooms and messages from a HipChat export file into Teams channels under a specific team. See below for missing features and bugs.

Note that this being a utility with just temporary use simplicity of implementation was favored against long-term maintainability. Note that the guide assumes you're using Windows but everything should work equally well under any OS supported by .NET Core.


## Usage

Keep in mind that you need to be both a HipChat and a Teams admin in your company for this to work.

1. As a HipChat admin export your HipChat data from under you HipChat URL (e.g https://lombiq.hipchat.com), Group admin, Data export. Select to export every kind of data and the whole history. Use a password without any special characters or spaces. Save the file under a path without any special characters.
2. Download the OpenSSL binaries if your system doesn't have them already. Recommended is the 1.0.2a (not any other version!) x64 zip from [here](https://bintray.com/vszakats/generic/openssl/1.0.2a) ([direct link to file](https://bintray.com/vszakats/generic/download_file?file_path=openssl-1.0.2a-win64-mingw.zip)). Unzip it, run *openssl.exe* and decrypt the export file with the following command: `aes-256-cbc -d -in C:\path\to\export\file.tar.gz.aes -out C:\export.tar.gz -pass pass:password`.
3. Use your favorite ZIP utility to extract the gz and tar so finally you'll end up with an unencrypted, unzipped export folder (this will contain folders like *rooms* and *users* and some further files like *rooms.json* and *users.json*). While this decrypt-unzip could be automated it's a yak shaving of epic proportions (but feel free to contribute it if you wish!) but you'll have to do it once any way.
4. Go to the [Graph Explorer](https://developer.microsoft.com/en-us/graph/graph-explorer) and log in, confirm the required permissions. Then do the following:
    1. Click on "show more samples", turn "Microsoft Teams" and "Microsoft Teams (beta)" on.
    2. Try to run e.g. the Microsoft Teams / create channel operation. You'll get an error that you don't have the necessary permissions. Click on "modify your permission".
    ![alt text](Screenshots/InsufficentPermissions.png)
    3. Select the following permissions: Group.ReadWrite.All, User.Read.All. You'll need to log in again.
5. Once the permissions are OK then run the request. Copy the bearer token (just the token, without the "Bearer" text) used by the request into the *AppSettings.json* configuration file under the `AuthorizationToken` config. You can e.g. use Chrome DevTools to see this token in the Request headers. Specify the rest of the configuration as well:
    - `ExportFolderPath`: The file system path to the folder where you unzipped the HipChat export package.
    - `TeamNameToImportChannelsInto`: Name of the Teams team where you want all the channels to be imported into. Currently all channels are imported into a single team. This wouldn't be an issue if channels could be moved (https://microsoftteams.uservoice.com/forums/555103-public/suggestions/16939708-move-channels-into-other-teams).
    - `ThrottlingCooldownSeconds`: How much time, in seconds, the tool will wait if API requests are throttled.
6. Run the app and wait for the import to complete. In the console you'll see status and possibly error messages.


## Notable features missing and bugs

Features:
- Ability to create channels under multiple teams.
- Pushing messages from a HipChat room to an existing channel (mostly needed for the General channel).
- Mentions
- [Request batching](https://docs.microsoft.com/en-us/graph/json-batching) to avoid API throttling slowing down the import.
- Possibly a better way to log in for the API instead of fishing out the authorization token.

Bugs:
- Message timestamps don't take effect.
- Attachments don't get attached.
- Messages are not posted in the name of the original user (while this is not implemented during initial testing we couldn't find a way to do this).

Also see: https://techcommunity.microsoft.com/t5/Microsoft-Teams/Chat-thread-creation-API-issues/m-p/302388#M22558.


## Some implementation notes

- [Here's](https://confluence.atlassian.com/hipchatkb/exporting-from-hipchat-server-or-data-center-for-data-portability-950821555.html) some information on the HipChat export's schema.
- Some inspiration is taken from https://github.com/microsoftgraph/csharp-teams-sample-graph.


## Contribution and Feedback

The module's source is available in two public source repositories, automatically mirrored in both directions with [Git-hg Mirror](https://githgmirror.com):

- [https://bitbucket.org/Lombiq/hipchat-to-microsoft-teams-migration-utility/](https://bitbucket.org/Lombiq/hipchat-to-microsoft-teams-migration-utility/) (Mercurial repository)
- [https://github.com/Lombiq/Orchard-Training-Demo-Module](https://github.com/Lombiq/Orchard-Training-Demo-Module) (Git repository)

Bug reports, feature requests and comments are warmly welcome, **please do so via GitHub**.
Feel free to send pull requests too, no matter which source repository you choose for this purpose.

This project is developed by [Lombiq Technologies Ltd](https://lombiq.com/). Commercial-grade support is available through Lombiq.