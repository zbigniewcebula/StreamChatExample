# Clan Chat Example using Stream Unity SDK
This is example usage of [Stream API](https://getstream.io/) with Unity engine, in a form of simple clan-like + chat feature in mobile games.

##### Used:
- Unity version: 2021.3.24f1
- SDK version: 4.2.0

### Features
* Join/Leave clan
* Basic chat messaging
* Chat message reactions
* Searching for clan (chat room = clan)
* Filtering through clan list

## How to start
Please register new profile on the [Stream API](https://getstream.io/) website. 
Proceed with [documentation](https://getstream.io/chat/docs/unity/?language=unity) to acquire API keys. 
Use API keys by setting them in _Assets/AuthCredentialsExample_ scriptable object. 
Using _Hierarchy_ find **StreamManager** object and **StreamManager** component. 
Bind the _AuthCredentialsExample_ to _Auth Credentials Asset_ variable. 

Press play to use your API!

## Important!
Your setup contains no users nor channels/chat rooms/clans. 
You need to create those using API (mainly server-focused)!

# Screenshots
![<Filtering through clans>](/README/filtering.png "Clan filtering")
![<Clan screen>](/README/clan.png "Clan preview")
![<Clan chat>](/README/chat.png "Clan chat")
![<Clan leave>](/README/leave.png "Clan leave")

