# Valorant Companion

A Windows desktop companion app for **Valorant**, built with **C#**, **WinForms**. It provides quick access to player stats, competitive ranks, store offers, and live match information using the **RadiantConnect API Wrapper**.

---

## Features

### Player Overview
- Displays Riot ID, Player Card.
- Fetches and shows current MMR and rank icon.
- Recent competitive matches

### Match Utilities
- **Dodge Pre-Game Lobby**: Leave a pre-game match before it starts.
- **Match Details**: View all players in the current or pre-game lobby, including:
  - Agent
  - Name (Including hidden)
  - Competitive rank

### Store & Skins
- Shows the current skin offers in the store.

### Insta Lock
- Select and lock and agent before the game starts
> [!WARNING]
> Riot may ban you for API abuse if you consistently use Insta Lock. Use at your own risk

### UI
- Built with **MaterialSkin** for a modern dark theme.
- Fixed, non-resizable windows with clean layout.
- Loading overlay for network operations.

### Dependencies

- .NET 8.0
- Riot Client needs to be signed in with Remember Me, or have the game open at the time of opening.

---

## Screenshots


![alt text](https://i.imgur.com/EgFLIm4.png "")
![alt text](https://i.imgur.com/7ahFlq9.png "")
![alt text](https://i.imgur.com/gYFapdS.png "")



---

