# Design System - Minimal Professional Template

Professional forex trading workspace using a minimalistic, intentional design system focused on clarity, readability, and trader workflows.

## Philosophy

**Minimal**: Only essential elements visible. No decorative borders, unnecessary shadows, or chrome. Content is prioritized.

**Professional**: Commercial-quality appearance. Intentional design choices. Polished and production-ready.

**Consistent**: All modules follow the same structure, spacing, and styling patterns. New features reuse established components.

**Trader-Focused**: Dark theme for extended viewing. High contrast for quick metric scanning. Quick access to frequent actions.

---

## Color Palette

| Name | Hex | Purpose |
|------|-----|---------|
| **Background** | #0F1115 | Main application background (very dark navy) |
| **Secondary BG** | #171B22 | Secondary background for depth |
| **Panel Surface** | #1A1F29 | Card/panel background (dark slate) |
| **Border** | #2D3748 | Subtle borders and dividers |
| **Primary Text** | #E5E7EB | Main text color (light gray) |
| **Secondary Text** | #9CA3AF | Supporting text, labels (muted gray) |
| **Accent** | #F5C400 | Interactive states, highlights (gold) |
| **Success** | #22C55E | Positive outcomes (green) |
| **Danger** | #EF4444 | Negative outcomes (red) |
| **Warning** | #F59E0B | Cautionary states (orange) |

