# This script distributes the built framework dll
# and it's dependencies all over my other unisave projects
# 
# This file thus works only on my PC and not anywhere else.

##################
# Patch PDB file #
##################

# DISABLED
#mono ~/ImportantCode/SharpPdbPatcher/Bin/Debug/SharpPdbPatcher.exe --regex ".*UnisaveFramework" --replace "UF:" ./UnisaveFramework/bin/Debug/UnisaveFramework.dll

##############
# Distribute #
##############

# web
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/Web/resources/assemblies/framework/0.2.0

# database proxy
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/DatabaseProxy/dlls/UnisaveFramework
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/DatabaseProxy/DatabaseProxy/bin/Debug

# unity asset
cp -R UnisaveFramework/bin/Debug/UnisaveFramework.dll ~/ImportantCode/Unisave/Asset/Assets/Unisave/Libraries/UnisaveFramework
cp -R UnisaveFramework/bin/Debug/UnisaveFramework.pdb ~/ImportantCode/Unisave/Asset/Assets/Unisave/Libraries/UnisaveFramework
