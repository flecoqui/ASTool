FROM microsoft/dotnet:2.2.103-sdk AS build-env
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
RUN dotnet publish --self-contained -r linux-x64 -c Release -o out
#RUN dotnet publish  -c Release -o out

FROM microsoft/dotnet:2.2.103 AS runtime
WORKDIR /app
COPY --from=build-env /app/ASTool/out ./
#COPY --from=build-env /app/outputvideo ./outputvideo/

ENTRYPOINT ["ASTool", "--version"]
