pushd %~dp0

dotnet restore

REM pushd src\MiniWeb.Core
REM dotnet  build
REM popd
REM pushd src\MiniWeb.Core.UI
REM dotnet  build
REM popd
REM pushd src\MiniWeb.Storage.JsonStorage
REM dotnet build
REM popd
REM pushd src\MiniWeb.Storage.XmlStorage
REM rem dotnet build
REM popd
REM rem pushd src\MiniWeb.Storage.EFStorage
REM dotnet build
REM popd
pushd samples\SampleWeb
dotnet build
popd

popd
exit