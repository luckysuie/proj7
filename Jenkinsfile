pipeline {
  agent { label 'windows' }

  tools {
    msbuild 'MSBuild'  // Ensure MSBuild is configured in Jenkins global tool configuration
  }
    

  stages {
    stage('Checkout') {
      steps {
        echo 'Checking out code from Git...'
        git branch: 'main', url: 'https://github.com/luckysuie/proj7'
      }
    }

    stage('Restore NuGet Packages') {
      steps {
        echo 'Restoring NuGet packages...'
        // If nuget.exe is on PATH:
        bat 'nuget restore proj7.sln'
        // If using a configured NuGet tool instead:
        // bat "\"%NUGET_HOME%\\nuget.exe\" restore proj7.sln"
      }
    }

//     stage('Build') {
//       steps {
//         echo 'Building solution with MSBuild...'
//         // If MSBuild tool adds to PATH, this works:
//         bat 'msbuild proj7.sln /t:Build /p:Configuration=Release /m'
//         // If you need the full path, use:
//         // bat "\"%MSBUILD_HOME%\\MSBuild.exe\" proj7.sln /t:Build /p:Configuration=Release /m"
//       }
//     }

//     // Optional: run tests if you have them
//     // stage('Test') {
//     //   steps {
//     //     bat 'vstest.console.exe path\\to\\YourTests.dll /Logger:trx'
//     //   }
//     // }

//     stage('Archive Artifacts') {
//       steps {
//         echo 'Archiving build outputs...'
//         archiveArtifacts artifacts: '**/bin/Release/**/*', fingerprint: true, onlyIfSuccessful: true
//       }
//     }
//   }

//   post {
//     always {
//       echo 'Pipeline completed.'
//       // junit '**/TestResults/*.trx'  // enable if you run vstest above
//     }
//     failure {
//       echo 'Build failed. Check Console Output for details.'
//     }
  }
}
