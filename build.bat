pushd src\MiniWeb.Core
dotnet restore
dotnet  compile
popd
REM pushd src\MiniWeb.Storage.JsonStorage
REM dotnet compile
REM popd
REM pushd src\MiniWeb.Storage.XmlStorage
REM dotnet compile
REM popd
REM pushd src\MiniWeb.Storage.EFStorage
REM dotnet compile
REM popd
REM pushd samples\SampleWeb
REM dotnet compile
REM popd