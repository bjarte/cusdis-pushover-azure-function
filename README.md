# cusdis-pushover-azure-function
Minimal Azure function to send messages from Cusdis to Pushover

_Cusdis_ is the commenting system I use on my websites, hvemder.no and basementmedi.no.

Cusdis has the option to send a request to a webhook every time someone adds a comment.

_Pushover_ is a webservice and an Android/iPhone app that lets you push notifications to a user with the app installed.

## Cusdis webhook docs

<https://cusdis.com/doc#/advanced/webhook?id=new-comment>

## Pushover API docs

<https://pushover.net/api>



## Azure function

Function deployed by GitHub Action on each commit.

Function available here:
https://xyz.azurewebsites.net/api/cusdisnotification

## Secrets

Store secrets in local.settings.json for local development. Create it by copying local.settings.json.example and filling in the values.

In Azure, create the following application settings for the function:

- `PushoverToken`: Your Pushover user key
- `PushoverUser`: Your Pushover app token
- `PushoverApiUrl`: The url for the Pushover API
- `CusdisUrl`: For ease of use, link to the Cusdis dashboard