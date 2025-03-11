# Development setup (⁠🚂 framework)

You need Rider, `mono` CLI, and .NET SDK. [Install](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install?tabs=dotnet8&pivots=os-linux-ubuntu-2204#install-the-sdk) the SDK 8.0:

```
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```


## Setting up

- Clone the repo `git clone git@github.com:unisave-cloud/framework.git`
- Follow the [After Cloning](../README.md#after-cloning) checklist in the root README file.


## New feature development

- Bump the version and add the `-dev` suffix in the assembly info
- Add the feature and commit changes


## Testing in isolation

- Write unit tests and run those


## Testing in the asset

- Keep the `-dev` suffix
- Build the framework DLL (from Rider)
- Copy the DLL to the asset repository
    - `make copy-dll-to-asset-and-releases`
- (for backend testing see the next section)
- Use framework in the asset client-side
    - (typically write unit tests or run some example scene)
- Optionally reset to the old DLL
    - `~/unisave/asset$ git checkout Assets/Plugins/Unisave/Libraries`


## Testing in the backend

- Keep the `-dev` suffix
- Build the framework DLL (from Rider)
- Distribute the DLL to the local releases folder
    - `make copy-dll-to-asset-and-releases`
- Upload the local releases folder to the minikube cluster
    - `make upload-frameworks-dev`
- Make sure all workers are dead
- Delete the uploaded backend record to re-compile it
- Run the asset tests


### Deploying new version (10 - 30 mins)

> Read [DLL file distribution](dll-file-distribution.md) for more info.

- Remove the `-dev` suffix from the assembly info
    - (or possibly make it a `-rc.1` release candidate)
- Build the framework DLL (from Rider)
- Sync local releases folder with the production cluster (pull)
    - `make download-frameworks-prod`
- Distribute the DLL to the local releases folder
    - `make copy-dll-to-asset-and-releases`
- Upload the local releases folder to the minikube cluster
    - `make upload-frameworks-dev`
- Run test fixture from the asset against the minikube cluster
- Upload the local releases folder to the production cluster
    - `make upload-frameworks-prod`
- Commit the version to git
- Create a Github release page
    - (no need to include the dll there)
- In the asset repository, commit the new framework dll to git
    - (possibly relase asset, see asset development setup deployment checklist)
