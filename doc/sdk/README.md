

---

# API pour la carte eID belge - Projet ASP.NET

Ce projet fournit une API pour interagir avec la carte eID belge en utilisant un wrapper C# (`EidSamples` et `Pkcs11Net`). Il permet de récupérer des données personnelles (par exemple, nom, date de naissance, photo) depuis la carte eID, de générer des défis (challenges), d'authentifier des utilisateurs, et de signer des données en utilisant les clés privées stockées sur la carte.
L'api Asp.net se trouve dans [examples/CS/TestApi](examples/CS/TestApi).


---

## Table des matières
1. [Fonctionnalités](#fonctionnalités)
2. [Dépendances](#dépendances)
3. [Installation](#installation)
4. [Endpoints de l'API](#endpoints-de-lapi)
5. [Utilisation](#utilisation)
6. [Licence](#licence)

---

## Fonctionnalités
- Récupérer des données personnelles depuis la carte eID belge :
  - Photo
  - Nom
  - Date de naissance
  - Labels des certificats
- Générer un défi aléatoire pour l'authentification.
- Authentifier les utilisateurs en vérifiant une signature par rapport à un défi.
- Signer des données en utilisant les clés privées stockées sur la carte eID.

---

## Dépendances
1. **EidSamples** : Un wrapper C# pour interagir avec la carte eID belge.
2. **Pkcs11Net** : Un wrapper .NET pour PKCS#11, utilisé pour communiquer avec le module cryptographique de la carte eID.
3. **ASP.NET Core** : Le framework utilisé pour construire l'API.
4. **Middleware eID belge** : Assurez-vous que le middleware eID belge est installé sur votre système pour accéder à la carte eID.

---

## Installation
1. **Installer les dépendances** :
   - Installez le middleware eID belge depuis le site officiel [eID belge](https://eid.belgium.be/).
   - Assurez-vous que le fichier `beidpkcs11.dll` est disponible sur votre système.

2. **Cloner le dépôt** :
   ```bash
   git clone https://github.com/your-repo/belgian-eid-api.git
   cd belgian-eid-api
   ```

3. **Compiler le projet** :
   - Ouvrez le projet dans Visual Studio ou votre IDE préféré.
   - Restaurez les packages NuGet :
     ```bash
     dotnet restore
     ```
   - Compilez le projet :
     ```bash
     dotnet build
     ```

4. **Lancer le projet** :
   - Démarrez l'API :
     ```bash
     dotnet run
     ```
   - L'API sera disponible à l'adresse `http://localhost:5013` (ou `https://localhost:7043` pour HTTPS).


### OU PLUS SIMPLE

1. **Cloner le dépôt** :
```bash
   git clone https://github.com/JeremyMarbaise/eid-mw.git
```
2. **Utiliser Visual Studio et ouvrir le fichier sln**

  Le fichier sln se trouve dans [doc/sdk/examples/CS/EidSamples.sln](examples/CS/EidSamples.sln)

3. **Lancer le projet**
   Avec Visual Studio choisir le projet de source TestApi et lancer
---


## Endpoints de l'API

### Fichier HTML pour tester l'api se trouve dans [index.html](index.html)

### 1. **Récupérer la photo**
- **Endpoint**: `GET /photo`
- **Description**: Récupère la photo stockée sur la carte eID.
- **Réponse**: Image JPEG.

### 2. **Récupérer le nom**
- **Endpoint**: `GET /name`
- **Description**: Récupère le nom (nom de famille) depuis la carte eID.
- **Réponse**:
  ```json
  {
    "Name": "Doe"
  }
  ```

### 3. **Récupérer les labels des certificats**
- **Endpoint**: `GET /labels`
- **Description**: Récupère les labels des certificats depuis la carte eID.
- **Réponse**:
  ```json
  {
    "Label": "Signature"
  }
  ```

### 4. **Récupérer la date de naissance**
- **Endpoint**: `GET /dob`
- **Description**: Récupère la date de naissance depuis la carte eID.
- **Réponse**:
  ```json
  {
    "DateOfBirth": "1980-01-01"
  }
  ```

### 5. **Générer un défi (challenge)**
- **Endpoint**: `GET /auth/challenge`
- **Description**: Génère un défi aléatoire pour l'authentification.
- **Réponse**:
  ```json
  {
    "Challenge": "550e8400-e29b-41d4-a716-446655440000"
  }
  ```

### 6. **Authentifier**
- **Endpoint**: `POST /auth/authenticate`
- **Description**: Authentifie l'utilisateur en vérifiant la signature par rapport au défi.
- **Corps de la requête**:
  ```json
  {
    "Challenge": "550e8400-e29b-41d4-a716-446655440000",
    "Signature": "signature-encodée-en-base64"
  }
  ```
- **Réponse**:
  ```json
  {
    "Authenticated": true
  }
  ```

### 7. **Signer des données**
- **Endpoint**: `POST /auth/sign`
- **Description**: Signe les données fournies en utilisant la clé privée stockée sur la carte eID.
- **Corps de la requête**:
  ```json
  {
    "data": "données-encodées-en-base64"
  }
  ```
- **Réponse**: Données signées encodées en base64.

---
### 8. **Récupération du certificat**
- **Endpoint**: `GET /auth/certificate`
- **Description**: Récupère le certificat RN de la carte eid.
- **Réponse**:
  ```json
  {
    "certificate": "certificat-encodées-en-base64"
  }
  ```

## Utilisation
1. **Récupérer des données** :
   - Utilisez les endpoints `/photo`, `/name`, `/labels`, et `/dob` pour récupérer les données personnelles depuis la carte eID.

2. **Authentification** :
   - Générez un défi avec `/auth/challenge`.
   - Signez le défi côté client en utilisant la carte eID.
   - Vérifiez la signature avec `/auth/authenticate`.

3. **Signer des données** :
   - Envoyez des données à `/auth/sign` pour les faire signer en utilisant la clé privée de la carte eID.

---

