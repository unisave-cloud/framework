# This script distributes the built framework dll
# and it's dependencies all over my other unisave projects
# 
# This file thus works only on my PC and not anywhere else.

##############
# Distribute #
##############

VERSION=$(grep -oP "AssemblyInformationalVersion\(\"\K[^\"]+" UnisaveFramework/Properties/AssemblyInfo.cs)

# services
mkdir -p ~/ImportantCode/Unisave/Services/unisave-framework/$VERSION
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/Services/unisave-framework/$VERSION
echo $VERSION > ~/ImportantCode/Unisave/Services/unisave-framework/latest.txt

# database proxy
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/DatabaseProxy/dlls/UnisaveFramework
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/DatabaseProxy/DatabaseProxy/bin/Debug

# unity asset
cp -R UnisaveFramework/bin/Debug/UnisaveFramework.dll ~/ImportantCode/Unisave/Asset/Assets/Unisave/Libraries/UnisaveFramework
cp -R UnisaveFramework/bin/Debug/UnisaveFramework.pdb ~/ImportantCode/Unisave/Asset/Assets/Unisave/Libraries/UnisaveFramework
