# CS2-Spectators

Shows a live spectators count under the Minimap, While someone is watching you (first person or third person) you see `##Spectators: 2` under the minimap. When nobody is watching, the text goes away.

## Requirements

- CounterStrikeSharp 1.0.371+

## Install

1. Extract into `game/csgo`.
2. Restart the server or `css_plugins load CS2-Spectators`.

## Config

```
{
  "HiddenAdminFlag": ""
}
```
Hide admins with the selected admin flag from the spectators count.
