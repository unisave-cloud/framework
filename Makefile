##############################
# Framework DLL distribution #
##############################

VERSION=$$(grep -oP "AssemblyInformationalVersion\(\"\K[^\"]+" UnisaveFramework/Properties/AssemblyInfo.cs)

.PHONY: copy-dll-to-asset-and-releases

copy-dll-to-asset-and-releases:
	@echo "Taking the version ${VERSION}"
	@echo "Copying to the asset..."
	@cp -R UnisaveFramework/bin/Debug/net472/UnisaveFramework.dll ../asset/Assets/Plugins/Unisave/Libraries/UnisaveFramework
	@cp -R UnisaveFramework/bin/Debug/net472/UnisaveFramework.pdb ../asset/Assets/Plugins/Unisave/Libraries/UnisaveFramework
	@echo "Copying to releases..."
	@mkdir -p releases/${VERSION}
	@cp -R UnisaveFramework/bin/Debug/net472/* releases/${VERSION}
	@echo "Updating the latest version in releases..."
	@echo ${VERSION} > releases/latest.txt
	@echo "Done."


##############################
# Releases folder management #
##############################

DEPLOYMENT_REPO=../deployment

.PHONY: upload-frameworks-dev upload-frameworks-stage upload-frameworks-prod download-frameworks-prod

upload-frameworks-dev:
	s3cmd sync -c ${DEPLOYMENT_REPO}/minio.s3cfg ./releases/ s3://unisave-dev/unisave-framework/

upload-frameworks-stage:
	s3cmd sync -c ${DEPLOYMENT_REPO}/do.s3cfg ./releases/ s3://unisave-staging/unisave-framework/

upload-frameworks-prod:
	s3cmd sync -c ${DEPLOYMENT_REPO}/do.s3cfg ./releases/ s3://unisave/unisave-framework/

download-frameworks-prod:
	s3cmd sync -c ${DEPLOYMENT_REPO}/do.s3cfg s3://unisave/unisave-framework/ ./releases/
