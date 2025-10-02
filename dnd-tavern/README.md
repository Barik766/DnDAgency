# DnD Tavern — Marketplace for Tabletop Games

A small marketplace for finding and booking tabletop RPG sessions — campaigns, game masters (GMs), bookings and a GM dashboard. Built with Angular (feature-based structure) and prepared for containerized deployment (Docker + nginx). This README explains how to run, build and deploy the project, and points out the most relevant places in the repo for recruiters or reviewers.

 Preview
![alt text](Screenshot_1.jpg)

Quick highlights
- Feature-based Angular app (src/app/core/features/*)
- SSR-capable sources present (main.server.ts / server.ts)
- Dockerfile + nginx config for static hosting
- docker-compose.yml for local multi-service runs (frontend + backend image)
- GitHub Actions workflow (.github/workflows/deploy.yml) — builds and pushes Docker images and deploys to AWS Elastic Beanstalk

Tech stack
- Frontend: Angular 20, RxJS, Bootstrap 5, SCSS
- Server-side rendering files present (Angular SSR)
- Containerization: Docker + nginx (frontend), backend image published on Docker Hub
- CI/CD: GitHub Actions → AWS Elastic Beanstalk (configured)

Repository layout (short)
- src/ — Angular app
  - app/ — feature modules & routes
  - core/ — models, shared services, features (auth, campaigns, bookings, masters, gm-dashboard)
  - stylesFolder/ — design tokens, mixins, components
  - main.ts / main.server.ts / server.ts — client & SSR entry points
- Dockerfile — frontend build + nginx static server
- nginx.conf — nginx reverse proxy config (for /api → backend)
- docker-compose.yml — composer for local multi-service deployment
- .github/workflows/deploy.yml — CI pipeline (build & push Docker images; deploy ZIP to EB)

Getting started — local development (fast)
Prerequisites
- Node.js 20.x (repo uses node:20 in Dockerfile)
- npm (or pnpm)
- Angular CLI (optional; dev scripts use ng)

Basic local dev (client only)
1. Install deps:
   - PowerShell / CMD:
     npm ci
2. Run dev server:
     npm start
   - Opens at http://localhost:4200 by default
3. To run tests:
     npm test

Run with backend (local backend required)
- The frontend expects an API at /api/* (proxy or full URL).
- If you have the backend running locally at :8080, configure the proxy or environment to forward /api to it.

Run with Docker (recommended for recruiters/demo)
- docker-compose is provided and expects backend image and secrets:
  - Edit environment variables (or create a .env file) required by backend: RDS_HOSTNAME, RDS_PORT, RDS_DB_NAME, RDS_USERNAME, RDS_PASSWORD, JwtSettings__Key, JwtSettings__Issuer, JwtSettings__Audience, GoogleOAuth__ClientId, GoogleOAuth__ClientSecret
- Start:
  - PowerShell / CMD:
    docker compose up --build
- Ports:
  - Frontend served on host port 80 (mapped from container)
  - Backend container in compose exposes 8080 and is mapped to host 5000 (docker-compose.yml maps "5000:8080")

Build frontend image locally (same steps as CI)
- From dnd-tavern directory:
  docker build -t my-dnd-frontend:local .
- The Dockerfile builds Angular, copies dist into nginx, and uses nginx.conf which forwards /api/ to a service named `backend` (use docker-compose to wire them).

Important config files
- docker-compose.yml — uses backend image barik766/dnd-backend:latest and frontend image barik766/dnd-frontend:v3; change tags to your builds or use local images.
- .github/workflows/deploy.yml — CI:
  - Builds backend and frontend Docker images, pushes to Docker Hub, zips docker-compose.yml and deploys to AWS Elastic Beanstalk.
- nginx.conf — proxies /api/ to http://backend:8080/ inside the Docker network and serves SPA routes via try_files.

Environment variables (used by docker-compose / backend)
- RDS_HOSTNAME, RDS_PORT, RDS_DB_NAME, RDS_USERNAME, RDS_PASSWORD — database connection
- JwtSettings__Key, JwtSettings__Issuer, JwtSettings__Audience — JWT config
- GoogleOAuth__ClientId, GoogleOAuth__ClientSecret — Google OAuth keys