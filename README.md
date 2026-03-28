# LogSage

Paste your logs. Know what broke and why. In seconds.

LogSage is a developer-focused log analysis tool. Upload or paste any log file
and get errors grouped by pattern, root causes surfaced, and the noise stripped
away so you can debug faster.

## Features

- Auto-detects Serilog, NLog, Log4Net, Standard, and Plain Text formats
- Groups similar errors by pattern — not just exact match
- Instant analysis — results in under a second
- Free tier — no account needed to start
- Clean 3-panel error explorer dashboard

## Tech Stack

- Backend: .NET 9 Minimal API
- Frontend: Next.js 15 App Router + Tailwind CSS + shadcn/ui
- Database: PostgreSQL + EF Core 9
- Auth: JWT + Refresh tokens
- Payments: Paddle
- Hosting: Netlify (frontend) + Railway (backend)

## Project Structure

    logsage/
    logsage-backend/     .NET 9 API and Core engine
        LogSage.Core/    Log parser — 5 formats
        LogSage.Api/     REST API endpoints
        docker-compose.yml
    logsage-web/         Next.js 15 frontend

## Local Development

Prerequisites: .NET 9 SDK, Node.js 20+, Docker Desktop

Start everything:

    .\dev-start.ps1

This starts PostgreSQL, the API on port 5000, and the frontend on port 3000.

Manual start:

    cd logsage-backend
    docker-compose up -d db
    dotnet run --project LogSage.Api

    cd logsage-web
    npm run dev

## API Documentation

Interactive docs at http://localhost:5000/scalar (development only).

## Website

https://logsage.dev

## License

MIT
