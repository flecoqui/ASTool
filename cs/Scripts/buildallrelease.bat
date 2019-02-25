dotnet publish C:\git\flecoqui\ASTool\cs\ASTool\ASTool --self-contained -c Release -r win-x64
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.win.zip C:\git\flecoqui\ASTool\cs\ASTool\ASTool\bin\Release\netcoreapp2.2\win-x64\publish\
dotnet publish C:\git\flecoqui\ASTool\cs\ASTool\ASTool  --self-contained -c Release -r linux-x64
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.linux.tar C:\git\flecoqui\ASTool\cs\ASTool\ASTool\bin\Release\netcoreapp2.2\linux-x64\publish\
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.linux.tar.gz c:\temp\LatestRelease.linux.tar 
dotnet publish C:\git\flecoqui\ASTool\cs\ASTool\ASTool  --self-contained -c Release -r linux-musl-x64
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.linux-musl.tar C:\git\flecoqui\ASTool\cs\ASTool\ASTool\bin\Release\netcoreapp2.2\linux-x64\publish\
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.linux-musl.tar.gz c:\temp\LatestRelease.linux-musl.tar 
dotnet publish C:\git\flecoqui\ASTool\cs\ASTool\ASTool  --self-contained -c Release -r rhel-x64
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.rhel.tar C:\git\flecoqui\ASTool\cs\ASTool\ASTool\bin\Release\netcoreapp2.2\rhel-x64\publish\
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.rhel.tar.gz c:\temp\LatestRelease.rhel.tar 
dotnet publish C:\git\flecoqui\ASTool\cs\ASTool\ASTool  --self-contained -c Release -r osx-x64
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.osx.tar C:\git\flecoqui\ASTool\cs\ASTool\ASTool\bin\Release\netcoreapp2.2\osx-x64\publish\
"C:\Program Files\7-Zip\7z.exe" a c:\temp\LatestRelease.osx.tar.gz c:\temp\LatestRelease.osx.tar 
copy /Y c:\temp\LatestRelease.win.zip C:\git\flecoqui\ASTool\Releases\LatestRelease.win.zip
copy /Y c:\temp\LatestRelease.linux.tar.gz C:\git\flecoqui\ASTool\Releases\LatestRelease.linux.tar.gz
copy /Y c:\temp\LatestRelease.linux-musl.tar.gz C:\git\flecoqui\ASTool\Releases\LatestRelease.linux-musl.tar.gz
copy /Y c:\temp\LatestRelease.rhel.tar.gz C:\git\flecoqui\ASTool\Releases\LatestRelease.rhel.tar.gz
copy /Y c:\temp\LatestRelease.osx.tar.gz C:\git\flecoqui\ASTool\Releases\LatestRelease.osx.tar.gz




