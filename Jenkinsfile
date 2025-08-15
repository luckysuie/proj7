pipeline{
    agent
    {
        label 'windows'
    }
    tools
    {
        msbuild 'MSBuild'
        nuget 'NuGet'
    }
    stages {
        stage('Git Checkout') {
            steps {
                echo 'Checking out code from Git...'
                git url: https://github.com/luckysuie/proj7.git, branch: 'main'
            }
        }
        stage('dotnet restore') {
            steps {
                echo 'Restoring NuGet packages...'
                bat 'nuget restore proj7.sln'
            }
        }
    }
}