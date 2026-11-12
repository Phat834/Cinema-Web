# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

ASP.NET Core 9.0 (Razor Pages / MVC), Bootstrap 5.3, Entity Framework Core 9.0, SQLite/SQL Server, Identity

## Users

**Primary:** Moviegoers looking to discover films, check showtimes, and book tickets online. They browse on mobile and desktop, often deciding last-minute or planning ahead.

**Secondary:** Cinema staff (employees) who scan tickets, manage showtimes, and operate the box office.

**Tertiary:** Administrators who manage movies, theaters, genres, combos, users, and view revenue analytics.

## Product Purpose

A cinema management and ticketing platform that lets moviegoers discover films, browse showtimes by theater/date/genre, select seats, and complete bookings — while giving staff the tools to scan tickets and admins the control to manage the full catalog and operations. Success means frictionless discovery-to-booking for customers and efficient daily operations for staff.

## Positioning

Unlike generic ticketing widgets, this is a full-stack cinema ecosystem: the same platform that powers the customer-facing discovery experience also runs the box-office scanner, the theater manager's showtime grid, and the admin's revenue dashboard. The mechanism is **unified theater operations** — one data model, one booking flow, one source of truth from poster to seat scan.

## Operating Context

- **Discovery:** Customers browse now-showing/coming-soon, filter by genre/theater, watch trailers, read descriptions, see ratings.
- **Booking:** Select theater → date → showtime → seats → combos → payment (MoMo QR, cash, card).
- **Box Office:** Staff scan QR codes on tickets to validate entry.
- **Management:** Admins CRUD movies, theaters, rooms, genres, combos; set showtimes; view revenue stats.
- **Environments:** Web-first, responsive; mobile-heavy for customers, desktop-heavy for staff/admin.

## Capabilities and Constraints

**Confirmed:**
- Movie CRUD with posters, banners (16:9), trailers, ratings, genres
- Theater/Room management with seat maps
- Showtime scheduling per room
- Seat selection with real-time availability
- Combo/food ordering integrated in booking
- Payment: MoMo QR code generation, pending/success states
- Role-based access: Admin, Employee, Customer
- Vietnamese language primary, English fallback
- Bootstrap 5 + custom CSS; jQuery for AJAX filtering

**Constraints:**
- No DESIGN.md or design tokens exist yet; current UI is Bootstrap defaults with inline styles
- Inline styles scattered across views (hero gradients, card shadows, modal styling)
- Bebas Neue loaded via Google Fonts for display headings only
- No dark mode, no design system, no component library
- Legacy IE-era patterns (fixed px heights, hardcoded colors)

**Undecided:** Design system scope (global vs. per-surface), asset pipeline for images, font self-hosting.

## Brand Commitments

**Name:** Cinema Management (displayed in navbar)
**Logo:** Film icon (FontAwesome `fa-film`) — no custom mark yet
**Voice:** Vietnamese-first, direct, functional. UI copy uses emoji as inline icons (🎬 🎭 🎥 ⭐).
**Colors in code:** Deep red `#ff0033` / `#ff4d4d` as primary accent; dark `#1a0000` / `#000` hero grounds; gold `#ffc107` for ratings.
**No formal brand guide exists.** The red/black/gold triad is the only visual continuity.

## Evidence on Hand

- **Real movie data:** Posters, banners, trailers, ratings, genres seeded in DB
- **Real theater data:** Cities, theaters, rooms with seat maps
- **Real showtimes:** Linked to movies, rooms, dates
- **No stock photography, no testimonials, no press, no case studies.**
- **Absences:** No hero photography, no lifestyle imagery, no custom illustrations, no icon system beyond FontAwesome.

## Product Principles

1. **Discovery before decision.** The homepage must sell the *experience* of cinema — immersion, anticipation, shared spectacle — not just list movies.
2. **Unified trust.** Customer, staff, and admin interfaces share one visual language so the brand feels whole, not fragmented.
3. **Density with breath.** Show rich data (showtimes, seats, combos) without crowding; use rhythm, not just white space.
4. **Red is the signal.** The brand red owns primary actions and urgent states; it never decorates.
5. **Motion as orientation.** Transitions guide the eye through the booking funnel (theater → date → time → seats → payment), not delight for its own sake.

## Accessibility & Inclusion

- WCAG AA contrast for all text and interactive elements
- Keyboard-navigable booking flow (seat map, date pickers, modals)
- Screen-reader labels for dynamic content (showtime loaders, seat selection)
- Vietnamese diacritics render correctly in chosen fonts
- Touch targets ≥ 44×44px on mobile