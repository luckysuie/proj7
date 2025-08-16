pipeline{
    agent
    {
        label 'windows'
    }
    tools
    {
        msbuild 'MSBuild'
    }
    stages {
        stage('Git Checkout') {
            steps {
                echo 'Checking out code from Git...'
                git branch: 'main', url: 'https://github.com/luckysuie/proj7.git'
            }
        }
    }
}