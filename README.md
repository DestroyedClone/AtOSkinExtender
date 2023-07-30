
# Skin Extender

| [![github issues/request link](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/github_link.webp)](https://github.com/DestroyedClone/AtOSkinExtender) | [![discord invite](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/discord_link.webp)](https://discord.gg/DpHu3qXMHK) |
|--|--|

Attempts to extend current skins.

## Changes (User)
* Adds scrolling support to character skin selection.
* Increases max amount of shown skins from 6 to 12.
* Character skin defaults if selected custom skin is missing
* Shows where the skin comes from (config)

## Changes (Dev)
* Adds UnityAction `onCreateSkins` which is called after `Globals_CreateGameContent` and before auto-adding skins from a dictionary.
* Adds method `CreateSkinData` to simplify initialization of creating a ScriptableObject instance of SkinData.
* Overrides `CharPopup.DoSkins` to only show specific amounts of skins per 'page'. Also fixes out of range exception.
	* UnityActions `Pre_onCharPopupDoSkins` and `Post_onCharPopupDoSkins` are made to substitute regular orig() calls.
	* If you know IL, submit a PR to override it.
* Adds post-fix to `On.HeroSelectionManager.Start` to check through all skins, choosing the base skin (or first skin if no base skins) to prevent a Nullref from the game trying to use a null SkinData.

## Simple Implementation (Dev)
1. In your mod's Awake, create your SkinData manually or by `AtOSkinExtender.Assets.CreateSkinData`
2. Subscribe to `AtOSkinExtender.Plugin.onCreateSkins` to ensure your SkinData gets added and recognized by the mod.
3. Within your subscription, add your SkinData to the packlist via `AtOSkinExtender.Assets.AddSkinDataToPack`
	2.1. Use the name of your mod for the identifier
	2.2. The identifier is used to autosort based on client's configuration settings and to show skin mod origin

> Written with [StackEdit](https://stackedit.io/).