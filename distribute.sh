# This script distributes the built framework dll
# and it's dependencies all over my other unisave projects
# 
# This file thus works only on my PC and not anywhere else.

cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/Web/resources/assemblies/framework/0.1.0
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/DatabaseProxy/dlls/UnisaveFramework
cp -R UnisaveFramework/bin/Debug/* ~/ImportantCode/Unisave/DatabaseProxy/DatabaseProxy/bin/Debug
cp -R UnisaveFramework/bin/Debug/UnisaveFramework.dll ~/ImportantCode/Unisave/Asset/Assets/Unisave/Libraries/UnisaveFramework
