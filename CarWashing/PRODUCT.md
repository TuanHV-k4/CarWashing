# AutoWash Pro

AutoWash Pro is a task-focused car washing management system for customers, staff, and administrators.

## Users

- Customers book wash appointments, manage vehicles, view loyalty points, redeem rewards, and chat with the AI assistant.
- Staff manage daily operations such as bookings, wash bays, payments, and wash completion.
- Administrators manage branches, services, loyalty tiers, rewards, promotions, users, behavioral logs, and AI reporting.

## Product Surface

The frontend is an authenticated operations dashboard with a customer-facing workspace and admin/staff management areas. The interface should prioritize fast scanning, predictable CRUD flows, clear API feedback, and dense operational data over marketing-style presentation.

## Backend Contract

The API is provided by the .NET backend in this repository. Local defaults:

- HTTP API: `http://localhost:5152`
- HTTPS API: `https://localhost:7083`
- Swagger: `/swagger`

The frontend should use the contract documented in `API_FOR_FE.md`, including auth, vehicles, customers, operations, payments, loyalty, promotions, wash histories, AI, admin users, and behavioral logs.

## Design Register

Product UI. Use restrained color, standard controls, readable tables, clear state handling, and responsive layouts for repeated operational use.
