dotnet clean
rm -rf pack
dotnet pack -c Release -o ./pack
