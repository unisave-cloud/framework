# This script distributes the built framework dll
# and it's dependencies all over my other unisave projects
# 
# This file thus works only on my PC and not anywhere else.

##############
# Distribute #
##############

VERSION=$(grep -oP "AssemblyInformationalVersion\(\"\K[^\"]+" UnisaveFramework/Properties/AssemblyInfo.cs)

# releases
echo "Copying to releases..."
mkdir -p releases/$VERSION
cp -R UnisaveFramework/bin/Debug/net472/* releases/$VERSION
echo $VERSION > releases/latest.txt

# unity asset
echo "Copying to the asset..."
cp -R UnisaveFramework/bin/Debug/net472/UnisaveFramework.dll ../asset/Assets/Plugins/Unisave/Libraries/UnisaveFramework
cp -R UnisaveFramework/bin/Debug/net472/UnisaveFramework.pdb ../asset/Assets/Plugins/Unisave/Libraries/UnisaveFramework

# done
echo "Done."