**Usage Rules:**
- Accent color (#F5C400) appears ONLY on:
  - Active navigation items
  - Active module indicators
  - Interactive element hover/pressed states
  - Key action buttons
  - Metric value accents in critical situations

- Success/Danger colors used sparingly for:
  - Winning trades (green)
  - Losing trades (red)
  - P/L displays (green positive, red negative)

---

## Typography

All fonts: **Segoe UI** (system font on Windows)

| Style | Size | Weight | Usage |
|-------|------|--------|-------|
| **DS_Title22** | 22px | SemiBold | Module headers, main titles |
| **DS_SectionTitle16** | 16px | SemiBold | Section headers, subsections |
| **DS_Body13** | 13px | Regular | Body text, descriptions |
| **DS_Caption11** | 11px | Regular | Labels, small text |
| **DS_MetricValue** | 20px | SemiBold | Large metric numbers |
| **DS_MetricLabel** | 11px | Regular | Metric descriptions |

**Rules:**
- Only use sizes: 11, 13, 16, 20, 22 (no intermediate values)
- Title sizes always SemiBold for visual weight
- Body text always Regular (400 weight)
- Secondary text uses secondary color (#9CA3AF)

---

## Spacing System

8px base unit for all spacing.

| Constant | Value | Usage |
|----------|-------|-------|
| **XS** | 4px | Tight component spacing |
| **S** | 8px | Normal component spacing, card padding |
| **M** | 16px | Section spacing, margins between groups |
| **L** | 24px | Large spacing between major sections |
| **XL** | 32px | Extra large gaps |
| **2XL** | 48px | Maximum spacing |

**Card/Component Padding:**
- Default card: 8px all sides
- Metric cards: 12px horizontal, 8px vertical (spacious for numbers)

**Section Spacing:**
- Between sections: 12px
- After headers: 12px
- Between metric groups: 12px

---

## Component Library

### Card Styles (3-Tier System)

**1. Primary Card** (Large metrics, emphasis)
- Min height: 120px
- Padding: 12px H, 8px V
- Background: #1A1F29
- Border: 2px bottom accent (#F5C400)
- Use for: Account balance, Net P/L, Win rate, key metrics
- Typography: Label (11px caption) + Value (20px SemiBold)

**2. Secondary Card** (Supporting data, standard display)
- Min height: 60px
- Padding: 8px all sides
- Background: #1A1F29
- Border: 1px subtle (#171B22)
- Use for: Supporting metrics, data sections, content areas
- Typography: Label + Standard text

**3. Tertiary Card** (Text-only metrics, minimal styling)
- Padding: 8px all sides
- Background: Transparent
- Border: None
- Use for: Inline metrics, secondary information, labels
- Typography: Caption text, muted color

### Button Styles

**Primary Action** (High emphasis)
- Background: Accent (#F5C400)
- Text: White
- Padding: 10px H, 6px V
- Examples: "+ Add Trade", "Save", "Submit"

**Secondary Action** (Low emphasis)
- Background: Transparent
- Text: Accent (#F5C400)
- Border: Subtle
- Padding: 10px H, 6px V
- Examples: "↻ Reload", "View", "Expand"

### Typography Style Rules

- **Metric Value**: 20px SemiBold, primary text color (unless color-coded)
- **Metric Label**: 11px Regular, secondary text color
- **Section Header**: 16px SemiBold, primary text
- **Module Header**: 22px SemiBold, primary text
- **Supporting Text**: 13px Regular, secondary text

---

## Module Template Pattern

Every module follows this structure for consistency:

```
┌─────────────────────────────────┐
│ MODULE HEADER (Title22)         │ ← Header Section (Top)
│ [Action buttons]                │    Margin bottom: 12px
├─────────────────────────────────┤
│ ┌──────────┐ ┌──────────┐      │ ← Primary Metrics (if applicable)
│ │  Metric  │ │  Metric  │      │    PrimaryCard style
│ │  Value   │ │  Value   │      │    Margin bottom: 12px
│ └──────────┘ └──────────┘      │
├─────────────────────────────────┤
│ Secondary info inline...        │ ← Secondary Info (if applicable)
│                                 │    Secondary text color
│                                 │    Margin bottom: 12px
├─────────────────────────────────┤
│ ┌─────────────────────────────┐ │ ← Content Area
│ │                             │ │    SecondaryCard or DataGrid
│ │  Data Grid / Text / Forms   │ │
│ │                             │ │
│ └─────────────────────────────┘ │
└─────────────────────────────────┘
```

**Minimal Professional Rules:**
1. No decorative elements (no icons in backgrounds, minimal borders)
2. Clear visual hierarchy (primary → secondary → tertiary)
3. High contrast for readability
4. Consistent spacing throughout
5. Data prioritized over chrome
6. Trader-friendly (quick metric scanning)

---

## Module Implementations

### Dashboard Module
- **Primary Metrics**: Account Balance, Net P/L, Win Rate (3-column PrimaryCard)
- **Secondary Info**: Daily Risk, Profit Factor, Drawdown, Total Trades (inline text)
- **Content**: Daily Notes (large text area) + Market Clocks
- **Pattern**: Header → Metrics → Content → Secondary Content

### Trade Journal Module
- **Header**: "TRADE JOURNAL" + Action buttons
- **Primary Metrics**: Total Trades, Win Rate, Profit Factor, Net Profit (4-column PrimaryCard)
- **Content**: DataGrid with trade history
- **Pattern**: Header → Metrics → Section Title → DataGrid

### Markets Module (Browser)
- **Header**: "MARKETS - BROWSER" with optional toolbar
- **Content**: WebView2 embedded browser + bookmarks bar (optional)
- **Pattern**: Header → Optional Toolbar → Browser

### Settings Module
- **Header**: "CONFIGURATION" or "SETTINGS"
- **Sections**: Visibility, Security, Paths, Browser Config, Profiles (SecondaryCards)
- **Actions**: Save, Discard buttons
- **Pattern**: Header → Multiple Sections → Action Buttons

---

## Usage Examples

### Creating a New Metric Card
```xaml
<Border Style="{StaticResource PrimaryCard}">
  <StackPanel>
    <TextBlock Style="{StaticResource DS_MetricLabel}" Text="METRIC NAME" />
    <TextBlock Style="{StaticResource DS_MetricValue}" Text="{Binding Value}" />
  </StackPanel>
</Border>
```

### Creating a Module Section
```xaml
<StackPanel Margin="0,0,0,12">
  <TextBlock Style="{StaticResource DS_SectionTitle16}" Text="SECTION NAME" Margin="0,0,0,8" />
  <!-- Content goes here -->
</StackPanel>
```

### Creating Action Buttons
```xaml
<StackPanel Orientation="Horizontal">
  <Button Content="+ Primary Action" Padding="10,6" Margin="0,0,8,0" />
  <Button Content="Secondary" Padding="10,6" />
</StackPanel>
```

---

## Spacing Checklist for New Features

- [ ] Header to content margin: 12px
- [ ] Between sections: 12px margin
- [ ] Card padding: 8px (or 12px for metrics)
- [ ] Component spacing: 4-8px
- [ ] Only use spacing from grid: 4, 8, 12, 16, 20, 24, 32px
- [ ] Typography only: 11, 13, 16, 20, 22px
- [ ] Colors match palette (only 10 base colors)
- [ ] No decorative elements (no extra borders/shadows)

---

## Visual Consistency Checklist

- [ ] All module headers use DS_Title22
- [ ] All section headers use DS_SectionTitle16
- [ ] Metric labels use DS_MetricLabel (11px, secondary color)
- [ ] Metric values use DS_MetricValue (20px SemiBold)
- [ ] Primary metrics use PrimaryCard style
- [ ] Secondary info uses secondary text color or SecondaryCard
- [ ] Accent color (#F5C400) only on interactive/active states
- [ ] Success (green) / Danger (red) for P/L only
- [ ] High contrast maintained (light text on dark background)
- [ ] No hard-coded colors (use design system brushes)

---

## Production Readiness

A module is "production-ready" when:

✅ Follows minimal professional template pattern
✅ Uses design system colors, typography, spacing
✅ No hard-coded padding/margin values
✅ Consistent with other modules
✅ Clear visual hierarchy (primary/secondary/tertiary)
✅ High contrast for readability
✅ No unnecessary borders or decorative elements
✅ Trader-focused layout (quick metric scanning)
✅ All interactive elements use accent color intentionally

---

## Future Enhancements

- Visual elevation (subtle shadows on cards)
- Animation on state changes (smooth transitions)
- Responsive layouts (adaptive for different window sizes)
- Advanced typography (letter spacing, line height tuning)
- Accessibility improvements (focus indicators, keyboard navigation)

These can be added without breaking the minimal professional aesthetic.
