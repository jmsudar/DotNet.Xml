#!/usr/bin/env bash

set -u -o pipefail

# Check if the required argument (project name) is provided
if [ $# -eq 0 ]; then
  echo "Usage: $0 <project-name>"
  exit 1
fi

# Statically set the version as it's unlikely to change frequently
VERSION="net6.0"

# Set license that will be included in properties file
LICENSE=GPL-3.0-or-later

PROJECT_NAME="$1"
AUTHOR="$2"

# Fetch the remote repository URL from 'origin'
REPO_URL=$(git remote get-url origin)

echo "Initializing $PROJECT_NAME"
echo "Author name: $AUTHOR"
echo "Repo URL: $REPO_URL"

# Check if the URL is retrieved successfully
if [ -z "$REPO_URL" ]; then
  echo "Error: Failed to retrieve repository URL from Git. Make sure you're in a Git repository and the 'origin' remote is set."
  exit 1
fi

# Format the namespace: replace dashes with dots and convert to lowercase if necessary
NAMESPACE="$(echo "$REPO_URL" | awk -F'/' '{print $(NF-1)}' | awk -F':' '{print $NF}').$(echo "$REPO_URL" | awk -F'/' '{print $NF}' | sed 's/\.git$//')"

echo "Generated namespace: $NAMESPACE"
echo ""
echo "Renaming directories and files"
echo ""

# Optional: Rename files or directories if needed
mv src/PROJECTNAME/PROJECTNAME.cs src/PROJECTNAME/"$PROJECT_NAME".cs
mv src/PROJECTNAME.Tests/PROJECTNAME.Test.cs src/PROJECTNAME.Tests/"$PROJECT_NAME".Test.cs
mv src/PROJECTNAME src/"$PROJECT_NAME"
mv src/PROJECTNAME.Tests src/"$PROJECT_NAME".Tests

echo "Initializing .NET solution and project files"
echo ""

# Create the main solution
dotnet new sln -n "$PROJECT_NAME"

# Create the source project
dotnet new classlib -n "$PROJECT_NAME" -o "src/$PROJECT_NAME" --framework $VERSION
dotnet sln "$PROJECT_NAME".sln add "src/$PROJECT_NAME/$PROJECT_NAME".csproj

# Create the test project
dotnet new mstest -n "${PROJECT_NAME}.Tests" -o "src/${PROJECT_NAME}.Tests" --framework $VERSION
dotnet sln "$PROJECT_NAME".sln add "src/${PROJECT_NAME}.Tests/${PROJECT_NAME}.Tests.csproj"

echo "Adding additional fields to project files"

# Populate property values used by NuGet publish
sed -i '' "/<\/PropertyGroup>/i \\
    <PackageId>$NAMESPACE<\/PackageId>\\
    <Version>1.0.0<\/Version>\\
    <Authors>$AUTHOR<\/Authors>\\
    <Description>PLACEHOLDER<\/Description>\\
    <PackageTags>PLACEHOLDER<\/PackageTags>\\
    <RepositoryUrl>$REPO_URL<\/RepositoryUrl>\\
    <PackageLicenseExpression>$LICENSE<\/PackageLicenseExpression>\\
    <PackageProjectUrl>$REPO_URL<\/PackageProjectUrl>\\
    <PackageReadmeFile>README.md<\/PackageReadmeFile>\\
" "src/${PROJECT_NAME}/${PROJECT_NAME}.csproj"

# Add the README to your project properties file
sed -i '' "/<\/Project>/i \\
  <ItemGroup>\\
    <None Include=\"..\/..\/README.md\" Pack=\"true\" PackagePath=\"\/\" \/>\\
  <\/ItemGroup>\\
  \\
" "src/${PROJECT_NAME}/${PROJECT_NAME}.csproj"

# Add newly created method class as covered in the test project file
sed -i '' "/<\/Project>/i \\
  <ItemGroup>\\
    <ProjectReference Include=\"..\/${PROJECT_NAME}\/${PROJECT_NAME}.csproj\" \/>\\
  <\/ItemGroup>\\
  \\
" "src/${PROJECT_NAME}.Tests/${PROJECT_NAME}.Tests.csproj"

echo "Adding CI workflow"
echo ""

cat <<EOF > .github/workflows/dotnet-ci.yaml
name: .NET Continuous Integration

on:
  pull_request:

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - name: Run .NET CI Action
        uses: jmsudar/dotnet-continuous-integration@main
EOF

echo "Cleaning up files and dependencies"
echo ""

echo "Replacing NAMESPACE with $NAMESPACE"
echo ""

# Replace NAMESPACE with your project name
sed -i '' "s/NAMESPACE/$NAMESPACE/g" "src/${PROJECT_NAME}/${PROJECT_NAME}.cs"

# Removes coverlet.collector third party dependency
sed -i '' '/coverlet.collector/d' "src/${PROJECT_NAME}.Tests/${PROJECT_NAME}.Tests.csproj"

# Removing auto-generated class file
rm "src/$PROJECT_NAME/Class1.cs"

# Removing auto-generated test class files
rm "src/${PROJECT_NAME}.Tests/UnitTest1.cs"
rm "src/${PROJECT_NAME}.Tests/Usings.cs"

echo "Deleting initialization script"
echo ""

# Self-delete the script
rm -- "$0"

echo "Setup completed and cleanup done."
