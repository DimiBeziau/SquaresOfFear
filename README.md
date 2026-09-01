# Squares Of Fear

Jeu de plateforme/puzzle développé avec **Unity 6 (6000.3.11f1)**, dans lequel le joueur doit progresser sur des niveaux composés de cubes/plateformes mobiles tout en évitant les pièges (bombes, marqueurs déclencheurs) qui parsèment le parcours.

## Aperçu

Le joueur avance à travers des niveaux générés à partir de plateformes de longueur variable. Certaines cases dissimulent des dangers (bombes de masse, déclencheurs) qu'il faut anticiper ou éviter pour progresser jusqu'à la fin du niveau.

## Structure du projet

Assets/
├── 00-Art/                     # Ressources artistiques (modèles, textures, matériaux)
├── 01-Levels/                  # Données et fichiers de niveaux
├── 02-Code/                    # Scripts C# du gameplay
├── 03-ThirdParty/              # Dépendances externes
├── MobileDependencyResolver/   # Résolution des dépendances mobiles
├── Scenes/                     # Scènes Unity
└── _Recovery/                  # Fichiers de récupération

### Scripts principaux (`Assets/02-Code`)

| Script | Rôle |
|---|---|
| `CameraScript.cs` | Gestion de la caméra |
| `CreatingLevel.cs` | Génération/construction des niveaux |
| `Level.cs` | Logique de niveau |
| `PlatformLength.cs` | Gestion de la longueur des plateformes |
| `CubeMove.cs` | Mouvement des cubes |
| `PlayerMove.cs` | Contrôle du joueur |
| `SimpleCharacterControl.cs` | Contrôle de personnage (base) |
| `MarkerTrigger.cs` | Déclencheurs/marqueurs de niveau |
| `MassBomb.cs` | Mécanique de bombe/piège |
| `MenuScript.cs` / `MenuSetup.cs` | Interface et configuration du menu |

## Prérequis

- [Unity Hub](https://unity.com/download)
- Unity **6000.3.11f1** (ou version compatible)

## Installation

1. Cloner le dépôt :
   ```bash
   git clone https://github.com/DimiBeziau/SquaresOfFear.git
2. Ouvrir le dossier du projet avec Unity Hub.
3. Laisser Unity importer les assets et résoudre les packages.
4. Ouvrir une scène dans Assets/Scenes/ et lancer le mode Play.
