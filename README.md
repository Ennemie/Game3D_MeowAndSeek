# 🐱 Meow And Seek

### 🎮 Multiplayer 3D Hide & Seek Game

> **Meow And Seek** là một game 3D multiplayer được phát triển bằng **Unity**, kết hợp **Photon Fusion** cho hệ thống networking và **PlayFab** cho backend/game data.

---

## 🎬 Game Preview

<p align="center">
  <img src="docs/images/gameplay.png" width="850" alt="Meow And Seek Gameplay">
</p>

> 🚧 **Gameplay screenshot / GIF coming soon**

<p align="center">
  <a href="https://github.com/Ennemie/Game3D_MeowAndSeek">
    <img src="https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github" alt="GitHub">
  </a>
  <img src="https://img.shields.io/badge/Unity-6000.3.9f1-black?style=for-the-badge&logo=unity" alt="Unity">
  <img src="https://img.shields.io/badge/Photon-Fusion-blue?style=for-the-badge" alt="Photon Fusion">
  <img src="https://img.shields.io/badge/PlayFab-Backend-orange?style=for-the-badge" alt="PlayFab">
</p>

---

## 🐾 About The Game

**Meow And Seek** là một trải nghiệm hide-and-seek multiplayer 3D, nơi người chơi được chia thành hai vai trò:

* 🔴 **Seeker** — tìm kiếm và tấn công những người chơi khác.
* 🟢 **Hider** — di chuyển, ẩn nấp và sử dụng khả năng disguise để tránh bị phát hiện.

Mỗi trận đấu có hệ thống chọn role, countdown, gameplay timer, HP/Mana và các khả năng riêng cho từng role.

---

## 🎮 Gameplay

### 🔴 Seeker

Mục tiêu của Seeker là tìm và đánh bại các Hider trước khi trận đấu kết thúc.

**Khả năng:**

* Tốc độ di chuyển được tăng.
* Sử dụng kỹ năng tấn công.
* Tiêu hao Mana khi sử dụng kỹ năng.
* Gây damage lên Hider trong phạm vi tấn công.

### 🟢 Hider

Hider phải tận dụng môi trường và khả năng disguise để sống sót.

**Khả năng:**

* Di chuyển và tìm vị trí ẩn nấp.
* Sử dụng disguise.
* Thay đổi ngoại hình để hòa vào môi trường.
* Sử dụng Mana cho kỹ năng.

---

## ⚡ Core Features

| Feature            | Description                              |
| ------------------ | ---------------------------------------- |
| 🌐 Multiplayer     | Multiplayer networking với Photon Fusion |
| 🎭 Seeker / Hider  | Hai role với gameplay khác nhau          |
| 🎲 Random Role     | Chọn Seeker ngẫu nhiên                   |
| 🏃 Player Movement | Di chuyển, nhảy và tương tác             |
| ❤️ HP System       | Quản lý máu và trạng thái player         |
| 💙 Mana System     | Tài nguyên cho các kỹ năng               |
| ⚔️ Attack          | Hệ thống tấn công của Seeker             |
| 🥸 Disguise        | Hider có thể sử dụng disguise            |
| 💬 Chat            | Chat giữa người chơi                     |
| ⏱️ Match Timer     | Quản lý thời gian trận đấu               |
| 🎥 Camera          | Camera system với Cinemachine            |
| 🔊 Audio           | Sound và gameplay audio                  |
| ☁️ PlayFab         | Player data và match data                |
| 🧭 Navigation      | AI Navigation support                    |
| ✨ URP              | Universal Render Pipeline                |

---

## 🧠 Game Flow

```text
                 ┌──────────────┐
                 │     Lobby    │
                 └──────┬───────┘
                        │
                        ▼
                 ┌──────────────┐
                 │ Players Ready│
                 └──────┬───────┘
                        │
                        ▼
              ┌────────────────────┐
              │ Randomly Select     │
              │      Seeker         │
              └─────────┬──────────┘
                        │
                        ▼
                 ┌──────────────┐
                 │   Countdown  │
                 └──────┬───────┘
                        │
             ┌──────────┴──────────┐
             ▼                     ▼
       🔴 SE E K E R          🟢 H I D E R
             │                     │
             │ Hunt                │ Hide
             │ Attack              │ Disguise
             │                     │
             └──────────┬──────────┘
                        ▼
                 ┌──────────────┐
                 │ Match Result │
                 └──────┬───────┘
                        │
                        ▼
                    ☁️ PlayFab
```

---

## 🛠️ Tech Stack

### 🎮 Game Development

* **Unity 6**
* **C#**
* **Universal Render Pipeline**
* **Unity Input System**
* **Cinemachine**
* **AI Navigation**
* **TextMesh Pro**

### 🌐 Multiplayer & Backend

* **Photon Fusion**
* **PlayFab**

### 🎨 Supporting Systems

* Animation
* Particle / visual effects
* Audio system
* UI system
* Timeline
* Shader Graph

---

## 🌐 Multiplayer Architecture

Networking được xây dựng dựa trên **Photon Fusion**.

```text
                 ┌─────────────────┐
                 │  Network Runner │
                 └────────┬────────┘
                          │
                    Photon Fusion
                          │
             ┌────────────┼────────────┐
             │            │            │
             ▼            ▼            ▼
          Player 1     Player 2     Player 3
             │            │            │
             └────────────┼────────────┘
                          │
                    Game Manager
                          │
                 Match State Sync
```

