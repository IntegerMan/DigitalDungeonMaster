# Basements and Basilisks

This is an AI Orchestration experiment to use AI to moderate a solo adventure game similar to Dungeons & Dragons.

The AI's role is to generate the story, the player's role is to make decisions and roll dice, carry out encounters, and correct the game master if it gets off track or goes down an unwanted direction.

The application is implemented as a console app at the moment.

## Setup

In order to run this project, you will need to configure some credentials. Specifically, you'll need to set your Azure OpenAI key and Azure OpenAI endpoint either in appsettings.json or, more preferably, in a user secrets file that looks like this:

```json
{
    "AzureOpenAiKey": "ReplaceWithYourValue",
    "AzureOpenAiEndpoint": "ReplaceWithYourValue"
}
```

You will also need to deploy a chat model of GPT-4 quality or later, since GPT-35-Turbo does not support the tooling capabilities Azure OpenAI needs from Semantic Kernel.

## Development Tasks

- [ ] Add Table Storage for finding Blob Resources to Look Up
- [ ] Load blob resources from Table Storage as requested
- [ ] Enable memory in Semantic Kernel
- [ ] Store game transcripts in Table Storage
- [ ] Short term: Load game transcripts from Table Storage
- [ ] Long term: Index game transcripts in Azure AI Search
- [ ] Long term: Use Azure AI search for memory
- [ ] Fetch player character sheet from an external API if we have an ID and schema
- [ ] Read markdown files in the game vault
- [ ] Add a lore service
- [ ] Add a game world service
- [ ] Investigate an agentic approach
- [ ] Investigate the new wrappers for .NET AI