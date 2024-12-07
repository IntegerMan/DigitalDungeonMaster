# Digital Dungeon Master

This is an AI Orchestration experiment to use AI to moderate a solo adventure game similar to Dungeons & Dragons.

The AI's role is to generate the story, the player's role is to make decisions and roll dice, carry out encounters, and correct the game master if it gets off track or goes down an unwanted direction.

The application is implemented as a console app at the moment.

## Setup

In order to run this project, you will need to configure some credentials. Specifically, you'll need to set your Azure OpenAI key and Azure OpenAI endpoint either in appsettings.json or, more preferably, in a user secrets file that looks like this:

```json
{
  "AzureResources": {
      "AzureOpenAiKey": "ReplaceWithYourValue",
      "AzureOpenAiEndpoint": "ReplaceWithYourValue",
      "AzureStorageConnectionString": "ReplaceWithYourValue"
    }
}
```

You'll also need to configure names of non-secret resource values in the `appconfig.json` file.

You will also need to deploy a chat model of GPT-4 quality or later, since GPT-35-Turbo does not support the tooling capabilities Azure OpenAI needs from Semantic Kernel.

The game also relies on a number of tables in Azure Table Storage. Documentation is not yet available on how to set these up, and I am actively working on wrapping around this via an API.

Efforts to get the application running on a local machine are being tracked against [this issue](https://github.com/IntegerMan/DigitalDungeonMaster/issues/13).