Một số thành phần networking chính:

```text
NetworkRunnerHandler.cs
PlayerSpawn.cs
PlayerSetup.cs
PlayerProperties.cs
GameManager.cs
```

---

## ☁️ Backend

**PlayFab** được sử dụng để hỗ trợ các chức năng backend của game.

Các hệ thống liên quan bao gồm:

* Player authentication
* Player information
* Match data
* Match result
* Player statistics
* Leaderboard
* PlayStream events

```text
Game
 │
 ▼
PlayFabManager
 │
 ├── Authentication
 ├── Player Data
 ├── Match Data
 ├── Statistics
 └── PlayStream
```

---

## 📁 Project Structure

```text
Game3D_MeowAndSeek/
│
├── Assets/
│   ├── Scenes/
│   │   └── GamePlay.unity
│   │
│   ├── Scripts/
│   │   ├── GameManager.cs
│   │   ├── PlayerMovement.cs
│   │   ├── PlayerProperties.cs
│   │   ├── PlayerSetup.cs
│   │   ├── PlayerSpawn.cs
│   │   ├── NetworkRunnerHandler.cs
│   │   ├── PlayFabManager.cs
│   │   ├── ChatManager.cs
│   │   └── ...
│   │
│   ├── Photon/
│   ├── PlayFabSDK/
│   ├── Sounds/
│   ├── prefabs/
│   └── ...
│
├── Packages/
├── ProjectSettings/
├── .gitignore
└── GAM302.slnx
```

---

## 🎮 Controls

| Action              | Input        |
| ------------------- | ------------ |
| 🚶 Move             | Input System |
| 🦘 Jump             | Input System |
| 🔊 Interact / Sound | Input System |
| ⚡ Ability           | `Q`          |

> Input được quản lý bằng **Unity Input System**.

---

## 🚀 Getting Started

### Requirements

* Unity **6000.3.9f1**
* Git
* Unity-compatible IDE
* Internet connection cho multiplayer/backend services

### Clone

```bash
git clone https://github.com/Ennemie/Game3D_MeowAndSeek.git
```

### Open Project

Mở project bằng **Unity 6000.3.9f1**.

Sau khi Unity hoàn tất import packages và assets, mở:

```text
Assets/Scenes/GamePlay.unity
```

Sau đó nhấn **Play** để chạy game.

---

## 🧪 Development

Project được tổ chức theo hướng tách các hệ thống gameplay thành nhiều component riêng biệt.

### Player System

```text
PlayerMovement
PlayerProperties
PlayerSetup
PlayerSpawn
```

### Game System

```text
GameManager
CanvaController
PlayHubController
```

### Network System

```text
NetworkRunnerHandler
PlayerSpawn
PlayerSetup
```

### Backend

```text
PlayFabManager
```

### Communication

```text
ChatManager
```

---

## 📸 Screenshots

### Gameplay

<p align="center">
  <img src="docs/images/gameplay.png" width="800" alt="Gameplay">
</p>

### Seeker

<p align="center">
  <img src="docs/images/seeker.png" width="800" alt="Seeker">
</p>

### Hider

<p align="center">
  <img src="docs/images/hider.png" width="800" alt="Hider">
</p>

> 💡 Thay các ảnh trên bằng screenshot thực tế của game trong thư mục `docs/images/`.

---

## 🎥 Demo

> 🚧 **Gameplay video coming soon**

<!--
[![Meow And Seek Gameplay](https://img.youtube.com/vi/YOUR_VIDEO_ID/maxresdefault.jpg)](https://www.youtube.com/watch?v=YOUR_VIDEO_ID)
-->

---

## 📚 What I Learned

Thông qua project này, mình đã có cơ hội làm việc với:

* 🎮 Unity game development
* 💻 C# gameplay programming
* 🌐 Multiplayer networking với Photon Fusion
* 🔄 Network state synchronization
* 👤 Player state management
* ⚔️ Gameplay ability systems
* 🎥 Camera và input systems
* ☁️ Backend integration với PlayFab
* 🗂️ Unity project organization
* 🐛 Debugging và multiplayer testing

---

## 🔮 Future Improvements

Một số hướng phát triển có thể mở rộng:

* [ ] Thêm nhiều map
* [ ] Thêm nhiều loại Hider / Seeker ability
* [ ] Cải thiện matchmaking
* [ ] Thêm player progression
* [ ] Cải thiện UI/UX
* [ ] Thêm nhiều visual effects
* [ ] Cải thiện balancing giữa Seeker và Hider
* [ ] Thêm nhiều gameplay modes

---

## 👨‍💻 Author

**Ennemie**

[![GitHub](https://img.shields.io/badge/GitHub-Ennemie-181717?style=for-the-badge\&logo=github)](https://github.com/Ennemie)

---

## 🔗 Repository

<p align="center">

<a href="https://github.com/Ennemie/Game3D_MeowAndSeek">
<img src="https://img.shields.io/badge/View%20Source%20Code-GitHub-181717?style=for-the-badge&logo=github" alt="View Source Code">
</a>

</p>

---

<p align="center">
  🐱 <b>Meow And Seek</b>
  <br>
  <i>Hide. Seek. Survive.</i>
</p>
