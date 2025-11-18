# MyTodos React Client

React app with TypeScript and Tailwind CSS.

## Tech Stack

- Vite + React 18 + TypeScript
- Tailwind CSS v4
- React Router
- Axios (JWT interceptors)

## Getting Started

```bash
npm install
npm run dev
```

Visit http://localhost:3000

Make sure the API Gateway is running on http://localhost:8080

## Structure

```
src/
├── api/          # API client (auth, todos)
├── components/   # TodoItem, etc.
├── contexts/     # AuthContext
├── pages/        # Login, Dashboard
├── routes/       # ProtectedRoute guard
└── types/        # TypeScript interfaces
```

## Features

- JWT auth (token in localStorage)
- Protected routes
- Todo CRUD with Tailwind UI
- Type-safe API calls

---

*React frontend for MyTodos microservices take-home assignment.*
