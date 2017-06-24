pushd %~dp0

dotnet restore
pushd src\MiniWeb.Core\Resources
call tsc admin.ts
popd
pushd src\MiniWeb.Core
dotnet  build
popd
pushd src\MiniWeb.Storage.JsonStorage
dotnet build
popd
pushd src\MiniWeb.Storage.XmlStorage
rem dotnet build
popd
rem pushd src\MiniWeb.Storage.EFStorage
dotnet build
popd
pushd samples\SampleWeb
dotnet build
popd

popd
exit