# GitHubWebHookDispatcher

The [Nirvana](https://github.com/Illumina/Nirvana) development team is starting to use [SonarQube](https://www.sonarqube.org/) more and more to provide static code analysis for our variant annotator. SonarQube is a slick tool that I encourage every team to test drive, however there are aspects of using SonarQube that are still rough around the edges. One such aspect is automating when the SonarQube scanner is launched.

We wanted to be able to run the SonarQube scanner whenever we push changes to our GitHub server. GitHub provides this functionality via webhooks, but we had nothing on the client side to accept the webhooks and then launch the scanner. GitHubWebHookDispatcher fills this gap with a simple ASP.NET core web api.

## Compiling & running the dispatcher

1. Make sure you have [.NET core](https://www.microsoft.com/net/download/core) installed for your platform (Windows, Mac, Linux)
1. git clone https://github.com/MichaelStromberg/GitHubWebHookDispatcher.git
1. cd GitHubWebHookDispatcher
1. dotnet restore
1. cd GitHubWebHookDispatcher
1. Edit appsettings.json with your favorite editor. In the ScriptRepositoryPairs section, you'll associate the script path with the git repository URL.
1. dotnet run -c Release

## Configuring GitHub

Note that the server will open port 7000 by default (we'll make this configurable soon). At this point, add a new webhook to your GitHub repository. Set the webhook to the following:

http://1.2.3.4:7000/api/hooks

where 1.2.3.4 is the IP of the machine running the GitHubWebHookDispatcher.

## Sequence diagram

<img align="center" src="https://github.com/MichaelStromberg/GitHubWebHookDispatcher/wiki/images/GitHubWebHookDispatcherUmlSequenceDiagram.png" width="600" />
