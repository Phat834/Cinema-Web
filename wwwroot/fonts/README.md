# Self-Hosted Fonts

This directory contains the self-hosted font files for the Cinema Management design system.

## Required Fonts

### 1. Syne (Display)
- **Source:** https://github.com/google/fonts/tree/main/ofl/syne
- **Weights needed:** 400, 600, 700, 800 (Variable font recommended)
- **File:** `Syne-VariableFont_wght.woff2` (or individual weight files)
- **Used for:** Hero headlines, section titles, card titles, display text

### 2. Instrument Sans (UI/Body)
- **Source:** https://github.com/google/fonts/tree/main/ofl/instrumentsans
- **Weights needed:** 400, 500, 600, 700 (Variable font recommended)
- **File:** `InstrumentSans-VariableFont_wght.woff2` (or individual weight files)
- **Used for:** Body text, UI labels, buttons, form controls, navigation

### 3. JetBrains Mono (Numeric/Data)
- **Source:** https://github.com/JetBrains/JetBrainsMono
- **Weights needed:** 400, 500, 600 (Variable font recommended)
- **File:** `JetBrainsMono-VariableFont_wght.woff2` (or individual weight files)
- **Used for:** Tabular numbers, seat codes, showtimes, prices, code

## Download Instructions

1. Download the variable font (.woff2) files from the sources above
2. Place them in their respective directories:
   - `wwwroot/fonts/Syne/Syne-VariableFont_wght.woff2`
   - `wwwroot/fonts/Instrument_Sans/InstrumentSans-VariableFont_wght.woff2`
   - `wwwroot/fonts/JetBrains_Mono/JetBrainsMono-VariableFont_wght.woff2`

## Fallback Stack

If fonts are not yet self-hosted, the CSS falls back to:
- Syne → system-ui, sans-serif
- Instrument Sans → system-ui, sans-serif
- JetBrains Mono → ui-monospace, monospace

## Vietnamese Diacritic Support

All three fonts have full Vietnamese diacritic support. Verify rendering of: àáảãạ, ăắằẳẵặ, âấầẩẫậ, èéẻẽẹ, êếềểễệ, ìíỉĩị, òóỏõọ, ôốồổỗộ, ơớờởỡợ, ùúủũụ, ưứừửữự, ỳýỷỹỵ, đ

## Licensing

All fonts are SIL Open Font License (OFL) - free for commercial use, modification, and distribution.