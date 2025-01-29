param([Switch]$push)

[xml]$csproj = Get-Content ./ObjectivismGH/Objectivism.csproj
$VERSION = $csproj.Project.PropertyGroup.Version
Write-Output "Version = $VERSION"

$RHINO_DIR = (Get-ItemProperty -Path "HKLM:\SOFTWARE\McNeel\Rhinoceros\8.0\Install\").Path
Write-Output "Rhino install path = $RHINO_DIR"

dotnet msbuild -restore -p:Configuration=Release ./ObjectivismGH/
if (-not $?) {throw "Build Failed"}

$PKG_DIR = 'yak-pkgs'
if(!(Test-Path $PKG_DIR)){
	mkdir $PKG_DIR
}

$RHINO_VERSION = "Rhino7"
./make-yak.ps1
$RHINO_VERSION = "Rhino8"
./make-yak.ps1

if($push.IsPresent)
{
	Write-Output "Logging in to YAK"
	& "$RHINO_DIR/Yak.exe" login

	cd $PKG_DIR
	foreach ($file in Get-ChildItem -Path .  -Filter "*.yak") 
	{
		Write-Output "Processing file: $($file.FullName)"
		& "$RHINO_DIR/Yak.exe" push $file.FullName
	}
}

