# Copilot Instructions for Authenticatly

## Build, Test, and Lint Commands

- **Build the solution:**
  ```sh
  dotnet build Authenticatly.sln
  ```
- **Run all tests:**
  ```sh
  dotnet test Authenticatly.sln
  ```
- **Run a single test file:**
  ```sh
  dotnet test src/Authenticatly.Tests/AuthenticatlyAuthorizationMiddlewareResultHandlerTest.cs
  ```
- **Azure Pipelines:**
  The pipeline restores NuGet packages, builds the solution, and runs all tests. See `azure-pipelines.yml` for details.

## High-Level Architecture

- **Solution Structure:**
  - `Authenticatly/`: Core authentication library (OAuth2-like, built on ASP.NET Core Identity)
  - `ExampleApi/`: Example API demonstrating integration and required service implementations
  - `Authenticatly.Tests/`: Unit tests for the core library
  - `Authenticatly.IntegrationTests/`: Integration tests for API endpoints

- **Key Features:**
  - Provides `/auth/v1/login` and `/auth/v1/logout` endpoints
  - Supports TOTP-based 2FA (implement `IMfaTokenService`)
  - Authorization for Minimal APIs via `.RequireAuthenticatlyAuth()`
  - Authorization for Controllers via `[AuthenticatlyAuthorize]` attribute

- **Required Service Implementations:**
  - `IMfaTokenService` (see `ExampleApi/Services/MfaTokenService.cs`)
  - `ISendSmsService` (see `ExampleApi/Services/SmsService.cs`)
  - `IClaimsInjectionService` (see `ExampleApi/Services/ClaimsInjectionService.cs`)

## Key Conventions

- **Claims Extraction:**
  Access claims in endpoints via `HttpContext.Items[AuthenticatlyAuthConstants.AUTHORIZED_ATTRIBUTES_KEY]`.
- **Configuration:**
  Register Identity and Authenticatly services in DI as shown in the README.
- **Testing:**
  Use MSTest for both unit and integration tests. Test files are in `Authenticatly.Tests` and `Authenticatly.IntegrationTests`.
- **.NET Version:**
  Target framework is `net9.0` for all projects.

---

This file was generated to help Copilot and other AI tools understand the structure, build/test workflow, and conventions of this repository. If you want to adjust or add coverage for additional areas, let me know!