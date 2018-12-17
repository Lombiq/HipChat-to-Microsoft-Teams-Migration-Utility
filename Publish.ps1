$root = $PSScriptRoot # It changes during zipping for some reason so needs to be saved to a variable.
$zipPath = "$root\Lombiq.HipChatToTeams\bin\Release\netcoreapp2.2\Lombiq.HipChatToTeams.zip"

dotnet publish -r win-x64 -c release

if (Test-Path $zipPath)
{
    Remove-Item -path $zipPath
}

Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory("$root\Lombiq.HipChatToTeams\bin\Release\netcoreapp2.2\win-x64", $zipPath)