$YAK_DIR = './yak-build'

if(Test-Path $YAK_DIR){
	rm -r -fo $YAK_DIR
}
mkdir $YAK_DIR

copy -Path ./bin/$RHINO_VERSION/Objectivism.gha -Destination $YAK_DIR
copy -Path ./bin/$RHINO_VERSION/*.dll -Destination $YAK_DIR
copy -Path ./logo.png -Destination $YAK_DIR
copy -Path ./manifest.yml -Destination $YAK_DIR
(Get-Content ./$YAK_DIR/manifest.yml) -replace "'__version__'", "$VERSION" | Set-Content ./$YAK_DIR/manifest.yml

cd $YAK_DIR

& "$RHINO_DIR/Yak.exe" build

copy -Path ./*.yak -Destination ../$PKG_DIR

cd ..

rm -r -fo $YAK_DIR