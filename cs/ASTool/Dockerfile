FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app


# copy csproj and restore as distinct layers
COPY  ASTool/*.csproj ./ASTool/
WORKDIR /app/ASTool
RUN dotnet restore

# copy everything else and build app
WORKDIR /app
#COPY outputvideo/. ./outputvideo/

COPY ASTool/. ./ASTool/
WORKDIR /app/ASTool
RUN dotnet publish -c Release -o out


FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /app
COPY --from=build-env /app/ASTool/out ./
#COPY --from=build-env /app/outputvideo ./outputvideo/

ENTRYPOINT ["dotnet", "ASTool.dll"]




#ENTRYPOINT ["dotnet", "ASTool.dll",  "--pullpush", "--input", "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest", "--minbitrate", "300000"   ,"--maxbitrate" ,"1000000","--liveoffset","10" ,"--output", "http://channel2-testlivevi-use2.channel.media.azure.net/ingest.isml"]

# dotnet ASTool.dll --pull --input http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest --output ./app --maxduration 10000

#ASTool.exe --pullpush --input http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest --minbitrate 300000   --maxbitrate 2000000  --output http://channel2-testlivevi-use2.channel.media.azure.net/ingest.isml  