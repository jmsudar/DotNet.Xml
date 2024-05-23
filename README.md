# dotnet-library-template

This template repository allows you to quickly create a .NET library project with minimal clutter and zero third-party (defined here as non-DotNet Framework) libraries. This is meant to be used as an alternative to project creation via Visual Studio's built in project generation, suitable for projects where the extra abstraction that VS introduces is not required. It can also be used to generate a solution project if you do not have access to Visual Studio, such as if you prefer to edit via Vim.

# Structure

Library repos created with this template have the following characteristics
- Minimal clutter: when initialized, this repo will contain a `src` directory with two subdirectories: one for methods and one for unit tests, each containing a single placeholder code file and .csproj file. No unnecessary or unneeded files created as part of the `dotnet new` command will persist.
- CI/CD via GitHub Actions: the repo comes pre-provisioned with a `.github/workflows` directory and will handle new version tagging, changelog generation, release creation, unit testing for CI, and publishing to nuget via CD.
- Utility files: The repo comes pre-provisioned with a .gitignore file, GPL-3 license, and this README for you to replace.

# Usage

To use this repo, create a repo in GitHub selecting this as your template. Clone the repo locally, then run `./INITIALIZE_REPOSITORY.sh <project-name> <author-name>`. These two arguments will replace placeholder tect used to pre-populate elements such as the property information in the primary class's .csproj file, which will ensure a rich nuget publish.

The initialization script will run, then delete itself, leaving you with a clean repository with the following structure:

.

|_.github/workflows

|__create-release.yaml

|_src

|__project-name

|___project-name.cs

|___project-name.csproj

|__project-name.Tests

|___project-name.Test.cs

|___project-name.Test.csproj

|_.gitignore

|_LICENSE

|_project-name.sln

You then simply need to write your class and unit tests, provision your GitHub secrets, and check your work back in. The PR and merge-hook GitHub actions will run, leaving you with an easy to work and publish project that auto-publishes to nuget!
