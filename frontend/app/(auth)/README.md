# TalentVerse Authentication Pages

## Overview
This directory contains the authentication pages (Login and Register) for TalentVerse, built with Next.js 14 App Router, TypeScript, and Tailwind CSS.

## Structure
```
app/(auth)/
├── components/
│   └── AuthLayout.tsx      # Reusable split-screen layout component
├── login/
│   └── page.tsx            # Login page
└── register/
    └── page.tsx            # Register page
```

## Features
- **Split-Screen Layout**: Left side displays an image with branding, right side contains the form
- **Form Validation**: Client-side validation using `react-hook-form` and `zod`
- **Backend Integration**: Axios calls to .NET 9 backend API
- **Error Handling**: Displays validation errors and API errors inline
- **Token Management**: Stores JWT token in localStorage on successful auth
- **Auto-redirect**: Redirects to `/dashboard` after successful login/registration

## Design System
- **Background**: Deep Jungle Green (`bg-emerald-900`)
- **Card**: White background with rounded corners
- **Primary Button**: Orange (`bg-orange-600`, hover: `bg-orange-700`)
- **Inputs**: Light gray background (`bg-gray-50`), rounded corners (`rounded-xl`)
- **Typography**: 
  - Headers: Poppins (font-heading)
  - Body/Inputs: Inter (font-sans)

## API Endpoints
- **Login**: `POST /api/account/login`
  - Payload: `{ email, password }`
- **Register**: `POST /api/account/register`
  - Payload: `{ username, email, password, bio? }`

## Environment Variables
Create a `.env.local` file in the frontend directory:
```env
NEXT_PUBLIC_API_URL=http://localhost:8080
```

## Usage
Navigate to:
- Login: `http://localhost:3000/login`
- Register: `http://localhost:3000/register`

## Dependencies
All required dependencies are already installed:
- `react-hook-form` - Form state management
- `@hookform/resolvers` - Zod resolver for react-hook-form
- `zod` - Schema validation
- `axios` - HTTP client
- `next` - Next.js framework
- `tailwindcss` - Styling
