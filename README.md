# UserService

UserService er en mikrotjeneste designet til at håndtere brugeroplysninger og validering af brugere for Auktionshuset. Tjenesten interagerer med AuthService via HTTP-klient og har grundlæggende CRUD-funktionalitet for brugerdata, lagret i MongoDB Atlas. Tjenesten bruger HashiCorp Vault til at sikre autorisation og administrerer roller som Admin og User via JWT Tokens.

## Funktioner

- Validerer loginoplysninger mod MongoDB Atlas-database og sender respons tilbage til AuthService.
- Grundlæggende CRUD-operationer for brugerdata.
- Integrerer med HashiCorp Vault for sikker autorisation på endepunkter.
- Understøtter JWT-baseret autorisation med role claims Admin og User som bliver generet over ved AuthService.
- Tjenesten kaldes af AuthService for validering og autentificering af brugere.

## Installation til development via docker

1. Klon dette repository:
    ```sh
    git clone https://github.com/Hablas-Si/UserService.git
    cd UserService
    code .
    ```
2. Klon AuthService repository :
   ```sh
    git clone https://github.com/Hablas-Si/AuthService.git
    cd AuthService/auth-test-env
    ```
3. Docker compose
      ```sh
    docker compose up -d
    ```
### Viderudvikling
Hvert push til main fra UserService eller andre vilkårlige services, bygger et dockerfile og pusher den automatisk op til dockerhub, så du skal bare compose down og compose up igen. Dette sker igennem github actions.
