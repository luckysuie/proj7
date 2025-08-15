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
                git branch: 'main', url: 'https://github.com/luckysuie/proj7.git'
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