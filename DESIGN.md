# Design System — Cinema Management

<!-- impeccable:design-schema 1 -->

## Design Thesis

The cinema is a portal: a dark room where light becomes story. This design system translates that mechanism — **controlled darkness punctuated by deliberate light** — into a digital experience. The brand red (#E60023) is the projector beam: it signals action, never decorates. The deep charcoal grounds (#0A0A0B, #131316) are the auditorium. Gold (#FFB800) is the screen's reflection. Every surface, component, and motion serves the visitor's journey from discovery to booked seat.

## Color System

**Color Space:** OKLCH (perceptual lightness/chroma/hue) for predictable ramps across themes.

### Primitive Palette (OKLCH)

| Token | Light | Dark | Role |
|-------|-------|------|------|
| `--color-bg-0` | `0.98 0.005 280` | `0.04 0.005 280` | Deepest surface (auditorium) |
| `--color-bg-1` | `0.96 0.006 280` | `0.07 0.006 280` | Primary canvas |
| `--color-bg-2` | `0.93 0.008 280` | `0.11 0.008 280` | Elevated cards, modals |
| `--color-bg-3` | `0.89 0.01 280` | `0.16 0.01 280` | Hover/active surfaces |
| `--color-fg-0` | `0.15 0.01 280` | `0.95 0.005 280` | Primary text |
| `--color-fg-1` | `0.35 0.01 280` | `0.75 0.008 280` | Secondary text |
| `--color-fg-2` | `0.55 0.01 280` | `0.55 0.01 280` | Muted/placeholder |
| `--color-border-0` | `0.88 0.01 280` | `0.22 0.01 280` | Hairline borders |
| `--color-border-1` | `0.82 0.015 280` | `0.3 0.015 280` | Visible borders |
| `--color-brand` | `0.52 0.24 25` | `0.58 0.26 25` | Primary action (projector red) |
| `--color-brand-hover` | `0.46 0.26 25` | `0.64 0.28 25` | Hover state |
| `--color-brand-active` | `0.4 0.24 25` | `0.52 0.24 25` | Pressed state |
| `--color-gold` | `0.82 0.16 95` | `0.86 0.18 95` | Rating, highlights |
| `--color-gold-dim` | `0.72 0.12 95` | `0.76 0.14 95` | Muted gold |
| `--color-success` | `0.55 0.16 145` | `0.62 0.18 145` | Confirmed, valid |
| `--color-warning` | `0.75 0.18 85` | `0.8 0.2 85` | Pending, caution |
| `--color-error` | `0.52 0.24 25` | `0.58 0.26 25` | Blocking errors |

### Semantic Roles

```css
/* Surfaces */
--surface-canvas: var(--color-bg-1);
--surface-elevated: var(--color-bg-2);
--surface-overlay: var(--color-bg-0);
--surface-hover: var(--color-bg-3);

/* Text */
--text-primary: var(--color-fg-0);
--text-secondary: var(--color-fg-1);
--text-muted: var(--color-fg-2);
--text-on-brand: var(--color-fg-0); /* for dark brand bg */
--text-on-gold: var(--color-bg-0);

/* Actions */
--action-primary: var(--color-brand);
--action-primary-hover: var(--color-brand-hover);
--action-primary-active: var(--color-brand-active);
--action-secondary: var(--color-border-1);
--action-secondary-hover: var(--color-bg-3);

/* State */
--state-success: var(--color-success);
--state-warning: var(--color-warning);
--state-error: var(--color-error);
--state-info: var(--color-brand);

/* Focus */
--focus-ring: var(--color-brand);
--focus-ring-offset: var(--surface-canvas);
```

### Theme Strategy

**Dark-first.** Cinema is experienced in darkness. Light theme exists for admin/operate surfaces but is derived — not inverted — with elevated contrast and warmer neutrals. Brand red shifts slightly lighter in light mode to maintain 4.5:1 on white.

## Typography

### Font Families

| Role | Face | Source | Weights |
|------|------|--------|---------|
| **Display** | **Syne** | Self-hosted (woff2) | 400, 600, 700, 800 |
| **UI / Body** | **Instrument Sans** | Self-hosted (woff2) | 400, 500, 600, 700 |
| **Numeric / Data** | **JetBrains Mono** | Self-hosted (woff2) | 400, 500, 600 |

**Rationale:** Syne's wide, geometric caps evoke cinema marquees and poster lettering without period pastiche. Instrument Sans is a rigorous UI workhorse with excellent Vietnamese diacritic support. JetBrains Mono handles tabular showtimes, seat codes, prices.

### Type Scale (rem, fluid clamp)

| Token | Mobile | Desktop | Weight | Line Height | Letter Spacing | Role |
|-------|--------|---------|--------|-------------|----------------|------|
| `--text-display-1` | `clamp(2.5rem, 6vw, 4.5rem)` | 700 | 1.05 | -0.03em | Hero headlines |
| `--text-display-2` | `clamp(2rem, 4.5vw, 3.5rem)` | 600 | 1.1 | -0.02em | Section heroes |
| `--text-display-3` | `clamp(1.5rem, 3vw, 2.25rem)` | 600 | 1.15 | -0.01em | Card titles, modal headers |
| `--text-heading-1` | `1.75rem` | `2rem` | 600 | 1.2 | 0 | Page titles |
| `--text-heading-2` | `1.375rem` | `1.5rem` | 600 | 1.25 | 0 | Section headers |
| `--text-heading-3` | `1.125rem` | `1.25rem` | 600 | 1.3 | 0 | Subsections |
| `--text-body-lg` | `1.125rem` | `1.125rem` | 400 | 1.6 | 0 | Lead copy |
| `--text-body` | `1rem` | `1rem` | 400 | 1.6 | 0 | Body copy |
| `--text-body-sm` | `0.875rem` | `0.875rem` | 400 | 1.55 | 0 | Metadata, captions |
| `--text-label` | `0.8125rem` | `0.8125rem` | 500 | 1.5 | 0.01em | Form labels, buttons |
| `--text-caption` | `0.75rem` | `0.75rem` | 400 | 1.5 | 0.02em | Timestamps, helpers |
| `--text-mono` | `0.875rem` | `0.875rem` | 400 | 1.5 | 0 | Seat codes, times, prices |

### Text Wrap

- Headings: `text-wrap: balance`
- Body: `text-wrap: pretty`
- Numeric: `font-variant-numeric: tabular-nums`

## Spacing System

**Base unit:** 4px (`--space-1 = 0.25rem`)

### Scale

| Token | Value | Use |
|-------|-------|-----|
| `--space-0` | `0` | Reset |
| `--space-1` | `0.25rem` (4px) | Tight inline gaps |
| `--space-2` | `0.5rem` (8px) | Component internal padding |
| `--space-3` | `0.75rem` (12px) | Form field gaps |
| `--space-4` | `1rem` (16px) | Card padding, button padding |
| `--space-5` | `1.5rem` (24px) | Section internal rhythm |
| `--space-6` | `2rem` (32px) | Section separation |
| `--space-7` | `3rem` (48px) | Major section breaks |
| `--space-8` | `4rem` (64px) | Page-level breaks |
| `--space-9` | `6rem` (96px) | Hero vertical space |

### Semantic Spacing

```css
--gap-inline: var(--space-3);      /* horizontal sibling gap */
--gap-stack: var(--space-4);       /* vertical sibling gap */
--gap-section: var(--space-7);     /* between major sections */
--gap-cluster: var(--space-2);     /* tight groups (badges, chips) */
--pad-card: var(--space-4);        /* card internal padding */
--pad-modal: var(--space-6);       /* modal internal padding */
--pad-container: var(--space-5);   /* page container padding */
```

## Border Radius

| Token | Value | Use |
|-------|-------|-----|
| `--radius-none` | `0` | Sharp corners (modals on mobile) |
| `--radius-sm` | `0.25rem` (4px) | Chips, badges, small buttons |
| `--radius-md` | `0.5rem` (8px) | Cards, inputs, dropdowns |
| `--radius-lg` | `0.75rem` (12px) | Modals, large cards |
| `--radius-xl` | `1rem` (16px) | Hero containers |
| `--radius-full` | `9999px` | Pills, avatars |

## Shadows & Depth

**Elevation scale** (layered, not stacked):

```css
--shadow-1: 0 1px 2px oklch(0 0 0 / 0.08), 0 1px 1px oklch(0 0 0 / 0.06);  /* resting card */
--shadow-2: 0 4px 8px oklch(0 0 0 / 0.12), 0 2px 4px oklch(0 0 0 / 0.08);  /* elevated card */
--shadow-3: 0 12px 24px oklch(0 0 0 / 0.18), 0 4px 8px oklch(0 0 0 / 0.12); /* modal, dropdown */
--shadow-4: 0 24px 48px oklch(0 0 0 / 0.25), 0 8px 16px oklch(0 0 0 / 0.15); /* hero overlay */
--shadow-focus: 0 0 0 3px oklch(var(--color-brand) / 0.35);                  /* focus ring */
```

**Dark mode shadows use higher opacity** since the ground is darker.

## Motion System

### Timing Functions

| Token | Value | Use |
|-------|-------|-----|
| `--ease-standard` | `cubic-bezier(0.2, 0, 0, 1)` | Default, most motion |
| `--ease-expressive` | `cubic-bezier(0.34, 1.56, 0.64, 1)` | Entrance, reveals |
| `--ease-sharp` | `cubic-bezier(0.4, 0, 1, 1)` | Exits, dismissals |

### Duration Scale

| Token | Value | Use |
|-------|-------|-----|
| `--duration-1` | `100ms` | Hover, press |
| `--duration-2` | `200ms` | Transitions, toggles |
| `--duration-3` | `300ms` | Modals, drawers, reveals |
| `--duration-4` | `500ms` | Page transitions, complex sequences |

### Motion Principles

1. **One authored moment per viewport.** The hero banner entrance (scale + fade) is the signature motion. Everything else uses standard transitions.
2. **Exponential ease-out from visible default.** No "enter from opacity:0" on content that should be readable immediately.
3. **Respect `prefers-reduced-motion`.** All non-essential motion disabled.
4. **Motion serves orientation.** Seat selection: row highlight flows directionally. Booking steps: slide transition matches funnel direction.

## Component Primitives

### Buttons

| Variant | Background | Text | Border | Use |
|---------|------------|------|--------|-----|
| Primary | `--action-primary` | `--text-on-brand` | none | Main CTAs (Book Now, Pay) |
| Primary-hover | `--action-primary-hover` | `--text-on-brand` | none | Hover |
| Primary-active | `--action-primary-active` | `--text-on-brand` | none | Pressed |
| Secondary | transparent | `--text-primary` | `1px solid var(--action-secondary)` | Secondary actions |
| Ghost | transparent | `--text-secondary` | none | Tertiary, destructive |
| Danger | `--state-error` | `--text-on-brand` | none | Delete, logout |

**Size tokens:** `--btn-padding-x: var(--space-4)`, `--btn-padding-y: var(--space-2)`, `--btn-gap: var(--space-2)`

### Cards

```css
.card {
  background: var(--surface-elevated);
  border: 1px solid var(--color-border-0);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-1);
  transition: box-shadow var(--duration-2) var(--ease-standard),
              transform var(--duration-2) var(--ease-standard);
}
.card:hover {
  box-shadow: var(--shadow-2);
  transform: translateY(-2px);
}
.card-interactive { cursor: pointer; }
```

### Form Controls

- Inputs: `--surface-elevated` bg, `--color-border-0` border, `--color-border-1` focus border
- Labels: `--text-label`, `--text-secondary` for helpers
- Error: `--state-error` border, `--state-error` text
- Select/styled: match input appearance

### Badges / Chips

```css
.badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-1);
  padding: var(--space-1) var(--space-2);
  border-radius: var(--radius-full);
  font: var(--text-caption);
  font-weight: 500;
}
.badge-genre { background: var(--surface-hover); color: var(--text-primary); border: 1px solid var(--color-border-0); }
.badge-rating { background: var(--color-gold); color: var(--text-on-gold); }
.badge-status { /* semantic colors */ }
```

### Modal / Drawer

- Backdrop: `oklch(0 0 0 / 0.6)` + `backdrop-filter: blur(4px)`
- Panel: `--surface-elevated`, `--shadow-3`, `--radius-lg`
- Focus trap, ESC to close, click backdrop to close (except critical flows)

## Layout Primitives

### Container

```css
.container {
  width: 100%;
  max-width: 1280px;
  margin-inline: auto;
  padding-inline: var(--pad-container);
}
@media (min-width: 1440px) { .container { max-width: 1400px; } }
```

### Grid

- 12-column fluid grid, CSS Grid based
- Gap: `var(--gap-section)` vertical, `var(--gap-inline)` horizontal
- Breakpoints: `sm: 640px`, `md: 768px`, `lg: 1024px`, `xl: 1280px`, `2xl: 1440px`

### Section Rhythm

```css
.section { padding-block: var(--gap-section); }
.section-hero { padding-block: var(--space-9); }
.section-compact { padding-block: var(--space-5); }
```

## Icon System

- **Primary:** Phosphor Icons (duotone weight) — self-hosted SVG sprite
- **Fallback:** FontAwesome 6 (current) — migrate progressively
- **Size tokens:** `--icon-sm: 1rem`, `--icon-md: 1.25rem`, `--icon-lg: 1.5rem`, `--icon-xl: 2rem`
- **Stroke:** `1.5px` equivalent, consistent across set

## Image Treatment

- Posters: 2:3 ratio, `object-fit: cover`, `--radius-md`
- Banners: 16:9 ratio, `object-fit: cover`, `--radius-lg` for hero
- Thumbnails: 16:9 or 1:1, `--radius-sm`
- Lazy load with `loading="lazy"`, LQIP blur placeholder
- Hero banner: subtle vignette overlay for text legibility

## Responsive Behavior

| Breakpoint | Container | Hero | Cards/Grid | Navigation |
|------------|-----------|------|------------|------------|
| `< 640px` | 100% - 16px | Full bleed, stacked | 1 col | Hamburger, bottom sheet |
| `640-1023px` | 100% - 24px | Full bleed | 2 col | Hamburger, drawer |
| `1024-1279px` | 100% - 32px | Contained | 3 col | Horizontal, condensed |
| `1280-1439px` | 1280px | Contained | 4 col | Full horizontal |
| `≥ 1440px` | 1400px | Contained | 4-5 col | Full horizontal |

## Accessibility Commitments

- All semantic tokens meet WCAG AA (4.5:1 text, 3:1 UI) in both themes
- Focus visible: `--shadow-focus` on all interactive elements
- Skip link at page start
- ARIA live regions for dynamic content (showtime loads, seat updates)
- Vietnamese diacritics tested in all font weights
- Touch targets ≥ 44×44px (enforced via min-height on buttons/links)

## Implementation Notes

- **CSS Custom Properties** for all tokens (no Sass variables in output)
- **Self-hosted fonts** via `@font-face` with `font-display: swap`
- **No Google Fonts, no external CDN for design assets** in production
- **Bootstrap 5** used only for utilities (grid, flex, spacing) — components rebuilt in design system vocabulary
- **RTL ready** (logical properties: `inline-start`, `block-end`)
- **Print stylesheet** hides nav, footer, modals; shows booking confirmation cleanly

## Version

`1.0.0` — Initial design system for Cinema Management homepage redesign (Persuade surface)