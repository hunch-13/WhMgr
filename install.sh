# Download .NET Core 2.1 installer
echo "Downloading .NET Core 2.1 installer..."
wget https://dotnetwebsite.azurewebsites.net/download/dotnet-core/scripts/v1/dotnet-install.sh

# Make installer executable
echo "Setting executable permissions..."
chmod +x dotnet-install.sh

# Install .NET Core 2.1.0
echo "Launching .NET Core installer..."
./dotnet-install.sh --version 2.1.803

# Delete .NET Core 2.1.0 installer
echo "Deleting .NET Core installer..."
rm dotnet-install.sh

# Clone repository
echo "Cloning repository..."
git clone https://github.com/versx/WhMgr

# Change directory into cloned repository
echo "Changing directory..."
cd WhMgr

# Build WhMgr.dll
echo "Building WhMgr..."
~/.dotnet/dotnet build

# Copy example config
echo "Copying example files..."
cp -R examples/alerts bin/alerts/
cp -R examples/filters bin/filters/
cp -R examples/geofences bin/geofences/
cp -R static/ bin/static/
cp alarms.example.json bin/alarms.json
cp config.example.json bin/config.json

echo "Changing directory to build folder..."
cd bin
